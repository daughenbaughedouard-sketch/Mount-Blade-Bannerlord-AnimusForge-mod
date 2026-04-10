using System;
using TaleWorlds.Core;
using TaleWorlds.DotNet;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.ObjectSystem;

namespace TaleWorlds.MountAndBlade.View.Tableaus.Thumbnails;

public class ItemThumbnailCache : ThumbnailCache<ItemThumbnailCreationData>
{
	private struct CustomPoseParameters
	{
		public enum Alignment
		{
			Center,
			Top,
			Bottom
		}

		public string CameraTag;

		public string FrameTag;

		public float DistanceModifier;

		public Alignment FocusAlignment;
	}

	private int _itemTableauGPUAllocationIndex;

	public ItemThumbnailCache(int capacity)
		: base(capacity)
	{
	}

	protected override void OnInitialize()
	{
		base.OnInitialize();
		_itemTableauGPUAllocationIndex = Utilities.RegisterGPUAllocationGroup("ItemTableauCache");
	}

	protected override void OnFinalize()
	{
		base.OnFinalize();
	}

	protected override TextureCreationInfo OnCreateTexture(ItemThumbnailCreationData thumbnailCreationData)
	{
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		ItemObject itemObject = thumbnailCreationData.ItemObject;
		_ = thumbnailCreationData.AdditionalArgs;
		Action<Texture> setAction = thumbnailCreationData.SetAction;
		Action cancelAction = thumbnailCreationData.CancelAction;
		string renderIdToUse = GetRenderIdToUse(thumbnailCreationData);
		if (((IThumbnailCache)this).GetValue(renderIdToUse, out Texture texture))
		{
			if (_renderCallbacks.ContainsKey(renderIdToUse))
			{
				_renderCallbacks[renderIdToUse].SetActions.Add(setAction);
				_renderCallbacks[renderIdToUse].CancelActions.Add(cancelAction);
			}
			else
			{
				setAction?.Invoke(texture);
			}
			((IThumbnailCache)this).AddReference(renderIdToUse);
			return TextureCreationInfo.WithExistingTexture(texture);
		}
		Camera camera = null;
		int num = 2;
		int num2 = 256;
		int num3 = 120;
		GameEntity val = CreateItemBaseEntity(itemObject, BannerlordTableauManager.TableauCharacterScenes[num], ref camera);
		string text = ThumbnailCache<ItemThumbnailCreationData>.CreateDebugIdFrom(renderIdToUse, "itm");
		ThumbnailRenderRequest val2 = ThumbnailRenderRequest.CreateWithoutTexture(BannerlordTableauManager.TableauCharacterScenes[num], camera, val, renderIdToUse, num2, num3, text, _itemTableauGPUAllocationIndex);
		_thumbnailCreatorView.RegisterRenderRequest(ref val2);
		((NativeObject)val).ManualInvalidate();
		((IThumbnailCache)this).Add(renderIdToUse, (Texture)null);
		((IThumbnailCache)this).AddReference(renderIdToUse);
		if (!_renderCallbacks.ContainsKey(renderIdToUse))
		{
			_renderCallbacks.Add(renderIdToUse, RenderCallbackCollection.CreateEmpty());
		}
		_renderCallbacks[renderIdToUse].SetActions.Add(setAction);
		_renderCallbacks[renderIdToUse].CancelActions.Add(cancelAction);
		return TextureCreationInfo.WithNewTexture();
	}

	protected override bool OnReleaseTexture(ItemThumbnailCreationData thumbnailCreationData)
	{
		string renderIdToUse = GetRenderIdToUse(thumbnailCreationData);
		return ((IThumbnailCache)this).RemoveReference(renderIdToUse);
	}

	private string GetRenderIdToUse(ItemThumbnailCreationData thumbnailCreationData)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Invalid comparison between Unknown and I4
		ItemObject itemObject = thumbnailCreationData.ItemObject;
		string additionalArgs = thumbnailCreationData.AdditionalArgs;
		_ = thumbnailCreationData.SetAction;
		string text = ((MBObjectBase)itemObject).StringId;
		if ((int)itemObject.Type == 8)
		{
			text = text + "_" + additionalArgs;
		}
		return text;
	}

	private GameEntity CreateItemBaseEntity(ItemObject item, Scene scene, ref Camera camera)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		MatrixFrame itemFrame = MatrixFrame.Identity;
		MatrixFrame itemFrame2 = MatrixFrame.Identity;
		MatrixFrame itemFrame3 = MatrixFrame.Identity;
		GetItemPoseAndCamera(item, scene, ref camera, ref itemFrame, ref itemFrame2, ref itemFrame3);
		return AddItem(scene, item, itemFrame, itemFrame2, itemFrame3);
	}

	private void GetItemPoseAndCamera(ItemObject item, Scene scene, ref Camera camera, ref MatrixFrame itemFrame, ref MatrixFrame itemFrame1, ref MatrixFrame itemFrame2)
	{
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Invalid comparison between Unknown and I4
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Invalid comparison between Unknown and I4
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Invalid comparison between Unknown and I4
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Invalid comparison between Unknown and I4
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Invalid comparison between Unknown and I4
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Invalid comparison between Unknown and I4
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Invalid comparison between Unknown and I4
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Invalid comparison between Unknown and I4
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Invalid comparison between Unknown and I4
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Invalid comparison between Unknown and I4
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Invalid comparison between Unknown and I4
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Invalid comparison between Unknown and I4
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Invalid comparison between Unknown and I4
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Invalid comparison between Unknown and I4
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Invalid comparison between Unknown and I4
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Invalid comparison between Unknown and I4
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01aa: Invalid comparison between Unknown and I4
		//IL_033a: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_036a: Unknown result type (might be due to invalid IL or missing references)
		//IL_036f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0385: Unknown result type (might be due to invalid IL or missing references)
		//IL_038a: Unknown result type (might be due to invalid IL or missing references)
		//IL_038f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0302: Unknown result type (might be due to invalid IL or missing references)
		//IL_0307: Unknown result type (might be due to invalid IL or missing references)
		//IL_07cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_07d1: Invalid comparison between Unknown and I4
		//IL_076c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0771: Unknown result type (might be due to invalid IL or missing references)
		//IL_077b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0780: Unknown result type (might be due to invalid IL or missing references)
		//IL_0782: Unknown result type (might be due to invalid IL or missing references)
		//IL_0787: Unknown result type (might be due to invalid IL or missing references)
		//IL_0791: Unknown result type (might be due to invalid IL or missing references)
		//IL_0796: Unknown result type (might be due to invalid IL or missing references)
		//IL_079b: Unknown result type (might be due to invalid IL or missing references)
		//IL_07ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_07b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_07bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_07c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_05ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_05ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_05f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_05fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0600: Unknown result type (might be due to invalid IL or missing references)
		//IL_0604: Unknown result type (might be due to invalid IL or missing references)
		//IL_0609: Unknown result type (might be due to invalid IL or missing references)
		//IL_060f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0614: Unknown result type (might be due to invalid IL or missing references)
		//IL_0618: Unknown result type (might be due to invalid IL or missing references)
		//IL_061d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0627: Unknown result type (might be due to invalid IL or missing references)
		//IL_062c: Unknown result type (might be due to invalid IL or missing references)
		//IL_062e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0633: Unknown result type (might be due to invalid IL or missing references)
		//IL_063a: Unknown result type (might be due to invalid IL or missing references)
		//IL_063c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0643: Unknown result type (might be due to invalid IL or missing references)
		//IL_0648: Unknown result type (might be due to invalid IL or missing references)
		//IL_064a: Unknown result type (might be due to invalid IL or missing references)
		//IL_064c: Unknown result type (might be due to invalid IL or missing references)
		//IL_064e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0653: Unknown result type (might be due to invalid IL or missing references)
		//IL_066d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0672: Unknown result type (might be due to invalid IL or missing references)
		//IL_0674: Unknown result type (might be due to invalid IL or missing references)
		//IL_0679: Unknown result type (might be due to invalid IL or missing references)
		//IL_0680: Unknown result type (might be due to invalid IL or missing references)
		//IL_0685: Unknown result type (might be due to invalid IL or missing references)
		//IL_068a: Unknown result type (might be due to invalid IL or missing references)
		//IL_03df: Unknown result type (might be due to invalid IL or missing references)
		//IL_0820: Unknown result type (might be due to invalid IL or missing references)
		//IL_0827: Invalid comparison between Unknown and I4
		//IL_07ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_07f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_07f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_07f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_07fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0806: Unknown result type (might be due to invalid IL or missing references)
		//IL_080b: Unknown result type (might be due to invalid IL or missing references)
		//IL_080f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0814: Unknown result type (might be due to invalid IL or missing references)
		//IL_0818: Unknown result type (might be due to invalid IL or missing references)
		//IL_06a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_06af: Unknown result type (might be due to invalid IL or missing references)
		//IL_06b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_06b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_06b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_06cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_06d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_06d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_072b: Unknown result type (might be due to invalid IL or missing references)
		//IL_06ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_06fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_06ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0701: Unknown result type (might be due to invalid IL or missing references)
		//IL_0706: Unknown result type (might be due to invalid IL or missing references)
		//IL_071a: Unknown result type (might be due to invalid IL or missing references)
		//IL_071f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0724: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_03fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0406: Unknown result type (might be due to invalid IL or missing references)
		//IL_040b: Unknown result type (might be due to invalid IL or missing references)
		//IL_041a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0421: Unknown result type (might be due to invalid IL or missing references)
		//IL_0428: Unknown result type (might be due to invalid IL or missing references)
		//IL_0434: Unknown result type (might be due to invalid IL or missing references)
		//IL_0439: Unknown result type (might be due to invalid IL or missing references)
		//IL_043d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0442: Unknown result type (might be due to invalid IL or missing references)
		//IL_044c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0453: Unknown result type (might be due to invalid IL or missing references)
		//IL_045a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0466: Unknown result type (might be due to invalid IL or missing references)
		//IL_046b: Unknown result type (might be due to invalid IL or missing references)
		//IL_046f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0474: Unknown result type (might be due to invalid IL or missing references)
		//IL_047e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0485: Unknown result type (might be due to invalid IL or missing references)
		//IL_048c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0498: Unknown result type (might be due to invalid IL or missing references)
		//IL_049d: Unknown result type (might be due to invalid IL or missing references)
		//IL_04a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_04a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_04be: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_04cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_04fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0501: Unknown result type (might be due to invalid IL or missing references)
		//IL_0505: Unknown result type (might be due to invalid IL or missing references)
		//IL_050a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0514: Unknown result type (might be due to invalid IL or missing references)
		//IL_051b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0522: Unknown result type (might be due to invalid IL or missing references)
		//IL_052e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0533: Unknown result type (might be due to invalid IL or missing references)
		//IL_0537: Unknown result type (might be due to invalid IL or missing references)
		//IL_053c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0546: Unknown result type (might be due to invalid IL or missing references)
		//IL_054d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0554: Unknown result type (might be due to invalid IL or missing references)
		//IL_0560: Unknown result type (might be due to invalid IL or missing references)
		//IL_0565: Unknown result type (might be due to invalid IL or missing references)
		//IL_0569: Unknown result type (might be due to invalid IL or missing references)
		//IL_056e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0578: Unknown result type (might be due to invalid IL or missing references)
		//IL_057f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0586: Unknown result type (might be due to invalid IL or missing references)
		//IL_0592: Unknown result type (might be due to invalid IL or missing references)
		//IL_0597: Unknown result type (might be due to invalid IL or missing references)
		//IL_059b: Unknown result type (might be due to invalid IL or missing references)
		//IL_05a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_05aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_05b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_05b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_05ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_05bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_05c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_05c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_05cc: Unknown result type (might be due to invalid IL or missing references)
		if (item.IsCraftedWeapon)
		{
			GetItemPoseAndCameraForCraftedItem(item, scene, ref camera, ref itemFrame, ref itemFrame1, ref itemFrame2);
			return;
		}
		string text = "";
		CustomPoseParameters customPoseParameters = new CustomPoseParameters
		{
			CameraTag = "goods_cam",
			DistanceModifier = 6f,
			FrameTag = "goods_frame"
		};
		if (item.WeaponComponent != null)
		{
			WeaponClass weaponClass = item.WeaponComponent.PrimaryWeapon.WeaponClass;
			if (weaponClass - 2 <= 1)
			{
				text = "sword";
			}
		}
		else
		{
			ItemTypeEnum type = item.Type;
			if ((int)type != 14)
			{
				if ((int)type == 15)
				{
					text = "armor";
				}
			}
			else
			{
				text = "helmet";
			}
		}
		if ((int)item.Type == 8)
		{
			text = "shield";
		}
		if ((int)item.Type == 11)
		{
			text = "sling";
		}
		if ((int)item.Type == 7)
		{
			text = "slingstones";
		}
		if ((int)item.Type == 10)
		{
			text = "crossbow";
		}
		if ((int)item.Type == 9)
		{
			text = "bow";
		}
		if ((int)item.Type == 16)
		{
			text = "boot";
		}
		if ((int)item.Type == 1)
		{
			text = ((HorseComponent)item.ItemComponent).Monster.MonsterUsage;
		}
		if ((int)item.Type == 25)
		{
			text = "horse";
		}
		if ((int)item.Type == 24)
		{
			text = "cape";
		}
		if ((int)item.Type == 17)
		{
			text = "glove";
		}
		if ((int)item.Type == 5)
		{
			text = "arrow";
		}
		if ((int)item.Type == 6)
		{
			text = "bolt";
		}
		if ((int)item.Type == 26)
		{
			customPoseParameters = new CustomPoseParameters
			{
				CameraTag = "banner_cam",
				DistanceModifier = 1.5f,
				FrameTag = "banner_frame",
				FocusAlignment = CustomPoseParameters.Alignment.Top
			};
		}
		if ((int)item.Type == 21)
		{
			customPoseParameters = new CustomPoseParameters
			{
				CameraTag = customPoseParameters.CameraTag,
				DistanceModifier = 3f,
				FrameTag = customPoseParameters.FrameTag
			};
		}
		if (((MBObjectBase)item).StringId == "iron" || ((MBObjectBase)item).StringId == "hardwood" || ((MBObjectBase)item).StringId == "charcoal" || ((MBObjectBase)item).StringId == "ironIngot1" || ((MBObjectBase)item).StringId == "ironIngot2" || ((MBObjectBase)item).StringId == "ironIngot3" || ((MBObjectBase)item).StringId == "ironIngot4" || ((MBObjectBase)item).StringId == "ironIngot5" || ((MBObjectBase)item).StringId == "ironIngot6" || item.ItemCategory == DefaultItemCategories.Silver)
		{
			text = "craftmat";
		}
		MatrixFrame val11;
		if (!string.IsNullOrEmpty(text))
		{
			string text2 = text + "_cam";
			string text3 = text + "_frame";
			GameEntity val = scene.FindEntityWithTag(text2);
			if (val != (GameEntity)null)
			{
				camera = Camera.CreateCamera();
				Vec3 val2 = default(Vec3);
				val.GetCameraParamsFromCameraScript(camera, ref val2);
			}
			GameEntity val3 = scene.FindEntityWithTag(text3);
			if (val3 != (GameEntity)null)
			{
				itemFrame = val3.GetGlobalFrame();
				val3.SetVisibilityExcludeParents(false);
			}
		}
		else
		{
			GameEntity val4 = scene.FindEntityWithTag(customPoseParameters.CameraTag);
			if (val4 != (GameEntity)null)
			{
				camera = Camera.CreateCamera();
				Vec3 val5 = default(Vec3);
				val4.GetCameraParamsFromCameraScript(camera, ref val5);
			}
			GameEntity val6 = scene.FindEntityWithTag(customPoseParameters.FrameTag);
			if (val6 != (GameEntity)null)
			{
				itemFrame = val6.GetGlobalFrame();
				val6.SetVisibilityExcludeParents(false);
				val6.UpdateGlobalBounds();
				MatrixFrame globalFrame = val6.GetGlobalFrame();
				MetaMesh itemMeshForInventory = ItemCollectionElementViewExtensions.GetItemMeshForInventory(new ItemRosterElement(item, 0, (ItemModifier)null));
				Vec3 val7 = default(Vec3);
				((Vec3)(ref val7))._002Ector(1000000f, 1000000f, 1000000f, -1f);
				Vec3 val8 = default(Vec3);
				((Vec3)(ref val8))._002Ector(-1000000f, -1000000f, -1000000f, -1f);
				Vec3 val9;
				if ((NativeObject)(object)itemMeshForInventory != (NativeObject)null)
				{
					_ = MatrixFrame.Identity;
					for (int i = 0; i != itemMeshForInventory.MeshCount; i++)
					{
						Vec3 boundingBoxMin = itemMeshForInventory.GetMeshAtIndex(i).GetBoundingBoxMin();
						Vec3 boundingBoxMax = itemMeshForInventory.GetMeshAtIndex(i).GetBoundingBoxMax();
						Vec3[] array = (Vec3[])(object)new Vec3[8];
						val9 = new Vec3(boundingBoxMin.x, boundingBoxMin.y, boundingBoxMin.z, -1f);
						array[0] = ((MatrixFrame)(ref globalFrame)).TransformToParent(ref val9);
						val9 = new Vec3(boundingBoxMin.x, boundingBoxMin.y, boundingBoxMax.z, -1f);
						array[1] = ((MatrixFrame)(ref globalFrame)).TransformToParent(ref val9);
						val9 = new Vec3(boundingBoxMin.x, boundingBoxMax.y, boundingBoxMin.z, -1f);
						array[2] = ((MatrixFrame)(ref globalFrame)).TransformToParent(ref val9);
						val9 = new Vec3(boundingBoxMin.x, boundingBoxMax.y, boundingBoxMax.z, -1f);
						array[3] = ((MatrixFrame)(ref globalFrame)).TransformToParent(ref val9);
						val9 = new Vec3(boundingBoxMax.x, boundingBoxMin.y, boundingBoxMin.z, -1f);
						array[4] = ((MatrixFrame)(ref globalFrame)).TransformToParent(ref val9);
						val9 = new Vec3(boundingBoxMax.x, boundingBoxMin.y, boundingBoxMax.z, -1f);
						array[5] = ((MatrixFrame)(ref globalFrame)).TransformToParent(ref val9);
						val9 = new Vec3(boundingBoxMax.x, boundingBoxMax.y, boundingBoxMin.z, -1f);
						array[6] = ((MatrixFrame)(ref globalFrame)).TransformToParent(ref val9);
						val9 = new Vec3(boundingBoxMax.x, boundingBoxMax.y, boundingBoxMax.z, -1f);
						array[7] = ((MatrixFrame)(ref globalFrame)).TransformToParent(ref val9);
						for (int j = 0; j < 8; j++)
						{
							val7 = Vec3.Vec3Min(val7, array[j]);
							val8 = Vec3.Vec3Max(val8, array[j]);
						}
					}
				}
				Vec3 val10 = (val7 + val8) * 0.5f;
				val11 = val6.GetGlobalFrame();
				Vec3 val12 = ((MatrixFrame)(ref val11)).TransformToLocal(ref val10);
				MatrixFrame globalFrame2 = val6.GetGlobalFrame();
				ref Vec3 origin = ref globalFrame2.origin;
				origin -= val12;
				itemFrame = globalFrame2;
				MatrixFrame frame = camera.Frame;
				val9 = val8 - val7;
				float num = ((Vec3)(ref val9)).Length * customPoseParameters.DistanceModifier;
				ref Vec3 origin2 = ref frame.origin;
				origin2 += frame.rotation.u * num;
				if (customPoseParameters.FocusAlignment == CustomPoseParameters.Alignment.Top)
				{
					ref Vec3 origin3 = ref frame.origin;
					Vec3 val13 = origin3;
					val9 = val8 - val7;
					origin3 = val13 + new Vec3(0f, 0f, ((Vec3)(ref val9)).Z * 0.3f, -1f);
				}
				else if (customPoseParameters.FocusAlignment == CustomPoseParameters.Alignment.Bottom)
				{
					ref Vec3 origin4 = ref frame.origin;
					Vec3 val14 = origin4;
					val9 = val8 - val7;
					origin4 = val14 - new Vec3(0f, 0f, ((Vec3)(ref val9)).Z * 0.3f, -1f);
				}
				camera.Frame = frame;
			}
		}
		if ((NativeObject)(object)camera == (NativeObject)null)
		{
			camera = Camera.CreateCamera();
			camera.SetViewVolume(false, -1f, 1f, -0.5f, 0.5f, 0.01f, 100f);
			MatrixFrame identity = MatrixFrame.Identity;
			ref Vec3 origin5 = ref identity.origin;
			origin5 -= identity.rotation.u * 7f;
			ref Vec3 u = ref identity.rotation.u;
			u *= -1f;
			camera.Frame = identity;
		}
		if ((int)item.Type == 8)
		{
			GameEntity val15 = scene.FindEntityWithTag("shield_cam");
			MatrixFrame holsterFrameByIndex = MBItem.GetHolsterFrameByIndex(MBItem.GetItemHolsterIndex(item.ItemHolsters[0]));
			itemFrame.rotation = holsterFrameByIndex.rotation;
			val11 = val15.GetFrame();
			MatrixFrame frame2 = ((MatrixFrame)(ref itemFrame)).TransformToParent(ref val11);
			camera.Frame = frame2;
		}
		if ((int)item.Type == 26 && ((MBObjectBase)item).StringId == "dragon_banner_center")
		{
			((Mat3)(ref itemFrame.rotation)).RotateAboutUp(MathF.PI);
		}
	}

	private GameEntity AddItem(Scene scene, ItemObject item, MatrixFrame itemFrame, MatrixFrame itemFrame1, MatrixFrame itemFrame2)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Invalid comparison between Unknown and I4
		//IL_01e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a9: Invalid comparison between Unknown and I4
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Invalid comparison between Unknown and I4
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Invalid comparison between Unknown and I4
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Invalid comparison between Unknown and I4
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Invalid comparison between Unknown and I4
		ItemRosterElement rosterElement = default(ItemRosterElement);
		((ItemRosterElement)(ref rosterElement))._002Ector(item, 0, (ItemModifier)null);
		MetaMesh itemMeshForInventory = rosterElement.GetItemMeshForInventory();
		if (item.IsCraftedWeapon)
		{
			MatrixFrame frame = itemMeshForInventory.Frame;
			((MatrixFrame)(ref frame)).Elevate((0f - item.WeaponDesign.CraftedWeaponLength) / 2f);
			itemMeshForInventory.Frame = frame;
		}
		GameEntity val = null;
		EquipmentElement equipmentElement;
		if ((NativeObject)(object)itemMeshForInventory != (NativeObject)null)
		{
			equipmentElement = ((ItemRosterElement)(ref rosterElement)).EquipmentElement;
			if ((int)((EquipmentElement)(ref equipmentElement)).Item.ItemType == 17)
			{
				val = GameEntity.CreateEmpty(scene, true, true, true);
				AnimationSystemData val2 = MonsterExtensions.FillAnimationSystemData(Game.Current.DefaultMonster, MBActionSet.GetActionSet(Game.Current.DefaultMonster.ActionSetCode), 1f, false);
				GameEntityExtensions.CreateSkeletonWithActionSet(val, ref val2);
				val.SetFrame(ref itemFrame, true);
				MBSkeletonExtensions.SetAgentActionChannel(val.Skeleton, 0, ref ActionIndexCache.act_tableau_hand_armor_pose, 0f, -0.2f, true, 0f);
				val.AddMultiMeshToSkeleton(itemMeshForInventory);
				val.Skeleton.TickAnimationsAndForceUpdate(0.01f, itemFrame, true);
				goto IL_0205;
			}
		}
		if ((NativeObject)(object)itemMeshForInventory != (NativeObject)null)
		{
			if (item.WeaponComponent != null)
			{
				WeaponClass weaponClass = item.WeaponComponent.PrimaryWeapon.WeaponClass;
				if ((int)weaponClass == 21 || (int)weaponClass == 22 || (int)weaponClass == 23 || (int)weaponClass == 13)
				{
					val = GameEntity.CreateEmpty(scene, true, true, true);
					MetaMesh val3 = itemMeshForInventory.CreateCopy();
					val3.Frame = itemFrame;
					val.AddMultiMesh(val3, true);
					MetaMesh val4 = itemMeshForInventory.CreateCopy();
					val4.Frame = itemFrame1;
					val.AddMultiMesh(val4, true);
					MetaMesh val5 = itemMeshForInventory.CreateCopy();
					val5.Frame = itemFrame2;
					val.AddMultiMesh(val5, true);
				}
				else
				{
					val = scene.AddItemEntity(ref itemFrame, itemMeshForInventory);
				}
			}
			else
			{
				val = scene.AddItemEntity(ref itemFrame, itemMeshForInventory);
				if ((int)item.Type == 25 && item.ArmorComponent != null)
				{
					MetaMesh copy = MetaMesh.GetCopy(item.ArmorComponent.ReinsMesh, true, true);
					if ((NativeObject)(object)copy != (NativeObject)null)
					{
						val.AddMultiMesh(copy, true);
					}
				}
			}
		}
		else
		{
			equipmentElement = ((ItemRosterElement)(ref rosterElement)).EquipmentElement;
			MBDebug.ShowWarning("[DEBUG]Item with " + ((MBObjectBase)((EquipmentElement)(ref equipmentElement)).Item).StringId + "[DEBUG] string id cannot be found");
		}
		goto IL_0205;
		IL_0205:
		val.SetVisibilityExcludeParents(false);
		return val;
	}

	private void GetItemPoseAndCameraForCraftedItem(ItemObject item, Scene scene, ref Camera camera, ref MatrixFrame itemFrame, ref MatrixFrame itemFrame1, ref MatrixFrame itemFrame2)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Invalid comparison between Unknown and I4
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Invalid comparison between Unknown and I4
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_018a: Invalid comparison between Unknown and I4
		//IL_019d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01be: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0201: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		//IL_018e: Invalid comparison between Unknown and I4
		//IL_0206: Unknown result type (might be due to invalid IL or missing references)
		//IL_0208: Invalid comparison between Unknown and I4
		//IL_0280: Unknown result type (might be due to invalid IL or missing references)
		//IL_0283: Invalid comparison between Unknown and I4
		//IL_0217: Unknown result type (might be due to invalid IL or missing references)
		//IL_0231: Unknown result type (might be due to invalid IL or missing references)
		//IL_0236: Unknown result type (might be due to invalid IL or missing references)
		//IL_0238: Unknown result type (might be due to invalid IL or missing references)
		//IL_023d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0241: Unknown result type (might be due to invalid IL or missing references)
		//IL_0243: Unknown result type (might be due to invalid IL or missing references)
		//IL_0248: Unknown result type (might be due to invalid IL or missing references)
		//IL_0266: Unknown result type (might be due to invalid IL or missing references)
		//IL_026b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0276: Unknown result type (might be due to invalid IL or missing references)
		//IL_027b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0387: Unknown result type (might be due to invalid IL or missing references)
		//IL_038a: Invalid comparison between Unknown and I4
		//IL_0295: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02da: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0307: Unknown result type (might be due to invalid IL or missing references)
		//IL_030c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0317: Unknown result type (might be due to invalid IL or missing references)
		//IL_031c: Unknown result type (might be due to invalid IL or missing references)
		//IL_033a: Unknown result type (might be due to invalid IL or missing references)
		//IL_033f: Unknown result type (might be due to invalid IL or missing references)
		//IL_034a: Unknown result type (might be due to invalid IL or missing references)
		//IL_034f: Unknown result type (might be due to invalid IL or missing references)
		//IL_036d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0372: Unknown result type (might be due to invalid IL or missing references)
		//IL_037d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0382: Unknown result type (might be due to invalid IL or missing references)
		//IL_0475: Unknown result type (might be due to invalid IL or missing references)
		//IL_0478: Invalid comparison between Unknown and I4
		//IL_039c: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_03bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_03bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_03fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0405: Unknown result type (might be due to invalid IL or missing references)
		//IL_040a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0428: Unknown result type (might be due to invalid IL or missing references)
		//IL_042d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0438: Unknown result type (might be due to invalid IL or missing references)
		//IL_043d: Unknown result type (might be due to invalid IL or missing references)
		//IL_045b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0460: Unknown result type (might be due to invalid IL or missing references)
		//IL_046b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0470: Unknown result type (might be due to invalid IL or missing references)
		//IL_0563: Unknown result type (might be due to invalid IL or missing references)
		//IL_0566: Invalid comparison between Unknown and I4
		//IL_048a: Unknown result type (might be due to invalid IL or missing references)
		//IL_04a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_04a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_04c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_04c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0516: Unknown result type (might be due to invalid IL or missing references)
		//IL_051b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0526: Unknown result type (might be due to invalid IL or missing references)
		//IL_052b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0549: Unknown result type (might be due to invalid IL or missing references)
		//IL_054e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0559: Unknown result type (might be due to invalid IL or missing references)
		//IL_055e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0587: Unknown result type (might be due to invalid IL or missing references)
		//IL_05a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_05a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_05a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_05ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_05b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_05b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_05b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_05d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_05db: Unknown result type (might be due to invalid IL or missing references)
		//IL_05e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_05eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0568: Unknown result type (might be due to invalid IL or missing references)
		//IL_056b: Invalid comparison between Unknown and I4
		//IL_056d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0570: Invalid comparison between Unknown and I4
		//IL_0572: Unknown result type (might be due to invalid IL or missing references)
		//IL_0574: Invalid comparison between Unknown and I4
		//IL_0576: Unknown result type (might be due to invalid IL or missing references)
		//IL_0578: Invalid comparison between Unknown and I4
		if ((NativeObject)(object)camera == (NativeObject)null)
		{
			camera = Camera.CreateCamera();
		}
		itemFrame = MatrixFrame.Identity;
		WeaponClass weaponClass = item.WeaponDesign.Template.WeaponDescriptions[0].WeaponClass;
		Vec3 u = itemFrame.rotation.u;
		Vec3 val = itemFrame.origin - u * (item.WeaponDesign.CraftedWeaponLength * 0.5f);
		Vec3 val2 = val + u * item.WeaponDesign.CraftedWeaponLength;
		Vec3 val3 = val - u * item.WeaponDesign.BottomPivotOffset;
		int num = 0;
		Vec3 val4 = default(Vec3);
		foreach (float topPivotOffset in item.WeaponDesign.TopPivotOffsets)
		{
			if (!(topPivotOffset <= MathF.Abs(1E-05f)))
			{
				Vec3 val5 = val + u * topPivotOffset;
				if (num == 1)
				{
					val4 = val5;
				}
				_ = 2;
				num++;
			}
		}
		if ((int)weaponClass == 2 || (int)weaponClass == 3)
		{
			GameEntity obj = scene.FindEntityWithTag("sword_camera");
			Vec3 val6 = default(Vec3);
			obj.GetCameraParamsFromCameraScript(camera, ref val6);
			obj.SetVisibilityExcludeParents(false);
			Vec3 val7 = ((MatrixFrame)(ref itemFrame)).TransformToLocal(ref val3);
			MatrixFrame identity = MatrixFrame.Identity;
			identity.origin = -val7;
			GameEntity val8 = scene.FindEntityWithTag("sword");
			val8.SetVisibilityExcludeParents(false);
			itemFrame = val8.GetGlobalFrame();
			itemFrame = ((MatrixFrame)(ref itemFrame)).TransformToParent(ref identity);
		}
		if ((int)weaponClass == 4 || (int)weaponClass == 5)
		{
			GameEntity obj2 = scene.FindEntityWithTag("axe_camera");
			Vec3 val9 = default(Vec3);
			obj2.GetCameraParamsFromCameraScript(camera, ref val9);
			obj2.SetVisibilityExcludeParents(false);
			Vec3 val10 = ((MatrixFrame)(ref itemFrame)).TransformToLocal(ref val4);
			MatrixFrame identity2 = MatrixFrame.Identity;
			identity2.origin = -val10;
			GameEntity val11 = scene.FindEntityWithTag("axe");
			val11.SetVisibilityExcludeParents(false);
			itemFrame = val11.GetGlobalFrame();
			itemFrame = ((MatrixFrame)(ref itemFrame)).TransformToParent(ref identity2);
		}
		if ((int)weaponClass == 1)
		{
			GameEntity obj3 = scene.FindEntityWithTag("sword_camera");
			Vec3 val12 = default(Vec3);
			obj3.GetCameraParamsFromCameraScript(camera, ref val12);
			obj3.SetVisibilityExcludeParents(false);
			Vec3 val13 = ((MatrixFrame)(ref itemFrame)).TransformToLocal(ref val3);
			MatrixFrame identity3 = MatrixFrame.Identity;
			identity3.origin = -val13;
			GameEntity val14 = scene.FindEntityWithTag("sword");
			val14.SetVisibilityExcludeParents(false);
			itemFrame = val14.GetGlobalFrame();
			itemFrame = ((MatrixFrame)(ref itemFrame)).TransformToParent(ref identity3);
		}
		if ((int)weaponClass == 21)
		{
			GameEntity obj4 = scene.FindEntityWithTag("throwing_axe_camera");
			Vec3 val15 = default(Vec3);
			obj4.GetCameraParamsFromCameraScript(camera, ref val15);
			obj4.SetVisibilityExcludeParents(false);
			Vec3 val16 = val + u * item.PrimaryWeapon.CenterOfMass;
			Vec3 val17 = ((MatrixFrame)(ref itemFrame)).TransformToLocal(ref val16);
			MatrixFrame identity4 = MatrixFrame.Identity;
			identity4.origin = -val17 * 2.5f;
			GameEntity val18 = scene.FindEntityWithTag("throwing_axe");
			val18.SetVisibilityExcludeParents(false);
			itemFrame = val18.GetGlobalFrame();
			itemFrame = ((MatrixFrame)(ref itemFrame)).TransformToParent(ref identity4);
			val18 = scene.FindEntityWithTag("throwing_axe_1");
			val18.SetVisibilityExcludeParents(false);
			itemFrame1 = val18.GetGlobalFrame();
			itemFrame1 = ((MatrixFrame)(ref itemFrame1)).TransformToParent(ref identity4);
			val18 = scene.FindEntityWithTag("throwing_axe_2");
			val18.SetVisibilityExcludeParents(false);
			itemFrame2 = val18.GetGlobalFrame();
			itemFrame2 = ((MatrixFrame)(ref itemFrame2)).TransformToParent(ref identity4);
		}
		if ((int)weaponClass == 23)
		{
			GameEntity obj5 = scene.FindEntityWithTag("javelin_camera");
			Vec3 val19 = default(Vec3);
			obj5.GetCameraParamsFromCameraScript(camera, ref val19);
			obj5.SetVisibilityExcludeParents(false);
			Vec3 val20 = ((MatrixFrame)(ref itemFrame)).TransformToLocal(ref val4);
			MatrixFrame identity5 = MatrixFrame.Identity;
			identity5.origin = -val20 * 2.2f;
			GameEntity val21 = scene.FindEntityWithTag("javelin");
			val21.SetVisibilityExcludeParents(false);
			itemFrame = val21.GetGlobalFrame();
			itemFrame = ((MatrixFrame)(ref itemFrame)).TransformToParent(ref identity5);
			val21 = scene.FindEntityWithTag("javelin_1");
			val21.SetVisibilityExcludeParents(false);
			itemFrame1 = val21.GetGlobalFrame();
			itemFrame1 = ((MatrixFrame)(ref itemFrame1)).TransformToParent(ref identity5);
			val21 = scene.FindEntityWithTag("javelin_2");
			val21.SetVisibilityExcludeParents(false);
			itemFrame2 = val21.GetGlobalFrame();
			itemFrame2 = ((MatrixFrame)(ref itemFrame2)).TransformToParent(ref identity5);
		}
		if ((int)weaponClass == 22)
		{
			GameEntity obj6 = scene.FindEntityWithTag("javelin_camera");
			Vec3 val22 = default(Vec3);
			obj6.GetCameraParamsFromCameraScript(camera, ref val22);
			obj6.SetVisibilityExcludeParents(false);
			Vec3 val23 = ((MatrixFrame)(ref itemFrame)).TransformToLocal(ref val2);
			MatrixFrame identity6 = MatrixFrame.Identity;
			identity6.origin = -val23 * 1.4f;
			GameEntity val24 = scene.FindEntityWithTag("javelin");
			val24.SetVisibilityExcludeParents(false);
			itemFrame = val24.GetGlobalFrame();
			itemFrame = ((MatrixFrame)(ref itemFrame)).TransformToParent(ref identity6);
			val24 = scene.FindEntityWithTag("javelin_1");
			val24.SetVisibilityExcludeParents(false);
			itemFrame1 = val24.GetGlobalFrame();
			itemFrame1 = ((MatrixFrame)(ref itemFrame1)).TransformToParent(ref identity6);
			val24 = scene.FindEntityWithTag("javelin_2");
			val24.SetVisibilityExcludeParents(false);
			itemFrame2 = val24.GetGlobalFrame();
			itemFrame2 = ((MatrixFrame)(ref itemFrame2)).TransformToParent(ref identity6);
		}
		if ((int)weaponClass == 10 || (int)weaponClass == 9 || (int)weaponClass == 11 || (int)weaponClass == 6 || (int)weaponClass == 8)
		{
			GameEntity obj7 = scene.FindEntityWithTag("spear_camera");
			Vec3 val25 = default(Vec3);
			obj7.GetCameraParamsFromCameraScript(camera, ref val25);
			obj7.SetVisibilityExcludeParents(false);
			Vec3 val26 = ((MatrixFrame)(ref itemFrame)).TransformToLocal(ref val4);
			MatrixFrame identity7 = MatrixFrame.Identity;
			identity7.origin = -val26;
			GameEntity val27 = scene.FindEntityWithTag("spear");
			val27.SetVisibilityExcludeParents(false);
			itemFrame = val27.GetGlobalFrame();
			itemFrame = ((MatrixFrame)(ref itemFrame)).TransformToParent(ref identity7);
		}
	}
}
