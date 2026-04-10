using System;
using TaleWorlds.Core;
using TaleWorlds.DotNet;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.ObjectSystem;

namespace TaleWorlds.MountAndBlade.View.Tableaus.Thumbnails;

public class CraftingPieceThumbnailCache : ThumbnailCache<CraftingPieceCreationData>
{
	private int _itemTableauGPUAllocationIndex;

	public CraftingPieceThumbnailCache(int capacity)
		: base(capacity)
	{
	}

	protected override void OnInitialize()
	{
		base.OnInitialize();
		_itemTableauGPUAllocationIndex = Utilities.RegisterGPUAllocationGroup("CraftingPieceThumbnailCache");
	}

	protected override void OnFinalize()
	{
		base.OnFinalize();
	}

	protected override TextureCreationInfo OnCreateTexture(CraftingPieceCreationData thumbnailCreationData)
	{
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		CraftingPiece craftingPiece = thumbnailCreationData.CraftingPiece;
		string type = thumbnailCreationData.Type;
		Action<Texture> setAction = thumbnailCreationData.SetAction;
		Action cancelAction = thumbnailCreationData.CancelAction;
		string text = ((MBObjectBase)craftingPiece).StringId + "$" + type;
		if (((IThumbnailCache)this).GetValue(text, out Texture texture))
		{
			if (_renderCallbacks.ContainsKey(text))
			{
				_renderCallbacks[text].SetActions.Add(setAction);
				_renderCallbacks[text].CancelActions.Add(cancelAction);
			}
			else
			{
				setAction?.Invoke(texture);
			}
			((IThumbnailCache)this).AddReference(text);
			return TextureCreationInfo.WithExistingTexture(texture);
		}
		Camera camera = null;
		int num = 2;
		int num2 = 256;
		int num3 = 180;
		GameEntity val = CreateCraftingPieceBaseEntity(craftingPiece, type, BannerlordTableauManager.TableauCharacterScenes[num], ref camera);
		string text2 = ThumbnailCache<CraftingPieceCreationData>.CreateDebugIdFrom(text, "crf");
		ThumbnailRenderRequest val2 = ThumbnailRenderRequest.CreateWithoutTexture(BannerlordTableauManager.TableauCharacterScenes[num], camera, val, text, num2, num3, text2, _itemTableauGPUAllocationIndex);
		_thumbnailCreatorView.RegisterRenderRequest(ref val2);
		((NativeObject)val).ManualInvalidate();
		((IThumbnailCache)this).Add(text, (Texture)null);
		((IThumbnailCache)this).AddReference(text);
		if (!_renderCallbacks.ContainsKey(text))
		{
			_renderCallbacks.Add(text, RenderCallbackCollection.CreateEmpty());
		}
		_renderCallbacks[text].SetActions.Add(setAction);
		_renderCallbacks[text].CancelActions.Add(cancelAction);
		return TextureCreationInfo.WithNewTexture();
	}

	protected override bool OnReleaseTexture(CraftingPieceCreationData thumbnailCreationData)
	{
		string renderId = thumbnailCreationData.RenderId;
		return ((IThumbnailCache)this).RemoveReference(renderId);
	}

	private GameEntity CreateCraftingPieceBaseEntity(CraftingPiece craftingPiece, string ItemType, Scene scene, ref Camera camera)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Invalid comparison between Unknown and I4
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Invalid comparison between Unknown and I4
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Invalid comparison between Unknown and I4
		//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0208: Unknown result type (might be due to invalid IL or missing references)
		//IL_020d: Unknown result type (might be due to invalid IL or missing references)
		//IL_020f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0214: Unknown result type (might be due to invalid IL or missing references)
		//IL_021e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0223: Unknown result type (might be due to invalid IL or missing references)
		//IL_0228: Unknown result type (might be due to invalid IL or missing references)
		//IL_023a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0244: Unknown result type (might be due to invalid IL or missing references)
		//IL_0249: Unknown result type (might be due to invalid IL or missing references)
		//IL_0251: Unknown result type (might be due to invalid IL or missing references)
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_025d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0262: Unknown result type (might be due to invalid IL or missing references)
		MatrixFrame val = MatrixFrame.Identity;
		bool flag = false;
		string text = "craftingPiece_cam";
		string text2 = "craftingPiece_frame";
		if ((int)craftingPiece.PieceType == 0)
		{
			switch (ItemType)
			{
			case "OneHandedAxe":
			case "ThrowingAxe":
				text = "craft_axe_camera";
				text2 = "craft_axe";
				break;
			case "TwoHandedAxe":
				text = "craft_big_axe_camera";
				text2 = "craft_big_axe";
				break;
			case "Dagger":
			case "ThrowingKnife":
			case "TwoHandedPolearm":
			case "Pike":
			case "Javelin":
				text = "craft_spear_blade_camera";
				text2 = "craft_spear_blade";
				break;
			case "Mace":
			case "TwoHandedMace":
				text = "craft_mace_camera";
				text2 = "craft_mace";
				break;
			default:
				text = "craft_blade_camera";
				text2 = "craft_blade";
				break;
			}
			flag = true;
		}
		else if ((int)craftingPiece.PieceType == 3)
		{
			text = "craft_pommel_camera";
			text2 = "craft_pommel";
			flag = true;
		}
		else if ((int)craftingPiece.PieceType == 1)
		{
			text = "craft_guard_camera";
			text2 = "craft_guard";
			flag = true;
		}
		else if ((int)craftingPiece.PieceType == 2)
		{
			text = "craft_handle_camera";
			text2 = "craft_handle";
			flag = true;
		}
		bool flag2 = false;
		if (flag)
		{
			GameEntity val2 = scene.FindEntityWithTag(text);
			if (val2 != (GameEntity)null)
			{
				camera = Camera.CreateCamera();
				Vec3 val3 = default(Vec3);
				val2.GetCameraParamsFromCameraScript(camera, ref val3);
			}
			GameEntity val4 = scene.FindEntityWithTag(text2);
			if (val4 != (GameEntity)null)
			{
				val = val4.GetGlobalFrame();
				val4.SetVisibilityExcludeParents(false);
				flag2 = true;
			}
		}
		else
		{
			GameEntity val5 = scene.FindEntityWithTag("old_system_item_frame");
			if (val5 != (GameEntity)null)
			{
				val = val5.GetGlobalFrame();
				val5.SetVisibilityExcludeParents(false);
			}
		}
		if ((NativeObject)(object)camera == (NativeObject)null)
		{
			camera = Camera.CreateCamera();
			camera.SetViewVolume(false, -1f, 1f, -0.5f, 0.5f, 0.01f, 100f);
			MatrixFrame identity = MatrixFrame.Identity;
			ref Vec3 origin = ref identity.origin;
			origin -= identity.rotation.u * 7f;
			ref Vec3 u = ref identity.rotation.u;
			u *= -1f;
			camera.Frame = identity;
		}
		if (!flag2)
		{
			val = craftingPiece.GetCraftingPieceFrameForInventory();
		}
		MetaMesh copy = MetaMesh.GetCopy(craftingPiece.MeshName, true, false);
		GameEntity val6 = null;
		if ((NativeObject)(object)copy != (NativeObject)null)
		{
			val6 = scene.AddItemEntity(ref val, copy);
		}
		else
		{
			MBDebug.ShowWarning("[DEBUG]craftingPiece with " + ((MBObjectBase)craftingPiece).StringId + "[DEBUG] string id cannot be found");
		}
		val6.SetVisibilityExcludeParents(false);
		return val6;
	}
}
