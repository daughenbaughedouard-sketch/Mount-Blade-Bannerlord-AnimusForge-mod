using TaleWorlds.GauntletUI;
using TaleWorlds.GauntletUI.BaseTypes;
using TaleWorlds.Library;
using TaleWorlds.TwoDimension;

namespace NavalDLC.GauntletUI.Widgets.Widgets;

public class PortUpgradesPanelArrowWidget : Widget
{
	private Widget _targetSlot;

	private float _currentLerpSpeed = -1f;

	public PortUpgradesPanelArrowWidget(UIContext context)
		: base(context)
	{
	}

	protected override void OnLateUpdate(float dt)
	{
		((Widget)this).OnLateUpdate(dt);
		if (_targetSlot != null)
		{
			UpdateAnimation(dt);
		}
	}

	private void UpdateAnimation(float dt)
	{
		((Widget)this).VerticalAlignment = (VerticalAlignment)0;
		float y = ((Rectangle2D)(ref _targetSlot.AreaRect)).GetCenter().Y;
		float y2 = ((Rectangle2D)(ref base.AreaRect)).GetCenter().Y;
		float num = y * ((Widget)this)._inverseScaleToUse - y2 * ((Widget)this)._inverseScaleToUse;
		if (_currentLerpSpeed > 0f)
		{
			float num2 = MathF.Lerp(0f, num, _currentLerpSpeed * dt, 1E-05f);
			if (MathF.Abs(num - num2) < 1f)
			{
				_currentLerpSpeed = -1f;
			}
			else
			{
				_currentLerpSpeed += 10f * dt;
			}
			num = num2;
		}
		((Widget)this).PositionYOffset = ((Widget)this).PositionYOffset + num;
	}

	public void SetTargetSlot(Widget targetSlot)
	{
		if (_targetSlot != targetSlot)
		{
			_targetSlot = targetSlot;
			_currentLerpSpeed = 10f;
		}
	}
}
