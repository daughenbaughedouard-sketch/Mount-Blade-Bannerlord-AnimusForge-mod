using System.Collections.Generic;
using System.IO;
using System.Text;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.View.Screens;
using TaleWorlds.MountAndBlade.ViewModelCollection.VideoPlayback;
using TaleWorlds.ScreenSystem;

namespace TaleWorlds.MountAndBlade.GauntletUI;

[GameStateScreen(typeof(VideoPlaybackState))]
public class GauntletVideoPlaybackScreen : VideoPlaybackScreen
{
	private GauntletLayer _layer;

	private VideoPlaybackVM _dataSource;

	public GauntletVideoPlaybackScreen(VideoPlaybackState videoPlaybackState)
		: base(videoPlaybackState)
	{
	}

	protected override void OnInitialize()
	{
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Expected O, but got Unknown
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Expected O, but got Unknown
		((ScreenBase)this).OnInitialize();
		string subtitleExtensionOfLanguage = LocalizedTextManager.GetSubtitleExtensionOfLanguage(BannerlordConfig.Language);
		List<SubtitleItem> subtitles = null;
		if (!string.IsNullOrEmpty(_videoPlaybackState.SubtitleFileBasePath))
		{
			string text = _videoPlaybackState.SubtitleFileBasePath + "_" + subtitleExtensionOfLanguage + ".srt";
			if (File.Exists(text))
			{
				subtitles = SrtParser.ParseStream((Stream)new FileStream(text, FileMode.Open, FileAccess.Read), Encoding.UTF8);
			}
			else
			{
				Debug.FailedAssert("No Subtitle file exists in path: " + text, "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.GauntletUI\\GauntletVideoPlaybackScreen.cs", "OnInitialize", 41);
			}
		}
		_layer = new GauntletLayer("VideoPlayback", 100002, false);
		_dataSource = new VideoPlaybackVM();
		_layer.LoadMovie("VideoPlayer", (ViewModel)(object)_dataSource);
		_dataSource.SetSubtitles(subtitles);
		((ScreenLayer)_layer).InputRestrictions.SetInputRestrictions(false, (InputUsageMask)7);
		((ScreenBase)this).AddLayer((ScreenLayer)(object)_layer);
		InformationManager.HideAllMessages();
	}

	protected override void OnVideoPlaybackTick(float dt)
	{
		base.OnVideoPlaybackTick(dt);
		_dataSource.Tick(_totalElapsedTimeSinceVideoStart);
	}
}
