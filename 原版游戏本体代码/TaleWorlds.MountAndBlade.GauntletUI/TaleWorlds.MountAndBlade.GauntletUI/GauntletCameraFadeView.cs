using TaleWorlds.Core.ViewModelCollection.Generic;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Library;
using TaleWorlds.ScreenSystem;

namespace TaleWorlds.MountAndBlade.GauntletUI;

public class GauntletCameraFadeView : GlobalLayer, IScreenFadeHandler
{
	private float _fadeAlpha;

	private ScreenFadeState _fadeState;

	private float _currentStateTimer;

	private float _currentStateBeginAlpha;

	private float _fadeOutDuration;

	private float _blackOutDuration;

	private float _fadeInDuration;

	private bool _autoFadeIn;

	private static bool _isInitialized;

	private readonly GauntletLayer _gauntletLayer;

	private readonly BindingListFloatItem _dataSource;

	public GauntletCameraFadeView()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Expected O, but got Unknown
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Expected O, but got Unknown
		_dataSource = new BindingListFloatItem(_fadeAlpha);
		_gauntletLayer = new GauntletLayer("CameraFade", 100000, false);
		_gauntletLayer.LoadMovie("CameraFade", (ViewModel)(object)_dataSource);
		((GlobalLayer)this).Layer = (ScreenLayer)(object)_gauntletLayer;
	}

	public static void Initialize()
	{
		if (!_isInitialized)
		{
			GauntletCameraFadeView gauntletCameraFadeView = new GauntletCameraFadeView();
			ScreenManager.AddGlobalLayer((GlobalLayer)(object)gauntletCameraFadeView, false);
			ScreenFadeController.RegisterHandler((IScreenFadeHandler)(object)gauntletCameraFadeView);
			_isInitialized = true;
		}
	}

	protected override void OnTick(float dt)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Expected I4, but got Unknown
		((GlobalLayer)this).OnTick(dt);
		ScreenFadeState fadeState = _fadeState;
		switch ((int)fadeState)
		{
		case 0:
			_fadeAlpha = 0f;
			break;
		case 1:
			_currentStateTimer += dt;
			_fadeAlpha = MathF.Lerp(_currentStateBeginAlpha, 1f, MathF.Min(_currentStateTimer / _fadeOutDuration, 1f), 1E-05f);
			if (_currentStateTimer > _fadeOutDuration)
			{
				SetFadeState((ScreenFadeState)2);
			}
			break;
		case 2:
			_fadeAlpha = 1f;
			if (_autoFadeIn)
			{
				_currentStateTimer += dt;
				if (_currentStateTimer > _blackOutDuration)
				{
					SetFadeState((ScreenFadeState)3);
				}
			}
			break;
		case 3:
			_currentStateTimer += dt;
			_fadeAlpha = MathF.Lerp(_currentStateBeginAlpha, 0f, MathF.Min(_currentStateTimer / _fadeInDuration, 1f), 1E-05f);
			if (_currentStateTimer > _fadeInDuration)
			{
				SetFadeState((ScreenFadeState)0);
			}
			break;
		}
		_dataSource.Item = _fadeAlpha;
	}

	public void BeginFadeOutAndIn(float fadeOutDuration = 0.5f, float blackOutDuration = 0.5f, float fadeInDuration = 0.5f)
	{
		_fadeOutDuration = MathF.Max(fadeOutDuration, 0f);
		_blackOutDuration = MathF.Max(blackOutDuration, 0f);
		_fadeInDuration = MathF.Max(fadeInDuration, 0f);
		_autoFadeIn = true;
		SetFadeState((ScreenFadeState)1);
	}

	public void BeginFadeOut(float fadeOutDuration = 0.5f)
	{
		_fadeOutDuration = MathF.Max(fadeOutDuration, 0f);
		_autoFadeIn = false;
		SetFadeState((ScreenFadeState)1);
	}

	public void BeginFadeIn(float fadeInDuration = 0.5f)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Invalid comparison between Unknown and I4
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Invalid comparison between Unknown and I4
		_fadeInDuration = MathF.Max(fadeInDuration, 0f);
		if ((int)_fadeState == 1 || (int)_fadeState == 2)
		{
			SetFadeState((ScreenFadeState)3);
		}
	}

	private void SetFadeState(ScreenFadeState fadeState)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		if (_fadeState != fadeState)
		{
			_fadeState = fadeState;
			_currentStateTimer = 0f;
			_currentStateBeginAlpha = _fadeAlpha;
		}
	}

	public ScreenFadeState GetScreenFadeState()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return _fadeState;
	}
}
