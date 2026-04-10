using System;
using System.Collections.Generic;
using Helpers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Conversation;
using TaleWorlds.CampaignSystem.Naval;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Party.PartyComponents;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;
using TaleWorlds.SaveSystem.Resolvers;

namespace NavalDLC.CampaignBehaviors;

public class ClanFleetManagementCampaignBehavior : CampaignBehaviorBase, IFleetManagementCampaignBehavior
{
	private class SentTroopsData
	{
		[SaveableProperty(0)]
		public TroopRoster SentTroops { get; private set; }

		[SaveableProperty(1)]
		public CampaignTime TroopReturnTime { get; private set; }

		[SaveableProperty(2)]
		public TextObject ShipName { get; private set; }

		[SaveableProperty(3)]
		public TextObject PartyName { get; private set; }

		public static SentTroopsData GetSentTroops(TroopRoster troops, CampaignTime returnTime, TextObject shipName, TextObject partyName)
		{
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			SentTroopsData obj = new SentTroopsData
			{
				SentTroops = TroopRoster.CreateDummyTroopRoster()
			};
			obj.SentTroops.Add(troops);
			obj.TroopReturnTime = returnTime;
			obj.ShipName = shipName;
			obj.PartyName = partyName;
			return obj;
		}
	}

	public class ClanFleetManagementCampaignBehaviorTypeDefiner : SaveableTypeDefiner
	{
		public ClanFleetManagementCampaignBehaviorTypeDefiner()
			: base(612504)
		{
		}

		protected override void DefineContainerDefinitions()
		{
			((SaveableTypeDefiner)this).ConstructContainerDefinition(typeof(List<SentTroopsData>));
		}

		protected override void DefineClassTypes()
		{
			((SaveableTypeDefiner)this).AddClassDefinition(typeof(SentTroopsData), 2, (IObjectResolver)null);
		}
	}

	private List<SentTroopsData> _sentTroops = new List<SentTroopsData>();

	public override void RegisterEvents()
	{
		CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener((object)this, (Action<CampaignGameStarter>)OnSessionLaunched);
		CampaignEvents.HourlyTickEvent.AddNonSerializedListener((object)this, (Action)HourlyTick);
	}

	private void HourlyTick()
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		if (!Campaign.Current.Models.FleetManagementModel.CanTroopsReturn())
		{
			return;
		}
		for (int num = _sentTroops.Count - 1; num >= 0; num--)
		{
			CampaignTime troopReturnTime = _sentTroops[num].TroopReturnTime;
			if (((CampaignTime)(ref troopReturnTime)).IsPast)
			{
				MakeTroopsReturn(_sentTroops[num]);
				_sentTroops.RemoveAt(num);
			}
		}
	}

	private void OnSessionLaunched(CampaignGameStarter starter)
	{
		AddDialogs(starter);
	}

	public override void SyncData(IDataStore dataStore)
	{
		dataStore.SyncData<List<SentTroopsData>>("_sentTroops", ref _sentTroops);
	}

	private void AddDialogs(CampaignGameStarter starter)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Expected O, but got Unknown
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Expected O, but got Unknown
		starter.AddPlayerLine("clan_party_manage_fleet", "hero_main_options", "clan_party_manage_fleet_screen", "{=7DdiFD9W}Let me inspect your ships.", new OnConditionDelegate(conversation_clan_member_manage_fleet_on_condition), (OnConsequenceDelegate)null, 90, (OnClickableConditionDelegate)null, (OnPersuasionOptionDelegate)null);
		starter.AddDialogLine("clan_party_manage_fleet_screen", "clan_party_manage_fleet_screen", "lord_pretalk", "{=!}fleet screen goes here.", (OnConditionDelegate)null, new OnConsequenceDelegate(conversation_clan_member_manage_fleet_on_consequence), 100, (OnClickableConditionDelegate)null);
	}

	private void MakeTroopsReturn(SentTroopsData sentTroops)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Expected O, but got Unknown
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Expected O, but got Unknown
		MobileParty.MainParty.MemberRoster.Add(sentTroops.SentTroops);
		TextObject val = new TextObject("{=CC5Aa5VH}Your troops have returned from delivering {SHIP_NAME} to {PARTY_NAME}.", (Dictionary<string, object>)null);
		val.SetTextVariable("SHIP_NAME", sentTroops.ShipName);
		val.SetTextVariable("PARTY_NAME", sentTroops.PartyName);
		InformationManager.DisplayMessage(new InformationMessage(((object)val).ToString(), new Color(0f, 1f, 0f, 1f)));
	}

	private bool conversation_clan_member_manage_fleet_on_condition()
	{
		Hero oneToOneConversationHero = Hero.OneToOneConversationHero;
		if (MobileParty.MainParty.MapEvent == null && oneToOneConversationHero != null && oneToOneConversationHero.Clan == Clan.PlayerClan && oneToOneConversationHero.PartyBelongedTo != null && oneToOneConversationHero.PartyBelongedTo.LeaderHero == oneToOneConversationHero && oneToOneConversationHero.PartyBelongedTo.MapEvent == null && !oneToOneConversationHero.PartyBelongedTo.IsCaravan && !oneToOneConversationHero.PartyBelongedTo.IsMilitia && !oneToOneConversationHero.PartyBelongedTo.IsVillager && !oneToOneConversationHero.PartyBelongedTo.IsPatrolParty)
		{
			if (((List<Ship>)(object)oneToOneConversationHero.PartyBelongedTo.Ships).Count <= 0)
			{
				return ((List<Ship>)(object)MobileParty.MainParty.Ships).Count > 0;
			}
			return true;
		}
		return false;
	}

	private void conversation_clan_member_manage_fleet_on_consequence()
	{
		PortStateHelper.OpenAsManageOtherFleet(Hero.OneToOneConversationHero.PartyBelongedTo.Party, (Action)OnManageOtherFleetDone);
	}

	private void OnManageOtherFleetDone()
	{
		Campaign.Current.ConversationManager.ContinueConversation();
	}

	public void SendShipToParty(Ship ship, MobileParty mobileParty)
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		TroopRoster troops = MobileParty.MainParty.MemberRoster.RemoveNumberOfNonHeroTroopsRandomly(Campaign.Current.Models.FleetManagementModel.MinimumTroopCountRequiredToSendShips);
		_sentTroops.Add(SentTroopsData.GetSentTroops(troops, Campaign.Current.Models.FleetManagementModel.GetReturnTimeForTroops(ship), ship.Name, mobileParty.Name));
		ChangeShipOwnerAction.ApplyByTransferring(mobileParty.Party, ship);
	}

	public void SendShipToClan(Ship ship, Clan clan)
	{
		float num = float.MinValue;
		MobileParty val = null;
		MBList<Ship> val2 = new MBList<Ship>();
		foreach (WarPartyComponent item in (List<WarPartyComponent>)(object)clan.WarPartyComponents)
		{
			if (NavalDLCManager.Instance.GameModels.ShipDistributionModel.CanSendShipToParty(ship, ((PartyComponent)item).MobileParty) && (val == null || ((List<Ship>)(object)val.Ships).Count >= ((List<Ship>)(object)((PartyComponent)item).Party.Ships).Count))
			{
				((List<Ship>)(object)val2).Clear();
				((List<Ship>)(object)val2).AddRange((IEnumerable<Ship>)((PartyComponent)item).Party.Ships);
				float scoreForPartyShipComposition = NavalDLCManager.Instance.GameModels.ShipDistributionModel.GetScoreForPartyShipComposition(((PartyComponent)item).MobileParty, (MBReadOnlyList<Ship>)(object)val2);
				((List<Ship>)(object)val2).Add(ship);
				float num2 = NavalDLCManager.Instance.GameModels.ShipDistributionModel.GetScoreForPartyShipComposition(((PartyComponent)item).MobileParty, (MBReadOnlyList<Ship>)(object)val2) - scoreForPartyShipComposition;
				if (num2 > num)
				{
					val = ((PartyComponent)item).MobileParty;
					num = num2;
				}
			}
		}
		if (val != null)
		{
			SendShipToParty(ship, val);
		}
		else
		{
			DestroyShipAction.Apply(ship);
		}
	}
}
