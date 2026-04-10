namespace NavalDLC.CustomBattle.CustomBattle;

public struct NavalCustomBattleCompositionData
{
	public readonly bool IsValid;

	public readonly float RangedPercentage;

	public NavalCustomBattleCompositionData(float rangedPercentage)
	{
		RangedPercentage = rangedPercentage;
		IsValid = true;
	}
}
