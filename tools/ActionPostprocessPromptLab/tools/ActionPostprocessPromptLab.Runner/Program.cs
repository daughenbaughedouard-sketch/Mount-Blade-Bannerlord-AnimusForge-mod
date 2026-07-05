using ActionPostprocessPromptLab.Core;

var options = ParseArgs(args);
var service = new PromptLabService();
var repoRoot = GetOption(options, "repo-root");
if (string.IsNullOrWhiteSpace(repoRoot))
{
    repoRoot = service.FindDefaultRepoRoot(AppContext.BaseDirectory);
}

if (string.IsNullOrWhiteSpace(repoRoot))
{
    repoRoot = service.FindDefaultRepoRoot(Directory.GetCurrentDirectory());
}

if (string.IsNullOrWhiteSpace(repoRoot))
{
    Console.Error.WriteLine("找不到仓库根目录。可以传入 --repo-root <路径>。");
    return 1;
}

var labRoot = service.ResolveLabRoot(repoRoot);
var settings = service.LoadSettings(labRoot);
ApplyOption(options, "api-url", value => settings.ApiUrl = value);
ApplyOption(options, "api-key", value => settings.ApiKey = value);
ApplyOption(options, "api-protocol", value => settings.ApiProtocol = PromptLabService.NormalizeApiProtocolSelection(value));
ApplyOption(options, "model", value => settings.Model = value);
ApplyOption(options, "prompt-version", value => settings.PromptVersionPath = value);
ApplyOption(options, "reasoning-effort", value => settings.ReasoningEffort = PromptLabService.NormalizeReasoningEffortSelection(value));
ApplyOption(options, "thinking-enabled", value => settings.ThinkingEnabled = ParseBool(value, settings.ThinkingEnabled));
if (int.TryParse(GetOption(options, "max-tokens"), out var maxTokens))
{
    settings.MaxTokens = maxTokens;
}

if (float.TryParse(GetOption(options, "temperature"), out var temperature))
{
    settings.Temperature = temperature;
}

var caseFile = GetOption(options, "case-file");
if (string.IsNullOrWhiteSpace(caseFile))
{
    caseFile = Path.Combine(labRoot, "cases", "sample_cases.jsonl");
}

var promptPath = File.Exists(settings.PromptVersionPath) ? settings.PromptVersionPath : "";
var catalog = service.LoadCatalog(repoRoot, promptPath);
ActionPostprocessConfigModel? promptOverride = null;
if (!string.IsNullOrWhiteSpace(promptPath))
{
    promptOverride = service.LoadPromptVersion(promptPath);
}

var cases = service.LoadCases(caseFile);
if (cases.Count == 0)
{
    Console.Error.WriteLine("没有找到案例：" + caseFile);
    return 1;
}

var originalCaseCount = cases.Count;
var skipCases = 0;
var takeCases = 0;
if (int.TryParse(GetOption(options, "skip"), out var parsedSkip) && parsedSkip > 0)
{
    skipCases = parsedSkip;
}

if (int.TryParse(GetOption(options, "take"), out var parsedTake) && parsedTake > 0)
{
    takeCases = parsedTake;
}

if (skipCases > 0 || takeCases > 0)
{
    var query = cases.Skip(skipCases);
    if (takeCases > 0)
    {
        query = query.Take(takeCases);
    }

    cases = query.ToList();
    if (cases.Count == 0)
    {
        Console.Error.WriteLine("切片后没有案例。原始案例数：" + originalCaseCount + "，skip=" + skipCases + "，take=" + takeCases);
        return 1;
    }
}

var runDir = service.CreateRunDirectory(labRoot);
var dryRun = HasFlag(options, "dry-run");
var dryResponse = GetOption(options, "dry-response");
if (string.IsNullOrWhiteSpace(dryResponse))
{
    dryResponse = "[ACTION:MOOD:NEUTRAL]";
}

var concurrency = 1;
if (int.TryParse(GetOption(options, "concurrency"), out var parsedConcurrency) && parsedConcurrency > 1)
{
    concurrency = Math.Min(parsedConcurrency, cases.Count);
}

Console.WriteLine("仓库：" + repoRoot);
Console.WriteLine("案例文件：" + caseFile);
Console.WriteLine("案例数：" + cases.Count + (cases.Count == originalCaseCount ? "" : "（原始 " + originalCaseCount + "，skip=" + skipCases + "，take=" + takeCases + "）"));
Console.WriteLine("提示词版本：" + (string.IsNullOrWhiteSpace(promptPath) ? "使用模组默认版本" : promptPath));
Console.WriteLine("思考模式：" + (settings.ThinkingEnabled ? "开启" : "关闭") + "，强度：" + PromptLabService.NormalizeReasoningEffortSelection(settings.ReasoningEffort));
Console.WriteLine("并发数：" + concurrency);
Console.WriteLine("运行目录：" + runDir);

if (concurrency <= 1)
{
    for (var i = 0; i < cases.Count; i++)
    {
        Console.WriteLine("[" + (i + 1) + "/" + cases.Count + "] " + cases[i].CaseId);
        if (dryRun)
        {
            service.WriteOfflineArtifacts(runDir, i + 1, catalog, cases[i], settings, dryResponse, promptOverride);
            continue;
        }

        var artifact = await service.RunCaseAsync(runDir, i + 1, catalog, cases[i], settings, promptOverride);
        Console.WriteLine(artifact.Result.Success ? "  成功" : "  失败：" + artifact.Result.Error);
    }
}
else
{
    var gate = new SemaphoreSlim(concurrency);
    var consoleLock = new object();
    var completed = 0;
    var succeeded = 0;
    var failed = 0;

    var tasks = cases.Select((labCase, index) => RunOneCaseAsync(index, labCase)).ToArray();
    await Task.WhenAll(tasks);
    Console.WriteLine("批次结果：成功 " + succeeded + "，失败 " + failed + "。");

    async Task RunOneCaseAsync(int index, PromptLabCase labCase)
    {
        await gate.WaitAsync();
        try
        {
            lock (consoleLock)
            {
                Console.WriteLine("[" + (index + 1) + "/" + cases.Count + "] 启动 " + labCase.CaseId);
            }

            if (dryRun)
            {
                service.WriteOfflineArtifacts(runDir, index + 1, catalog, labCase, settings, dryResponse, promptOverride);
                Interlocked.Increment(ref succeeded);
                lock (consoleLock)
                {
                    Console.WriteLine("[" + (index + 1) + "/" + cases.Count + "] 完成 " + labCase.CaseId + "：成功");
                }

                return;
            }

            var artifact = await service.RunCaseAsync(runDir, index + 1, catalog, labCase, settings, promptOverride);
            if (artifact.Result.Success)
            {
                Interlocked.Increment(ref succeeded);
                lock (consoleLock)
                {
                    Console.WriteLine("[" + (index + 1) + "/" + cases.Count + "] 完成 " + labCase.CaseId + "：成功");
                }
            }
            else
            {
                Interlocked.Increment(ref failed);
                lock (consoleLock)
                {
                    Console.WriteLine("[" + (index + 1) + "/" + cases.Count + "] 完成 " + labCase.CaseId + "：失败：" + artifact.Result.Error);
                }
            }
        }
        catch (Exception ex)
        {
            Interlocked.Increment(ref failed);
            lock (consoleLock)
            {
                Console.WriteLine("[" + (index + 1) + "/" + cases.Count + "] 完成 " + labCase.CaseId + "：失败：" + ex.GetType().Name + ": " + ex.Message);
            }
        }
        finally
        {
            var done = Interlocked.Increment(ref completed);
            gate.Release();
            lock (consoleLock)
            {
                Console.WriteLine("进度：" + done + "/" + cases.Count);
            }
        }
    }
}

Console.WriteLine("完成：" + runDir);
return 0;

static Dictionary<string, string> ParseArgs(string[] args)
{
    var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    for (var i = 0; i < args.Length; i++)
    {
        var arg = args[i] ?? "";
        if (!arg.StartsWith("--", StringComparison.Ordinal))
        {
            continue;
        }

        var key = arg.Substring(2).Trim();
        if (string.IsNullOrWhiteSpace(key))
        {
            continue;
        }

        if (i + 1 < args.Length && !(args[i + 1] ?? "").StartsWith("--", StringComparison.Ordinal))
        {
            result[key] = args[++i] ?? "";
        }
        else
        {
            result[key] = "true";
        }
    }

    return result;
}

static string GetOption(Dictionary<string, string> options, string key)
{
    return options.TryGetValue(key, out var value) ? value : "";
}

static bool HasFlag(Dictionary<string, string> options, string key)
{
    return options.TryGetValue(key, out var value) && !string.Equals(value, "false", StringComparison.OrdinalIgnoreCase);
}

static void ApplyOption(Dictionary<string, string> options, string key, Action<string> apply)
{
    var value = GetOption(options, key);
    if (!string.IsNullOrWhiteSpace(value))
    {
        apply(value);
    }
}

static bool ParseBool(string value, bool fallback)
{
    var text = (value ?? "").Trim();
    if (bool.TryParse(text, out var parsed))
    {
        return parsed;
    }

    if (string.Equals(text, "1", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(text, "yes", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(text, "on", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(text, "开启", StringComparison.OrdinalIgnoreCase))
    {
        return true;
    }

    if (string.Equals(text, "0", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(text, "no", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(text, "off", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(text, "关闭", StringComparison.OrdinalIgnoreCase))
    {
        return false;
    }

    return fallback;
}
