using TaleWorlds.Library;

namespace TaleWorlds.MountAndBlade.Multiplayer.Admin.Internal;

internal class AdminPanelNumericOption : AdminPanelOption<int>, IAdminPanelNumericOption, IAdminPanelOption<int>, IAdminPanelOption
{
	private int? _minimumValue;

	private int? _maximumValue;

	public AdminPanelNumericOption(string uniqueId)
		: base(uniqueId)
	{
	}

	protected override bool AreEqualValues(int first, int second)
	{
		return first == second;
	}

	public AdminPanelNumericOption SetMinimumValue(int value)
	{
		_minimumValue = value;
		return this;
	}

	public AdminPanelNumericOption SetMaximumValue(int value)
	{
		_maximumValue = value;
		return this;
	}

	public AdminPanelNumericOption SetMinimumAndMaximumFrom(OptionType optionType)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		MultiplayerOptionsProperty optionProperty = MultiplayerOptionsExtensions.GetOptionProperty(optionType);
		if (optionProperty != null && optionProperty.HasBounds)
		{
			_minimumValue = MultiplayerOptionsExtensions.GetMinimumValue(optionType);
			_maximumValue = MultiplayerOptionsExtensions.GetMaximumValue(optionType);
			SetValue(MBMath.ClampInt(base.CurrentValue, _minimumValue.Value, _maximumValue.Value));
		}
		return this;
	}

	public int? GetMinimumValue()
	{
		return _minimumValue;
	}

	public int? GetMaximumValue()
	{
		return _maximumValue;
	}
}
