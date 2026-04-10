using System;
using SandBox.Objects.AreaMarkers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.Objects;

namespace SandBox.ViewModelCollection.Missions.NameMarker.Targets;

public class MissionCommonAreaMarkerTargetVM : MissionNameMarkerTargetVM<CommonAreaMarker>
{
	public readonly Alley TargetAlley;

	public MissionCommonAreaMarkerTargetVM(CommonAreaMarker target)
		: base(target)
	{
		base.NameType = "Passage";
		base.IconType = "common_area";
		TargetAlley = Hero.MainHero.CurrentSettlement.Alleys[((AreaMarker)target).AreaIndex - 1];
		UpdateAlleyStatus();
		CampaignEvents.AlleyOwnerChanged.AddNonSerializedListener((object)this, (Action<Alley, Hero, Hero>)OnAlleyOwnerChanged);
		((ViewModel)this).RefreshValues();
	}

	public override void OnFinalize()
	{
		((ViewModel)this).OnFinalize();
		((CampaignEventReceiver)CampaignEventDispatcher.Instance).RemoveListeners((object)this);
	}

	private void OnAlleyOwnerChanged(Alley alley, Hero newOwner, Hero oldOwner)
	{
		if (TargetAlley == alley && (newOwner == Hero.MainHero || oldOwner == Hero.MainHero))
		{
			UpdateAlleyStatus();
		}
	}

	public override void UpdatePosition(Camera missionCamera)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		UpdatePositionWith(missionCamera, ((AreaMarker)base.Target).GetPosition() + MissionNameMarkerHelper.DefaultHeightOffset);
	}

	protected override TextObject GetName()
	{
		return ((AreaMarker)base.Target).GetName();
	}

	private void UpdateAlleyStatus()
	{
		if (TargetAlley == null)
		{
			return;
		}
		Hero owner = ((SettlementArea)TargetAlley).Owner;
		if (owner != null)
		{
			if (owner == Hero.MainHero)
			{
				base.NameType = "Friendly";
				base.IsFriendly = true;
				base.IsEnemy = false;
			}
			else
			{
				base.NameType = "Passage";
				base.IsFriendly = false;
				base.IsEnemy = true;
			}
		}
		else
		{
			base.NameType = "Normal";
			base.IsFriendly = false;
			base.IsEnemy = false;
		}
	}
}
