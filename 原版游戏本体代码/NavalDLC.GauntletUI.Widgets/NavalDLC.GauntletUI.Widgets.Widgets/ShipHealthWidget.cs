using TaleWorlds.GauntletUI;
using TaleWorlds.GauntletUI.BaseTypes;
using TaleWorlds.GauntletUI.ExtraWidgets;
using TaleWorlds.Library;

namespace NavalDLC.GauntletUI.Widgets.Widgets;

public class ShipHealthWidget : Widget
{
	public float AnimationDelay = 0.2f;

	public float AnimationDuration = 0.8f;

	private float _animationStartHealth;

	private float _smoothedCurrentAmount;

	private float _currentAmountAnimationDelta;

	private int _health;

	private int _maxHealth;

	private FillBarVerticalWidget _healthBar;

	private Widget _changeVisualWidget;

	private Widget _dividerWidget;

	private Widget _dividerVisualWidget;

	[Editor(false)]
	public int Health
	{
		get
		{
			return _health;
		}
		set
		{
			if (_health != value)
			{
				int health = _health;
				_health = value;
				((PropertyOwnerObject)this).OnPropertyChanged(value, "Health");
				if (_health < health)
				{
					OnHealthDrop(health);
				}
			}
		}
	}

	[Editor(false)]
	public int MaxHealth
	{
		get
		{
			return _maxHealth;
		}
		set
		{
			if (_maxHealth != value)
			{
				_maxHealth = value;
				((PropertyOwnerObject)this).OnPropertyChanged(value, "MaxHealth");
			}
		}
	}

	[Editor(false)]
	public FillBarVerticalWidget HealthBar
	{
		get
		{
			return _healthBar;
		}
		set
		{
			if (_healthBar != value)
			{
				_healthBar = value;
				((PropertyOwnerObject)this).OnPropertyChanged<FillBarVerticalWidget>(value, "HealthBar");
			}
		}
	}

	[Editor(false)]
	public Widget ChangeVisualWidget
	{
		get
		{
			return _changeVisualWidget;
		}
		set
		{
			if (_changeVisualWidget != value)
			{
				_changeVisualWidget = value;
				((PropertyOwnerObject)this).OnPropertyChanged<Widget>(value, "ChangeVisualWidget");
			}
		}
	}

	[Editor(false)]
	public Widget DividerWidget
	{
		get
		{
			return _dividerWidget;
		}
		set
		{
			if (_dividerWidget != value)
			{
				_dividerWidget = value;
				((PropertyOwnerObject)this).OnPropertyChanged<Widget>(value, "DividerWidget");
			}
		}
	}

	[Editor(false)]
	public Widget DividerVisualWidget
	{
		get
		{
			return _dividerVisualWidget;
		}
		set
		{
			if (_dividerVisualWidget != value)
			{
				_dividerVisualWidget = value;
				((PropertyOwnerObject)this).OnPropertyChanged<Widget>(value, "DividerVisualWidget");
			}
		}
	}

	public ShipHealthWidget(UIContext context)
		: base(context)
	{
	}

	protected override void OnUpdate(float dt)
	{
		((Widget)this).OnUpdate(dt);
		if (HealthBar != null && ((Widget)this).IsVisible)
		{
			HealthBar.MaxAmount = MaxHealth;
			HealthBar.InitialAmount = Health;
			if (ChangeVisualWidget != null && HealthBar.ChangeWidget != null)
			{
				ChangeVisualWidget.PositionYOffset = 0f - HealthBar.ChangeWidget.PositionYOffset;
			}
			if (DividerWidget != null && DividerVisualWidget != null && HealthBar.FillWidget != null)
			{
				DividerWidget.PositionYOffset = DividerWidget.Size.Y * ((Widget)this)._inverseScaleToUse - HealthBar.FillWidget.Size.Y * ((Widget)this)._inverseScaleToUse;
				DividerVisualWidget.PositionYOffset = 0f - DividerWidget.PositionYOffset;
			}
			AnimateHealthDrop(dt);
		}
	}

	private void OnHealthDrop(int previousValue)
	{
		if (_smoothedCurrentAmount == (float)previousValue)
		{
			_animationStartHealth = previousValue;
		}
		else
		{
			_animationStartHealth = _smoothedCurrentAmount;
		}
		_currentAmountAnimationDelta = 0f;
	}

	private void AnimateHealthDrop(float dt)
	{
		if (_currentAmountAnimationDelta < AnimationDelay + AnimationDuration)
		{
			_currentAmountAnimationDelta += dt;
			float num = MathF.Clamp((_currentAmountAnimationDelta - AnimationDelay) / AnimationDuration, 0f, 1f);
			num = AnimationInterpolation.Ease((Type)2, (Function)0, num);
			_smoothedCurrentAmount = MathF.Lerp(_animationStartHealth, (float)Health, num, 1E-05f);
		}
		else
		{
			_smoothedCurrentAmount = Health;
		}
		HealthBar.CurrentAmount = (int)_smoothedCurrentAmount;
	}
}
