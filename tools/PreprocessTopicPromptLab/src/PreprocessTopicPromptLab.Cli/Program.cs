using System.Text.Json;
using PreprocessTopicPromptLab.Core;

var app = new BatchRunnerApp(args);
return await app.RunAsync();

internal sealed class BatchRunnerApp
{
    private readonly string[] _args;
    private readonly JsonFileStore _json = new();
    private readonly PreprocessTopicLabService _service = new();

    public BatchRunnerApp(string[] args)
    {
        _args = args ?? Array.Empty<string>();
    }

    public async Task<int> RunAsync()
    {
        var repoRoot = GetArg("--repo") ?? _service.FindDefaultRepoRoot(Directory.GetCurrentDirectory());
        var labRoot = _service.GetLabRoot(repoRoot);
        var settingsPath = GetArg("--settings") ?? Path.Combine(labRoot, "local.settings.json");
        var parallelism = Math.Clamp(GetIntArg("--parallel", 16), 1, 256);
        var retryCount = Math.Clamp(GetIntArg("--retries", 2), 0, 5);

        if (!File.Exists(settingsPath))
        {
            Console.Error.WriteLine("local.settings.json not found: " + settingsPath);
            return 2;
        }

        var local = _json.Deserialize<LocalSettings>(_json.ReadUtf8(settingsPath)) ?? new LocalSettings();
        if (!string.IsNullOrWhiteSpace(local.RepoRoot))
        {
            repoRoot = local.RepoRoot;
            labRoot = _service.GetLabRoot(repoRoot);
        }

        var caseFile = GetArg("--cases") ?? local.CaseFilePath;
        if (string.IsNullOrWhiteSpace(caseFile))
        {
            var manualCases = Path.Combine(labRoot, "cases", "training_20260705_topics_200.manual.jsonl");
            caseFile = File.Exists(manualCases) ? manualCases : Path.Combine(labRoot, "cases", "sample_cases.jsonl");
        }

        var settings = local.Api ?? new PreprocessLabSettings();
        var promptPath = GetArg("--prompt") ?? local.PromptVersionPath;
        var prompt = !string.IsNullOrWhiteSpace(promptPath) && File.Exists(promptPath)
            ? _service.LoadPromptVersion(promptPath)
            : _service.GetDefaultPromptConfig();
        if (!string.IsNullOrWhiteSpace(promptPath))
        {
            settings.PromptVersionPath = promptPath;
        }

        if (string.IsNullOrWhiteSpace(settings.ApiUrl) ||
            string.IsNullOrWhiteSpace(settings.ApiKey) ||
            string.IsNullOrWhiteSpace(settings.Model))
        {
            Console.Error.WriteLine("API settings are incomplete. Check URL, key, and model in local.settings.json.");
            return 2;
        }

        var catalog = _service.LoadCatalog(repoRoot);
        var cases = _service.LoadCases(caseFile);
        if (cases.Count == 0)
        {
            Console.Error.WriteLine("No cases loaded: " + caseFile);
            return 2;
        }

        var runDir = _service.CreateRunDirectory(labRoot);
        Console.WriteLine("repo=" + repoRoot);
        Console.WriteLine("cases=" + cases.Count);
        Console.WriteLine("parallel=" + parallelism);
        Console.WriteLine("prompt=" + (string.IsNullOrWhiteSpace(promptPath) ? "(mod-default)" : promptPath));
        Console.WriteLine("runDir=" + runDir);

        var completed = 0;
        var artifacts = new RunArtifact[cases.Count];
        using var gate = new SemaphoreSlim(parallelism);
        var tasks = cases.Select(async (labCase, i) =>
        {
            await gate.WaitAsync();
            try
            {
                var artifact = await RunWithRetriesAsync(runDir, i + 1, catalog, labCase, settings, prompt, retryCount);
                artifacts[i] = artifact;
                var done = Interlocked.Increment(ref completed);
                Console.WriteLine(
                    done.ToString("000") + "/" + cases.Count.ToString("000") +
                    " " + labCase.CaseId +
                    " success=" + artifact.Result.Success +
                    " exact=" + artifact.Score.ExactMatch +
                    " recall=" + artifact.Score.Recall.ToString("0.####") +
                    " precision=" + artifact.Score.Precision.ToString("0.####") +
                    (string.IsNullOrWhiteSpace(artifact.Result.Error) ? "" : " error=" + artifact.Result.Error));
            }
            finally
            {
                gate.Release();
            }
        }).ToList();

        await Task.WhenAll(tasks);
        var finalArtifacts = artifacts.Where(x => x != null).ToList();
        var summary = BuildSummary(runDir, caseFile, finalArtifacts);
        var summaryPath = Path.Combine(runDir, "_summary.json");
        _json.WriteUtf8(summaryPath, JsonSerializer.Serialize(summary, JsonFileStore.JsonOptions));

        Console.WriteLine("summary=" + summaryPath);
        Console.WriteLine(
            "success=" + summary.Success +
            " failed=" + summary.Failed +
            " exact=" + summary.Exact +
            " exactRate=" + summary.ExactRate.ToString("P2") +
            " avgRecall=" + summary.AverageRecall.ToString("P2") +
            " avgPrecision=" + summary.AveragePrecision.ToString("P2"));
        Console.WriteLine(
            "tokens input=" + summary.InputTokens +
            " cacheRead=" + summary.CacheReadInputTokens +
            " output=" + summary.OutputTokens +
            " total=" + summary.TotalTokens);
        return summary.Failed == 0 ? 0 : 1;
    }

    private async Task<RunArtifact> RunWithRetriesAsync(
        string runDir,
        int index,
        PromptCatalog catalog,
        PreprocessLabCase labCase,
        PreprocessLabSettings settings,
        PreprocessPromptConfig prompt,
        int retryCount)
    {
        RunArtifact? artifact = null;
        for (var attempt = 0; attempt <= retryCount; attempt++)
        {
            artifact = await _service.RunCaseAsync(runDir, index, catalog, labCase, settings, prompt);
            if (artifact.Result.Success)
            {
                return artifact;
            }

            if (attempt < retryCount)
            {
                await Task.Delay(TimeSpan.FromSeconds(2 + attempt * 3));
            }
        }

        return artifact!;
    }

    private static RunSummary BuildSummary(string runDir, string caseFile, List<RunArtifact> artifacts)
    {
        var success = artifacts.Count(x => x.Result.Success);
        var failed = artifacts.Count - success;
        var exact = artifacts.Count(x => x.Result.Success && x.Score.ExactMatch);
        var missing = artifacts.Sum(x => x.Score.MissingTopics.Count);
        var unexpected = artifacts.Sum(x => x.Score.UnexpectedTopics.Count);
        var forbidden = artifacts.Sum(x => x.Score.ForbiddenHits.Count);
        var successfulArtifacts = artifacts.Where(x => x.Result.Success).ToList();
        return new RunSummary
        {
            RunDir = runDir,
            CaseFile = caseFile,
            Total = artifacts.Count,
            Success = success,
            Failed = failed,
            Exact = exact,
            ExactRate = success == 0 ? 0 : Math.Round(exact * 1.0 / success, 4),
            AverageRecall = success == 0 ? 0 : Math.Round(artifacts.Where(x => x.Result.Success).Average(x => x.Score.Recall), 4),
            AveragePrecision = success == 0 ? 0 : Math.Round(artifacts.Where(x => x.Result.Success).Average(x => x.Score.Precision), 4),
            MissingTopicCount = missing,
            UnexpectedTopicCount = unexpected,
            ForbiddenHitCount = forbidden,
            InputTokens = successfulArtifacts.Sum(x => x.Result.InputTokens),
            CacheCreationInputTokens = successfulArtifacts.Sum(x => x.Result.CacheCreationInputTokens),
            CacheReadInputTokens = successfulArtifacts.Sum(x => x.Result.CacheReadInputTokens),
            OutputTokens = successfulArtifacts.Sum(x => x.Result.OutputTokens),
            PromptTokens = successfulArtifacts.Sum(x => x.Result.PromptTokens),
            CompletionTokens = successfulArtifacts.Sum(x => x.Result.CompletionTokens),
            TotalTokens = successfulArtifacts.Sum(x => x.Result.TotalTokens),
            FailedCases = artifacts
                .Where(x => !x.Result.Success || !x.Score.ExactMatch)
                .Select(x => new RunSummaryCase
                {
                    CaseId = x.CaseId,
                    Success = x.Result.Success,
                    Error = x.Result.Error,
                    ActualTopics = x.Score.ActualTopics,
                    MissingTopics = x.Score.MissingTopics,
                    UnexpectedTopics = x.Score.UnexpectedTopics,
                    ForbiddenHits = x.Score.ForbiddenHits
                })
                .ToList()
        };
    }

    private string? GetArg(string name)
    {
        for (var i = 0; i < _args.Length; i++)
        {
            if (string.Equals(_args[i], name, StringComparison.OrdinalIgnoreCase) && i + 1 < _args.Length)
            {
                return _args[i + 1];
            }
        }

        return null;
    }

    private int GetIntArg(string name, int fallback)
    {
        var value = GetArg(name);
        return int.TryParse(value, out var result) ? result : fallback;
    }

    private sealed class LocalSettings
    {
        public string RepoRoot { get; set; } = "";

        public string CaseFilePath { get; set; } = "";

        public string PromptVersionPath { get; set; } = "";

        public PreprocessLabSettings Api { get; set; } = new();

        public PreprocessPromptConfig Prompt { get; set; } = new();
    }

    private sealed class RunSummary
    {
        public string RunDir { get; set; } = "";

        public string CaseFile { get; set; } = "";

        public int Total { get; set; }

        public int Success { get; set; }

        public int Failed { get; set; }

        public int Exact { get; set; }

        public double ExactRate { get; set; }

        public double AverageRecall { get; set; }

        public double AveragePrecision { get; set; }

        public int MissingTopicCount { get; set; }

        public int UnexpectedTopicCount { get; set; }

        public int ForbiddenHitCount { get; set; }

        public int InputTokens { get; set; }

        public int CacheCreationInputTokens { get; set; }

        public int CacheReadInputTokens { get; set; }

        public int OutputTokens { get; set; }

        public int PromptTokens { get; set; }

        public int CompletionTokens { get; set; }

        public int TotalTokens { get; set; }

        public List<RunSummaryCase> FailedCases { get; set; } = new();
    }

    private sealed class RunSummaryCase
    {
        public string CaseId { get; set; } = "";

        public bool Success { get; set; }

        public string Error { get; set; } = "";

        public List<string> ActualTopics { get; set; } = new();

        public List<string> MissingTopics { get; set; } = new();

        public List<string> UnexpectedTopics { get; set; } = new();

        public List<string> ForbiddenHits { get; set; } = new();
    }
}
