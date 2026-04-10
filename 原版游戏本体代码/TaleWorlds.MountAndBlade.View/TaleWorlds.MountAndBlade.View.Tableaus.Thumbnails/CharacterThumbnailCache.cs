using System;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.DotNet;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.View.Scripts;

namespace TaleWorlds.MountAndBlade.View.Tableaus.Thumbnails;

public class CharacterThumbnailCache : ThumbnailCache<CharacterThumbnailCreationData>
{
	private int _characterCount;

	private int _characterTableauGPUAllocationIndex;

	public CharacterThumbnailCache(int capacity)
		: base(capacity)
	{
	}

	protected override void OnInitialize()
	{
		base.OnInitialize();
		_characterTableauGPUAllocationIndex = Utilities.RegisterGPUAllocationGroup("CharacterTableauCache");
	}

	protected override void OnFinalize()
	{
		base.OnFinalize();
	}

	protected override TextureCreationInfo OnCreateTexture(CharacterThumbnailCreationData thumbnailCreationData)
	{
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		string renderId = thumbnailCreationData.RenderId;
		CharacterCode characterCode = thumbnailCreationData.CharacterCode;
		bool isBig = thumbnailCreationData.IsBig;
		Action<Texture> setAction = thumbnailCreationData.SetAction;
		Action cancelAction = thumbnailCreationData.CancelAction;
		int customSizeX = thumbnailCreationData.CustomSizeX;
		int customSizeY = thumbnailCreationData.CustomSizeY;
		if (((IThumbnailCache)this).GetValue(renderId, out Texture texture))
		{
			if (_renderCallbacks.ContainsKey(renderId))
			{
				_renderCallbacks[renderId].SetActions.Add(setAction);
				_renderCallbacks[renderId].CancelActions.Add(cancelAction);
			}
			else
			{
				setAction?.Invoke(texture);
			}
			((IThumbnailCache)this).AddReference(renderId);
			return TextureCreationInfo.WithExistingTexture(texture);
		}
		Camera camera = null;
		int num = ((!isBig) ? 4 : 0);
		GameEntity poseEntity = CreateCharacterBaseEntity(characterCode, BannerlordTableauManager.TableauCharacterScenes[num], ref camera, isBig);
		poseEntity = FillEntityWithPose(characterCode, poseEntity, BannerlordTableauManager.TableauCharacterScenes[num]);
		int num2 = 256;
		int num3 = (isBig ? 120 : 174);
		if (customSizeX > 0)
		{
			num2 = customSizeX;
		}
		if (customSizeY > 0)
		{
			num3 = customSizeY;
		}
		string text = ThumbnailCache<CharacterThumbnailCreationData>.CreateDebugIdFrom(renderId, "cha");
		ThumbnailRenderRequest val = ThumbnailRenderRequest.CreateWithoutTexture(BannerlordTableauManager.TableauCharacterScenes[num], camera, poseEntity, renderId, num2, num3, text, _characterTableauGPUAllocationIndex);
		_thumbnailCreatorView.RegisterRenderRequest(ref val);
		((NativeObject)poseEntity).ManualInvalidate();
		_characterCount++;
		((IThumbnailCache)this).Add(renderId, (Texture)null);
		((IThumbnailCache)this).AddReference(renderId);
		if (!_renderCallbacks.ContainsKey(renderId))
		{
			_renderCallbacks.Add(renderId, RenderCallbackCollection.CreateEmpty());
		}
		_renderCallbacks[renderId].SetActions.Add(setAction);
		_renderCallbacks[renderId].CancelActions.Add(cancelAction);
		return TextureCreationInfo.WithNewTexture();
	}

	protected override bool OnReleaseTexture(CharacterThumbnailCreationData thumbnailCreationData)
	{
		string renderId = thumbnailCreationData.RenderId;
		return ((IThumbnailCache)this).RemoveReference(renderId);
	}

	private GameEntity CreateCharacterBaseEntity(CharacterCode characterCode, Scene scene, ref Camera camera, bool isBig)
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		GetPoseParamsFromCharacterCode(characterCode, out var poseName, out var _);
		string text = poseName + "_pose";
		string text2 = (isBig ? (poseName + "_cam") : (poseName + "_cam_small"));
		WeakGameEntity val = scene.FindWeakEntityWithTag(text);
		if (val == (GameEntity)null)
		{
			return null;
		}
		((WeakGameEntity)(ref val)).SetVisibilityExcludeParents(true);
		GameEntity val2 = GameEntity.CopyFromPrefab(val);
		val2.Name = ((WeakGameEntity)(ref val)).Name + "Instance";
		val2.RemoveTag(text);
		scene.AttachEntity(val2, false);
		val2.SetVisibilityExcludeParents(true);
		((WeakGameEntity)(ref val)).SetVisibilityExcludeParents(false);
		WeakGameEntity val3 = scene.FindWeakEntityWithTag(text2);
		Vec3 val4 = default(Vec3);
		camera = Camera.CreateCamera();
		if (val3 != (GameEntity)null)
		{
			((WeakGameEntity)(ref val3)).GetCameraParamsFromCameraScript(camera, ref val4);
			camera.Frame = ((WeakGameEntity)(ref val3)).GetGlobalFrame();
		}
		return val2;
	}

	private void GetPoseParamsFromCharacterCode(CharacterCode characterCode, out string poseName, out bool hasHorse)
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Expected I4, but got Unknown
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_0253: Unknown result type (might be due to invalid IL or missing references)
		//IL_0258: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_019b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01eb: Expected I4, but got Unknown
		//IL_01eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ef: Invalid comparison between Unknown and I4
		//IL_0270: Unknown result type (might be due to invalid IL or missing references)
		//IL_0275: Unknown result type (might be due to invalid IL or missing references)
		//IL_0202: Unknown result type (might be due to invalid IL or missing references)
		//IL_0207: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02da: Unknown result type (might be due to invalid IL or missing references)
		//IL_02dd: Invalid comparison between Unknown and I4
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_02df: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e3: Invalid comparison between Unknown and I4
		//IL_02fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0302: Unknown result type (might be due to invalid IL or missing references)
		hasHorse = false;
		if (characterCode.IsHero)
		{
			int num = MBRandom.NondeterministicRandomInt % 8;
			poseName = "lord_" + num;
			return;
		}
		poseName = "troop_villager";
		int num2 = -1;
		int num3 = -1;
		Equipment val = characterCode.CalculateEquipment();
		FormationClass formationClass = characterCode.FormationClass;
		EquipmentElement val2;
		switch ((int)formationClass)
		{
		case 0:
		case 2:
		case 4:
		case 5:
		case 6:
		case 7:
		case 8:
		case 9:
		{
			for (int j = 0; j < 4; j++)
			{
				val2 = val[j];
				ItemObject item2 = ((EquipmentElement)(ref val2)).Item;
				if (((item2 != null) ? item2.PrimaryWeapon : null) == null)
				{
					continue;
				}
				if (num3 == -1)
				{
					val2 = val[j];
					if (Extensions.HasAnyFlag<ItemFlags>(((EquipmentElement)(ref val2)).Item.ItemFlags, (ItemFlags)524288))
					{
						num3 = j;
					}
				}
				if (num2 == -1)
				{
					val2 = val[j];
					if (Extensions.HasAnyFlag<WeaponFlags>(((EquipmentElement)(ref val2)).Item.PrimaryWeapon.WeaponFlags, (WeaponFlags)1))
					{
						num2 = j;
					}
				}
			}
			break;
		}
		case 1:
		case 3:
		{
			for (int i = 0; i < 4; i++)
			{
				val2 = val[i];
				ItemObject item = ((EquipmentElement)(ref val2)).Item;
				if (((item != null) ? item.PrimaryWeapon : null) == null)
				{
					continue;
				}
				if (num3 == -1)
				{
					val2 = val[i];
					if (Extensions.HasAnyFlag<ItemFlags>(((EquipmentElement)(ref val2)).Item.ItemFlags, (ItemFlags)524288))
					{
						num3 = i;
					}
				}
				if (num2 == -1)
				{
					val2 = val[i];
					if (Extensions.HasAnyFlag<WeaponFlags>(((EquipmentElement)(ref val2)).Item.PrimaryWeapon.WeaponFlags, (WeaponFlags)2))
					{
						num2 = i;
					}
				}
			}
			break;
		}
		}
		if (num2 != -1)
		{
			val2 = val[num2];
			WeaponClass weaponClass = ((EquipmentElement)(ref val2)).Item.PrimaryWeapon.WeaponClass;
			switch (weaponClass - 2)
			{
			default:
				if ((int)weaponClass != 23)
				{
					break;
				}
				goto case 9;
			case 0:
			case 2:
				if (num3 == -1)
				{
					poseName = "troop_infantry_sword1h";
					break;
				}
				val2 = val[num3];
				if (((EquipmentElement)(ref val2)).Item.PrimaryWeapon.IsShield)
				{
					poseName = "troop_infantry_sword1h";
				}
				break;
			case 1:
			case 3:
			case 6:
				poseName = "troop_infantry_sword2h";
				break;
			case 15:
				poseName = "troop_crossbow";
				break;
			case 14:
				poseName = "troop_bow";
				break;
			case 9:
				poseName = "troop_spear";
				break;
			case 7:
			case 8:
				poseName = "troop_spear";
				break;
			case 4:
			case 5:
			case 10:
			case 11:
			case 12:
			case 13:
				break;
			}
		}
		val2 = val[(EquipmentIndex)10];
		if (((EquipmentElement)(ref val2)).IsEmpty)
		{
			return;
		}
		if (num2 != -1)
		{
			val2 = val[(EquipmentIndex)10];
			HorseComponent horseComponent = ((EquipmentElement)(ref val2)).Item.HorseComponent;
			int num4;
			if (horseComponent == null)
			{
				num4 = 0;
			}
			else
			{
				Monster monster = horseComponent.Monster;
				num4 = ((((monster != null) ? new int?(monster.FamilyType) : ((int?)null)) == 2) ? 1 : 0);
			}
			bool flag = (byte)num4 != 0;
			val2 = val[num2];
			ItemTypeEnum type = ((EquipmentElement)(ref val2)).Item.Type;
			if ((int)type != 2)
			{
				if ((int)type == 9)
				{
					poseName = "troop_cavalry_archer";
				}
				else
				{
					poseName = "troop_cavalry_lance";
				}
			}
			else if (num3 == -1)
			{
				poseName = "troop_cavalry_sword";
			}
			else
			{
				val2 = val[num3];
				if (((EquipmentElement)(ref val2)).Item.PrimaryWeapon.IsShield)
				{
					poseName = "troop_cavalry_sword";
				}
			}
			if (flag)
			{
				poseName = "camel_" + poseName;
			}
		}
		hasHorse = true;
	}

	private GameEntity FillEntityWithPose(CharacterCode characterCode, GameEntity poseEntity, Scene scene)
	{
		if (characterCode.IsEmpty)
		{
			Debug.FailedAssert("Trying to fill entity with empty character code", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.View\\Tableaus\\Thumbnails\\CharacterThumbnailCache.cs", "FillEntityWithPose", 306);
			return poseEntity;
		}
		if (string.IsNullOrEmpty(characterCode.EquipmentCode))
		{
			Debug.FailedAssert("Trying to fill entity with invalid equipment code", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.View\\Tableaus\\Thumbnails\\CharacterThumbnailCache.cs", "FillEntityWithPose", 312);
			return poseEntity;
		}
		if (FaceGen.GetBaseMonsterFromRace(characterCode.Race) == null)
		{
			Debug.FailedAssert("There are no monster data for the race: " + characterCode.Race, "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.View\\Tableaus\\Thumbnails\\CharacterThumbnailCache.cs", "FillEntityWithPose", 319);
			return poseEntity;
		}
		if (poseEntity != (GameEntity)null)
		{
			GetPoseParamsFromCharacterCode(characterCode, out var _, out var _);
			CharacterSpawner characterSpawner = poseEntity.GetScriptComponents<CharacterSpawner>().First();
			characterSpawner.SetCreateFaceImmediately(value: false);
			characterSpawner.InitWithCharacter(characterCode);
		}
		return poseEntity;
	}
}
