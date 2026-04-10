using TaleWorlds.GauntletUI;
using TaleWorlds.GauntletUI.BaseTypes;
using TaleWorlds.Library;

namespace NavalDLC.GauntletUI.Widgets.Widgets;

public class PortUpgradesPanelParentWidget : Widget
{
	private bool _isFirstFrame = true;

	private float _visibilityAnimationTimer;

	private float _fullMarginLeft;

	private bool _visibilityCondition;

	private float _visibilityAnimationDuration;

	[Editor(false)]
	public bool VisibilityCondition
	{
		get
		{
			return _visibilityCondition;
		}
		set
		{
			if (value != _visibilityCondition)
			{
				_visibilityCondition = value;
				((PropertyOwnerObject)this).OnPropertyChanged(value, "VisibilityCondition");
			}
		}
	}

	[Editor(false)]
	public float VisibilityAnimationDuration
	{
		get
		{
			return _visibilityAnimationDuration;
		}
		set
		{
			if (value != _visibilityAnimationDuration)
			{
				_visibilityAnimationDuration = value;
				((PropertyOwnerObject)this).OnPropertyChanged(value, "VisibilityAnimationDuration");
			}
		}
	}

	public PortUpgradesPanelParentWidget(UIContext context)
		: base(context)
	{
		((Widget)this).IsVisible = false;
	}

	protected override void OnLateUpdate(float dt)
	{
		((Widget)this).OnLateUpdate(dt);
		if (_isFirstFrame)
		{
			_fullMarginLeft = ((Widget)this).MarginLeft;
			_isFirstFrame = false;
		}
		if (VisibilityCondition)
		{
			((Widget)this).IsVisible = true;
			if (_visibilityAnimationTimer < _visibilityAnimationDuration)
			{
				float ratio = AnimationInterpolation.Ease((Type)3, (Function)4, MathF.Clamp(_visibilityAnimationTimer / _visibilityAnimationDuration, 0f, 1f));
				UpdateAnimation(ratio);
				_visibilityAnimationTimer += dt;
			}
			else
			{
				_visibilityAnimationTimer = _visibilityAnimationDuration;
				UpdateAnimation(1f);
			}
		}
		else if (_visibilityAnimationTimer > 0f)
		{
			float ratio2 = AnimationInterpolation.Ease((Type)3, (Function)4, MathF.Clamp(_visibilityAnimationTimer / _visibilityAnimationDuration, 0f, 1f));
			UpdateAnimation(ratio2);
			_visibilityAnimationTimer -= dt;
		}
		else
		{
			_visibilityAnimationTimer = 0f;
			UpdateAnimation(0f);
			((Widget)this).IsVisible = false;
		}
	}

	private void UpdateAnimation(float ratio)
	{
		((Widget)this).MarginLeft = MathF.Lerp(_fullMarginLeft / 2f, _fullMarginLeft, ratio, 1E-05f);
		GauntletExtensions.SetGlobalAlphaRecursively((Widget)(object)this, ratio * ((Widget)this).ParentWidget.AlphaFactor);
	}
}
