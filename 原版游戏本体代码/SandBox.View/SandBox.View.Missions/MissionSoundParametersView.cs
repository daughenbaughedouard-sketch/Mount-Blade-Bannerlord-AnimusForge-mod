using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TaleWorlds.ObjectSystem;

namespace SandBox.View.Missions;

[DefaultView]
public class MissionSoundParametersView : MissionView
{
	public enum SoundParameterMissionCulture : short
	{
		None,
		Aserai,
		Battania,
		Empire,
		Khuzait,
		Sturgia,
		Vlandia,
		Nord,
		ReservedA,
		ReservedB,
		Bandit
	}

	private enum SoundParameterMissionProsperityLevel : short
	{
		None = 0,
		Low = 0,
		Mid = 1,
		High = 2
	}

	private const string CultureParameterId = "MissionCulture";

	private const string ProsperityParameterId = "MissionProsperity";

	private const string CombatParameterId = "MissionCombatMode";

	public override void EarlyStart()
	{
		((MissionBehavior)this).EarlyStart();
		InitializeGlobalParameters();
	}

	public override void OnMissionScreenFinalize()
	{
		((MissionView)this).OnMissionScreenFinalize();
		SoundManager.SetGlobalParameter("MissionCulture", 0f);
		SoundManager.SetGlobalParameter("MissionProsperity", 0f);
		SoundManager.SetGlobalParameter("MissionCombatMode", 0f);
	}

	public override void OnMissionModeChange(MissionMode oldMissionMode, bool atStart)
	{
		InitializeCombatModeParameter();
	}

	private void InitializeGlobalParameters()
	{
		InitializeCultureParameter();
		InitializeProsperityParameter();
		InitializeCombatModeParameter();
	}

	private void InitializeCultureParameter()
	{
		SoundParameterMissionCulture soundParameterMissionCulture = SoundParameterMissionCulture.None;
		if (Campaign.Current != null)
		{
			Settlement currentSettlement = Settlement.CurrentSettlement;
			if (currentSettlement != null)
			{
				if (currentSettlement.IsHideout)
				{
					soundParameterMissionCulture = SoundParameterMissionCulture.Bandit;
				}
				else
				{
					switch (((MBObjectBase)currentSettlement.Culture).StringId)
					{
					case "empire":
						soundParameterMissionCulture = SoundParameterMissionCulture.Empire;
						break;
					case "sturgia":
						soundParameterMissionCulture = SoundParameterMissionCulture.Sturgia;
						break;
					case "aserai":
						soundParameterMissionCulture = SoundParameterMissionCulture.Aserai;
						break;
					case "vlandia":
						soundParameterMissionCulture = SoundParameterMissionCulture.Vlandia;
						break;
					case "battania":
						soundParameterMissionCulture = SoundParameterMissionCulture.Battania;
						break;
					case "khuzait":
						soundParameterMissionCulture = SoundParameterMissionCulture.Khuzait;
						break;
					case "nord":
						soundParameterMissionCulture = SoundParameterMissionCulture.Nord;
						break;
					}
				}
			}
		}
		SoundManager.SetGlobalParameter("MissionCulture", (float)soundParameterMissionCulture);
	}

	private void InitializeProsperityParameter()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Expected I4, but got Unknown
		SoundParameterMissionProsperityLevel soundParameterMissionProsperityLevel = SoundParameterMissionProsperityLevel.None;
		if (Campaign.Current != null && Settlement.CurrentSettlement != null)
		{
			ProsperityLevel prosperityLevel = Settlement.CurrentSettlement.SettlementComponent.GetProsperityLevel();
			switch ((int)prosperityLevel)
			{
			case 0:
				soundParameterMissionProsperityLevel = SoundParameterMissionProsperityLevel.None;
				break;
			case 1:
				soundParameterMissionProsperityLevel = SoundParameterMissionProsperityLevel.Mid;
				break;
			case 2:
				soundParameterMissionProsperityLevel = SoundParameterMissionProsperityLevel.High;
				break;
			}
		}
		SoundManager.SetGlobalParameter("MissionProsperity", (float)soundParameterMissionProsperityLevel);
	}

	private void InitializeCombatModeParameter()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Invalid comparison between Unknown and I4
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Invalid comparison between Unknown and I4
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Invalid comparison between Unknown and I4
		bool flag = (int)((MissionBehavior)this).Mission.Mode == 2 || (int)((MissionBehavior)this).Mission.Mode == 3 || (int)((MissionBehavior)this).Mission.Mode == 7;
		SoundManager.SetGlobalParameter("MissionCombatMode", (float)(flag ? 1 : 0));
	}
}
