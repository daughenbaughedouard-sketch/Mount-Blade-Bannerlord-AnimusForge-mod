# AnimusForge 后处理提示词实验室

这是一个独立 Windows 工具，用来在不启动 Bannerlord 的情况下测试 AnimusForge 后处理提示词。

## 编译

```powershell
dotnet build tools\ActionPostprocessPromptLab\ActionPostprocessPromptLab.slnx
```

## 运行开发版

```powershell
dotnet run --project tools\ActionPostprocessPromptLab\src\ActionPostprocessPromptLab.App\ActionPostprocessPromptLab.App.csproj
```

## 发布可双击 EXE

生成自包含的一键打开版：

```powershell
powershell -ExecutionPolicy Bypass -File tools\ActionPostprocessPromptLab\publish-win-x64.ps1
```

输出：

```text
tools/ActionPostprocessPromptLab/dist/win-x64-self-contained/AnimusForgeActionPostprocessPromptLab.exe
tools/ActionPostprocessPromptLab/dist/packages/AnimusForgeActionPostprocessPromptLab-win-x64-self-contained-<timestamp>.zip
```

## 冒烟测试

```powershell
dotnet run --project tools\ActionPostprocessPromptLab\tests\ActionPostprocessPromptLab.SmokeTests\ActionPostprocessPromptLab.SmokeTests.csproj
```

## 批量运行器

如果需要让 Codex 或其他操作者不打开窗口也能批量运行案例，可以用 runner：

```powershell
dotnet run --project tools\ActionPostprocessPromptLab\tools\ActionPostprocessPromptLab.Runner\ActionPostprocessPromptLab.Runner.csproj -- --case-file tools\ActionPostprocessPromptLab\cases\sample_cases.jsonl
```

只生成请求文件、不真正调用 API：

```powershell
dotnet run --project tools\ActionPostprocessPromptLab\tools\ActionPostprocessPromptLab.Runner\ActionPostprocessPromptLab.Runner.csproj -- --dry-run
```

只跑一部分案例，适合先验证 API、模型和提示词版本：

```powershell
dotnet run --project tools\ActionPostprocessPromptLab\tools\ActionPostprocessPromptLab.Runner\ActionPostprocessPromptLab.Runner.csproj -- --case-file tools\ActionPostprocessPromptLab\cases\material_high_value_allow_cases.jsonl --take 5 --reasoning-effort low
```

跳过前 20 个案例后再跑 10 个：

```powershell
dotnet run --project tools\ActionPostprocessPromptLab\tools\ActionPostprocessPromptLab.Runner\ActionPostprocessPromptLab.Runner.csproj -- --case-file tools\ActionPostprocessPromptLab\cases\material_high_value_allow_cases.jsonl --skip 20 --take 10 --reasoning-effort low
```

并发运行案例：

```powershell
dotnet run --project tools\ActionPostprocessPromptLab\tools\ActionPostprocessPromptLab.Runner\ActionPostprocessPromptLab.Runner.csproj -- --case-file tools\ActionPostprocessPromptLab\cases\material_high_value_allow_cases.jsonl --reasoning-effort low --concurrency 20
```

`--concurrency` 表示同时最多发出多少个 API 请求。可以设为案例数一次打满，例如 `--concurrency 140`；如果接口返回 429、超时或空回复，运行目录里的 meta 会记录失败，之后可以只重跑失败或降低并发。

## API 协议

接口地址会自动识别协议：

- OpenAI-compatible：填 `https://.../v1/chat/completions` 或服务商 base URL。
- DeepSeek Anthropic-compatible：填 `https://api.deepseek.com/anthropic`，工具会自动请求 `/v1/messages`。

Runner 也可以显式指定协议：

```powershell
dotnet run --project tools\ActionPostprocessPromptLab\tools\ActionPostprocessPromptLab.Runner\ActionPostprocessPromptLab.Runner.csproj -- --api-url https://api.deepseek.com/anthropic --api-protocol anthropic
```

请求文件仍然不会写入 API Key。运行 meta 会记录 `apiProtocol`、`resolvedApiProtocol` 和最终请求 URL。

指定后处理思考模式：

```powershell
dotnet run --project tools\ActionPostprocessPromptLab\tools\ActionPostprocessPromptLab.Runner\ActionPostprocessPromptLab.Runner.csproj -- --dry-run --thinking-enabled true --reasoning-effort max
```

WinForms 界面顶部也有“思考”和“强度”设置。默认开启思考，强度为 `max`。OpenAI-compatible 请求会写入 `thinking.type=enabled` 和 `reasoning_effort=max`；Anthropic-compatible 请求会写入 `thinking.type=enabled` 和 `budget_tokens`。

批量跑标签案例时建议先用 `--reasoning-effort low`。部分模型在 `max` 下会把全部输出 token 用在 `reasoning_content`，导致最终 `content` 为空；low 仍然开启思考，但更适合短标签输出。

## 素材案例提取

如果 `tools\ActionPostprocessPromptLab\dist\素材` 中放入了 `Token_Stats*.txt` 日志，可以从其中的 `mode=action_postprocess_http` 轮次自动提取高价值案例：

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File tools\ActionPostprocessPromptLab\extract-material-cases.ps1
```

默认输出：

```text
tools/ActionPostprocessPromptLab/cases/material_high_value_allow_cases.jsonl
tools/ActionPostprocessPromptLab/cases/material_high_value_allow_cases.summary.json
```

提取脚本只做离线解析，不调用 API，不修改游戏链路。它会保留能映射到当前后处理规则的非情绪动作标签，跳过只有情绪标签、重复样本、旧标签/不支持标签，以及明显不适合作为基准的文本。`expectedTags` 来自原日志里的后处理输出，不代表一定正确，后续仍需要人工评分。

可以调整总数和单类上限：

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File tools\ActionPostprocessPromptLab\extract-material-cases.ps1 -MaxCases 160 -MaxPerRule 24
```

生成后可用 runner 离线验证案例能否渲染请求体：

```powershell
dotnet run --project tools\ActionPostprocessPromptLab\tools\ActionPostprocessPromptLab.Runner\ActionPostprocessPromptLab.Runner.csproj -- --dry-run --case-file tools\ActionPostprocessPromptLab\cases\material_high_value_allow_cases.jsonl
```

批量运行后可以生成结果索引，方便后续人工评分时快速定位每个案例的请求、回复和 meta：

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File tools\ActionPostprocessPromptLab\summarize-run-results.ps1 -RunDirs "20260705_130916,20260705_131022" -OutputPath tools\ActionPostprocessPromptLab\runs\material_low.index.jsonl
```

索引里的 `exactTagMatch` 只是 expectedTags 和 actualTags 的机械对比，不代表最终质量评分。评分会忽略 `[ACTION:DUEL_LINE_WIN:...]`、`[ACTION:DUEL_LINE_LOSE:...]` 的台词正文，以及 `[AD;金额;天数;备注]`、`[AD;金额;天数;N/P;备注]` 的备注正文；金额、天数、N/P 等结构字段仍然参与比对。动态金额、标签顺序或可接受的等价标签仍需要人工判断。

## 修改标签提示词

在“标签提示词”页可以修改每个后处理标签的说明、触发条件和禁止条件。标签本身是只读的，例如 `[ACTION:DUEL]`、`[AD;金额;天数;备注内容]` 不允许在工具里修改。

标签提示词是全局提示词版本的一部分，不属于某个案例。左侧“本案例命中规则”只决定当前案例注入哪些规则；“标签提示词”页里的“全局标签规则”才是编辑同一套全局标签说明。保存“全局提示词版本”时，工具只保存标签说明覆盖，不会把标签格式替换掉。加载该版本后，所有案例和批量运行都会用同一组编辑后的 `{tag_rules}`。

## 查看完整上下文

- “完整提示词”页：查看最终发送给模型的 system prompt、user prompt、标签表、情绪表、history 和 latest_reply。
- “请求体 JSON”页：查看最终 OpenAI-compatible 请求体。
- “模型回复”页：查看从接口响应里提取出的正文。
- “原始回复”页：查看完整 HTTP 返回内容或错误。

每轮运行还会额外保存 `<index>_<caseId>.prompt.txt`，方便从运行目录直接检查完整提示词。

## 数据位置

- 共享案例：`tools/ActionPostprocessPromptLab/cases/*.jsonl`
- 提示词版本：`tools/ActionPostprocessPromptLab/prompt_versions/*.json`
- 本地设置：`tools/ActionPostprocessPromptLab/local.settings.json`
- 运行产物：`tools/ActionPostprocessPromptLab/runs/<timestamp>/`
- 素材日志：`tools/ActionPostprocessPromptLab/dist/素材/Token_Stats*.txt`

每轮运行都会分别写出请求体、模型回复和元数据文件。请求体文件不会包含 API Key。
