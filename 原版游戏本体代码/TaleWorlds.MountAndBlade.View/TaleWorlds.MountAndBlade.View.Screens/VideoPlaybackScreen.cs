using TaleWorlds.Core;
using TaleWorlds.DotNet;
using TaleWorlds.Engine;
using TaleWorlds.InputSystem;
using TaleWorlds.ScreenSystem;

namespace TaleWorlds.MountAndBlade.View.Screens;

public class VideoPlaybackScreen : ScreenBase, IGameStateListener
{
	protected VideoPlaybackState _videoPlaybackState;

	protected VideoPlayerView _videoPlayerView;

	protected float _totalElapsedTimeSinceVideoStart;

	public VideoPlaybackScreen(VideoPlaybackState videoPlaybackState)
	{
		_videoPlaybackState = videoPlaybackState;
		_videoPlayerView = VideoPlayerView.CreateVideoPlayerView();
		((View)_videoPlayerView).SetRenderOrder(-10000);
	}

	protected sealed override void OnFrameTick(float dt)
	{
		_totalElapsedTimeSinceVideoStart += dt;
		((ScreenBase)this).OnFrameTick(dt);
		if ((NativeObject)(object)_videoPlayerView != (NativeObject)null && _videoPlaybackState != null)
		{
			if (_videoPlaybackState.CanUserSkip && (Input.IsKeyReleased((InputKey)1) || Input.IsKeyReleased((InputKey)251)))
			{
				_videoPlayerView.StopVideo();
			}
			if (_videoPlayerView.IsVideoFinished())
			{
				_videoPlaybackState.OnVideoFinished();
				((View)_videoPlayerView).SetEnable(false);
				_videoPlayerView.FinalizePlayer();
				_videoPlayerView = null;
			}
			if ((object)ScreenManager.TopScreen == this)
			{
				OnVideoPlaybackTick(dt);
			}
		}
	}

	protected virtual void OnVideoPlaybackTick(float dt)
	{
	}

	void IGameStateListener.OnInitialize()
	{
		_videoPlayerView.PlayVideo(_videoPlaybackState.VideoPath, _videoPlaybackState.AudioPath, _videoPlaybackState.FrameRate, false);
		_videoPlaybackState.OnVideoStarted();
		LoadingWindow.DisableGlobalLoadingWindow();
		Utilities.DisableGlobalLoadingWindow();
	}

	void IGameStateListener.OnFinalize()
	{
		VideoPlayerView videoPlayerView = _videoPlayerView;
		if (videoPlayerView != null)
		{
			((View)videoPlayerView).SetEnable(false);
		}
		VideoPlayerView videoPlayerView2 = _videoPlayerView;
		if (videoPlayerView2 != null)
		{
			videoPlayerView2.FinalizePlayer();
		}
	}

	void IGameStateListener.OnActivate()
	{
		((ScreenBase)this).OnActivate();
	}

	void IGameStateListener.OnDeactivate()
	{
		((ScreenBase)this).OnDeactivate();
	}
}
