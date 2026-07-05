# AnimusForge Preprocess Topic Route Tester

用于测试前处理话题筛选准确性。工具不进入游戏、不改 mod 运行链路、不执行覆盖。

工具固定复刻 mod 前处理辅助路由结构：`AIConfigHandler.BuildAuxiliaryGuardrailRoutingPrompt` + `BuildAuxiliaryRouterMessages` + `rule_codes` JSON 输出。这里不再支持自创前处理提示词模板；旧 `Prompt` 设置只保留文件兼容，运行时会被忽略。

为避免前处理批测烧掉大量输出 token，关闭思考时请求会显式发送 `thinking.type=disabled`，并把实验室输出上限封顶为 512；prompt 结构和 4 条话题返回规则仍按 mod 路由测试。

工具按游戏默认口径测试：前处理返回 4 个 `rule_codes`，再映射回话题 ID。评分只比较话题集合：缺失核心话题、混入非 allowed 话题、命中 forbidden 会扣分，输出顺序不作为准确性依据。

## 运行

```powershell
dotnet run --project tools/PreprocessTopicPromptLab/src/PreprocessTopicPromptLab.App/PreprocessTopicPromptLab.App.csproj
```

也可以双击：

```text
tools/PreprocessTopicPromptLab/RunPreprocessTopicPromptLab.bat
```

命令行批跑当前本地设置：

```powershell
dotnet run --project tools/PreprocessTopicPromptLab/src/PreprocessTopicPromptLab.Cli/PreprocessTopicPromptLab.Cli.csproj -- --parallel 16 --retries 2
```

## 测试

```powershell
dotnet build tools/PreprocessTopicPromptLab/PreprocessTopicPromptLab.slnx
dotnet run --project tools/PreprocessTopicPromptLab/tests/PreprocessTopicPromptLab.SmokeTests/PreprocessTopicPromptLab.SmokeTests.csproj
```

## 文件

- 案例：`tools/PreprocessTopicPromptLab/cases/*.jsonl`
- 20260705 训练集人工基准：`tools/PreprocessTopicPromptLab/cases/training_20260705_topics_200.manual.jsonl`
- 20260705 自动提取草稿：`tools/PreprocessTopicPromptLab/cases/training_20260705_topics_200.jsonl`
- 本地设置：`tools/PreprocessTopicPromptLab/local.settings.json`
- 运行结果：`tools/PreprocessTopicPromptLab/runs/<timestamp>/`

每轮运行会保存：

- `<index>_<caseId>.prompt.txt`
- `<index>_<caseId>.request.json`
- `<index>_<caseId>.response.txt`
- `<index>_<caseId>.injected_rules.txt`
- `<index>_<caseId>.meta.json`
