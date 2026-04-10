using System;
using System.Collections.Generic;
using System.Linq;
using Helpers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Conversation;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.Core;
using TaleWorlds.DotNet;
using TaleWorlds.Engine;
using TaleWorlds.Engine.Options;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.Tableaus;

namespace SandBox.View.Map;

public class MapConversationTableau
{
	private struct DefaultConversationAnimationData
	{
		public static readonly DefaultConversationAnimationData Invalid = new DefaultConversationAnimationData
		{
			ActionName = "",
			AnimationDataValid = false
		};

		public ConversationAnimData AnimationData;

		public string ActionName;

		public bool AnimationDataValid;
	}

	private static int _tableauIndex;

	private const float MinimumTimeRequiredToChangeIdleAction = 8f;

	private Scene _tableauScene;

	private float _animationFrequencyThreshold = 2.5f;

	private MatrixFrame _frame;

	private GameEntity _cameraEntity;

	private SoundEvent _conversationSoundEvent;

	private Camera _continuousRenderCamera;

	private MapConversationTableauData _data;

	private float _cameraRatio;

	private IMapConversationDataProvider _dataProvider;

	private bool _initialized;

	private Timer _changeIdleActionTimer;

	private int _tableauSizeX;

	private int _tableauSizeY;

	private uint _clothColor1;

	private uint _clothColor2;

	private List<AgentVisuals> _agentVisuals;

	private static readonly string fallbackAnimActName = "act_inventory_idle_start";

	private readonly string RainingEntityTag;

	private readonly string SnowingEntityTag;

	private float _animationGap;

	private bool _isEnabled;

	private float RenderScale;

	private const float _baseCameraRatio = 1.7777778f;

	private float _baseCameraFOV;

	private string _cachedAtmosphereName;

	private string _opponentLeaderEquipmentCache;

	public Texture Texture { get; private set; }

	private TableauView View
	{
		get
		{
			Texture texture = Texture;
			if (texture == null)
			{
				return null;
			}
			return texture.TableauView;
		}
	}

	public MapConversationTableau()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Expected O, but got Unknown
		Color val = new Color(1f, 1f, 1f, 1f);
		_clothColor1 = ((Color)(ref val)).ToUnsignedInteger();
		val = new Color(1f, 1f, 1f, 1f);
		_clothColor2 = ((Color)(ref val)).ToUnsignedInteger();
		RainingEntityTag = "raining_entity";
		SnowingEntityTag = "snowing_entity";
		_isEnabled = true;
		RenderScale = 1f;
		_baseCameraFOV = -1f;
		_cachedAtmosphereName = "";
		base._002Ector();
		_changeIdleActionTimer = new Timer(Game.Current.ApplicationTime, 8f, true);
		_agentVisuals = new List<AgentVisuals>();
		TableauView view = View;
		if (view != null)
		{
			((View)view).SetEnable(_isEnabled);
		}
		_dataProvider = SandBoxViewSubModule.MapConversationDataProvider;
	}

	public void SetEnabled(bool enabled)
	{
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Expected O, but got Unknown
		if (_isEnabled == enabled)
		{
			return;
		}
		if (enabled)
		{
			TableauView view = View;
			if (view != null)
			{
				((View)view).SetEnable(false);
			}
			TableauView view2 = View;
			if (view2 != null)
			{
				((SceneView)view2).AddClearTask(true);
			}
			Texture texture = Texture;
			if (texture != null)
			{
				texture.Release();
			}
			Texture = TableauView.AddTableau($"MapConvTableau_{_tableauIndex++}", new TextureUpdateEventHandler(CharacterTableauContinuousRenderFunction), (object)_tableauScene, _tableauSizeX, _tableauSizeY);
			((SceneView)Texture.TableauView).SetSceneUsesContour(false);
			((SceneView)Texture.TableauView).SetPointlightResolutionMultiplier(0f);
		}
		else
		{
			TableauView view3 = View;
			if (view3 != null)
			{
				((View)view3).SetEnable(false);
			}
			TableauView view4 = View;
			if (view4 != null)
			{
				((SceneView)view4).ClearAll(false, false);
			}
			RemovePreviousAgentsSoundEvent();
			StopConversationSoundEvent();
			ThumbnailCacheManager.Current.ReturnCachedMapConversationTableauScene();
		}
		_isEnabled = enabled;
	}

	public void SetData(object data)
	{
		if (_data == data)
		{
			return;
		}
		if (_data != null)
		{
			_initialized = false;
			foreach (AgentVisuals agentVisual in _agentVisuals)
			{
				agentVisual.Reset();
			}
			_agentVisuals.Clear();
			(MapScreen.Instance?.GetMapView<MapConversationView>()).ConversationMission.SetConversationTableau(null);
		}
		_data = data as MapConversationTableauData;
	}

	public void SetTargetSize(int width, int height)
	{
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Expected O, but got Unknown
		int num = 0;
		int num2 = 0;
		if (width <= 0 || height <= 0)
		{
			num = 10;
			num2 = 10;
		}
		else
		{
			RenderScale = NativeOptions.GetConfig((NativeOptionsType)24) / 100f;
			num = (int)((float)width * RenderScale);
			num2 = (int)((float)height * RenderScale);
		}
		if (num != _tableauSizeX || num2 != _tableauSizeY)
		{
			_tableauSizeX = num;
			_tableauSizeY = num2;
			_cameraRatio = (float)_tableauSizeX / (float)_tableauSizeY;
			TableauView view = View;
			if (view != null)
			{
				((View)view).SetEnable(false);
			}
			TableauView view2 = View;
			if (view2 != null)
			{
				((SceneView)view2).AddClearTask(true);
			}
			Texture texture = Texture;
			if (texture != null)
			{
				texture.Release();
			}
			Texture = TableauView.AddTableau($"MapConvTableau_{_tableauIndex++}", new TextureUpdateEventHandler(CharacterTableauContinuousRenderFunction), (object)_tableauScene, _tableauSizeX, _tableauSizeY);
		}
	}

	public void OnFinalize(bool clearNextFrame)
	{
		TableauView view = View;
		if (view != null)
		{
			((View)view).SetEnable(false);
		}
		RemovePreviousAgentsSoundEvent();
		StopConversationSoundEvent();
		Camera continuousRenderCamera = _continuousRenderCamera;
		if (continuousRenderCamera != null)
		{
			continuousRenderCamera.ReleaseCameraEntity();
		}
		_continuousRenderCamera = null;
		foreach (AgentVisuals agentVisual in _agentVisuals)
		{
			agentVisual.ResetNextFrame();
		}
		_agentVisuals = null;
		((SceneView)View).ClearAll(false, false);
		Texture.Release();
		Texture = null;
		IEnumerable<GameEntity> enumerable = _tableauScene.FindEntitiesWithTag(_cachedAtmosphereName);
		_cachedAtmosphereName = "";
		foreach (GameEntity item in enumerable)
		{
			item.SetVisibilityExcludeParents(false);
		}
		ThumbnailCacheManager.Current.ReturnCachedMapConversationTableauScene();
		_tableauScene = null;
	}

	public void OnTick(float dt)
	{
		if (!_isEnabled)
		{
			return;
		}
		if (_data != null && !_initialized)
		{
			FirstTimeInit();
			(MapScreen.Instance?.GetMapView<MapConversationView>()).ConversationMission.SetConversationTableau(this);
		}
		if (_conversationSoundEvent != null && !_conversationSoundEvent.IsPlaying())
		{
			RemovePreviousAgentsSoundEvent();
			_conversationSoundEvent.Stop();
			_conversationSoundEvent = null;
		}
		if (_animationFrequencyThreshold > _animationGap)
		{
			_animationGap += dt;
		}
		TableauView view = View;
		if ((NativeObject)(object)view != (NativeObject)null)
		{
			if ((NativeObject)(object)_continuousRenderCamera == (NativeObject)null)
			{
				_continuousRenderCamera = Camera.CreateCamera();
			}
			view.SetDoNotRenderThisFrame(false);
		}
		if (_agentVisuals != null && _agentVisuals.Count > 0)
		{
			_agentVisuals[0].TickVisuals();
		}
		if (!(_agentVisuals[0].GetEquipment().CalculateEquipmentCode() != _opponentLeaderEquipmentCache))
		{
			return;
		}
		_initialized = false;
		foreach (AgentVisuals agentVisual in _agentVisuals)
		{
			agentVisual.Reset();
		}
		_agentVisuals.Clear();
	}

	private void FirstTimeInit()
	{
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01db: Unknown result type (might be due to invalid IL or missing references)
		//IL_0233: Unknown result type (might be due to invalid IL or missing references)
		//IL_0238: Unknown result type (might be due to invalid IL or missing references)
		//IL_023a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0244: Unknown result type (might be due to invalid IL or missing references)
		//IL_0253: Unknown result type (might be due to invalid IL or missing references)
		if ((NativeObject)(object)_tableauScene == (NativeObject)null)
		{
			_tableauScene = ThumbnailCacheManager.Current.GetCachedMapConversationTableauScene();
		}
		string atmosphereNameFromData = _dataProvider.GetAtmosphereNameFromData(_data);
		_tableauScene.SetAtmosphereWithName(atmosphereNameFromData);
		IEnumerable<GameEntity> enumerable = _tableauScene.FindEntitiesWithTag(atmosphereNameFromData);
		_cachedAtmosphereName = atmosphereNameFromData;
		foreach (GameEntity item in enumerable)
		{
			item.SetVisibilityExcludeParents(true);
		}
		if ((NativeObject)(object)_continuousRenderCamera == (NativeObject)null)
		{
			_continuousRenderCamera = Camera.CreateCamera();
			_cameraEntity = _tableauScene.FindEntityWithTag("player_infantry_to_infantry");
			Vec3 val = default(Vec3);
			_cameraEntity.GetCameraParamsFromCameraScript(_continuousRenderCamera, ref val);
			_baseCameraFOV = _continuousRenderCamera.HorizontalFov;
		}
		SpawnOpponentLeader();
		PartyBase party = _data.ConversationPartnerData.Party;
		if (party != null)
		{
			TroopRoster memberRoster = party.MemberRoster;
			if (((memberRoster != null) ? new int?(memberRoster.TotalManCount) : ((int?)null)) > 1)
			{
				int num = MathF.Min(2, ((IEnumerable<FlattenedTroopRosterElement>)_data.ConversationPartnerData.Party.MemberRoster.ToFlattenedRoster()).Count() - 1);
				IOrderedEnumerable<TroopRosterElement> orderedEnumerable = ((IEnumerable<TroopRosterElement>)_data.ConversationPartnerData.Party.MemberRoster.GetTroopRoster()).OrderByDescending((TroopRosterElement t) => ((BasicCharacterObject)t.Character).Level);
				foreach (TroopRosterElement item2 in orderedEnumerable)
				{
					CharacterObject character = item2.Character;
					if (character != _data.ConversationPartnerData.Character && !((BasicCharacterObject)character).IsPlayerCharacter)
					{
						num--;
						SpawnOpponentBodyguardCharacter(character, num, _data.ConversationPartnerData.Party);
					}
					if (num == 0)
					{
						break;
					}
				}
				if (num == 1)
				{
					num--;
					TroopRosterElement val2 = ((IEnumerable<TroopRosterElement>)orderedEnumerable).FirstOrDefault((Func<TroopRosterElement, bool>)((TroopRosterElement troop) => !((BasicCharacterObject)troop.Character).IsHero));
					if (val2.Character != null)
					{
						SpawnOpponentBodyguardCharacter(val2.Character, num, _data.ConversationPartnerData.Party);
					}
				}
			}
		}
		_agentVisuals.ForEach(delegate(AgentVisuals a)
		{
			a.SetAgentLodZeroOrMaxExternal(true);
		});
		_tableauScene.ForceLoadResources(true);
		_cameraRatio = Screen.RealScreenResolutionWidth / Screen.RealScreenResolutionHeight;
		SetTargetSize((int)Screen.RealScreenResolutionWidth, (int)Screen.RealScreenResolutionHeight);
		uint num2 = uint.MaxValue;
		num2 &= 0xFFFFFBFFu;
		TableauView view = View;
		if (view != null)
		{
			((SceneView)view).SetPostfxConfigParams((int)num2);
		}
		_tableauScene.FindEntityWithTag(RainingEntityTag).SetVisibilityExcludeParents(_data.IsRaining);
		_tableauScene.FindEntityWithTag(SnowingEntityTag).SetVisibilityExcludeParents(_data.IsSnowing);
		_tableauScene.Tick(3f);
		TableauView view2 = View;
		if (view2 != null)
		{
			((View)view2).SetEnable(true);
		}
		_initialized = true;
	}

	private void SpawnOpponentLeader()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Invalid comparison between Unknown and I4
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Invalid comparison between Unknown and I4
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01db: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_020a: Unknown result type (might be due to invalid IL or missing references)
		//IL_021a: Unknown result type (might be due to invalid IL or missing references)
		//IL_021f: Unknown result type (might be due to invalid IL or missing references)
		//IL_028a: Unknown result type (might be due to invalid IL or missing references)
		//IL_029d: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b8: Unknown result type (might be due to invalid IL or missing references)
		CharacterObject character = _data.ConversationPartnerData.Character;
		if (character == null)
		{
			return;
		}
		GameEntity val = _tableauScene.FindEntityWithTag("player_infantry_spawn");
		DefaultConversationAnimationData defaultAnimForCharacter = GetDefaultAnimForCharacter(character, preferLoopAnimationIfAvailable: false, _data.ConversationPartnerData.Party);
		_opponentLeaderEquipmentCache = null;
		Equipment val2 = null;
		val2 = ((!_data.ConversationPartnerData.IsCivilianEquipmentRequiredForLeader) ? (((BasicCharacterObject)_data.ConversationPartnerData.Character).IsHero ? ((BasicCharacterObject)character).FirstBattleEquipment : ((BasicCharacterObject)character).BattleEquipments.ElementAt(((BasicCharacterObject)_data.ConversationPartnerData.Character).GetDefaultFaceSeed(0) % ((BasicCharacterObject)character).BattleEquipments.Count())) : (((BasicCharacterObject)_data.ConversationPartnerData.Character).IsHero ? ((BasicCharacterObject)character).FirstCivilianEquipment : ((BasicCharacterObject)character).CivilianEquipments.ElementAt(((BasicCharacterObject)_data.ConversationPartnerData.Character).GetDefaultFaceSeed(0) % ((BasicCharacterObject)character).CivilianEquipments.Count())));
		val2 = val2.Clone(false);
		for (EquipmentIndex val3 = (EquipmentIndex)0; (int)val3 < 12; val3 = (EquipmentIndex)(val3 + 1))
		{
			EquipmentElement val4 = val2[val3];
			if (!((EquipmentElement)(ref val4)).IsEmpty)
			{
				val4 = val2[val3];
				if ((int)((EquipmentElement)(ref val4)).Item.Type == 26)
				{
					val2[val3] = EquipmentElement.Invalid;
					break;
				}
			}
		}
		int num = -1;
		if (_data.ConversationPartnerData.Party != null)
		{
			num = CharacterHelper.GetPartyMemberFaceSeed(_data.ConversationPartnerData.Party, (BasicCharacterObject)(object)character, 0);
		}
		(uint, uint) deterministicColorsForCharacter = CharacterHelper.GetDeterministicColorsForCharacter(character, _data.ConversationPartnerData.Party);
		Monster baseMonsterFromRace = FaceGen.GetBaseMonsterFromRace(((BasicCharacterObject)character).Race);
		AgentVisualsData val5 = new AgentVisualsData();
		Hero heroObject = character.HeroObject;
		AgentVisualsData obj = val5.Banner((heroObject != null) ? heroObject.ClanBanner : null).Equipment(val2).Race(((BasicCharacterObject)character).Race);
		Hero heroObject2 = character.HeroObject;
		AgentVisualsData obj2 = obj.BodyProperties((heroObject2 != null) ? heroObject2.BodyProperties : ((BasicCharacterObject)character).GetBodyProperties(val2, num)).Frame(val.GetGlobalFrame()).UseMorphAnims(true)
			.ActionSet(MBGlobals.GetActionSetWithSuffix(baseMonsterFromRace, ((BasicCharacterObject)character).IsFemale, "_warrior"));
		ActionIndexCache val6 = ActionIndexCache.Create(defaultAnimForCharacter.ActionName);
		AgentVisuals val7 = AgentVisuals.Create(obj2.ActionCode(ref val6).Scene(_tableauScene).Monster(baseMonsterFromRace)
			.PrepareImmediately(true)
			.SkeletonType((SkeletonType)(((BasicCharacterObject)character).IsFemale ? 1 : 0))
			.ClothColor1(deterministicColorsForCharacter.Item1)
			.ClothColor2(deterministicColorsForCharacter.Item2), "MapConversationTableau", true, false, false);
		val7.GetVisuals().GetSkeleton().TickAnimationsAndForceUpdate(0.1f, _frame, true);
		Vec3 globalStableEyePoint = val7.GetVisuals().GetGlobalStableEyePoint(true);
		val7.SetLookDirection(_cameraEntity.GetGlobalFrame().origin - globalStableEyePoint);
		string defaultFaceIdle = CharacterHelper.GetDefaultFaceIdle(character);
		MBSkeletonExtensions.SetFacialAnimation(val7.GetVisuals().GetSkeleton(), (FacialAnimChannel)1, defaultFaceIdle, false, true);
		_agentVisuals.Add(val7);
		_opponentLeaderEquipmentCache = ((val2 != null) ? val2.CalculateEquipmentCode() : null);
	}

	private void SpawnOpponentBodyguardCharacter(CharacterObject character, int indexOfBodyguard, PartyBase party)
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_021e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0297: Unknown result type (might be due to invalid IL or missing references)
		//IL_02aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_02af: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02be: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c5: Unknown result type (might be due to invalid IL or missing references)
		if (indexOfBodyguard >= 0 && indexOfBodyguard <= 1)
		{
			GameEntity val = _tableauScene.FindEntitiesWithTag("player_bodyguard_infantry_spawn").ElementAt(indexOfBodyguard);
			DefaultConversationAnimationData defaultAnimForCharacter = GetDefaultAnimForCharacter(character, preferLoopAnimationIfAvailable: true, party);
			int num = (indexOfBodyguard + 10) * 5;
			Equipment val2 = ((!_data.ConversationPartnerData.IsCivilianEquipmentRequiredForBodyGuardCharacters) ? (((BasicCharacterObject)_data.ConversationPartnerData.Character).IsHero ? ((BasicCharacterObject)character).FirstBattleEquipment : ((BasicCharacterObject)character).BattleEquipments.ElementAt(num % ((BasicCharacterObject)character).BattleEquipments.Count())) : (((BasicCharacterObject)_data.ConversationPartnerData.Character).IsHero ? ((BasicCharacterObject)character).FirstCivilianEquipment : ((BasicCharacterObject)character).CivilianEquipments.ElementAt(num % ((BasicCharacterObject)character).CivilianEquipments.Count())));
			int num2 = -1;
			if (_data.ConversationPartnerData.Party != null)
			{
				num2 = CharacterHelper.GetPartyMemberFaceSeed(_data.ConversationPartnerData.Party, (BasicCharacterObject)(object)_data.ConversationPartnerData.Character, num);
			}
			Monster baseMonsterFromRace = FaceGen.GetBaseMonsterFromRace(((BasicCharacterObject)character).Race);
			AgentVisualsData val3 = new AgentVisualsData();
			PartyBase party2 = _data.ConversationPartnerData.Party;
			object obj;
			if (party2 == null)
			{
				obj = null;
			}
			else
			{
				Hero leaderHero = party2.LeaderHero;
				obj = ((leaderHero != null) ? leaderHero.ClanBanner : null);
			}
			AgentVisualsData obj2 = val3.Banner((Banner)obj).Equipment(val2).Race(((BasicCharacterObject)character).Race)
				.BodyProperties(((BasicCharacterObject)character).GetBodyProperties(val2, num2))
				.Frame(val.GetGlobalFrame())
				.UseMorphAnims(true)
				.ActionSet(MBGlobals.GetActionSetWithSuffix(baseMonsterFromRace, ((BasicCharacterObject)character).IsFemale, "_warrior"));
			ActionIndexCache val4 = ActionIndexCache.Create(defaultAnimForCharacter.ActionName);
			AgentVisualsData obj3 = obj2.ActionCode(ref val4).Scene(_tableauScene).Monster(baseMonsterFromRace)
				.PrepareImmediately(true)
				.SkeletonType((SkeletonType)(((BasicCharacterObject)character).IsFemale ? 1 : 0));
			PartyBase party3 = _data.ConversationPartnerData.Party;
			uint? obj4;
			if (party3 == null)
			{
				obj4 = null;
			}
			else
			{
				Hero leaderHero2 = party3.LeaderHero;
				obj4 = ((leaderHero2 != null) ? new uint?(leaderHero2.MapFaction.Color) : ((uint?)null));
			}
			AgentVisualsData obj5 = obj3.ClothColor1((uint)(((int?)obj4) ?? (-1)));
			PartyBase party4 = _data.ConversationPartnerData.Party;
			uint? obj6;
			if (party4 == null)
			{
				obj6 = null;
			}
			else
			{
				Hero leaderHero3 = party4.LeaderHero;
				obj6 = ((leaderHero3 != null) ? new uint?(leaderHero3.MapFaction.Color2) : ((uint?)null));
			}
			AgentVisuals val5 = AgentVisuals.Create(obj5.ClothColor2((uint)(((int?)obj6) ?? (-1))), "MapConversationTableau", true, false, false);
			val5.GetVisuals().GetSkeleton().TickAnimationsAndForceUpdate(0.1f, _frame, true);
			Vec3 globalStableEyePoint = val5.GetVisuals().GetGlobalStableEyePoint(true);
			val5.SetLookDirection(_cameraEntity.GetGlobalFrame().origin - globalStableEyePoint);
			string defaultFaceIdle = CharacterHelper.GetDefaultFaceIdle(character);
			MBSkeletonExtensions.SetFacialAnimation(val5.GetVisuals().GetSkeleton(), (FacialAnimChannel)1, defaultFaceIdle, false, true);
			_agentVisuals.Add(val5);
		}
	}

	internal void CharacterTableauContinuousRenderFunction(Texture sender, EventArgs e)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Expected O, but got Unknown
		Scene val = (Scene)sender.UserData;
		Texture = sender;
		TableauView tableauView = sender.TableauView;
		if ((NativeObject)(object)val == (NativeObject)null)
		{
			tableauView.SetContinuousRendering(false);
			tableauView.SetDeleteAfterRendering(true);
			return;
		}
		val.EnsurePostfxSystem();
		val.SetDofMode(true);
		val.SetMotionBlurMode(false);
		val.SetBloom(true);
		val.SetDynamicShadowmapCascadesRadiusMultiplier(0.31f);
		((SceneView)tableauView).SetRenderWithPostfx(true);
		uint num = uint.MaxValue;
		num &= 0xFFFFFBFFu;
		if (tableauView != null)
		{
			((SceneView)tableauView).SetPostfxConfigParams((int)num);
		}
		if (!((NativeObject)(object)_continuousRenderCamera != (NativeObject)null))
		{
			return;
		}
		float num2 = _cameraRatio / 1.7777778f;
		_continuousRenderCamera.SetFovHorizontal(num2 * _baseCameraFOV, _cameraRatio, 0.2f, 200f);
		((SceneView)tableauView).SetCamera(_continuousRenderCamera);
		((SceneView)tableauView).SetScene(val);
		((SceneView)tableauView).SetSceneUsesSkybox(true);
		tableauView.SetDeleteAfterRendering(false);
		tableauView.SetContinuousRendering(true);
		((View)tableauView).SetClearColor(0u);
		((SceneView)tableauView).SetClearGbuffer(true);
		((SceneView)tableauView).DoNotClear(false);
		((SceneView)tableauView).SetFocusedShadowmap(true, ref _frame.origin, 1.55f);
		val.ForceLoadResources(true);
		bool flag = true;
		do
		{
			flag = true;
			foreach (AgentVisuals agentVisual in _agentVisuals)
			{
				flag = flag && agentVisual.GetVisuals().CheckResources(true);
			}
		}
		while (!flag);
	}

	private DefaultConversationAnimationData GetDefaultAnimForCharacter(CharacterObject character, bool preferLoopAnimationIfAvailable, PartyBase party)
	{
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Invalid comparison between Unknown and I4
		DefaultConversationAnimationData invalid = DefaultConversationAnimationData.Invalid;
		CultureObject culture = character.Culture;
		if (culture != null && ((BasicCultureObject)culture).IsBandit)
		{
			invalid.ActionName = "aggressive";
		}
		else
		{
			Hero heroObject = character.HeroObject;
			if (heroObject != null && heroObject.IsWounded)
			{
				PlayerEncounter current = PlayerEncounter.Current;
				if (current != null && (int)current.EncounterState == 6)
				{
					invalid.ActionName = "weary";
					goto IL_006e;
				}
			}
			invalid.ActionName = CharacterHelper.GetStandingBodyIdle(character, party);
		}
		goto IL_006e;
		IL_006e:
		if (Campaign.Current.ConversationManager.ConversationAnimationManager.ConversationAnims.TryGetValue(invalid.ActionName, out var value))
		{
			bool flag = !string.IsNullOrEmpty(value.IdleAnimStart);
			bool flag2 = !string.IsNullOrEmpty(value.IdleAnimLoop);
			invalid.ActionName = (((preferLoopAnimationIfAvailable && flag2) || !flag) ? value.IdleAnimLoop : value.IdleAnimStart);
			invalid.AnimationData = value;
			invalid.AnimationDataValid = true;
		}
		else
		{
			invalid.ActionName = fallbackAnimActName;
			if (Campaign.Current.ConversationManager.ConversationAnimationManager.ConversationAnims.TryGetValue(invalid.ActionName, out value))
			{
				invalid.AnimationData = value;
				invalid.AnimationDataValid = true;
			}
		}
		return invalid;
	}

	public void OnConversationPlay(string idleActionId, string idleFaceAnimId, string reactionId, string reactionFaceAnimId, string soundPath)
	{
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		if (!_initialized)
		{
			Debug.FailedAssert("Conversation Tableau shouldn't play before initialization", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\SandBox.View\\Map\\MapConversationTableau.cs", "OnConversationPlay", 605);
			return;
		}
		if (!Campaign.Current.ConversationManager.SpeakerAgent.Character.IsPlayerCharacter)
		{
			bool flag = false;
			bool flag2 = string.IsNullOrEmpty(idleActionId);
			ConversationAnimData value;
			if (flag2)
			{
				DefaultConversationAnimationData defaultAnimForCharacter = GetDefaultAnimForCharacter(_data.ConversationPartnerData.Character, preferLoopAnimationIfAvailable: false, _data.ConversationPartnerData.Party);
				value = defaultAnimForCharacter.AnimationData;
				flag = defaultAnimForCharacter.AnimationDataValid;
			}
			else if (Campaign.Current.ConversationManager.ConversationAnimationManager.ConversationAnims.TryGetValue(idleActionId, out value))
			{
				flag = true;
			}
			if (flag)
			{
				if (!string.IsNullOrEmpty(reactionId))
				{
					AgentVisuals obj = _agentVisuals[0];
					ActionIndexCache val = ActionIndexCache.Create(value.Reactions[reactionId]);
					obj.SetAction(ref val, 0f, false);
				}
				else if (!flag2 || _changeIdleActionTimer.Check(Game.Current.ApplicationTime))
				{
					ActionIndexCache val2 = ActionIndexCache.Create(value.IdleAnimStart);
					if (!_agentVisuals[0].DoesActionContinueWithCurrentAction(ref val2))
					{
						_changeIdleActionTimer.Reset(Game.Current.ApplicationTime);
						_agentVisuals[0].SetAction(ref val2, 0f, false);
					}
				}
			}
			if (!string.IsNullOrEmpty(reactionFaceAnimId))
			{
				MBSkeletonExtensions.SetFacialAnimation(_agentVisuals[0].GetVisuals().GetSkeleton(), (FacialAnimChannel)1, reactionFaceAnimId, false, false);
			}
			else if (!string.IsNullOrEmpty(idleFaceAnimId))
			{
				MBSkeletonExtensions.SetFacialAnimation(_agentVisuals[0].GetVisuals().GetSkeleton(), (FacialAnimChannel)1, idleFaceAnimId, false, true);
			}
		}
		RemovePreviousAgentsSoundEvent();
		StopConversationSoundEvent();
		if (!string.IsNullOrEmpty(soundPath))
		{
			PlayConversationSoundEvent(soundPath);
		}
	}

	public void RemovePreviousAgentsSoundEvent()
	{
		if (_conversationSoundEvent != null)
		{
			_agentVisuals[0].StartRhubarbRecord("", -1);
		}
	}

	private void PlayConversationSoundEvent(string soundPath)
	{
		Debug.Print("Conversation sound playing: " + soundPath, 5, (DebugColor)12, 17592186044416uL);
		_conversationSoundEvent = SoundEvent.CreateEventFromExternalFile("event:/Extra/voiceover", soundPath, _tableauScene, true, false);
		_conversationSoundEvent.Play();
		int soundId = _conversationSoundEvent.GetSoundId();
		string rhubarbXmlPathFromSoundPath = GetRhubarbXmlPathFromSoundPath(soundPath);
		_agentVisuals[0].StartRhubarbRecord(rhubarbXmlPathFromSoundPath, soundId);
	}

	public void StopConversationSoundEvent()
	{
		if (_conversationSoundEvent != null)
		{
			_conversationSoundEvent.Stop();
			_conversationSoundEvent = null;
		}
	}

	private string GetRhubarbXmlPathFromSoundPath(string soundPath)
	{
		return soundPath[..soundPath.LastIndexOf('.')] + ".xml";
	}
}
