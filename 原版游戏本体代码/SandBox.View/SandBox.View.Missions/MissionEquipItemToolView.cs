using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.AgentOrigins;
using TaleWorlds.Core;
using TaleWorlds.DotNet;
using TaleWorlds.Engine;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.ModuleManager;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TaleWorlds.ObjectSystem;

namespace SandBox.View.Missions;

public class MissionEquipItemToolView : MissionView
{
	private enum Filter
	{
		Head = 5,
		Cape = 9,
		Body = 6,
		Hand = 8,
		Leg = 7,
		Shield = 12,
		Bow = 13,
		Crossbow = 15,
		Horse = 10,
		Onehanded = 1,
		Twohanded = 2,
		Polearm = 3,
		Thrown = 4,
		Arrow = 14,
		Bolt = 16,
		Harness = 11
	}

	private delegate void MainThreadDelegate();

	private class ItemData
	{
		public GameEntity Entity;

		public string Name;

		public string Id;

		public BasicCultureObject Culture;

		public ItemTypeEnum itemType;

		public GenderEnum Gender;
	}

	public enum GenderEnum
	{
		Male = 1,
		Unisex,
		Female
	}

	private string str = "";

	private int _itemCulture;

	private bool[] _filters = new bool[17];

	private bool _genderSet;

	private Agent _mainAgent;

	private List<ItemObject> _allItemObjects = new List<ItemObject>();

	private List<ItemData> _allItems = new List<ItemData>();

	private List<ItemData> _currentItems = new List<ItemData>();

	private List<Tuple<int, int, int, int>> _currentArmorValues = new List<Tuple<int, int, int, int>>();

	private List<CultureObject> _allFactions = new List<CultureObject>();

	private List<CharacterObject> _allCharacters = new List<CharacterObject>();

	private List<FormationClass> _groups = new List<FormationClass>();

	private int _activeIndex = -1;

	private int _factionIndex;

	private int _groupIndex;

	private XmlDocument _charactersXml;

	private List<XmlDocument> _itemsXmls;

	private string[] _attributes = new string[6] { "id", "name", "level", "occupation", "culture", "group" };

	private string[] _spawnAttributes = new string[6] { "id", "name", "level", "occupation", "culture", "group" };

	private bool underscoreGuard;

	private bool yGuard;

	private bool zGuard;

	private bool xGuard;

	private bool _capsLock;

	private List<ItemObject> _activeItems = new List<ItemObject>();

	private int _setIndex;

	private int _spawnSetIndex;

	private Camera _cam;

	private bool _init = true;

	private int _index;

	private float _diff = 0.75f;

	private int _activeFilter;

	private int _activeWeaponSlot;

	private Vec3 _textStart;

	private List<BoundingBox> _bounds = new List<BoundingBox>();

	private float _pivotDiff;

	private Agent _mountAgent;

	private ItemObject _horse;

	private ItemObject _harness;

	public override void AfterStart()
	{
		_itemsXmls = new List<XmlDocument>();
		string text = ModuleHelper.GetModuleFullPath("SandBoxCore") + "ModuleData/items/";
		FileInfo[] files = new DirectoryInfo(text).GetFiles("*.xml");
		foreach (FileInfo fileInfo in files)
		{
			_itemsXmls.Add(LoadXmlFile(text + fileInfo.Name));
		}
		_cam = Camera.CreateCamera();
		GetItems("Item");
		GetItems("CraftedItem");
		foreach (Kingdom item in ((IEnumerable<Kingdom>)Game.Current.ObjectManager.GetObjectTypeList<Kingdom>()).ToList())
		{
			if (((IFaction)item).IsKingdomFaction || ((object)item.Name).ToString() == "Looters")
			{
				_allFactions.Add(item.Culture);
			}
		}
		foreach (Clan item2 in ((IEnumerable<Clan>)Game.Current.ObjectManager.GetObjectTypeList<Clan>()).ToList())
		{
			if (((object)item2.Name).ToString() == "Looters")
			{
				_allFactions.Add(item2.Culture);
			}
		}
		_groups.Add((FormationClass)0);
		_groups.Add((FormationClass)1);
		_groups.Add((FormationClass)2);
		_groups.Add((FormationClass)3);
		_groups.Add((FormationClass)4);
		_groups.Add((FormationClass)5);
		_groups.Add((FormationClass)6);
		_groups.Add((FormationClass)7);
		_allCharacters = ((IEnumerable<CharacterObject>)Game.Current.ObjectManager.GetObjectTypeList<CharacterObject>()).ToList();
		SpawnAgent("guard");
		SpawnItems();
	}

	private void GetItems(string str)
	{
		List<ItemObject> list = ((IEnumerable<ItemObject>)Game.Current.ObjectManager.GetObjectTypeList<ItemObject>()).ToList();
		foreach (XmlDocument itemsXml in _itemsXmls)
		{
			foreach (XmlNode item in itemsXml.DocumentElement.SelectNodes(str))
			{
				if (item.Attributes == null || item.Attributes["id"] == null)
				{
					continue;
				}
				string innerText = item.Attributes["id"].InnerText;
				foreach (ItemObject item2 in list)
				{
					if (((MBObjectBase)item2).StringId == innerText)
					{
						_allItemObjects.Add(item2);
					}
				}
			}
		}
	}

	public override void OnMissionTick(float dt)
	{
		//IL_0f7b: Unknown result type (might be due to invalid IL or missing references)
		//IL_137b: Unknown result type (might be due to invalid IL or missing references)
		//IL_1380: Unknown result type (might be due to invalid IL or missing references)
		//IL_13a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_13a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_1246: Unknown result type (might be due to invalid IL or missing references)
		//IL_124a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0812: Unknown result type (might be due to invalid IL or missing references)
		//IL_0814: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d3f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d44: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d54: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d59: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d62: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d67: Unknown result type (might be due to invalid IL or missing references)
		//IL_0837: Unknown result type (might be due to invalid IL or missing references)
		//IL_0839: Unknown result type (might be due to invalid IL or missing references)
		//IL_083e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0818: Unknown result type (might be due to invalid IL or missing references)
		//IL_081b: Invalid comparison between Unknown and I4
		//IL_1273: Unknown result type (might be due to invalid IL or missing references)
		//IL_1277: Unknown result type (might be due to invalid IL or missing references)
		//IL_1278: Unknown result type (might be due to invalid IL or missing references)
		//IL_125d: Unknown result type (might be due to invalid IL or missing references)
		//IL_1261: Unknown result type (might be due to invalid IL or missing references)
		//IL_1262: Unknown result type (might be due to invalid IL or missing references)
		//IL_0867: Unknown result type (might be due to invalid IL or missing references)
		//IL_086b: Invalid comparison between Unknown and I4
		//IL_084f: Unknown result type (might be due to invalid IL or missing references)
		//IL_081d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0820: Invalid comparison between Unknown and I4
		//IL_0898: Unknown result type (might be due to invalid IL or missing references)
		//IL_089c: Invalid comparison between Unknown and I4
		//IL_0822: Unknown result type (might be due to invalid IL or missing references)
		//IL_0825: Invalid comparison between Unknown and I4
		//IL_08d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_08de: Unknown result type (might be due to invalid IL or missing references)
		//IL_08fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0900: Unknown result type (might be due to invalid IL or missing references)
		//IL_0905: Unknown result type (might be due to invalid IL or missing references)
		//IL_0921: Unknown result type (might be due to invalid IL or missing references)
		//IL_0927: Unknown result type (might be due to invalid IL or missing references)
		//IL_0929: Unknown result type (might be due to invalid IL or missing references)
		//IL_0933: Expected O, but got Unknown
		//IL_092e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0964: Unknown result type (might be due to invalid IL or missing references)
		//IL_0969: Unknown result type (might be due to invalid IL or missing references)
		//IL_0979: Unknown result type (might be due to invalid IL or missing references)
		//IL_097e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0827: Unknown result type (might be due to invalid IL or missing references)
		//IL_082a: Invalid comparison between Unknown and I4
		//IL_05c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_05c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_05e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_05eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_05f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_05ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_05cd: Invalid comparison between Unknown and I4
		//IL_0616: Unknown result type (might be due to invalid IL or missing references)
		//IL_061c: Invalid comparison between Unknown and I4
		//IL_0601: Unknown result type (might be due to invalid IL or missing references)
		//IL_05cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_05d2: Invalid comparison between Unknown and I4
		//IL_0628: Unknown result type (might be due to invalid IL or missing references)
		//IL_062f: Invalid comparison between Unknown and I4
		//IL_05d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_05d7: Invalid comparison between Unknown and I4
		//IL_0639: Unknown result type (might be due to invalid IL or missing references)
		//IL_05d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_05dc: Invalid comparison between Unknown and I4
		//IL_063d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0640: Invalid comparison between Unknown and I4
		//IL_0642: Unknown result type (might be due to invalid IL or missing references)
		//IL_0645: Invalid comparison between Unknown and I4
		//IL_0678: Unknown result type (might be due to invalid IL or missing references)
		//IL_0647: Unknown result type (might be due to invalid IL or missing references)
		//IL_064a: Invalid comparison between Unknown and I4
		//IL_0696: Unknown result type (might be due to invalid IL or missing references)
		//IL_069d: Unknown result type (might be due to invalid IL or missing references)
		//IL_06cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_06d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_06d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_06f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_06f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_06fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0704: Expected O, but got Unknown
		//IL_06ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0735: Unknown result type (might be due to invalid IL or missing references)
		//IL_073a: Unknown result type (might be due to invalid IL or missing references)
		//IL_074a: Unknown result type (might be due to invalid IL or missing references)
		//IL_074f: Unknown result type (might be due to invalid IL or missing references)
		//IL_064c: Unknown result type (might be due to invalid IL or missing references)
		//IL_064f: Invalid comparison between Unknown and I4
		//IL_036b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0370: Unknown result type (might be due to invalid IL or missing references)
		//IL_0379: Unknown result type (might be due to invalid IL or missing references)
		//IL_037e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0389: Unknown result type (might be due to invalid IL or missing references)
		//IL_038e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0393: Unknown result type (might be due to invalid IL or missing references)
		//IL_0419: Unknown result type (might be due to invalid IL or missing references)
		//IL_041e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0427: Unknown result type (might be due to invalid IL or missing references)
		//IL_042c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0437: Unknown result type (might be due to invalid IL or missing references)
		//IL_043c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0441: Unknown result type (might be due to invalid IL or missing references)
		OnEquipToolDebugTick(dt);
		if (_init)
		{
			_init = false;
			UpdateCamera();
		}
		if (_activeIndex == -1)
		{
			if (!((MissionBehavior)this).DebugInput.IsKeyDown((InputKey)29) && !((MissionBehavior)this).DebugInput.IsKeyDown((InputKey)157) && !((MissionBehavior)this).DebugInput.IsKeyDown((InputKey)42) && !((MissionBehavior)this).DebugInput.IsKeyDown((InputKey)54) && !((MissionBehavior)this).DebugInput.IsKeyDown((InputKey)56) && !((MissionBehavior)this).DebugInput.IsKeyDown((InputKey)184))
			{
				if (((MissionBehavior)this).DebugInput.IsKeyPressed((InputKey)2))
				{
					_activeIndex = 0;
				}
				else if (((MissionBehavior)this).DebugInput.IsKeyPressed((InputKey)3))
				{
					_activeIndex = 1;
				}
				else if (((MissionBehavior)this).DebugInput.IsKeyPressed((InputKey)4))
				{
					_activeIndex = 2;
				}
				else if (((MissionBehavior)this).DebugInput.IsKeyPressed((InputKey)5))
				{
					_activeIndex = 3;
				}
				else if (((MissionBehavior)this).DebugInput.IsKeyPressed((InputKey)8))
				{
					_activeWeaponSlot = 0;
				}
				else if (((MissionBehavior)this).DebugInput.IsKeyPressed((InputKey)9))
				{
					_activeWeaponSlot = 1;
				}
				else if (((MissionBehavior)this).DebugInput.IsKeyPressed((InputKey)10))
				{
					_activeWeaponSlot = 2;
				}
				else if (((MissionBehavior)this).DebugInput.IsKeyPressed((InputKey)11))
				{
					_activeWeaponSlot = 3;
				}
				else if (((MissionBehavior)this).DebugInput.IsKeyPressed((InputKey)80))
				{
					_factionIndex++;
					if (_factionIndex >= _allFactions.Count)
					{
						_factionIndex = 0;
					}
				}
				else if (((MissionBehavior)this).DebugInput.IsKeyPressed((InputKey)79))
				{
					_factionIndex--;
					if (_factionIndex < 0)
					{
						_factionIndex = _allFactions.Count - 1;
					}
				}
				else if (((MissionBehavior)this).DebugInput.IsKeyPressed((InputKey)76))
				{
					_groupIndex++;
					if (_groupIndex >= _groups.Count)
					{
						_groupIndex = 0;
					}
				}
				else if (((MissionBehavior)this).DebugInput.IsKeyPressed((InputKey)75))
				{
					_groupIndex--;
					if (_groupIndex < 0)
					{
						_groupIndex = _groups.Count - 1;
					}
				}
				else if (((MissionBehavior)this).DebugInput.IsKeyPressed((InputKey)72))
				{
					_setIndex++;
					if (_setIndex >= _mainAgent.Character.BattleEquipments.Count() + _mainAgent.Character.CivilianEquipments.Count() + 1)
					{
						_setIndex = 0;
					}
				}
				else if (((MissionBehavior)this).DebugInput.IsKeyPressed((InputKey)71))
				{
					_setIndex--;
					if (_setIndex < 0)
					{
						_setIndex = _mainAgent.Character.BattleEquipments.Count() + _mainAgent.Character.CivilianEquipments.Count() - 1;
					}
				}
				else if (((MissionBehavior)this).DebugInput.IsKeyPressed((InputKey)229) && _index > 0)
				{
					foreach (ItemData currentItem in _currentItems)
					{
						MatrixFrame frame = currentItem.Entity.GetFrame();
						ref Vec3 origin = ref frame.origin;
						origin += Vec3.Up * _diff;
						currentItem.Entity.SetFrame(ref frame, true);
					}
					_index--;
				}
				else if (((MissionBehavior)this).DebugInput.IsKeyPressed((InputKey)230) && _index < _currentItems.Count - 1)
				{
					foreach (ItemData currentItem2 in _currentItems)
					{
						MatrixFrame frame2 = currentItem2.Entity.GetFrame();
						ref Vec3 origin2 = ref frame2.origin;
						origin2 -= Vec3.Up * _diff;
						currentItem2.Entity.SetFrame(ref frame2, true);
					}
					_index++;
				}
				else if (((MissionBehavior)this).DebugInput.IsKeyPressed((InputKey)59))
				{
					if (!_genderSet)
					{
						_mainAgent.Character.IsFemale = false;
						_genderSet = true;
						SpawnAgent(_attributes[0]);
						SpawnItems();
					}
				}
				else if (((MissionBehavior)this).DebugInput.IsKeyPressed((InputKey)60) && !_genderSet)
				{
					_mainAgent.Character.IsFemale = true;
					_genderSet = true;
					SpawnAgent(_attributes[0]);
					SpawnItems();
				}
				MissionWeapon val2;
				if (((MissionBehavior)this).DebugInput.IsKeyPressed((InputKey)33))
				{
					if (_currentItems.Count > 0)
					{
						MissionWeapon val3 = default(MissionWeapon);
						foreach (ItemObject allItemObject in _allItemObjects)
						{
							if (!(((MBObjectBase)allItemObject).StringId == _currentItems[_index].Id))
							{
								continue;
							}
							int num = 0;
							num = ((_activeFilter != 5 && _activeFilter != 9 && _activeFilter != 6 && _activeFilter != 7 && _activeFilter != 8 && _activeFilter != 10 && _activeFilter != 11) ? _activeWeaponSlot : _activeFilter);
							EquipmentIndex val = (EquipmentIndex)num;
							if ((int)val == 0 || (int)val == 1 || (int)val == 2 || (int)val == 3 || (int)val == 4)
							{
								val2 = _mainAgent.Equipment[val];
								if (!((MissionWeapon)(ref val2)).IsEmpty)
								{
									_mainAgent.DropItem(val, (WeaponClass)0);
									((MissionBehavior)this).Mission.RemoveSpawnedItemsAndMissiles();
								}
							}
							if ((int)allItemObject.Type == 1)
							{
								_horse = allItemObject;
							}
							if ((int)allItemObject.Type == 25)
							{
								_harness = allItemObject;
							}
							if ((int)val == 0 || (int)val == 1 || (int)val == 2 || (int)val == 3 || (int)val == 4)
							{
								IAgentOriginBase origin3 = _mainAgent.Origin;
								((MissionWeapon)(ref val3))._002Ector(allItemObject, (ItemModifier)null, (origin3 != null) ? origin3.Banner : null);
								_mainAgent.EquipWeaponWithNewEntity(val, ref val3);
							}
							Equipment val4 = _mainAgent.SpawnEquipment.Clone(false);
							val4[val] = new EquipmentElement(allItemObject, (ItemModifier)null, (ItemObject)null, false);
							BasicCharacterObject character = _mainAgent.Character;
							SpawnHorse(_horse, _harness);
							Mat3 rotation = _mainAgent.Frame.rotation;
							_mainAgent.FadeOut(true, false);
							Mission mission = ((MissionBehavior)this).Mission;
							AgentBuildData obj = new AgentBuildData((IAgentOriginBase)new SimpleAgentOrigin(character, -1, (Banner)null, default(UniqueTroopDescriptor))).Equipment(val4).NoHorses(true).Team(((MissionBehavior)this).Mission.DefenderTeam);
							Vec3 val5 = new Vec3(500f, 200f, 1f, -1f);
							AgentBuildData obj2 = obj.InitialPosition(ref val5);
							Vec2 asVec = ((Vec3)(ref rotation.f)).AsVec2;
							_mainAgent = mission.SpawnAgent(obj2.InitialDirection(ref asVec), false);
							foreach (Agent item in (List<Agent>)(object)((MissionBehavior)this).Mission.Agents)
							{
								if (item != _mainAgent)
								{
									_mountAgent = item;
								}
							}
							UpdateCamera();
							UpdateActiveItems();
							break;
						}
					}
				}
				else
				{
					if (((MissionBehavior)this).DebugInput.IsKeyPressed((InputKey)14))
					{
						int num2 = _activeFilter;
						if (_activeFilter < 5 || _activeFilter > 11)
						{
							num2 = _activeWeaponSlot;
						}
						EquipmentIndex val6 = (EquipmentIndex)num2;
						if ((int)val6 == 0 || (int)val6 == 1 || (int)val6 == 2 || (int)val6 == 3 || (int)val6 == 4)
						{
							val2 = _mainAgent.Equipment[val6];
							if (!((MissionWeapon)(ref val2)).IsEmpty)
							{
								_mainAgent.DropItem(val6, (WeaponClass)0);
								((MissionBehavior)this).Mission.RemoveSpawnedItemsAndMissiles();
								goto IL_0992;
							}
						}
						if ((int)val6 == 10)
						{
							_mountAgent.FadeOut(true, false);
							_horse = null;
							SpawnHorse(_horse, _harness);
						}
						else if ((int)val6 == 11)
						{
							_mountAgent.FadeOut(true, false);
							_harness = null;
							SpawnHorse(_horse, _harness);
						}
						else
						{
							Equipment spawnEquipment = _mainAgent.SpawnEquipment;
							spawnEquipment[val6] = new EquipmentElement((ItemObject)null, (ItemModifier)null, (ItemObject)null, false);
							BasicCharacterObject character2 = _mainAgent.Character;
							Mat3 rotation2 = _mainAgent.Frame.rotation;
							_mainAgent.FadeOut(true, false);
							Mission mission2 = ((MissionBehavior)this).Mission;
							AgentBuildData obj3 = new AgentBuildData((IAgentOriginBase)new SimpleAgentOrigin(character2, -1, (Banner)null, default(UniqueTroopDescriptor))).Equipment(spawnEquipment).NoHorses(true).Team(((MissionBehavior)this).Mission.DefenderTeam);
							Vec3 val5 = new Vec3(500f, 200f, 1f, -1f);
							AgentBuildData obj4 = obj3.InitialPosition(ref val5);
							Vec2 asVec = ((Vec3)(ref rotation2.f)).AsVec2;
							_mainAgent = mission2.SpawnAgent(obj4.InitialDirection(ref asVec), false);
						}
						goto IL_0992;
					}
					if (((MissionBehavior)this).DebugInput.IsKeyPressed((InputKey)16))
					{
						_activeFilter = 5;
						Clear(_filters);
						_filters[_activeFilter] = true;
						SortFilter((ItemTypeEnum)14);
					}
					else if (((MissionBehavior)this).DebugInput.IsKeyPressed((InputKey)17))
					{
						_activeFilter = 9;
						Clear(_filters);
						_filters[_activeFilter] = true;
						SortFilter((ItemTypeEnum)24);
					}
					else if (((MissionBehavior)this).DebugInput.IsKeyPressed((InputKey)18))
					{
						_activeFilter = 6;
						Clear(_filters);
						_filters[_activeFilter] = true;
						SortFilter((ItemTypeEnum)15);
					}
					else if (((MissionBehavior)this).DebugInput.IsKeyPressed((InputKey)19))
					{
						_activeFilter = 8;
						Clear(_filters);
						_filters[_activeFilter] = true;
						SortFilter((ItemTypeEnum)17);
					}
					else if (((MissionBehavior)this).DebugInput.IsKeyPressed((InputKey)20))
					{
						_activeFilter = 7;
						Clear(_filters);
						_filters[_activeFilter] = true;
						SortFilter((ItemTypeEnum)16);
					}
					else if (((MissionBehavior)this).DebugInput.IsKeyPressed((InputKey)45))
					{
						_activeFilter = 12;
						Clear(_filters);
						_filters[_activeFilter] = true;
						SortFilter((ItemTypeEnum)8);
					}
					else if (((MissionBehavior)this).DebugInput.IsKeyPressed((InputKey)48))
					{
						_activeFilter = 13;
						Clear(_filters);
						_filters[_activeFilter] = true;
						SortFilter((ItemTypeEnum)9);
					}
					else if (((MissionBehavior)this).DebugInput.IsKeyPressed((InputKey)46))
					{
						_activeFilter = 15;
						Clear(_filters);
						_filters[_activeFilter] = true;
						SortFilter((ItemTypeEnum)10);
					}
					else if (((MissionBehavior)this).DebugInput.IsKeyPressed((InputKey)34))
					{
						_activeFilter = 10;
						Clear(_filters);
						_filters[_activeFilter] = true;
						SortFilter((ItemTypeEnum)1);
					}
				}
			}
			else if (((MissionBehavior)this).DebugInput.IsKeyDown((InputKey)29) || ((MissionBehavior)this).DebugInput.IsKeyDown((InputKey)157))
			{
				if (((MissionBehavior)this).DebugInput.IsKeyPressed((InputKey)31))
				{
					SaveToXml();
				}
				if (((MissionBehavior)this).DebugInput.IsKeyPressed((InputKey)24))
				{
					CheckForLoad();
				}
				if (((MissionBehavior)this).DebugInput.IsKeyPressed((InputKey)18))
				{
					EditNode();
				}
				if (((MissionBehavior)this).DebugInput.IsKeyPressed((InputKey)19))
				{
					SpawnAgent(_attributes[0]);
				}
				if (((MissionBehavior)this).DebugInput.IsKeyPressed((InputKey)79))
				{
					_itemCulture--;
					if (_itemCulture < -1)
					{
						_itemCulture = _allFactions.Count - 1;
					}
				}
				else if (((MissionBehavior)this).DebugInput.IsKeyPressed((InputKey)80))
				{
					_itemCulture++;
					if (_itemCulture >= _allFactions.Count)
					{
						_itemCulture = -1;
					}
				}
				else if (((MissionBehavior)this).DebugInput.IsKeyPressed((InputKey)229) || ((MissionBehavior)this).DebugInput.IsKeyPressed((InputKey)230))
				{
					float num3 = 30f;
					bool flag = ((MissionBehavior)this).DebugInput.IsKeyDown((InputKey)56);
					if (((MissionBehavior)this).DebugInput.IsKeyPressed((InputKey)230))
					{
						num3 *= -1f;
					}
					foreach (ItemData currentItem3 in _currentItems)
					{
						MatrixFrame frame3 = currentItem3.Entity.GetFrame();
						ref Mat3 rotation3 = ref frame3.rotation;
						MatrixFrame frame4 = currentItem3.Entity.GetFrame();
						MatrixFrame val7 = new MatrixFrame(ref rotation3, ref frame4.origin);
						if (!flag)
						{
							((Mat3)(ref val7.rotation)).RotateAboutUp(MathF.PI / 180f * num3);
						}
						currentItem3.Entity.SetFrame(ref val7, true);
					}
				}
			}
			else if (((MissionBehavior)this).DebugInput.IsKeyDown((InputKey)42) || ((MissionBehavior)this).DebugInput.IsKeyDown((InputKey)54))
			{
				if (((MissionBehavior)this).DebugInput.IsKeyPressed((InputKey)16))
				{
					_activeFilter = 1;
					Clear(_filters);
					_filters[_activeFilter] = true;
					SortFilter((ItemTypeEnum)2);
				}
				else if (((MissionBehavior)this).DebugInput.IsKeyPressed((InputKey)17))
				{
					_activeFilter = 2;
					Clear(_filters);
					_filters[_activeFilter] = true;
					SortFilter((ItemTypeEnum)3);
				}
				else if (((MissionBehavior)this).DebugInput.IsKeyPressed((InputKey)18))
				{
					_activeFilter = 3;
					Clear(_filters);
					_filters[_activeFilter] = true;
					SortFilter((ItemTypeEnum)4);
				}
				else if (((MissionBehavior)this).DebugInput.IsKeyPressed((InputKey)19))
				{
					_activeFilter = 4;
					Clear(_filters);
					_filters[_activeFilter] = true;
					SortFilter((ItemTypeEnum)12);
				}
				else if (((MissionBehavior)this).DebugInput.IsKeyPressed((InputKey)48))
				{
					_activeFilter = 14;
					Clear(_filters);
					_filters[_activeFilter] = true;
					SortFilter((ItemTypeEnum)5);
				}
				else if (((MissionBehavior)this).DebugInput.IsKeyPressed((InputKey)46))
				{
					_activeFilter = 16;
					Clear(_filters);
					_filters[_activeFilter] = true;
					SortFilter((ItemTypeEnum)6);
				}
				else if (((MissionBehavior)this).DebugInput.IsKeyPressed((InputKey)34))
				{
					_activeFilter = 11;
					Clear(_filters);
					_filters[_activeFilter] = true;
					SortFilter((ItemTypeEnum)25);
				}
			}
		}
		else
		{
			InputKey val8 = (InputKey)11;
			if (((MissionBehavior)this).DebugInput.IsKeyDown((InputKey)21) && !yGuard)
			{
				str += "y";
				yGuard = true;
			}
			if (((MissionBehavior)this).DebugInput.IsKeyDown((InputKey)44) && !zGuard)
			{
				str += "z";
				zGuard = true;
			}
			if (((MissionBehavior)this).DebugInput.IsKeyDown((InputKey)45) && !xGuard)
			{
				str += "x";
				xGuard = true;
			}
			if (((MissionBehavior)this).DebugInput.IsKeyPressed((InputKey)28))
			{
				_attributes[_activeIndex] = ((_activeIndex != 0) ? str : str.ToLower());
				_activeIndex = -1;
				str = "";
				underscoreGuard = false;
				yGuard = false;
				xGuard = false;
				zGuard = false;
			}
			if (((MissionBehavior)this).DebugInput.IsKeyPressed((InputKey)1))
			{
				_activeIndex = -1;
				str = "";
				underscoreGuard = false;
				yGuard = false;
				xGuard = false;
				zGuard = false;
			}
			if (((MissionBehavior)this).DebugInput.IsKeyPressed((InputKey)14) && str.Length > 0)
			{
				str = str.TrimEnd(new char[1] { str[str.Length - 1] });
				underscoreGuard = false;
				yGuard = false;
				xGuard = false;
				zGuard = false;
			}
			if (((MissionBehavior)this).DebugInput.IsKeyPressed((InputKey)57))
			{
				str += " ";
				underscoreGuard = false;
				yGuard = false;
				xGuard = false;
				zGuard = false;
			}
			if (((MissionBehavior)this).DebugInput.IsKeyPressed((InputKey)58))
			{
				_capsLock = !_capsLock;
				underscoreGuard = false;
				yGuard = false;
				xGuard = false;
				zGuard = false;
			}
			if (((MissionBehavior)this).DebugInput.IsKeyDown((InputKey)54) && ((MissionBehavior)this).DebugInput.IsKeyDown((InputKey)12) && !underscoreGuard)
			{
				str += "_";
				underscoreGuard = true;
			}
			if (((MissionBehavior)this).DebugInput.IsKeyDown((InputKey)29) && ((MissionBehavior)this).DebugInput.IsKeyPressed((InputKey)47))
			{
				string clipboardText = Input.GetClipboardText();
				str += clipboardText;
				underscoreGuard = false;
				yGuard = false;
				xGuard = false;
				zGuard = false;
				return;
			}
			for (int i = 0; i < 40; i++)
			{
				if (((MissionBehavior)this).DebugInput.IsKeyPressed((InputKey)(val8 + i)))
				{
					string text = (_capsLock ? ((object)(InputKey)(val8 + i)/*cast due to .constrained prefix*/).ToString().ToLower() : ((object)(InputKey)(val8 + i)/*cast due to .constrained prefix*/).ToString());
					text = ((text.ToLower() == "d" + i) ? text.ToLower().Replace("d", "") : text);
					str += text;
					underscoreGuard = false;
					yGuard = false;
					xGuard = false;
					zGuard = false;
				}
			}
		}
		goto IL_1307;
		IL_1307:
		if (((MissionBehavior)this).DebugInput.IsKeyDown((InputKey)56) && !((MissionBehavior)this).DebugInput.IsKeyDown((InputKey)29) && (((MissionBehavior)this).DebugInput.IsKeyPressed((InputKey)229) || ((MissionBehavior)this).DebugInput.IsKeyPressed((InputKey)230)))
		{
			float num4 = 60f;
			if (((MissionBehavior)this).DebugInput.IsKeyPressed((InputKey)230))
			{
				num4 *= -1f;
			}
			MatrixFrame frame5 = _mainAgent.Frame;
			((Mat3)(ref frame5.rotation)).RotateAboutUp(MathF.PI / 180f * num4);
			Agent mainAgent = _mainAgent;
			Vec2 asVec = ((Vec3)(ref frame5.origin)).AsVec2;
			mainAgent.SetTargetPositionAndDirection(ref asVec, ref frame5.rotation.f);
		}
		return;
		IL_0992:
		UpdateActiveItems();
		goto IL_1307;
	}

	private void OnEquipToolDebugTick(float dt)
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Invalid comparison between Unknown and I4
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Invalid comparison between Unknown and I4
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Invalid comparison between Unknown and I4
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Invalid comparison between Unknown and I4
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Invalid comparison between Unknown and I4
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Invalid comparison between Unknown and I4
		if (_genderSet)
		{
			_ = 10f + 70f + 15f + 120f;
			for (int i = 0; i < _currentItems.Count; i++)
			{
				_ = _index;
				ItemTypeEnum itemType = _currentItems[i].itemType;
			}
		}
	}

	private void SpawnItems()
	{
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		float num = 0f - (float)_allItemObjects.Count * _diff / 2f;
		_allItems.Clear();
		foreach (ItemObject allItemObject in _allItemObjects)
		{
			MatrixFrame frame = _mainAgent.Frame;
			ref Mat3 rotation = ref frame.rotation;
			Vec3 val = _mainAgent.Position + new Vec3(-1f, 1f, 0f, -1f) + Vec3.Up * num;
			MatrixFrame val2 = new MatrixFrame(ref rotation, ref val);
			GameEntity val3 = GameEntity.CreateEmpty(Mission.Current.Scene, true, true, true);
			MetaMesh copy = MetaMesh.GetCopy(allItemObject.MultiMeshName, true, false);
			val3.AddMultiMesh(copy, true);
			val3.SetFrame(ref val2, true);
			val3.SetVisibilityExcludeParents(false);
			ItemData itemData = new ItemData();
			itemData.Entity = val3;
			itemData.Name = ((object)allItemObject.Name).ToString();
			itemData.Id = ((MBObjectBase)allItemObject).StringId;
			itemData.Culture = allItemObject.Culture;
			if (Extensions.HasAnyFlag<ItemFlags>(allItemObject.ItemFlags, (ItemFlags)1024))
			{
				itemData.Gender = GenderEnum.Male;
			}
			else if (Extensions.HasAnyFlag<ItemFlags>(allItemObject.ItemFlags, (ItemFlags)2048))
			{
				itemData.Gender = GenderEnum.Female;
			}
			else
			{
				itemData.Gender = GenderEnum.Unisex;
			}
			itemData.itemType = allItemObject.ItemType;
			_allItems.Add(itemData);
			num += _diff;
		}
	}

	private void SortFilter(ItemTypeEnum type)
	{
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bd: Unknown result type (might be due to invalid IL or missing references)
		_currentItems.Clear();
		_currentArmorValues.Clear();
		foreach (ItemData item in _allItems)
		{
			if (item.itemType == type && (_itemCulture == -1 || (object)item.Culture == _allFactions[_itemCulture]))
			{
				int num = 0;
				bool flag = false;
				for (num = 0; num < _currentItems.Count; num++)
				{
					string text = _currentItems[num].Name.ToLower();
					for (int i = 0; i < item.Name.Length && i < text.Length; i++)
					{
						if (item.Name.ToLower()[i] < text[i])
						{
							flag = true;
							break;
						}
						if (item.Name.ToLower()[i] > text[i])
						{
							break;
						}
					}
					if (flag)
					{
						break;
					}
				}
				if (item.Gender == GenderEnum.Unisex || (_mainAgent.Character.IsFemale && item.Gender == GenderEnum.Female) || (!_mainAgent.Character.IsFemale && item.Gender == GenderEnum.Male))
				{
					_currentItems.Insert(num, item);
					ItemComponent itemComponent = _allItemObjects.Where((ItemObject val2) => ((MBObjectBase)val2).StringId == item.Id).FirstOrDefault().ItemComponent;
					ArmorComponent val = ((itemComponent == null || !(itemComponent is ArmorComponent)) ? ((ArmorComponent)null) : ((ArmorComponent)itemComponent));
					int item2 = 0;
					int item3 = 0;
					int item4 = 0;
					int item5 = 0;
					if (val != null)
					{
						item2 = val.HeadArmor;
						item3 = val.BodyArmor;
						item4 = val.LegArmor;
						item5 = val.ArmArmor;
					}
					Tuple<int, int, int, int> item6 = new Tuple<int, int, int, int>(item2, item3, item4, item5);
					_currentArmorValues.Insert(num, item6);
				}
			}
			else
			{
				item.Entity.SetVisibilityExcludeParents(false);
			}
		}
		PositionCurrentItems();
	}

	private void SpawnHorse(ItemObject horse, ItemObject harness)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		ItemRosterElement val = default(ItemRosterElement);
		ItemRosterElement val2 = default(ItemRosterElement);
		if (horse != null)
		{
			((ItemRosterElement)(ref val))._002Ector(horse, 1, (ItemModifier)null);
			if (harness != null)
			{
				((ItemRosterElement)(ref val2))._002Ector(harness, 1, (ItemModifier)null);
			}
			else
			{
				val2 = ItemRosterElement.Invalid;
			}
		}
		else
		{
			if (harness == null)
			{
				return;
			}
			((ItemRosterElement)(ref val))._002Ector(Game.Current.ObjectManager.GetObject<ItemObject>("mule"), 1, (ItemModifier)null);
			((ItemRosterElement)(ref val2))._002Ector(harness, 1, (ItemModifier)null);
		}
		if (_mountAgent != null)
		{
			_mountAgent.FadeOut(true, false);
		}
		EquipmentElement equipmentElement = ((ItemRosterElement)(ref val)).EquipmentElement;
		_horse = ((EquipmentElement)(ref equipmentElement)).Item;
		equipmentElement = ((ItemRosterElement)(ref val2)).EquipmentElement;
		_harness = ((EquipmentElement)(ref equipmentElement)).Item;
		Mission mission = ((MissionBehavior)this).Mission;
		ItemRosterElement val3 = val;
		ItemRosterElement val4 = val2;
		Vec3 val5 = new Vec3(500f, _mainAgent.Position.y - 3f, 1f, -1f);
		_mountAgent = mission.SpawnMonster(val3, val4, ref val5, ref Vec2.Forward, -1);
		_mountAgent.Controller = (AgentControllerType)0;
	}

	private void SpawnAgent(string id)
	{
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Expected O, but got Unknown
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		Agent mountAgent = _mountAgent;
		if (mountAgent != null)
		{
			mountAgent.FadeOut(true, false);
		}
		Agent mainAgent = _mainAgent;
		if (mainAgent != null)
		{
			mainAgent.FadeOut(true, false);
		}
		CharacterObject val = Game.Current.ObjectManager.GetObject<CharacterObject>(id);
		List<Equipment> list = ((BasicCharacterObject)val).BattleEquipments.ToList();
		list.AddRange(((BasicCharacterObject)val).CivilianEquipments.ToList());
		list.AddRange(val.StealthEquipments.ToList());
		Mission mission = ((MissionBehavior)this).Mission;
		AgentBuildData obj = new AgentBuildData((IAgentOriginBase)new SimpleAgentOrigin((BasicCharacterObject)(object)val, -1, (Banner)null, default(UniqueTroopDescriptor))).Equipment(list[_setIndex]).NoHorses(true).Team(((MissionBehavior)this).Mission.DefenderTeam);
		Vec3 val2 = new Vec3(500f, 200f, 1f, -1f);
		_mainAgent = mission.SpawnAgent(obj.InitialPosition(ref val2).InitialDirection(ref Vec2.Forward), false);
		EquipmentElement val3 = list[_setIndex].Horse;
		if (((EquipmentElement)(ref val3)).Item != null)
		{
			val3 = list[_setIndex].Horse;
			ItemObject item = ((EquipmentElement)(ref val3)).Item;
			val3 = list[_setIndex].GetEquipmentFromSlot((EquipmentIndex)11);
			SpawnHorse(item, ((EquipmentElement)(ref val3)).Item);
		}
		_groupIndex = ((BasicCharacterObject)val).DefaultFormationGroup;
		_attributes[0] = ((MBObjectBase)val).StringId;
		_attributes[1] = ((object)((BasicCharacterObject)val).Name).ToString();
		_attributes[2] = ((BasicCharacterObject)val).Level.ToString();
		_attributes[3] = ((object)val.Occupation/*cast due to .constrained prefix*/).ToString();
		for (int i = 0; i < _attributes.Length; i++)
		{
			_spawnAttributes[i] = _attributes[i];
		}
		_groupIndex = ((BasicCharacterObject)val).DefaultFormationGroup;
		for (int j = 0; j < _allFactions.Count; j++)
		{
			if (((MBObjectBase)_allFactions[j]).StringId == ((MBObjectBase)val.Culture).StringId)
			{
				_factionIndex = j;
				_itemCulture = -1;
				break;
			}
		}
		_spawnSetIndex = _setIndex;
		UpdateActiveItems();
		UpdateCamera();
	}

	private void PositionCurrentItems()
	{
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_0221: Unknown result type (might be due to invalid IL or missing references)
		//IL_0226: Unknown result type (might be due to invalid IL or missing references)
		//IL_022f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0231: Unknown result type (might be due to invalid IL or missing references)
		float num = 0f;
		float num2 = 0f;
		if (_activeFilter == 6)
		{
			_diff = 1.5f;
		}
		else if (_activeFilter == 10 || _activeFilter == 11)
		{
			_diff = 4f;
			num = -1f;
		}
		else if (_activeFilter == 1 || _activeFilter == 2 || _activeFilter == 14 || _activeFilter == 16)
		{
			_diff = 1.5f;
		}
		else if (_activeFilter == 13 || _activeFilter == 15)
		{
			_diff = 2.5f;
			num2 = 1f;
		}
		else
		{
			_diff = 0.75f;
		}
		float num3 = 0f - (float)(_currentItems.Count / 2) * _diff;
		_textStart = _mainAgent.Position + new Vec3(-1f, 1f, 0f, -1f) + Vec3.Up * num3;
		foreach (ItemData currentItem in _currentItems)
		{
			Mat3 identity = Mat3.Identity;
			Vec3 val = _mainAgent.Position + new Vec3(-1f, 1f + num, num2, -1f) + Vec3.Up * num3;
			MatrixFrame val2 = new MatrixFrame(ref identity, ref val);
			currentItem.Entity.SetVisibilityExcludeParents(true);
			currentItem.Entity.SetFrame(ref val2, true);
			num3 += _diff;
			if ((NativeObject)(object)currentItem.Entity.GetMetaMesh(0) != (NativeObject)null)
			{
				BoundingBox boundingBox = currentItem.Entity.GetMetaMesh(0).GetBoundingBox();
				if (!_bounds.Contains(boundingBox))
				{
					_bounds.Add(boundingBox);
				}
			}
		}
		_index = _currentItems.Count / 2;
		_pivotDiff = 0f;
		foreach (BoundingBox bound in _bounds)
		{
			_pivotDiff += bound.center.z;
		}
		_pivotDiff /= _bounds.Count;
		_bounds.Clear();
	}

	private void EditNode()
	{
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		_charactersXml = LoadXmlFile(ModuleHelper.GetModuleFullPath("SandBoxCore") + "ModuleData/spnpccharacters.xml");
		_charactersXml.DocumentElement.SelectNodes("NPCCharacter");
		bool flag = false;
		foreach (XmlNode item in _charactersXml.DocumentElement.SelectNodes("NPCCharacter"))
		{
			if (item.Attributes["id"] != null && _spawnAttributes[0] == item.Attributes["id"].InnerText)
			{
				item.Attributes["id"].InnerText = _attributes[0];
				item.Attributes["name"].InnerText = _attributes[1];
				if (item.Attributes["level"] != null)
				{
					item.Attributes["level"].InnerText = _attributes[2];
				}
				item.Attributes["occupation"].InnerText = _attributes[3];
				item.Attributes["culture"].InnerText = "Culture." + ((MBObjectBase)_allFactions[_factionIndex]).StringId;
				item.Attributes["default_group"].InnerText = FormationClassExtensions.GetName(_groups[_groupIndex]);
				SlotCheck("Head", 0, item);
				SlotCheck("Cape", 1, item);
				SlotCheck("Body", 2, item);
				SlotCheck("Gloves", 3, item);
				SlotCheck("Leg", 4, item);
				SlotCheck("Item0", 5, item);
				SlotCheck("Item1", 6, item);
				SlotCheck("Item2", 7, item);
				SlotCheck("Item3", 8, item);
				SlotCheck("Horse", -1, item, _horse);
				SlotCheck("HorseHarness", -1, item, _harness);
				_charactersXml.Save(ModuleHelper.GetModuleFullPath("SandBoxCore") + "ModuleData/spnpccharacters.xml");
				for (int i = 0; i < _attributes.Length; i++)
				{
					_spawnAttributes[i] = _attributes[i];
				}
				flag = true;
			}
		}
	}

	private void CheckForLoad()
	{
		if (_spawnAttributes[0] != _attributes[0] && Game.Current.ObjectManager.GetObject<CharacterObject>(_attributes[0]) != null)
		{
			SpawnAgent(_attributes[0]);
			return;
		}
		if (_spawnAttributes[1] != _attributes[1])
		{
			foreach (CharacterObject allCharacter in _allCharacters)
			{
				if (((object)((BasicCharacterObject)allCharacter).Name).ToString() == _attributes[1])
				{
					SpawnAgent(((MBObjectBase)allCharacter).StringId);
					return;
				}
			}
		}
		if (_setIndex != _spawnSetIndex)
		{
			SpawnAgent(((MBObjectBase)_mainAgent.Character).StringId);
		}
	}

	private void UpdateActiveItems()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_0190: Unknown result type (might be due to invalid IL or missing references)
		//IL_0195: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bb: Unknown result type (might be due to invalid IL or missing references)
		_activeItems.Clear();
		List<ItemObject> activeItems = _activeItems;
		EquipmentElement val = _mainAgent.SpawnEquipment[(EquipmentIndex)5];
		activeItems.Add(((EquipmentElement)(ref val)).Item);
		List<ItemObject> activeItems2 = _activeItems;
		val = _mainAgent.SpawnEquipment[(EquipmentIndex)9];
		activeItems2.Add(((EquipmentElement)(ref val)).Item);
		List<ItemObject> activeItems3 = _activeItems;
		val = _mainAgent.SpawnEquipment[(EquipmentIndex)6];
		activeItems3.Add(((EquipmentElement)(ref val)).Item);
		List<ItemObject> activeItems4 = _activeItems;
		val = _mainAgent.SpawnEquipment[(EquipmentIndex)8];
		activeItems4.Add(((EquipmentElement)(ref val)).Item);
		List<ItemObject> activeItems5 = _activeItems;
		val = _mainAgent.SpawnEquipment[(EquipmentIndex)7];
		activeItems5.Add(((EquipmentElement)(ref val)).Item);
		MissionWeapon val2 = _mainAgent.Equipment[(EquipmentIndex)0];
		if (((MissionWeapon)(ref val2)).WeaponsCount > 0)
		{
			_activeItems.Add(((MissionWeapon)(ref val2)).Item);
		}
		MissionWeapon val3 = _mainAgent.Equipment[(EquipmentIndex)1];
		if (((MissionWeapon)(ref val3)).WeaponsCount > 0)
		{
			_activeItems.Add(((MissionWeapon)(ref val3)).Item);
		}
		MissionWeapon val4 = _mainAgent.Equipment[(EquipmentIndex)2];
		if (((MissionWeapon)(ref val4)).WeaponsCount > 0)
		{
			_activeItems.Add(((MissionWeapon)(ref val4)).Item);
		}
		MissionWeapon val5 = _mainAgent.Equipment[(EquipmentIndex)3];
		if (((MissionWeapon)(ref val5)).WeaponsCount > 0)
		{
			_activeItems.Add(((MissionWeapon)(ref val5)).Item);
		}
		List<ItemObject> activeItems6 = _activeItems;
		val = _mainAgent.SpawnEquipment[(EquipmentIndex)10];
		activeItems6.Add(((EquipmentElement)(ref val)).Item);
		List<ItemObject> activeItems7 = _activeItems;
		val = _mainAgent.SpawnEquipment[(EquipmentIndex)11];
		activeItems7.Add(((EquipmentElement)(ref val)).Item);
	}

	private void SlotCheck(string slotName, int index, XmlNode parentNode, ItemObject obj = null)
	{
		XmlNodeList? xmlNodeList = parentNode.SelectNodes("equipmentSet")[_setIndex].SelectNodes("equipment");
		bool flag = false;
		foreach (XmlNode item in xmlNodeList)
		{
			if (item.Attributes != null && item.Attributes["slot"].InnerText == slotName)
			{
				if ((index != -1 && _activeItems[index] == null) || (index == -1 && obj == null))
				{
					item.ParentNode.RemoveChild(item);
					return;
				}
				item.Attributes["id"].Value = "Item." + ((obj == null) ? ((MBObjectBase)_activeItems[index]).StringId : ((MBObjectBase)obj).StringId);
				flag = true;
				break;
			}
		}
		if (!flag && (index == -1 || _activeItems[index] != null) && (index != -1 || obj != null))
		{
			XmlElement xmlElement = _charactersXml.CreateElement("equipment");
			XmlAttribute xmlAttribute = _charactersXml.CreateAttribute("slot");
			xmlAttribute.Value = slotName;
			XmlAttribute xmlAttribute2 = _charactersXml.CreateAttribute("id");
			xmlAttribute2.Value = "Item." + ((obj == null) ? ((MBObjectBase)_activeItems[index]).StringId : ((MBObjectBase)obj).StringId);
			xmlElement.Attributes.Append(xmlAttribute);
			xmlElement.Attributes.Append(xmlAttribute2);
			parentNode.SelectNodes("equipmentSet")[_setIndex].AppendChild(xmlElement);
		}
	}

	private void UpdateCamera()
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		Vec3 val = ((_mainAgent.MountAgent == null) ? new Vec3(1.3f, 2f, 1f, -1f) : new Vec3(2f, 3f, 2f, -1f));
		MatrixFrame val2 = default(MatrixFrame);
		ref Mat3 rotation = ref val2.rotation;
		Vec3 val3 = _mainAgent.Position - _cam.Position;
		rotation.u = -((Vec3)(ref val3)).NormalizedCopy();
		val2.rotation.f = Vec3.Up;
		val2.rotation.s = Vec3.CrossProduct(val2.rotation.f, val2.rotation.u);
		((Vec3)(ref val2.rotation.s)).Normalize();
		((Mat3)(ref val2.rotation)).Orthonormalize();
		float aspectRatio = Screen.AspectRatio;
		_cam.SetFovVertical(MathF.PI * 13f / 36f, aspectRatio, 1E-08f, 1000f);
		val2.origin = _mainAgent.Position + val;
		_cam.Frame = val2;
		((MissionView)this).MissionScreen.CustomCamera = _cam;
	}

	private void SaveToXml()
	{
		//IL_032f: Unknown result type (might be due to invalid IL or missing references)
		_charactersXml = LoadXmlFile(ModuleHelper.GetModuleFullPath("Sandbox") + "ModuleData/spnpccharacters.xml");
		XmlNodeList xmlNodeList = _charactersXml.DocumentElement.SelectNodes("/NPCCharacters");
		bool flag = false;
		bool flag2 = false;
		string text = "\n  <equipmentSet>\n";
		foreach (XmlNode item in _charactersXml.DocumentElement.SelectNodes("NPCCharacter"))
		{
			if (item.Attributes["id"] != null && _attributes[0] == item.Attributes["id"].InnerText)
			{
				flag2 = true;
				if (_setIndex <= _spawnSetIndex)
				{
					return;
				}
				for (int i = 0; i < 9; i++)
				{
					if (_activeItems[i] != null)
					{
						string text2 = "";
						switch (i)
						{
						case 0:
							text2 = "Head";
							break;
						case 1:
							text2 = "Cape";
							break;
						case 2:
							text2 = "Body";
							break;
						case 3:
							text2 = "Gloves";
							break;
						case 4:
							text2 = "Leg";
							break;
						case 5:
							text2 = "Item0";
							break;
						case 6:
							text2 = "Item1";
							break;
						case 7:
							text2 = "Item2";
							break;
						case 8:
							text2 = "Item3";
							break;
						}
						text = text + "    <equipment slot=\"" + text2 + "\" id=\"Item." + ((MBObjectBase)_activeItems[i]).StringId + "\" />\n";
					}
				}
				if (_horse != null)
				{
					text = text + "    <equipment slot=\"Horse\" id=\"Item." + ((MBObjectBase)_horse).StringId + "\" />\n";
				}
				if (_harness != null)
				{
					text = text + "    <equipment slot=\"HorseHarness\" id=\"Item." + ((MBObjectBase)_harness).StringId + "\" />\n";
				}
				text += "  </equipmentSet>\n";
				item.InnerXml += text;
				_charactersXml.Save(ModuleHelper.GetModuleFullPath("SandBoxCore") + "ModuleData/spnpccharacters.xml");
				Utilities.ConstructMainThreadJob((Delegate)new MainThreadDelegate(Mission.Current.EndMission), Array.Empty<object>());
				MBGameManager.EndGame();
				for (int j = 0; j < 10; j++)
				{
				}
				break;
			}
			if (item.Attributes["id"] != null && _attributes[1] == item.Attributes["name"].InnerText)
			{
				flag = true;
			}
		}
		if (flag2)
		{
			return;
		}
		XmlElement xmlElement = _charactersXml.CreateElement("NPCCharacter");
		XmlAttribute xmlAttribute = _charactersXml.CreateAttribute("id");
		xmlAttribute.Value = _attributes[0];
		xmlElement.Attributes.Append(xmlAttribute);
		XmlAttribute xmlAttribute2 = _charactersXml.CreateAttribute("default_group");
		xmlAttribute2.Value = FormationClassExtensions.GetName(_groups[_groupIndex]);
		xmlElement.Attributes.Append(xmlAttribute2);
		XmlAttribute xmlAttribute3 = _charactersXml.CreateAttribute("level");
		xmlAttribute3.Value = _attributes[2];
		xmlElement.Attributes.Append(xmlAttribute3);
		XmlAttribute xmlAttribute4 = _charactersXml.CreateAttribute("name");
		xmlAttribute4.Value = _attributes[1];
		xmlElement.Attributes.Append(xmlAttribute4);
		XmlAttribute xmlAttribute5 = _charactersXml.CreateAttribute("occupation");
		xmlAttribute5.Value = _attributes[3];
		xmlElement.Attributes.Append(xmlAttribute5);
		XmlAttribute xmlAttribute6 = _charactersXml.CreateAttribute("culture");
		xmlAttribute6.Value = "Culture." + ((MBObjectBase)_allFactions[_factionIndex]).StringId;
		xmlElement.Attributes.Append(xmlAttribute6);
		XmlElement xmlElement2 = _charactersXml.CreateElement("face");
		XmlElement xmlElement3 = _charactersXml.CreateElement("face_key_template");
		XmlAttribute xmlAttribute7 = _charactersXml.CreateAttribute("value");
		xmlAttribute7.Value = "NPCCharacter.villager_vlandia";
		xmlElement3.Attributes.Append(xmlAttribute7);
		xmlElement2.AppendChild(xmlElement3);
		xmlElement.AppendChild(xmlElement2);
		XmlElement xmlElement4 = _charactersXml.CreateElement("equipmentSet");
		for (int k = 0; k < 9; k++)
		{
			if (_activeItems[k] != null)
			{
				XmlElement xmlElement5 = _charactersXml.CreateElement("equipment");
				XmlAttribute xmlAttribute8 = _charactersXml.CreateAttribute("slot");
				string value = "";
				switch (k)
				{
				case 0:
					value = "Head";
					break;
				case 1:
					value = "Cape";
					break;
				case 2:
					value = "Body";
					break;
				case 3:
					value = "Gloves";
					break;
				case 4:
					value = "Leg";
					break;
				case 5:
					value = "Item0";
					break;
				case 6:
					value = "Item1";
					break;
				case 7:
					value = "Item2";
					break;
				case 8:
					value = "Item3";
					break;
				}
				xmlAttribute8.Value = value;
				xmlElement5.Attributes.Append(xmlAttribute8);
				XmlAttribute xmlAttribute9 = _charactersXml.CreateAttribute("id");
				xmlAttribute9.Value = "Item." + ((MBObjectBase)_activeItems[k]).StringId;
				xmlElement5.Attributes.Append(xmlAttribute9);
				xmlElement4.AppendChild(xmlElement5);
			}
		}
		xmlElement.AppendChild(xmlElement4);
		if (_horse != null)
		{
			XmlElement xmlElement6 = _charactersXml.CreateElement("equipment");
			XmlAttribute xmlAttribute10 = _charactersXml.CreateAttribute("slot");
			xmlAttribute10.Value = "Horse";
			xmlElement6.Attributes.Append(xmlAttribute10);
			XmlAttribute xmlAttribute11 = _charactersXml.CreateAttribute("id");
			xmlAttribute11.Value = "Item." + ((MBObjectBase)_horse).StringId;
			xmlElement6.Attributes.Append(xmlAttribute11);
			xmlElement.AppendChild(xmlElement6);
		}
		if (_harness != null)
		{
			XmlElement xmlElement7 = _charactersXml.CreateElement("equipment");
			XmlAttribute xmlAttribute12 = _charactersXml.CreateAttribute("slot");
			xmlAttribute12.Value = "HorseHarness";
			xmlElement7.Attributes.Append(xmlAttribute12);
			XmlAttribute xmlAttribute13 = _charactersXml.CreateAttribute("id");
			xmlAttribute13.Value = "Item." + ((MBObjectBase)_harness).StringId;
			xmlElement7.Attributes.Append(xmlAttribute13);
			xmlElement.AppendChild(xmlElement7);
		}
		xmlNodeList[xmlNodeList.Count - 1].AppendChild(xmlElement);
		_charactersXml.Save(ModuleHelper.GetModuleFullPath("SandBoxCore") + "ModuleData/spnpccharacters.xml");
		Utilities.ConstructMainThreadJob((Delegate)new MainThreadDelegate(Mission.Current.EndMission), Array.Empty<object>());
		MBGameManager.EndGame();
		for (int l = 0; l < 10; l++)
		{
		}
	}

	private void Clear(bool[] array)
	{
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = false;
		}
	}

	private XmlDocument LoadXmlFile(string path)
	{
		Debug.Print("opening " + path, 0, (DebugColor)12, 17592186044416uL);
		XmlDocument xmlDocument = new XmlDocument();
		StreamReader streamReader = new StreamReader(path);
		string xml = streamReader.ReadToEnd();
		xmlDocument.LoadXml(xml);
		streamReader.Close();
		return xmlDocument;
	}
}
