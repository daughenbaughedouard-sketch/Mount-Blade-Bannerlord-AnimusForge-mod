using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Naval;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;

namespace NavalDLC.CampaignBehaviors;

public class NavalDLCFigureheadCampaignBehavior : CampaignBehaviorBase
{
	private MBReadOnlyList<Figurehead> _allFigureheadsCache;

	private CampaignTime _lastFigureheadLootTime = CampaignTime.Zero;

	private Dictionary<Hero, Figurehead> _aiLordSelectedFigureheads = new Dictionary<Hero, Figurehead>();

	public CampaignTime LastFigureheadLootTime => _lastFigureheadLootTime;

	public override void SyncData(IDataStore dataStore)
	{
		dataStore.SyncData<CampaignTime>("_lastFigureheadLootTime", ref _lastFigureheadLootTime);
		dataStore.SyncData<Dictionary<Hero, Figurehead>>("_aiLordSelectedFigureheads", ref _aiLordSelectedFigureheads);
	}

	public override void RegisterEvents()
	{
		CampaignEvents.OnShipOwnerChangedEvent.AddNonSerializedListener((object)this, (Action<Ship, PartyBase, ShipOwnerChangeDetail>)OnShipOwnerChanged);
		CampaignEvents.OnFigureheadUnlockedEvent.AddNonSerializedListener((object)this, (Action<Figurehead>)OnFigureheadUnlocked);
	}

	private void OnFigureheadUnlocked(Figurehead figurehead)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Expected O, but got Unknown
		//IL_0030: Expected O, but got Unknown
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Expected O, but got Unknown
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		TextObject val = new TextObject("{=jBGi3saG}New figurehead \"{FIGUREHEAD_NAME}\" unlocked!", (Dictionary<string, object>)null);
		val.SetTextVariable("FIGUREHEAD_NAME", ((PropertyObject)figurehead).Name);
		MBInformationManager.AddQuickInformation(val, 0, (BasicCharacterObject)null, (Equipment)null, "event:/ui/notification/quest_update");
		InformationManager.DisplayMessage(new InformationMessage(((object)val).ToString(), new Color(0f, 1f, 0f, 1f)));
		_lastFigureheadLootTime = CampaignTime.Now;
	}

	private MBReadOnlyList<Figurehead> GetAllFigureheads()
	{
		if (_allFigureheadsCache == null || Extensions.IsEmpty<Figurehead>((IEnumerable<Figurehead>)_allFigureheadsCache))
		{
			_allFigureheadsCache = MBObjectManager.Instance.GetObjectTypeList<Figurehead>();
		}
		return _allFigureheadsCache;
	}

	private void OnShipOwnerChanged(Ship ship, PartyBase oldOwner, ShipOwnerChangeDetail changeDetail)
	{
		if (!ship.CanEquipFigurehead)
		{
			return;
		}
		if (ship.Figurehead == null)
		{
			PartyBase owner = ship.Owner;
			if (owner == null || !owner.IsMobile || ship.Owner.MobileParty.LeaderHero == null || ship.Owner.MobileParty.ActualClan == Clan.PlayerClan)
			{
				return;
			}
			if (_aiLordSelectedFigureheads.TryGetValue(ship.Owner.MobileParty.LeaderHero, out var value))
			{
				ship.ChangeFigurehead(value);
				return;
			}
			List<(Figurehead, float)> list = new List<(Figurehead, float)>();
			foreach (Figurehead item in (List<Figurehead>)(object)GetAllFigureheads())
			{
				if (item.Culture == ship.Owner.MobileParty.LeaderHero.Culture)
				{
					list.Add((item, 0.2f));
				}
				else
				{
					list.Add((item, 0.1f));
				}
			}
			Figurehead val = MBRandom.ChooseWeighted<Figurehead>((IReadOnlyList<ValueTuple<Figurehead, float>>)list);
			ship.ChangeFigurehead(val);
			_aiLordSelectedFigureheads.Add(ship.Owner.MobileParty.LeaderHero, val);
			return;
		}
		PartyBase owner2 = ship.Owner;
		if (owner2 == null || !owner2.IsSettlement)
		{
			PartyBase owner3 = ship.Owner;
			if (owner3 == null || !owner3.IsMobile || ship.Owner.MobileParty.ActualClan != Clan.PlayerClan)
			{
				return;
			}
			object obj;
			if (oldOwner == null)
			{
				obj = null;
			}
			else
			{
				MobileParty mobileParty = oldOwner.MobileParty;
				obj = ((mobileParty != null) ? mobileParty.ActualClan : null);
			}
			if (obj == Clan.PlayerClan)
			{
				return;
			}
		}
		ship.ChangeFigurehead((Figurehead)null);
	}
}
