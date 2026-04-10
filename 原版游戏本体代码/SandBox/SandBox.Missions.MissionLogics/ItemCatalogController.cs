using System;
using System.Collections.Generic;
using System.IO;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace SandBox.Missions.MissionLogics;

public class ItemCatalogController : MissionLogic
{
	public delegate void BeforeCatalogTickDelegate(int currentItemIndex);

	private Campaign _campaign;

	private Game _game;

	private Agent _playerAgent;

	private int curItemIndex = 1;

	private Timer timer;

	public MBReadOnlyList<ItemObject> AllItems { get; private set; }

	public event BeforeCatalogTickDelegate BeforeCatalogTick;

	public event Action AfterCatalogTick;

	public ItemCatalogController()
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Expected O, but got Unknown
		_campaign = Campaign.Current;
		_game = Game.Current;
		timer = new Timer(((MissionBehavior)this).Mission.CurrentTime, 1f, true);
	}

	public override void AfterStart()
	{
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Expected O, but got Unknown
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_018e: Expected O, but got Unknown
		//IL_01c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		((MissionBehavior)this).AfterStart();
		((MissionBehavior)this).Mission.SetMissionMode((MissionMode)2, true);
		AllItems = Game.Current.ObjectManager.GetObjectTypeList<ItemObject>();
		if (!_campaign.IsSinglePlayerReferencesInitialized)
		{
			_campaign.InitializeSinglePlayerReferences();
		}
		CharacterObject playerCharacter = CharacterObject.PlayerCharacter;
		MobileParty.MainParty.MemberRoster.AddToCounts(playerCharacter, 1, false, 0, 0, true, -1);
		if (!Extensions.IsEmpty<Team>((IEnumerable<Team>)((MissionBehavior)this).Mission.Teams))
		{
			throw new MBIllegalValueException("Number of teams is not 0.");
		}
		((MissionBehavior)this).Mission.Teams.Add((BattleSideEnum)0, 4284776512u, uint.MaxValue, (Banner)null, true, false, true);
		((MissionBehavior)this).Mission.Teams.Add((BattleSideEnum)1, 4281877080u, uint.MaxValue, (Banner)null, true, false, true);
		((MissionBehavior)this).Mission.PlayerTeam = ((MissionBehavior)this).Mission.AttackerTeam;
		EquipmentElement val = ((BasicCharacterObject)playerCharacter).Equipment[0];
		EquipmentElement val2 = ((BasicCharacterObject)playerCharacter).Equipment[1];
		EquipmentElement val3 = ((BasicCharacterObject)playerCharacter).Equipment[2];
		EquipmentElement val4 = ((BasicCharacterObject)playerCharacter).Equipment[3];
		EquipmentElement val5 = ((BasicCharacterObject)playerCharacter).Equipment[4];
		((BasicCharacterObject)playerCharacter).Equipment[0] = val;
		((BasicCharacterObject)playerCharacter).Equipment[1] = val2;
		((BasicCharacterObject)playerCharacter).Equipment[2] = val3;
		((BasicCharacterObject)playerCharacter).Equipment[3] = val4;
		((BasicCharacterObject)playerCharacter).Equipment[4] = val5;
		ItemObject item = ((List<ItemObject>)(object)AllItems)[0];
		Equipment val6 = new Equipment();
		val6.AddEquipmentToSlotWithoutAgent(GetEquipmentIndexOfItem(item), new EquipmentElement(((List<ItemObject>)(object)AllItems)[0], (ItemModifier)null, (ItemObject)null, false));
		AgentBuildData val7 = new AgentBuildData((BasicCharacterObject)(object)playerCharacter);
		val7.Equipment(val6);
		Mission mission = ((MissionBehavior)this).Mission;
		AgentBuildData obj = val7.Team(((MissionBehavior)this).Mission.AttackerTeam);
		Vec3 val8 = new Vec3(15f, 12f, 1f, -1f);
		_playerAgent = mission.SpawnAgent(obj.InitialPosition(ref val8).InitialDirection(ref Vec2.Forward).Controller((AgentControllerType)2), false);
		_playerAgent.WieldInitialWeapons((WeaponWieldActionType)2, (InitialWeaponEquipPreference)0);
		_playerAgent.Health = 10000f;
	}

	private EquipmentIndex GetEquipmentIndexOfItem(ItemObject item)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Expected I4, but got Unknown
		if (Extensions.HasAnyFlag<ItemFlags>(item.ItemFlags, (ItemFlags)12288))
		{
			return (EquipmentIndex)4;
		}
		ItemTypeEnum itemType = item.ItemType;
		switch (itemType - 1)
		{
		case 0:
			return (EquipmentIndex)10;
		case 1:
		case 2:
		case 3:
		case 4:
		case 5:
		case 6:
		case 7:
		case 8:
		case 9:
		case 10:
		case 11:
		case 17:
		case 18:
		case 19:
			return (EquipmentIndex)0;
		case 13:
			return (EquipmentIndex)5;
		case 14:
			return (EquipmentIndex)6;
		case 15:
			return (EquipmentIndex)7;
		case 16:
			return (EquipmentIndex)8;
		case 20:
			return (EquipmentIndex)10;
		case 24:
			return (EquipmentIndex)11;
		case 23:
			return (EquipmentIndex)9;
		default:
			Debug.FailedAssert("false", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\SandBox\\Missions\\MissionLogics\\ItemCatalogController.cs", "GetEquipmentIndexOfItem", 138);
			return (EquipmentIndex)(-1);
		}
	}

	public override void OnMissionTick(float dt)
	{
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		//IL_019d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Expected O, but got Unknown
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01af: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cb: Expected O, but got Unknown
		//IL_0201: Unknown result type (might be due to invalid IL or missing references)
		//IL_0206: Unknown result type (might be due to invalid IL or missing references)
		((MissionBehavior)this).OnMissionTick(dt);
		if (timer.Check(((MissionBehavior)this).Mission.CurrentTime))
		{
			if (!Directory.Exists("ItemCatalog"))
			{
				Directory.CreateDirectory("ItemCatalog");
			}
			this.BeforeCatalogTick?.Invoke(curItemIndex);
			timer.Reset(((MissionBehavior)this).Mission.CurrentTime);
			MatrixFrame val = new MatrixFrame
			{
				origin = new Vec3(10000f, 10000f, 10000f, -1f),
				rotation = Mat3.Identity
			};
			_playerAgent.AgentVisuals.SetFrame(ref val);
			_playerAgent.TeleportToPosition(val.origin);
			Blow val2 = default(Blow);
			((Blow)(ref val2))._002Ector(_playerAgent.Index);
			val2.DamageType = (DamageTypes)2;
			val2.BaseMagnitude = 1E+09f;
			val2.GlobalPosition = _playerAgent.Position;
			_playerAgent.Die(val2, (KillInfo)20);
			_playerAgent = null;
			Blow val4 = default(Blow);
			for (int num = ((List<Agent>)(object)((MissionBehavior)this).Mission.Agents).Count - 1; num >= 0; num--)
			{
				Agent val3 = ((List<Agent>)(object)((MissionBehavior)this).Mission.Agents)[num];
				((Blow)(ref val4))._002Ector(val3.Index);
				val4.DamageType = (DamageTypes)2;
				val4.BaseMagnitude = 1E+09f;
				val4.GlobalPosition = val3.Position;
				Blow val5 = val4;
				val3.TeleportToPosition(val.origin);
				val3.Die(val5, (KillInfo)20);
			}
			ItemObject val6 = ((List<ItemObject>)(object)AllItems)[curItemIndex];
			Equipment val7 = new Equipment();
			val7.AddEquipmentToSlotWithoutAgent(GetEquipmentIndexOfItem(val6), new EquipmentElement(val6, (ItemModifier)null, (ItemObject)null, false));
			AgentBuildData val8 = new AgentBuildData(_game.PlayerTroop);
			val8.Equipment(val7);
			Mission mission = ((MissionBehavior)this).Mission;
			AgentBuildData obj = val8.Team(((MissionBehavior)this).Mission.AttackerTeam);
			Vec3 val9 = new Vec3(15f, 12f, 1f, -1f);
			_playerAgent = mission.SpawnAgent(obj.InitialPosition(ref val9).InitialDirection(ref Vec2.Forward).Controller((AgentControllerType)2), false);
			_playerAgent.WieldInitialWeapons((WeaponWieldActionType)2, (InitialWeaponEquipPreference)0);
			_playerAgent.Health = 10000f;
			this.AfterCatalogTick?.Invoke();
			curItemIndex++;
		}
	}
}
