using System.Numerics;
using TaleWorlds.GauntletUI;
using TaleWorlds.GauntletUI.BaseTypes;
using TaleWorlds.Library;
using TaleWorlds.TwoDimension;

namespace NavalDLC.GauntletUI.Widgets.Widgets;

public class PortPieceInspectionWidget : BrushWidget
{
	private PortInspectionParentWidget _targetPiece;

	private float _fadeInOutDelta;

	private float _currentAlpha;

	private bool _isInspected;

	private float _animationSpeed;

	private float _fadeInOutDuration;

	private float _fadeOutDelay;

	private float _offsetFromTarget;

	private Widget _topFrameWidget;

	[Editor(false)]
	public bool IsInspected
	{
		get
		{
			return _isInspected;
		}
		set
		{
			if (value != _isInspected)
			{
				_isInspected = value;
				((PropertyOwnerObject)this).OnPropertyChanged(value, "IsInspected");
			}
		}
	}

	[Editor(false)]
	public float AnimationSpeed
	{
		get
		{
			return _animationSpeed;
		}
		set
		{
			if (value != _animationSpeed)
			{
				_animationSpeed = value;
				((PropertyOwnerObject)this).OnPropertyChanged(value, "AnimationSpeed");
			}
		}
	}

	[Editor(false)]
	public float FadeInOutDuration
	{
		get
		{
			return _fadeInOutDuration;
		}
		set
		{
			if (value != _fadeInOutDuration)
			{
				_fadeInOutDuration = value;
				((PropertyOwnerObject)this).OnPropertyChanged(value, "FadeInOutDuration");
			}
		}
	}

	[Editor(false)]
	public float FadeOutDelay
	{
		get
		{
			return _fadeOutDelay;
		}
		set
		{
			if (value != _fadeOutDelay)
			{
				_fadeOutDelay = value;
				((PropertyOwnerObject)this).OnPropertyChanged(value, "FadeOutDelay");
			}
		}
	}

	[Editor(false)]
	public float OffsetFromTarget
	{
		get
		{
			return _offsetFromTarget;
		}
		set
		{
			if (value != _offsetFromTarget)
			{
				_offsetFromTarget = value;
				((PropertyOwnerObject)this).OnPropertyChanged(value, "OffsetFromTarget");
			}
		}
	}

	[Editor(false)]
	public Widget TopFrameWidget
	{
		get
		{
			return _topFrameWidget;
		}
		set
		{
			if (value != _topFrameWidget)
			{
				_topFrameWidget = value;
				((PropertyOwnerObject)this).OnPropertyChanged<Widget>(value, "TopFrameWidget");
			}
		}
	}

	public PortPieceInspectionWidget(UIContext context)
		: base(context)
	{
		GauntletExtensions.SetGlobalAlphaRecursively((Widget)(object)this, 0f);
	}

	protected override void OnLateUpdate(float dt)
	{
		((Widget)this).OnLateUpdate(dt);
		if (_targetPiece != null)
		{
			UpdateAnimation(dt);
		}
		HandleAlphaFactor(dt);
	}

	private void HandleAlphaFactor(float dt)
	{
		bool flag = _targetPiece != null && IsInspected;
		if (FadeInOutDuration <= 0f)
		{
			_currentAlpha = (flag ? 1f : 0f);
		}
		else
		{
			if (flag)
			{
				_fadeInOutDelta += dt;
			}
			else
			{
				_fadeInOutDelta -= dt;
			}
			_fadeInOutDelta = MathF.Clamp(_fadeInOutDelta, 0f, FadeInOutDuration + FadeOutDelay);
			float num = MathF.Clamp(_fadeInOutDelta / FadeInOutDuration, 0f, 1f);
			float num2 = AnimationInterpolation.Ease((Type)3, (Function)2, num);
			_currentAlpha = MathF.Lerp(0f, 1f, num2, 1E-05f);
		}
		GauntletExtensions.SetGlobalAlphaRecursively((Widget)(object)this, _currentAlpha);
	}

	private void UpdateAnimation(float dt)
	{
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		bool num = ((Widget)this).PositionXOffset == 0f && ((Widget)this).PositionYOffset == 0f;
		((Widget)this).VerticalAlignment = (VerticalAlignment)0;
		((Widget)this).HorizontalAlignment = (HorizontalAlignment)0;
		float num2 = ((AnimationSpeed != 0f) ? MBMath.ClampFloat(AnimationSpeed * dt, 0f, 1f) : 1f);
		Vector2 center = ((Rectangle2D)(ref ((Widget)_targetPiece).AreaRect)).GetCenter();
		Vector2 value = new Vector2(((Widget)this).PositionXOffset, ((Widget)this).PositionYOffset);
		Vector2 value2 = center * ((Widget)this)._inverseScaleToUse + new Vector2(OffsetFromTarget, (0f - ((Widget)this).Size.Y) * ((Widget)this)._inverseScaleToUse * 0.5f);
		Vector2 vector = Vector2.Lerp(value, value2, num2);
		((Widget)this).PositionXOffset = vector.X;
		((Widget)this).PositionYOffset = ClampYPosition(vector.Y);
		Vector2 vector2 = center * ((Widget)this)._inverseScaleToUse;
		float num3 = ClampYPosition(value2.Y);
		float num4 = ((Rectangle2D)(ref ((Widget)this).AreaRect)).GetBoundingBox().Y - ((Rectangle2D)(ref TopFrameWidget.AreaRect)).GetBoundingBox().Y;
		float num5 = vector2.Y - num3 + num4;
		TopFrameWidget.SuggestedHeight = MathF.Max(0f, MathF.Lerp(TopFrameWidget.SuggestedHeight, num5, num2, 1E-05f));
		if (num)
		{
			((Widget)this).PositionXOffset = value2.X;
			((Widget)this).PositionYOffset = ClampYPosition(value2.Y);
			TopFrameWidget.SuggestedHeight = MathF.Max(0f, num5);
		}
	}

	private float ClampYPosition(float positionToClamp)
	{
		return MBMath.ClampFloat(positionToClamp, 0f, (((Widget)this).EventManager.PageSize.Y - ((Widget)this).Size.Y) * ((Widget)this)._inverseScaleToUse - 70f);
	}

	public void SetTargetPiece(PortInspectionParentWidget targetPiece)
	{
		if (_targetPiece != targetPiece && IsInspected)
		{
			_targetPiece = targetPiece;
		}
	}
}
