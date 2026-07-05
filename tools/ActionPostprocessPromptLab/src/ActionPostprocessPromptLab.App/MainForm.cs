using ActionPostprocessPromptLab.Core;

namespace ActionPostprocessPromptLab.App;

public sealed class MainForm : Form
{
    private readonly PromptLabService _service = new();

    private readonly TextBox _repoRootBox = new();
    private readonly TextBox _caseFileBox = new();
    private readonly TextBox _apiUrlBox = new();
    private readonly TextBox _apiKeyBox = new();
    private readonly TextBox _modelBox = new();
    private readonly NumericUpDown _temperatureBox = new();
    private readonly NumericUpDown _maxTokensBox = new();
    private readonly CheckBox _thinkingEnabledBox = new();
    private readonly ComboBox _reasoningEffortBox = new();
    private readonly TextBox _promptVersionBox = new();
    private readonly ListBox _caseList = new();
    private readonly CheckedListBox _rulesList = new();
    private readonly ListBox _tagRuleList = new();
    private readonly Label _tagRuleHeaderLabel = new();
    private readonly ListBox _tagEntryList = new();
    private readonly TextBox _tagTextBox = new();
    private readonly TextBox _tagDescriptionBox = new();
    private readonly TextBox _caseIdBox = new();
    private readonly TextBox _titleBox = new();
    private readonly TextBox _playerTextBox = new();
    private readonly TextBox _npcReplyBox = new();
    private readonly TextBox _historyBox = new();
    private readonly TextBox _afefPlayerBox = new();
    private readonly TextBox _afefNpcBox = new();
    private readonly TextBox _runtimeContextBox = new();
    private readonly TextBox _expectedTagsBox = new();
    private readonly TextBox _notesBox = new();
    private readonly TextBox _systemPromptBox = new();
    private readonly TextBox _userTemplateBox = new();
    private readonly TextBox _promptPreviewBox = new();
    private readonly TextBox _requestBox = new();
    private readonly TextBox _responseBox = new();
    private readonly TextBox _rawResponseBox = new();
    private readonly TextBox _filesBox = new();
    private readonly Label _statusLabel = new();

    private PromptCatalog _catalog = new();
    private PromptLabSettings _settings = new();
    private readonly List<PromptLabCase> _cases = new();
    private string _labRoot = "";
    private string _lastRunDir = "";
    private bool _loadingCase;
    private bool _loadingTagPrompt;

    private sealed class TagRuleListItem
    {
        public TagRuleListItem(PostprocessRuleEntry entry)
        {
            Entry = entry;
        }

        public PostprocessRuleEntry Entry { get; }

        public override string ToString()
        {
            return Entry.Tag ?? "";
        }
    }

    public MainForm()
    {
        Text = "AnimusForge 后处理提示词实验室";
        Width = 1420;
        Height = 920;
        MinimumSize = new Size(1120, 760);
        StartPosition = FormStartPosition.CenterScreen;

        BuildLayout();
        LoadDefaults();
    }

    private void BuildLayout()
    {
        var root = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 4, ColumnCount = 1 };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 78));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 26));
        Controls.Add(root);

        var repoBar = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 7, Padding = new Padding(8, 6, 8, 4) };
        repoBar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 76));
        repoBar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55));
        repoBar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 84));
        repoBar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 76));
        repoBar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 76));
        repoBar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 108));
        repoBar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45));
        root.Controls.Add(repoBar, 0, 0);

        repoBar.Controls.Add(Label("仓库"), 0, 0);
        _repoRootBox.Dock = DockStyle.Fill;
        repoBar.Controls.Add(_repoRootBox, 1, 0);
        var browseRepoButton = Button("浏览");
        var reloadButton = Button("刷新");
        var saveSettingsButton = Button("保存");
        var openRunsButton = Button("运行目录", 100);
        repoBar.Controls.Add(browseRepoButton, 2, 0);
        repoBar.Controls.Add(reloadButton, 3, 0);
        repoBar.Controls.Add(saveSettingsButton, 4, 0);
        repoBar.Controls.Add(openRunsButton, 5, 0);
        _promptVersionBox.Dock = DockStyle.Fill;
        _promptVersionBox.PlaceholderText = "全局提示词版本 JSON（可选）";
        repoBar.Controls.Add(_promptVersionBox, 6, 0);

        var apiBar = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 16, Padding = new Padding(8, 4, 8, 4) };
        apiBar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 60));
        apiBar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35));
        apiBar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 46));
        apiBar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
        apiBar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 54));
        apiBar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20));
        apiBar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 52));
        apiBar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 78));
        apiBar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 72));
        apiBar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 84));
        apiBar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 52));
        apiBar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 72));
        apiBar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 52));
        apiBar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 86));
        apiBar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 112));
        apiBar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 112));
        root.Controls.Add(apiBar, 0, 1);

        apiBar.Controls.Add(Label("接口"), 0, 0);
        _apiUrlBox.Dock = DockStyle.Fill;
        _apiUrlBox.PlaceholderText = "OpenAI: https://.../v1/chat/completions；DeepSeek Anthropic: https://api.deepseek.com/anthropic";
        apiBar.Controls.Add(_apiUrlBox, 1, 0);
        apiBar.Controls.Add(Label("密钥"), 2, 0);
        _apiKeyBox.Dock = DockStyle.Fill;
        _apiKeyBox.UseSystemPasswordChar = true;
        apiBar.Controls.Add(_apiKeyBox, 3, 0);
        apiBar.Controls.Add(Label("模型"), 4, 0);
        _modelBox.Dock = DockStyle.Fill;
        apiBar.Controls.Add(_modelBox, 5, 0);
        apiBar.Controls.Add(Label("温度"), 6, 0);
        _temperatureBox.DecimalPlaces = 2;
        _temperatureBox.Increment = 0.05m;
        _temperatureBox.Maximum = 2;
        _temperatureBox.Dock = DockStyle.Fill;
        apiBar.Controls.Add(_temperatureBox, 7, 0);
        apiBar.Controls.Add(Label("输出"), 8, 0);
        _maxTokensBox.Minimum = 16;
        _maxTokensBox.Maximum = 200000;
        _maxTokensBox.Increment = 128;
        _maxTokensBox.Dock = DockStyle.Fill;
        apiBar.Controls.Add(_maxTokensBox, 9, 0);
        apiBar.Controls.Add(Label("思考"), 10, 0);
        _thinkingEnabledBox.Text = "开启";
        _thinkingEnabledBox.Dock = DockStyle.Fill;
        _thinkingEnabledBox.TextAlign = ContentAlignment.MiddleLeft;
        apiBar.Controls.Add(_thinkingEnabledBox, 11, 0);
        apiBar.Controls.Add(Label("强度"), 12, 0);
        _reasoningEffortBox.DropDownStyle = ComboBoxStyle.DropDownList;
        _reasoningEffortBox.Dock = DockStyle.Fill;
        foreach (var effort in PromptLabService.GetReasoningEffortOptions())
        {
            _reasoningEffortBox.Items.Add(effort);
        }

        apiBar.Controls.Add(_reasoningEffortBox, 13, 0);
        var loadPromptButton = Button("加载全局提示词", 114);
        var savePromptButton = Button("保存全局提示词", 114);
        apiBar.Controls.Add(loadPromptButton, 14, 0);
        apiBar.Controls.Add(savePromptButton, 15, 0);

        var mainSplit = new SplitContainer { Dock = DockStyle.Fill, FixedPanel = FixedPanel.Panel1, SplitterDistance = 350 };
        root.Controls.Add(mainSplit, 0, 2);

        var left = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 5, Padding = new Padding(8, 4, 4, 4) };
        left.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
        left.RowStyles.Add(new RowStyle(SizeType.Percent, 42));
        left.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
        left.RowStyles.Add(new RowStyle(SizeType.Percent, 58));
        left.RowStyles.Add(new RowStyle(SizeType.Absolute, 108));
        mainSplit.Panel1.Controls.Add(left);

        var caseFilePanel = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2 };
        caseFilePanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        caseFilePanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 72));
        _caseFileBox.Dock = DockStyle.Fill;
        caseFilePanel.Controls.Add(_caseFileBox, 0, 0);
        var browseCaseButton = Button("案例");
        caseFilePanel.Controls.Add(browseCaseButton, 1, 0);
        left.Controls.Add(caseFilePanel, 0, 0);

        _caseList.Dock = DockStyle.Fill;
        _caseList.DisplayMember = nameof(PromptLabCase.DisplayName);
        left.Controls.Add(_caseList, 0, 1);

        left.Controls.Add(Label("本案例命中规则"), 0, 2);
        _rulesList.Dock = DockStyle.Fill;
        _rulesList.CheckOnClick = true;
        left.Controls.Add(_rulesList, 0, 3);

        var leftButtons = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 3, Padding = new Padding(0, 4, 0, 0) };
        leftButtons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        leftButtons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        leftButtons.RowStyles.Add(new RowStyle(SizeType.Percent, 33));
        leftButtons.RowStyles.Add(new RowStyle(SizeType.Percent, 33));
        leftButtons.RowStyles.Add(new RowStyle(SizeType.Percent, 34));
        var newCaseButton = Button("新建");
        var saveCaseButton = Button("保存案例");
        var renderButton = Button("生成请求");
        var runSelectedButton = Button("运行当前");
        var runAllButton = Button("批量运行全部");
        leftButtons.Controls.Add(newCaseButton, 0, 0);
        leftButtons.Controls.Add(saveCaseButton, 1, 0);
        leftButtons.Controls.Add(renderButton, 0, 1);
        leftButtons.Controls.Add(runSelectedButton, 1, 1);
        leftButtons.Controls.Add(runAllButton, 0, 2);
        leftButtons.SetColumnSpan(runAllButton, 2);
        left.Controls.Add(leftButtons, 0, 4);

        var tabs = new TabControl { Dock = DockStyle.Fill };
        mainSplit.Panel2.Controls.Add(tabs);
        tabs.TabPages.Add(BuildCaseTab());
        tabs.TabPages.Add(BuildPromptTab());
        tabs.TabPages.Add(BuildTagPromptTab());
        tabs.TabPages.Add(BuildTextTab("完整提示词", _promptPreviewBox));
        tabs.TabPages.Add(BuildTextTab("请求体 JSON", _requestBox));
        tabs.TabPages.Add(BuildTextTab("模型回复", _responseBox));
        tabs.TabPages.Add(BuildTextTab("原始回复", _rawResponseBox));
        tabs.TabPages.Add(BuildTextTab("运行文件", _filesBox));

        _statusLabel.Dock = DockStyle.Fill;
        _statusLabel.Padding = new Padding(8, 0, 8, 0);
        _statusLabel.TextAlign = ContentAlignment.MiddleLeft;
        root.Controls.Add(_statusLabel, 0, 3);

        browseRepoButton.Click += (_, _) => BrowseRepoRoot();
        reloadButton.Click += (_, _) => ReloadCatalogAndCases();
        saveSettingsButton.Click += (_, _) => SaveSettingsFromUi();
        openRunsButton.Click += (_, _) => OpenRunsFolder();
        loadPromptButton.Click += (_, _) => LoadPromptVersion();
        savePromptButton.Click += (_, _) => SavePromptVersion();
        browseCaseButton.Click += (_, _) => BrowseCaseFile();
        newCaseButton.Click += (_, _) => NewCase();
        saveCaseButton.Click += (_, _) => SaveCurrentCase();
        renderButton.Click += (_, _) => RenderCurrentCase();
        runSelectedButton.Click += async (_, _) => await RunSelectedCaseAsync();
        runAllButton.Click += async (_, _) => await RunAllCasesAsync();
        _caseList.SelectedIndexChanged += (_, _) => LoadSelectedCaseToUi();
    }

    private TabPage BuildCaseTab()
    {
        var tab = new TabPage("案例");
        var grid = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 8, Padding = new Padding(8) };
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130));
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
        grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
        grid.RowStyles.Add(new RowStyle(SizeType.Percent, 14));
        grid.RowStyles.Add(new RowStyle(SizeType.Percent, 18));
        grid.RowStyles.Add(new RowStyle(SizeType.Percent, 18));
        grid.RowStyles.Add(new RowStyle(SizeType.Percent, 18));
        grid.RowStyles.Add(new RowStyle(SizeType.Percent, 18));
        grid.RowStyles.Add(new RowStyle(SizeType.Percent, 14));
        tab.Controls.Add(grid);

        AddLabeled(grid, 0, "案例 ID", _caseIdBox, singleLine: true);
        AddLabeled(grid, 1, "标题", _titleBox, singleLine: true);
        AddLabeled(grid, 2, "玩家本轮发言", _playerTextBox);
        AddLabeled(grid, 3, "NPC 本轮回复", _npcReplyBox);

        var afefSplit = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Vertical, SplitterDistance = 430 };
        ConfigureMultiline(_afefPlayerBox);
        ConfigureMultiline(_afefNpcBox);
        afefSplit.Panel1.Controls.Add(BuildTitledPanel("AFEF 玩家事实", _afefPlayerBox));
        afefSplit.Panel2.Controls.Add(BuildTitledPanel("AFEF NPC 事实", _afefNpcBox));
        grid.Controls.Add(Label("AFEF 事实"), 0, 4);
        grid.Controls.Add(afefSplit, 1, 4);

        AddLabeled(grid, 5, "普通历史", _historyBox);
        AddLabeled(grid, 6, "运行时事实", _runtimeContextBox);

        var bottomSplit = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Vertical, SplitterDistance = 430 };
        ConfigureMultiline(_expectedTagsBox);
        ConfigureMultiline(_notesBox);
        bottomSplit.Panel1.Controls.Add(BuildTitledPanel("期望标签", _expectedTagsBox));
        bottomSplit.Panel2.Controls.Add(BuildTitledPanel("备注", _notesBox));
        grid.Controls.Add(Label("期望/备注"), 0, 7);
        grid.Controls.Add(bottomSplit, 1, 7);
        return tab;
    }

    private TabPage BuildPromptTab()
    {
        var tab = new TabPage("提示词");
        var split = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Horizontal, SplitterDistance = 270 };
        ConfigureMultiline(_systemPromptBox);
        ConfigureMultiline(_userTemplateBox);
        split.Panel1.Controls.Add(BuildTitledPanel("系统提示词 SystemPrompt", _systemPromptBox));
        split.Panel2.Controls.Add(BuildTitledPanel("用户提示词模板 UserPromptTemplate", _userTemplateBox));
        tab.Controls.Add(split);
        return tab;
    }

    private TabPage BuildTagPromptTab()
    {
        var tab = new TabPage("标签提示词");
        var mainSplit = new SplitContainer { Dock = DockStyle.Fill, FixedPanel = FixedPanel.Panel1, SplitterDistance = 330, Padding = new Padding(8) };
        tab.Controls.Add(mainSplit);

        _tagRuleList.Dock = DockStyle.Fill;
        _tagRuleList.DisplayMember = nameof(PromptRuleInfo.DisplayName);
        mainSplit.Panel1.Controls.Add(BuildTitledPanel("全局标签规则", _tagRuleList));

        var root = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2, ColumnCount = 1, Padding = new Padding(8, 0, 0, 0) };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        mainSplit.Panel2.Controls.Add(root);

        _tagRuleHeaderLabel.Dock = DockStyle.Fill;
        _tagRuleHeaderLabel.TextAlign = ContentAlignment.MiddleLeft;
        root.Controls.Add(_tagRuleHeaderLabel, 0, 0);

        var split = new SplitContainer { Dock = DockStyle.Fill, FixedPanel = FixedPanel.Panel1, SplitterDistance = 360 };
        root.Controls.Add(split, 0, 1);

        _tagEntryList.Dock = DockStyle.Fill;
        split.Panel1.Controls.Add(_tagEntryList);

        var right = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 4, ColumnCount = 1, Padding = new Padding(8, 0, 0, 0) };
        right.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));
        right.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
        right.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));
        right.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        split.Panel2.Controls.Add(right);

        right.Controls.Add(Label("标签（只读）"), 0, 0);
        _tagTextBox.Dock = DockStyle.Fill;
        _tagTextBox.ReadOnly = true;
        _tagTextBox.Font = new Font(FontFamily.GenericMonospace, 10f);
        right.Controls.Add(_tagTextBox, 0, 1);

        right.Controls.Add(Label("标签提示词 / 输出条件"), 0, 2);
        ConfigureMultiline(_tagDescriptionBox);
        right.Controls.Add(_tagDescriptionBox, 0, 3);

        _tagEntryList.SelectedIndexChanged += (_, _) => LoadSelectedTagEntryToUi();
        _tagDescriptionBox.TextChanged += (_, _) => UpdateSelectedTagDescription();
        _tagRuleList.SelectedIndexChanged += (_, _) => LoadSelectedRuleToTagPromptUi();
        return tab;
    }

    private static TabPage BuildTextTab(string title, TextBox box)
    {
        var tab = new TabPage(title);
        ConfigureMultiline(box);
        tab.Controls.Add(box);
        return tab;
    }

    private void LoadDefaults()
    {
        var repo = _service.FindDefaultRepoRoot(AppContext.BaseDirectory);
        if (string.IsNullOrWhiteSpace(repo))
        {
            repo = _service.FindDefaultRepoRoot(Directory.GetCurrentDirectory());
        }

        _repoRootBox.Text = repo;
        if (!string.IsNullOrWhiteSpace(repo))
        {
            _labRoot = _service.ResolveLabRoot(repo);
            _settings = _service.LoadSettings(_labRoot);
            ApplySettingsToUi();
            _caseFileBox.Text = Path.Combine(_labRoot, "cases", "sample_cases.jsonl");
            ReloadCatalogAndCases();
        }
    }

    private void ReloadCatalogAndCases()
    {
        try
        {
            var repo = _repoRootBox.Text.Trim();
            _labRoot = _service.ResolveLabRoot(repo);
            Directory.CreateDirectory(_labRoot);
            _catalog = _service.LoadCatalog(repo, ResolvePromptVersionPath());
            LoadPromptIntoUi(_catalog.ActionConfig);
            LoadRulesToUi();
            LoadCasesFromFile();
            SetStatus("已加载规则=" + _catalog.Rules.Count + "，案例=" + _cases.Count);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "加载失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void ApplySettingsToUi()
    {
        _apiUrlBox.Text = _settings.ApiUrl;
        _apiKeyBox.Text = _settings.ApiKey;
        _modelBox.Text = _settings.Model;
        _temperatureBox.Value = ClampDecimal((decimal)_settings.Temperature, _temperatureBox.Minimum, _temperatureBox.Maximum);
        _maxTokensBox.Value = ClampDecimal(_settings.MaxTokens, _maxTokensBox.Minimum, _maxTokensBox.Maximum);
        _thinkingEnabledBox.Checked = _settings.ThinkingEnabled;
        SelectReasoningEffort(_settings.ReasoningEffort);
        _promptVersionBox.Text = _settings.PromptVersionPath;
    }

    private void SaveSettingsFromUi()
    {
        try
        {
            _settings = ReadSettingsFromUi();
            _service.SaveSettings(_labRoot, _settings);
            SetStatus("已保存本地设置。");
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "保存设置失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private PromptLabSettings ReadSettingsFromUi()
    {
        return new PromptLabSettings
        {
            ApiUrl = _apiUrlBox.Text.Trim(),
            ApiKey = _apiKeyBox.Text.Trim(),
            Model = _modelBox.Text.Trim(),
            Temperature = (float)_temperatureBox.Value,
            MaxTokens = (int)_maxTokensBox.Value,
            ThinkingEnabled = _thinkingEnabledBox.Checked,
            ReasoningEffort = PromptLabService.NormalizeReasoningEffortSelection(_reasoningEffortBox.SelectedItem?.ToString()),
            PromptVersionPath = _promptVersionBox.Text.Trim()
        };
    }

    private void SelectReasoningEffort(string effort)
    {
        var normalized = PromptLabService.NormalizeReasoningEffortSelection(effort);
        var index = _reasoningEffortBox.Items.IndexOf(normalized);
        _reasoningEffortBox.SelectedIndex = index >= 0 ? index : Math.Max(0, _reasoningEffortBox.Items.Count - 1);
    }

    private void LoadPromptIntoUi(ActionPostprocessConfigModel config)
    {
        _systemPromptBox.Text = config.SystemPrompt ?? "";
        _userTemplateBox.Text = config.UserPromptTemplate ?? "";
    }

    private void LoadRulesToUi()
    {
        _rulesList.Items.Clear();
        _tagRuleList.Items.Clear();
        foreach (var rule in _catalog.Rules)
        {
            _rulesList.Items.Add(rule, false);
            _tagRuleList.Items.Add(rule);
        }

        _rulesList.DisplayMember = nameof(PromptRuleInfo.DisplayName);
        _tagRuleList.DisplayMember = nameof(PromptRuleInfo.DisplayName);
        if (_tagRuleList.Items.Count > 0)
        {
            _tagRuleList.SelectedIndex = 0;
        }
        else
        {
            LoadSelectedRuleToTagPromptUi();
        }
    }

    private void LoadSelectedRuleToTagPromptUi()
    {
        _tagEntryList.BeginUpdate();
        try
        {
            _tagEntryList.Items.Clear();
            if (_tagRuleList.SelectedItem is not PromptRuleInfo rule)
            {
                _tagRuleHeaderLabel.Text = "";
                LoadSelectedTagEntryToUi();
                return;
            }

            _tagRuleHeaderLabel.Text = rule.DisplayName;
            foreach (var entry in rule.PostprocessRules ?? new List<PostprocessRuleEntry>())
            {
                if (entry != null && !string.IsNullOrWhiteSpace(entry.Tag))
                {
                    _tagEntryList.Items.Add(new TagRuleListItem(entry));
                }
            }
        }
        finally
        {
            _tagEntryList.EndUpdate();
        }

        if (_tagEntryList.Items.Count > 0)
        {
            _tagEntryList.SelectedIndex = 0;
        }
        else
        {
            LoadSelectedTagEntryToUi();
        }
    }

    private void LoadSelectedTagEntryToUi()
    {
        _loadingTagPrompt = true;
        try
        {
            if (_tagEntryList.SelectedItem is not TagRuleListItem item)
            {
                _tagTextBox.Text = "";
                _tagDescriptionBox.Text = "";
                _tagDescriptionBox.ReadOnly = true;
                return;
            }

            _tagTextBox.Text = item.Entry.Tag ?? "";
            _tagDescriptionBox.Text = item.Entry.Description ?? "";
            _tagDescriptionBox.ReadOnly = false;
        }
        finally
        {
            _loadingTagPrompt = false;
        }
    }

    private void UpdateSelectedTagDescription()
    {
        if (_loadingTagPrompt || _tagEntryList.SelectedItem is not TagRuleListItem item)
        {
            return;
        }

        item.Entry.Description = _tagDescriptionBox.Text;
    }

    private void LoadCasesFromFile()
    {
        _cases.Clear();
        _cases.AddRange(_service.LoadCases(_caseFileBox.Text.Trim()));
        RebindCases();
    }

    private void RebindCases()
    {
        var selectedId = (_caseList.SelectedItem as PromptLabCase)?.CaseId;
        _caseList.DataSource = null;
        _caseList.DisplayMember = nameof(PromptLabCase.DisplayName);
        _caseList.DataSource = _cases;
        if (!string.IsNullOrWhiteSpace(selectedId))
        {
            var item = _cases.FirstOrDefault(x => string.Equals(x.CaseId, selectedId, StringComparison.OrdinalIgnoreCase));
            if (item != null)
            {
                _caseList.SelectedItem = item;
            }
        }
    }

    private void LoadSelectedCaseToUi()
    {
        if (_loadingCase || _caseList.SelectedItem is not PromptLabCase item)
        {
            return;
        }

        _loadingCase = true;
        try
        {
            _caseIdBox.Text = item.CaseId;
            _titleBox.Text = item.Title;
            _playerTextBox.Text = item.PlayerText;
            _npcReplyBox.Text = item.NpcReplyText;
            _historyBox.Text = string.Join(Environment.NewLine, item.HistoryLines ?? new List<string>());
            _afefPlayerBox.Text = string.Join(Environment.NewLine, (item.AfefFacts ?? new List<AfefFact>()).Where(x => !string.Equals(x.Kind, "npc", StringComparison.OrdinalIgnoreCase)).Select(x => x.Text));
            _afefNpcBox.Text = string.Join(Environment.NewLine, (item.AfefFacts ?? new List<AfefFact>()).Where(x => string.Equals(x.Kind, "npc", StringComparison.OrdinalIgnoreCase)).Select(x => x.Text));
            _runtimeContextBox.Text = item.RuntimeContext;
            _expectedTagsBox.Text = string.Join(Environment.NewLine, item.ExpectedTags ?? new List<string>());
            _notesBox.Text = item.Notes;
            ApplyRuleChecks(item.PreprocessHits);
            RenderCurrentCase();
        }
        finally
        {
            _loadingCase = false;
        }
    }

    private void ApplyRuleChecks(IEnumerable<string> hits)
    {
        var selected = new HashSet<string>((hits ?? Array.Empty<string>()).Select(x => x.Trim()), StringComparer.OrdinalIgnoreCase);
        for (var i = 0; i < _rulesList.Items.Count; i++)
        {
            if (_rulesList.Items[i] is PromptRuleInfo rule)
            {
                _rulesList.SetItemChecked(i, selected.Contains(rule.Id) || selected.Contains(rule.Source) || selected.Contains(rule.TopicLabel));
            }
        }
    }

    private PromptLabCase ReadCaseFromUi()
    {
        var facts = new List<AfefFact>();
        facts.AddRange(ReadLines(_afefPlayerBox.Text).Select(x => new AfefFact { Kind = "player", Text = x }));
        facts.AddRange(ReadLines(_afefNpcBox.Text).Select(x => new AfefFact { Kind = "npc", Text = x }));

        var caseId = _caseIdBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(caseId))
        {
            caseId = "case_" + DateTime.Now.ToString("yyyyMMdd_HHmmss");
        }

        return new PromptLabCase
        {
            CaseId = caseId,
            Title = _titleBox.Text.Trim(),
            PreprocessHits = GetCheckedRuleIds(),
            PlayerText = _playerTextBox.Text.Trim(),
            NpcReplyText = _npcReplyBox.Text.Trim(),
            HistoryLines = ReadLines(_historyBox.Text),
            AfefFacts = facts,
            RuntimeContext = _runtimeContextBox.Text.Trim(),
            ExpectedTags = ReadLines(_expectedTagsBox.Text),
            Notes = _notesBox.Text.Trim()
        };
    }

    private List<string> GetCheckedRuleIds()
    {
        return _rulesList.CheckedItems
            .OfType<PromptRuleInfo>()
            .Select(x => x.Id)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToList();
    }

    private ActionPostprocessConfigModel CurrentPromptConfig()
    {
        return _service.CloneActionConfigWithPromptsAndRuleDescriptions(_catalog.ActionConfig, _systemPromptBox.Text, _userTemplateBox.Text, _catalog.Rules);
    }

    private void NewCase()
    {
        _caseList.ClearSelected();
        _caseIdBox.Text = "case_" + DateTime.Now.ToString("yyyyMMdd_HHmmss");
        _titleBox.Text = "";
        _playerTextBox.Text = "";
        _npcReplyBox.Text = "";
        _historyBox.Text = "";
        _afefPlayerBox.Text = "";
        _afefNpcBox.Text = "";
        _runtimeContextBox.Text = "";
        _expectedTagsBox.Text = "";
        _notesBox.Text = "";
        ApplyRuleChecks(Array.Empty<string>());
        _promptPreviewBox.Text = "";
        _requestBox.Text = "";
        _responseBox.Text = "";
        _rawResponseBox.Text = "";
        _filesBox.Text = "";
    }

    private void SaveCurrentCase()
    {
        try
        {
            var item = ReadCaseFromUi();
            var existing = _cases.FindIndex(x => string.Equals(x.CaseId, item.CaseId, StringComparison.OrdinalIgnoreCase));
            if (existing >= 0)
            {
                _cases[existing] = item;
            }
            else
            {
                _cases.Add(item);
            }

            _service.SaveCases(_caseFileBox.Text.Trim(), _cases);
            RebindCases();
            _caseList.SelectedItem = _cases.FirstOrDefault(x => string.Equals(x.CaseId, item.CaseId, StringComparison.OrdinalIgnoreCase));
            SetStatus("已保存案例：" + item.CaseId);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "保存案例失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void RenderCurrentCase()
    {
        try
        {
            var rendered = _service.RenderPrompt(_catalog, ReadCaseFromUi(), ReadSettingsFromUi(), CurrentPromptConfig());
            _promptPreviewBox.Text = PromptLabService.FormatRenderedPromptText(rendered);
            _requestBox.Text = rendered.RequestJson;
            _responseBox.Text = "";
            _rawResponseBox.Text = "";
            SetStatus("已生成请求体 JSON。");
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "生成请求体失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async Task RunSelectedCaseAsync()
    {
        try
        {
            var item = ReadCaseFromUi();
            var settings = ReadSettingsFromUi();
            var runDir = _service.CreateRunDirectory(_labRoot);
            _lastRunDir = runDir;
            SetStatus("正在运行 " + item.CaseId + "...");
            var artifact = await _service.RunCaseAsync(runDir, 1, _catalog, item, settings, CurrentPromptConfig());
            if (File.Exists(artifact.PromptPath))
            {
                _promptPreviewBox.Text = File.ReadAllText(artifact.PromptPath);
            }

            _responseBox.Text = artifact.Result.Success ? artifact.Result.AssistantText : artifact.Result.Error;
            _rawResponseBox.Text = BuildRawResponseText(new[] { artifact });
            _filesBox.Text = BuildArtifactText(new[] { artifact });
            SetStatus("运行完成：" + artifact.ResponsePath);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "运行失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async Task RunAllCasesAsync()
    {
        try
        {
            if (_cases.Count == 0)
            {
                return;
            }

            var settings = ReadSettingsFromUi();
            var runDir = _service.CreateRunDirectory(_labRoot);
            _lastRunDir = runDir;
            var artifacts = new List<RunArtifact>();
            var prompt = CurrentPromptConfig();
            for (var i = 0; i < _cases.Count; i++)
            {
                SetStatus("正在运行 " + (i + 1) + "/" + _cases.Count + "：" + _cases[i].CaseId);
                artifacts.Add(await _service.RunCaseAsync(runDir, i + 1, _catalog, _cases[i], settings, prompt));
            }

            _responseBox.Text = string.Join(Environment.NewLine + Environment.NewLine, artifacts.Select(x => x.CaseId + ":" + Environment.NewLine + (x.Result.Success ? x.Result.AssistantText : x.Result.Error)));
            if (artifacts.Count > 0 && File.Exists(artifacts[^1].PromptPath))
            {
                _promptPreviewBox.Text = File.ReadAllText(artifacts[^1].PromptPath);
            }

            _rawResponseBox.Text = BuildRawResponseText(artifacts);
            _filesBox.Text = BuildArtifactText(artifacts);
            SetStatus("批量运行完成：" + runDir);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "批量运行失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private string BuildArtifactText(IEnumerable<RunArtifact> artifacts)
    {
        return string.Join(Environment.NewLine + Environment.NewLine, artifacts.Select(x =>
            x.CaseId + Environment.NewLine +
            "完整提示词：" + x.PromptPath + Environment.NewLine +
            "请求体：" + x.RequestPath + Environment.NewLine +
            "回复：" + x.ResponsePath + Environment.NewLine +
            "元数据：" + x.MetaPath));
    }

    private static string BuildRawResponseText(IEnumerable<RunArtifact> artifacts)
    {
        return string.Join(Environment.NewLine + Environment.NewLine, artifacts.Select(x =>
            "===== " + x.CaseId + " =====" + Environment.NewLine +
            "成功：" + x.Result.Success + Environment.NewLine +
            "状态码：" + x.Result.StatusCode + Environment.NewLine +
            "错误：" + (x.Result.Error ?? "") + Environment.NewLine +
            Environment.NewLine +
            "----- 提取后的模型回复 -----" + Environment.NewLine +
            (x.Result.AssistantText ?? "") + Environment.NewLine +
            Environment.NewLine +
            "----- 原始 HTTP 回复 -----" + Environment.NewLine +
            (string.IsNullOrWhiteSpace(x.Result.RawResponse) ? x.Result.Error : x.Result.RawResponse)));
    }

    private void BrowseRepoRoot()
    {
        using var dialog = new FolderBrowserDialog
        {
            Description = "选择 AnimusForge 仓库根目录",
            SelectedPath = Directory.Exists(_repoRootBox.Text) ? _repoRootBox.Text : Directory.GetCurrentDirectory()
        };
        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            _repoRootBox.Text = dialog.SelectedPath;
            _labRoot = _service.ResolveLabRoot(dialog.SelectedPath);
            _caseFileBox.Text = Path.Combine(_labRoot, "cases", "sample_cases.jsonl");
            ReloadCatalogAndCases();
        }
    }

    private void BrowseCaseFile()
    {
        using var dialog = new OpenFileDialog
        {
            Title = "打开 JSONL 案例文件",
            Filter = "JSONL 案例文件 (*.jsonl)|*.jsonl|所有文件 (*.*)|*.*",
            FileName = _caseFileBox.Text,
            InitialDirectory = Directory.Exists(Path.GetDirectoryName(_caseFileBox.Text)) ? Path.GetDirectoryName(_caseFileBox.Text) : _labRoot
        };
        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            _caseFileBox.Text = dialog.FileName;
            LoadCasesFromFile();
        }
    }

    private void LoadPromptVersion()
    {
        using var dialog = new OpenFileDialog
        {
            Title = "加载后处理提示词版本",
            Filter = "JSON 文件 (*.json)|*.json|所有文件 (*.*)|*.*",
            InitialDirectory = Directory.Exists(Path.Combine(_labRoot, "prompt_versions")) ? Path.Combine(_labRoot, "prompt_versions") : _labRoot
        };
        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        try
        {
            _promptVersionBox.Text = dialog.FileName;
            ReloadCatalogAndCases();
            SetStatus("已加载提示词版本：" + dialog.FileName);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "加载提示词失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void SavePromptVersion()
    {
        Directory.CreateDirectory(Path.Combine(_labRoot, "prompt_versions"));
        using var dialog = new SaveFileDialog
        {
            Title = "保存提示词版本",
            Filter = "JSON 文件 (*.json)|*.json|所有文件 (*.*)|*.*",
            InitialDirectory = Path.Combine(_labRoot, "prompt_versions"),
            FileName = "ActionPostprocessPrompt_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".json"
        };
        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        try
        {
            var config = CurrentPromptConfig();
            _service.SavePromptVersion(dialog.FileName, config);
            _promptVersionBox.Text = dialog.FileName;
            _catalog.ActionConfig = config;
            SetStatus("已保存提示词版本：" + dialog.FileName);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "保存提示词失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private string ResolvePromptVersionPath()
    {
        var path = _promptVersionBox.Text.Trim();
        return File.Exists(path) ? path : "";
    }

    private void OpenRunsFolder()
    {
        var path = !string.IsNullOrWhiteSpace(_lastRunDir) ? _lastRunDir : Path.Combine(_labRoot, "runs");
        Directory.CreateDirectory(path);
        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo { FileName = path, UseShellExecute = true });
    }

    private static List<string> ReadLines(string text)
    {
        return (text ?? "")
            .Replace("\r\n", "\n")
            .Replace('\r', '\n')
            .Split('\n')
            .Select(x => x.Trim())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToList();
    }

    private static decimal ClampDecimal(decimal value, decimal min, decimal max)
    {
        if (value < min)
        {
            return min;
        }

        if (value > max)
        {
            return max;
        }

        return value;
    }

    private void SetStatus(string text)
    {
        _statusLabel.Text = DateTime.Now.ToString("HH:mm:ss") + "  " + text;
    }

    private static void AddLabeled(TableLayoutPanel grid, int row, string label, TextBox box, bool singleLine = false)
    {
        grid.Controls.Add(Label(label), 0, row);
        if (singleLine)
        {
            box.Dock = DockStyle.Fill;
            box.Multiline = false;
        }
        else
        {
            ConfigureMultiline(box);
        }

        grid.Controls.Add(box, 1, row);
    }

    private static Panel BuildTitledPanel(string title, Control content)
    {
        var panel = new Panel { Dock = DockStyle.Fill };
        var label = new Label { Text = title, Dock = DockStyle.Top, Height = 22, TextAlign = ContentAlignment.MiddleLeft };
        content.Dock = DockStyle.Fill;
        panel.Controls.Add(content);
        panel.Controls.Add(label);
        return panel;
    }

    private static void ConfigureMultiline(TextBox box)
    {
        box.Dock = DockStyle.Fill;
        box.Multiline = true;
        box.AcceptsReturn = true;
        box.AcceptsTab = true;
        box.ScrollBars = ScrollBars.Both;
        box.WordWrap = false;
        box.Font = new Font(FontFamily.GenericMonospace, 10f);
    }

    private static Label Label(string text)
    {
        return new Label { Text = text, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(4, 0, 4, 0) };
    }

    private static Button Button(string text, int width = 0)
    {
        return new Button { Text = text, Dock = width <= 0 ? DockStyle.Fill : DockStyle.None, Width = width <= 0 ? 75 : width };
    }
}
