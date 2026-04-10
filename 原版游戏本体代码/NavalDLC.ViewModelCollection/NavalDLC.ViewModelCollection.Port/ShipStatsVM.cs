using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TaleWorlds.CampaignSystem.Naval;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;

namespace NavalDLC.ViewModelCollection.Port;

public class ShipStatsVM : ViewModel
{
	private readonly Ship _ship;

	private MBBindingList<ShipStatVM> _statList;

	[DataSourceProperty]
	public MBBindingList<ShipStatVM> StatList
	{
		get
		{
			return _statList;
		}
		set
		{
			if (value != _statList)
			{
				_statList = value;
				((ViewModel)this).OnPropertyChangedWithValue<MBBindingList<ShipStatVM>>(value, "StatList");
			}
		}
	}

	public ShipStatsVM(Ship ship)
	{
		_ship = ship;
		StatList = new MBBindingList<ShipStatVM>();
		RefreshStats(_ship.HitPoints, null);
		((ViewModel)this).RefreshValues();
	}

	public override void RefreshValues()
	{
		((ViewModel)this).RefreshValues();
		StatList.ApplyActionOnAllItems((Action<ShipStatVM>)delegate(ShipStatVM s)
		{
			((ViewModel)s).RefreshValues();
		});
	}

	public void RefreshStats(float currentHp, MBReadOnlyList<(string, ShipUpgradePiece)> newlySelectedPieces)
	{
		//IL_0274: Unknown result type (might be due to invalid IL or missing references)
		//IL_029a: Expected O, but got Unknown
		//IL_02b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cd: Expected O, but got Unknown
		//IL_02e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0317: Expected O, but got Unknown
		//IL_032d: Unknown result type (might be due to invalid IL or missing references)
		//IL_035e: Expected O, but got Unknown
		//IL_0374: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b7: Expected O, but got Unknown
		//IL_03cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0403: Expected O, but got Unknown
		//IL_0419: Unknown result type (might be due to invalid IL or missing references)
		//IL_0431: Expected O, but got Unknown
		//IL_0447: Unknown result type (might be due to invalid IL or missing references)
		//IL_0464: Expected O, but got Unknown
		//IL_047a: Unknown result type (might be due to invalid IL or missing references)
		//IL_04a8: Expected O, but got Unknown
		//IL_04be: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e8: Expected O, but got Unknown
		((Collection<ShipStatVM>)(object)StatList).Clear();
		MissionShipObject val = MBObjectManager.Instance.GetObject<MissionShipObject>(_ship.ShipHull.MissionShipObjectId);
		if (val == null)
		{
			Debug.FailedAssert("Failed to find mission ship object with id: " + _ship.ShipHull.MissionShipObjectId, "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\NavalDLC.ViewModelCollection\\Port\\ShipStatsVM.cs", "RefreshStats", 40);
			return;
		}
		MBList<ShipUpgradePiece> val2 = new MBList<ShipUpgradePiece>();
		foreach (KeyValuePair<string, ShipSlot> availableSlot in _ship.ShipHull.AvailableSlots)
		{
			((List<ShipUpgradePiece>)(object)val2).Add(_ship.GetPieceAtSlot(availableSlot.Key));
		}
		float num = 1f;
		float num2 = 1f;
		float num3 = 1f;
		float num4 = 1f;
		float num5 = 1f;
		for (int i = 0; i < ((List<ShipUpgradePiece>)(object)val2).Count; i++)
		{
			ShipUpgradePiece val3 = ((List<ShipUpgradePiece>)(object)val2)[i];
			if (val3 != null)
			{
				num += val3.CampaignSpeedBonusMultiplier;
				num2 += val3.MaxHitPointsBonusMultiplier;
				num3 += val3.InventoryCapacityBonusMultiplier;
				num4 += val3.ShipWeightBonusMultiplier;
				num5 += val3.CrewCapacityBonusMultiplier;
			}
		}
		float num6 = 0f;
		float num7 = 0f;
		float num8 = 0f;
		float num9 = 0f;
		float num10 = 0f;
		int num11 = 0;
		if (newlySelectedPieces != null && ((List<(string, ShipUpgradePiece)>)(object)newlySelectedPieces).Count > 0)
		{
			for (int j = 0; j < ((List<(string, ShipUpgradePiece)>)(object)newlySelectedPieces).Count; j++)
			{
				string item = ((List<(string, ShipUpgradePiece)>)(object)newlySelectedPieces)[j].Item1;
				ShipUpgradePiece item2 = ((List<(string, ShipUpgradePiece)>)(object)newlySelectedPieces)[j].Item2;
				if (item2 != null)
				{
					num6 += item2.CampaignSpeedBonusMultiplier;
					num7 += item2.MaxHitPointsBonusMultiplier;
					num8 += item2.InventoryCapacityBonusMultiplier;
					num9 += item2.ShipWeightBonusMultiplier;
					num10 += item2.CrewCapacityBonusMultiplier;
					num11 += item2.SeaWorthinessBonus;
				}
				ShipUpgradePiece pieceAtSlot = _ship.GetPieceAtSlot(item);
				if (pieceAtSlot != null)
				{
					num6 -= pieceAtSlot.CampaignSpeedBonusMultiplier;
					num7 -= pieceAtSlot.MaxHitPointsBonusMultiplier;
					num8 -= pieceAtSlot.InventoryCapacityBonusMultiplier;
					num9 -= pieceAtSlot.ShipWeightBonusMultiplier;
					num10 -= pieceAtSlot.CrewCapacityBonusMultiplier;
					num11 -= pieceAtSlot.SeaWorthinessBonus;
				}
			}
		}
		num6 /= num;
		num7 /= num2;
		num8 /= num3;
		num9 /= num4;
		num10 /= num5;
		((Collection<ShipStatVM>)(object)StatList).Add(new ShipStatVM("hull", new TextObject("{=wEmx6fZi}Hull", (Dictionary<string, object>)null), ((object)_ship.ShipHull.Name).ToString()));
		((Collection<ShipStatVM>)(object)StatList).Add(new ShipStatVM("class", new TextObject("{=sqdzHOPe}Class", (Dictionary<string, object>)null), GetClassStr(_ship)));
		((Collection<ShipStatVM>)(object)StatList).Add(new ShipStatVM("crew", new TextObject("{=wXCM8BnW}Crew", (Dictionary<string, object>)null), GetCrewCapacityStr(_ship), GetBonusStr(num10, isPercentage: true), num10 > 0f, () => GetCrewCapacityTooltip(_ship)));
		((Collection<ShipStatVM>)(object)StatList).Add(new ShipStatVM("cargo_capacity", new TextObject("{=IE1KbkaH}Cargo Capacity", (Dictionary<string, object>)null), _ship.InventoryCapacity.ToString(), GetBonusStr(num8, isPercentage: true), num8 > 0f));
		((Collection<ShipStatVM>)(object)StatList).Add(new ShipStatVM("weight", new TextObject("{=4Dd2xgPm}Weight", (Dictionary<string, object>)null), (val.Mass * (1f + _ship.ShipWeightFactor)).ToString("0"), GetBonusStr(num9, isPercentage: true), num9 < 0f));
		((Collection<ShipStatVM>)(object)StatList).Add(new ShipStatVM("travel_speed", new TextObject("{=DbERaPfF}Travel Speed", (Dictionary<string, object>)null), _ship.GetCampaignSpeed().ToString("0.##"), GetBonusStr(num6, isPercentage: true), num6 > 0f));
		((Collection<ShipStatVM>)(object)StatList).Add(new ShipStatVM("sail_type", new TextObject("{=PJyFY05L}Sail", (Dictionary<string, object>)null), GetSailTypeStr(val)));
		((Collection<ShipStatVM>)(object)StatList).Add(new ShipStatVM("draft_type", new TextObject("{=I4bu7cLr}Draft", (Dictionary<string, object>)null), GetDraftTypeStr(_ship)));
		((Collection<ShipStatVM>)(object)StatList).Add(new ShipStatVM("sea_worthiness", new TextObject("{=yCzuXN3O}Seaworthiness", (Dictionary<string, object>)null), _ship.SeaWorthiness.ToString(), GetBonusStr(num11, isPercentage: false), num11 > 0));
		((Collection<ShipStatVM>)(object)StatList).Add(new ShipStatVM("hit_points", new TextObject("{=oBbiVeKE}Hit Points", (Dictionary<string, object>)null), GetHitPointsStr(_ship, currentHp), GetBonusStr(num7, isPercentage: true), num7 > 0f));
	}

	private string GetBonusStr(float bonus, bool isPercentage)
	{
		if (MathF.Abs(bonus) < 0.001f)
		{
			return string.Empty;
		}
		if (isPercentage)
		{
			string text = ((object)GameTexts.FindText("str_NUMBER_percent", (string)null).SetTextVariable("NUMBER", (bonus * 100f).ToString("+#;-#"))).ToString();
			return ((object)GameTexts.FindText("str_STR_in_parentheses", (string)null).SetTextVariable("STR", text)).ToString();
		}
		return ((object)GameTexts.FindText("str_STR_in_parentheses", (string)null).SetTextVariable("STR", bonus.ToString("+#;-#"))).ToString();
	}

	private string GetClassStr(Ship ship)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		return ((object)GameTexts.FindText("str_ship_type", ((object)ship.ShipHull.Type/*cast due to .constrained prefix*/).ToString().ToLowerInvariant())).ToString();
	}

	private string GetCrewCapacityStr(Ship ship)
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Expected O, but got Unknown
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Expected O, but got Unknown
		int skeletalCrewCapacity = ship.SkeletalCrewCapacity;
		int mainDeckCrewCapacity = ship.MainDeckCrewCapacity;
		int num = ship.TotalCrewCapacity - ship.MainDeckCrewCapacity;
		TextObject val = ((num <= 0) ? new TextObject("{=!}{SKELETAL} • {DECK}", (Dictionary<string, object>)null) : new TextObject("{=!}{SKELETAL} • {DECK} + {RESERVE}", (Dictionary<string, object>)null));
		return ((object)val.SetTextVariable("SKELETAL", skeletalCrewCapacity).SetTextVariable("DECK", mainDeckCrewCapacity).SetTextVariable("RESERVE", num)).ToString();
	}

	private List<TooltipProperty> GetCrewCapacityTooltip(Ship ship)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Expected O, but got Unknown
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Expected O, but got Unknown
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Expected O, but got Unknown
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Expected O, but got Unknown
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Expected O, but got Unknown
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Expected O, but got Unknown
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Expected O, but got Unknown
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Expected O, but got Unknown
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Expected O, but got Unknown
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Expected O, but got Unknown
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Expected O, but got Unknown
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Expected O, but got Unknown
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Expected O, but got Unknown
		//IL_016d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0177: Expected O, but got Unknown
		//IL_0194: Unknown result type (might be due to invalid IL or missing references)
		//IL_019e: Expected O, but got Unknown
		List<TooltipProperty> list = new List<TooltipProperty>();
		int skeletalCrewCapacity = ship.SkeletalCrewCapacity;
		int mainDeckCrewCapacity = ship.MainDeckCrewCapacity;
		int totalCrewCapacity = ship.TotalCrewCapacity;
		int num = totalCrewCapacity - mainDeckCrewCapacity;
		list.Add(new TooltipProperty(((object)new TextObject("{=kalMphFt}Skeletal Capacity", (Dictionary<string, object>)null)).ToString(), skeletalCrewCapacity.ToString(), 0, false, (TooltipPropertyFlags)0));
		list.Add(new TooltipProperty(string.Empty, ((object)GameTexts.FindText("str_ship_stat_explanation", "crewskeletal")).ToString(), -1, false, (TooltipPropertyFlags)1));
		list.Add(new TooltipProperty(string.Empty, string.Empty, 0, false, (TooltipPropertyFlags)1024));
		list.Add(new TooltipProperty(((object)new TextObject("{=Bt82dbKu}Deck Capacity", (Dictionary<string, object>)null)).ToString(), mainDeckCrewCapacity.ToString(), 0, false, (TooltipPropertyFlags)0));
		list.Add(new TooltipProperty(string.Empty, ((object)GameTexts.FindText("str_ship_stat_explanation", "crewdeck")).ToString(), -1, false, (TooltipPropertyFlags)1));
		list.Add(new TooltipProperty(string.Empty, string.Empty, 0, false, (TooltipPropertyFlags)0));
		list.Add(new TooltipProperty(((object)new TextObject("{=HThruy9f}Reserve Capacity", (Dictionary<string, object>)null)).ToString(), num.ToString(), 0, false, (TooltipPropertyFlags)0));
		list.Add(new TooltipProperty(string.Empty, ((object)GameTexts.FindText("str_ship_stat_explanation", "crewreserve")).ToString(), -1, false, (TooltipPropertyFlags)1));
		list.Add(new TooltipProperty(string.Empty, string.Empty, 0, false, (TooltipPropertyFlags)512));
		list.Add(new TooltipProperty(((object)new TextObject("{=kLvWPxIK}Total Capacity", (Dictionary<string, object>)null)).ToString(), totalCrewCapacity.ToString(), 0, false, (TooltipPropertyFlags)0));
		list.Add(new TooltipProperty(string.Empty, ((object)GameTexts.FindText("str_ship_stat_explanation", "crewtotal")).ToString(), -1, false, (TooltipPropertyFlags)1));
		return list;
	}

	private string GetSailTypeStr(MissionShipObject missionShipObject)
	{
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Expected O, but got Unknown
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Expected O, but got Unknown
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Expected O, but got Unknown
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Expected O, but got Unknown
		if (missionShipObject.HasSails)
		{
			bool flag = ((IEnumerable<ShipSail>)missionShipObject.Sails).Any((ShipSail x) => (int)x.Type == 1);
			bool flag2 = ((IEnumerable<ShipSail>)missionShipObject.Sails).Any((ShipSail x) => (int)x.Type == 0);
			if (flag && flag2)
			{
				return ((object)new TextObject("{=bXJLb0BE}Hybrid", (Dictionary<string, object>)null)).ToString();
			}
			if (flag)
			{
				return ((object)new TextObject("{=kNxD2oer}Lateen", (Dictionary<string, object>)null)).ToString();
			}
			if (flag2)
			{
				return ((object)new TextObject("{=squareSail}Square", (Dictionary<string, object>)null)).ToString();
			}
		}
		return ((object)new TextObject("{=koX9okuG}None", (Dictionary<string, object>)null)).ToString();
	}

	private string GetDraftTypeStr(Ship ship)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Expected O, but got Unknown
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Expected O, but got Unknown
		if (!ship.ShipHull.HasHold)
		{
			return ((object)new TextObject("{=ShipDraftTypeShallow}Shallow", (Dictionary<string, object>)null)).ToString();
		}
		return ((object)new TextObject("{=ShipDraftTypeDeep}Deep", (Dictionary<string, object>)null)).ToString();
	}

	private string GetHitPointsStr(Ship ship, float currentHp)
	{
		return ((object)GameTexts.FindText("str_LEFT_over_RIGHT_no_space", (string)null).SetTextVariable("LEFT", currentHp.ToString("0")).SetTextVariable("RIGHT", ship.MaxHitPoints.ToString("0"))).ToString();
	}
}
