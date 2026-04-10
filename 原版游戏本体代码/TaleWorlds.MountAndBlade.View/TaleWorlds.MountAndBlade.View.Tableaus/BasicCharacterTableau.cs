using System;
using System.Collections.Generic;
using System.Globalization;
using TaleWorlds.Core;
using TaleWorlds.DotNet;
using TaleWorlds.Engine;
using TaleWorlds.Engine.Options;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.View.Tableaus.Thumbnails;

namespace TaleWorlds.MountAndBlade.View.Tableaus;

public class BasicCharacterTableau
{
	private static int _tableauIndex;

	private bool _isVersionCompatible;

	private const int _expectedCharacterCodeVersion = 4;

	private bool _initialized;

	private int _tableauSizeX;

	private int _tableauSizeY;

	private float RenderScale = 1f;

	private float _cameraRatio;

	private List<string> _equipmentMeshes;

	private List<bool> _equipmentHasColors;

	private List<bool> _equipmentHasGenderVariations;

	private List<bool> _equipmentHasTableau;

	private uint _clothColor1;

	private uint _clothColor2;

	private MatrixFrame _mountSpawnPoint;

	private MatrixFrame _initialSpawnFrame;

	private Scene _tableauScene;

	private SkinMask _skinMeshesMask;

	private bool _isFemale;

	private string _skeletonName;

	private string _characterCode;

	private UnderwearTypes _underwearType;

	private string _mountMeshName;

	private string _mountCreationKey;

	private string _mountMaterialName;

	private uint _mountManeMeshMultiplier;

	private BodyMeshTypes _bodyMeshType;

	private HairCoverTypes _hairCoverType;

	private BeardCoverTypes _beardCoverType;

	private BodyDeformTypes _bodyDeformType;

	private string _mountSkeletonName;

	private string _mountIdleAnimationName;

	private string _mountHarnessMeshName;

	private string _mountReinsMeshName;

	private string[] _maneMeshNames;

	private bool _mountHarnessHasColors;

	private bool _isFirstFrame;

	private float _faceDirtAmount;

	private float _mainCharacterRotation;

	private bool _isVisualsDirty;

	private bool _isRotatingCharacter;

	private bool _isEnabled;

	private int _race;

	private readonly GameEntity[] _currentCharacters;

	private readonly GameEntity[] _currentMounts;

	private int _currentEntityToShowIndex;

	private bool _checkWhetherEntitiesAreReady;

	private BodyProperties _bodyProperties = BodyProperties.Default;

	private Banner _banner;

	public Texture Texture { get; private set; }

	public bool IsVersionCompatible => _isVersionCompatible;

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

	public BasicCharacterTableau()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		_isFirstFrame = true;
		_isVisualsDirty = false;
		_bodyProperties = BodyProperties.Default;
		_currentCharacters = (GameEntity[])(object)new GameEntity[2];
		_currentCharacters[0] = null;
		_currentCharacters[1] = null;
		_currentMounts = (GameEntity[])(object)new GameEntity[2];
		_currentMounts[0] = null;
		_currentMounts[1] = null;
	}

	public void OnTick(float dt)
	{
		if (_isEnabled && _isRotatingCharacter)
		{
			UpdateCharacterRotation((int)Input.MouseMoveX);
		}
		TableauView view = View;
		if (view != null)
		{
			view.SetDoNotRenderThisFrame(false);
		}
		if (_isFirstFrame)
		{
			FirstTimeInit();
			_isFirstFrame = false;
		}
		if (_isVisualsDirty)
		{
			RefreshCharacterTableau();
			_isVisualsDirty = false;
		}
		if (_checkWhetherEntitiesAreReady)
		{
			int num = (_currentEntityToShowIndex + 1) % 2;
			bool flag = true;
			if (!_currentCharacters[_currentEntityToShowIndex].CheckResources(true, true))
			{
				flag = false;
			}
			if (!_currentMounts[_currentEntityToShowIndex].CheckResources(true, true))
			{
				flag = false;
			}
			if (!flag)
			{
				_currentCharacters[_currentEntityToShowIndex].SetVisibilityExcludeParents(false);
				_currentMounts[_currentEntityToShowIndex].SetVisibilityExcludeParents(false);
				_currentCharacters[num].SetVisibilityExcludeParents(true);
				_currentMounts[num].SetVisibilityExcludeParents(true);
			}
			else
			{
				_currentCharacters[_currentEntityToShowIndex].SetVisibilityExcludeParents(true);
				_currentMounts[_currentEntityToShowIndex].SetVisibilityExcludeParents(true);
				_currentCharacters[num].SetVisibilityExcludeParents(false);
				_currentMounts[num].SetVisibilityExcludeParents(false);
				_checkWhetherEntitiesAreReady = false;
			}
		}
	}

	private void UpdateCharacterRotation(int mouseMoveX)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		if (_initialized && _skeletonName != null)
		{
			_mainCharacterRotation += (float)mouseMoveX * 0.005f;
			MatrixFrame initialSpawnFrame = _initialSpawnFrame;
			((Mat3)(ref initialSpawnFrame.rotation)).RotateAboutUp(_mainCharacterRotation);
			_currentCharacters[0].SetFrame(ref initialSpawnFrame, true);
			_currentCharacters[1].SetFrame(ref initialSpawnFrame, true);
		}
	}

	private void SetEnabled(bool enabled)
	{
		_isEnabled = enabled;
		TableauView view = View;
		if (view != null)
		{
			((View)view).SetEnable(_isEnabled);
		}
	}

	public void SetTargetSize(int width, int height)
	{
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Expected O, but got Unknown
		_isRotatingCharacter = false;
		if (width <= 0 || height <= 0)
		{
			_tableauSizeX = 10;
			_tableauSizeY = 10;
		}
		else
		{
			RenderScale = NativeOptions.GetConfig((NativeOptionsType)24) / 100f;
			_tableauSizeX = (int)((float)width * RenderScale);
			_tableauSizeY = (int)((float)height * RenderScale);
		}
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
		Texture = TableauView.AddTableau($"BasicCharacterTableau_{_tableauIndex++}", new TextureUpdateEventHandler(CharacterTableauContinuousRenderFunction), (object)_tableauScene, _tableauSizeX, _tableauSizeY);
		((SceneView)View).SetScene(_tableauScene);
		((SceneView)View).SetSceneUsesSkybox(false);
		View.SetDeleteAfterRendering(false);
		View.SetContinuousRendering(true);
		View.SetDoNotRenderThisFrame(true);
		((View)View).SetClearColor(0u);
		((SceneView)View).SetFocusedShadowmap(true, ref _initialSpawnFrame.origin, 1.55f);
		((SceneView)View).SetRenderWithPostfx(true);
		SetCamera();
	}

	public void OnFinalize()
	{
		TableauView view = View;
		if (view != null)
		{
			((View)view).SetEnable(false);
		}
		TableauView view2 = View;
		if (view2 != null)
		{
			((SceneView)view2).AddClearTask(false);
		}
		Texture = null;
		_banner = null;
		if ((NativeObject)(object)_tableauScene != (NativeObject)null)
		{
			((NativeObject)_tableauScene).ManualInvalidate();
			_tableauScene = null;
		}
	}

	public void DeserializeCharacterCode(string code)
	{
		//IL_0184: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f8: Invalid comparison between Unknown and I4
		//IL_01ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f2: Unknown result type (might be due to invalid IL or missing references)
		if (!(code != _characterCode))
		{
			return;
		}
		if (_initialized)
		{
			ResetProperties();
		}
		_characterCode = code;
		string[] array = code.Split(new char[1] { '|' });
		if (int.TryParse(array[0], out var result) && 4 == result)
		{
			_isVersionCompatible = true;
			int num = 0;
			try
			{
				num++;
				_skeletonName = array[num];
				num++;
				Enum.TryParse<SkinMask>(array[num], ignoreCase: false, out _skinMeshesMask);
				num++;
				bool.TryParse(array[num], out _isFemale);
				num++;
				_race = int.Parse(array[num]);
				num++;
				Enum.TryParse<UnderwearTypes>(array[num], ignoreCase: false, out _underwearType);
				num++;
				Enum.TryParse<BodyMeshTypes>(array[num], ignoreCase: false, out _bodyMeshType);
				num++;
				Enum.TryParse<HairCoverTypes>(array[num], ignoreCase: false, out _hairCoverType);
				num++;
				Enum.TryParse<BeardCoverTypes>(array[num], ignoreCase: false, out _beardCoverType);
				num++;
				Enum.TryParse<BodyDeformTypes>(array[num], ignoreCase: false, out _bodyDeformType);
				num++;
				float.TryParse(array[num], NumberStyles.Any, CultureInfo.InvariantCulture, out _faceDirtAmount);
				num++;
				BodyProperties.FromString(array[num], ref _bodyProperties);
				num++;
				uint.TryParse(array[num], out _clothColor1);
				num++;
				uint.TryParse(array[num], out _clothColor2);
				_equipmentMeshes = new List<string>();
				_equipmentHasColors = new List<bool>();
				_equipmentHasGenderVariations = new List<bool>();
				_equipmentHasTableau = new List<bool>();
				for (EquipmentIndex val = (EquipmentIndex)5; (int)val < 10; val = (EquipmentIndex)(val + 1))
				{
					num++;
					_equipmentMeshes.Add(array[num]);
					num++;
					bool.TryParse(array[num], out var result2);
					_equipmentHasColors.Add(result2);
					num++;
					bool.TryParse(array[num], out var result3);
					_equipmentHasGenderVariations.Add(result3);
					num++;
					bool.TryParse(array[num], out var result4);
					_equipmentHasTableau.Add(result4);
				}
				num++;
				_mountSkeletonName = array[num];
				num++;
				_mountMeshName = array[num];
				num++;
				_mountCreationKey = array[num];
				num++;
				_mountMaterialName = array[num];
				num++;
				if (array[num].Length > 0)
				{
					uint.TryParse(array[num], out _mountManeMeshMultiplier);
				}
				else
				{
					_mountManeMeshMultiplier = 0u;
				}
				num++;
				_mountIdleAnimationName = array[num];
				num++;
				_mountHarnessMeshName = array[num];
				num++;
				bool.TryParse(array[num], out _mountHarnessHasColors);
				num++;
				_mountReinsMeshName = array[num];
				num++;
				int num2 = int.Parse(array[num]);
				_maneMeshNames = new string[num2];
				for (int i = 0; i < num2; i++)
				{
					num++;
					_maneMeshNames[i] = array[num];
				}
			}
			catch (Exception ex)
			{
				ResetProperties();
				Debug.FailedAssert("Exception: " + ex.Message, "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.View\\Tableaus\\BasicCharacterTableau.cs", "DeserializeCharacterCode", 348);
				Debug.FailedAssert("Couldn't parse character code: " + code, "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.View\\Tableaus\\BasicCharacterTableau.cs", "DeserializeCharacterCode", 349);
			}
		}
		_isVisualsDirty = true;
	}

	private void ResetProperties()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		_skeletonName = string.Empty;
		_skinMeshesMask = (SkinMask)0;
		_isFemale = false;
		_underwearType = (UnderwearTypes)0;
		_bodyMeshType = (BodyMeshTypes)0;
		_hairCoverType = (HairCoverTypes)0;
		_beardCoverType = (BeardCoverTypes)0;
		_bodyDeformType = (BodyDeformTypes)0;
		_faceDirtAmount = 0f;
		_bodyProperties = BodyProperties.Default;
		_clothColor1 = 0u;
		_clothColor2 = 0u;
		_equipmentMeshes?.Clear();
		_equipmentHasColors?.Clear();
		_mountSkeletonName = string.Empty;
		_mountMeshName = string.Empty;
		_mountCreationKey = string.Empty;
		_mountMaterialName = string.Empty;
		_mountManeMeshMultiplier = uint.MaxValue;
		_mountIdleAnimationName = string.Empty;
		_mountHarnessMeshName = string.Empty;
		_mountReinsMeshName = string.Empty;
		_maneMeshNames = null;
		_mountHarnessHasColors = false;
		_race = 0;
		_isVersionCompatible = false;
		_characterCode = string.Empty;
	}

	private void FirstTimeInit()
	{
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		if ((NativeObject)(object)_tableauScene == (NativeObject)null)
		{
			_tableauScene = Scene.CreateNewScene(true, false, (DecalAtlasGroup)0, "mono_renderscene");
			_tableauScene.SetName("BasicCharacterTableau");
			_tableauScene.DisableStaticShadows(true);
			SceneInitializationData val = default(SceneInitializationData);
			((SceneInitializationData)(ref val))._002Ector(true);
			val.InitPhysicsWorld = false;
			val.DoNotUseLoadingScreen = true;
			SceneInitializationData val2 = val;
			_tableauScene.Read("inventory_character_scene", ref val2, "");
			_tableauScene.SetShadow(true);
			_mountSpawnPoint = _tableauScene.FindEntityWithTag("horse_inv").GetGlobalFrame();
			_initialSpawnFrame = _tableauScene.FindEntityWithTag("agent_inv").GetGlobalFrame();
			_tableauScene.EnsurePostfxSystem();
			_tableauScene.SetDofMode(false);
			_tableauScene.SetMotionBlurMode(false);
			_tableauScene.SetBloom(true);
			_tableauScene.SetDynamicShadowmapCascadesRadiusMultiplier(0.31f);
			_tableauScene.RemoveEntity(_tableauScene.FindEntityWithTag("agent_inv"), 99);
			_tableauScene.RemoveEntity(_tableauScene.FindEntityWithTag("horse_inv"), 100);
			_currentCharacters[0] = GameEntity.CreateEmpty(_tableauScene, false, true, true);
			_currentCharacters[1] = GameEntity.CreateEmpty(_tableauScene, false, true, true);
			_currentMounts[0] = GameEntity.CreateEmpty(_tableauScene, false, true, true);
			_currentMounts[1] = GameEntity.CreateEmpty(_tableauScene, false, true, true);
		}
		SetEnabled(enabled: true);
		_initialized = true;
	}

	private static void ApplyBannerTextureToMesh(Mesh armorMesh, Texture bannerTexture)
	{
		if ((NativeObject)(object)armorMesh != (NativeObject)null)
		{
			Material val = armorMesh.GetMaterial().CreateCopy();
			val.SetTexture((MBTextureType)1, bannerTexture);
			uint num = (uint)val.GetShader().GetMaterialShaderFlagMask("use_tableau_blending", true);
			ulong shaderFlags = val.GetShaderFlags();
			val.SetShaderFlags(shaderFlags | num);
			armorMesh.SetMaterial(val);
		}
	}

	private void RefreshCharacterTableau()
	{
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0300: Unknown result type (might be due to invalid IL or missing references)
		//IL_0306: Unknown result type (might be due to invalid IL or missing references)
		//IL_030c: Unknown result type (might be due to invalid IL or missing references)
		//IL_032e: Expected I4, but got Unknown
		//IL_032e: Expected I4, but got Unknown
		//IL_032e: Expected I4, but got Unknown
		//IL_032e: Expected I4, but got Unknown
		//IL_032e: Expected I4, but got Unknown
		//IL_032f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0332: Unknown result type (might be due to invalid IL or missing references)
		if (!_initialized)
		{
			return;
		}
		_currentEntityToShowIndex = (_currentEntityToShowIndex + 1) % 2;
		GameEntity val = _currentCharacters[_currentEntityToShowIndex];
		val.ClearEntityComponents(true, true, true);
		GameEntity val2 = _currentMounts[_currentEntityToShowIndex];
		val2.ClearEntityComponents(true, true, true);
		_mainCharacterRotation = 0f;
		if (!string.IsNullOrEmpty(_skeletonName))
		{
			AnimationSystemData hardcodedAnimationSystemDataForHumanSkeleton = AnimationSystemData.GetHardcodedAnimationSystemDataForHumanSkeleton();
			bool flag = ((BodyProperties)(ref _bodyProperties)).Age >= 14f && _isFemale;
			val.Skeleton = MBSkeletonExtensions.CreateWithActionSet(ref hardcodedAnimationSystemDataForHumanSkeleton);
			MetaMesh val3 = null;
			bool flag2 = _equipmentMeshes[3].Length > 0;
			for (int i = 0; i < 5; i++)
			{
				string text = _equipmentMeshes[i];
				if (text.Length <= 0)
				{
					continue;
				}
				bool flag3 = flag && _equipmentHasGenderVariations[i];
				MetaMesh val4 = MetaMesh.GetCopy(flag3 ? (text + "_female") : (text + "_male"), false, true);
				if ((NativeObject)(object)val4 == (NativeObject)null)
				{
					string text2 = text;
					text2 = ((!flag3) ? (text2 + (flag2 ? "_slim" : "")) : (text2 + (flag2 ? "_converted_slim" : "_converted")));
					val4 = MetaMesh.GetCopy(text2, false, true) ?? MetaMesh.GetCopy(text, false, true);
				}
				if (!((NativeObject)(object)val4 != (NativeObject)null))
				{
					continue;
				}
				if (i == 3)
				{
					val3 = val4;
				}
				val.AddMultiMeshToSkeleton(val4);
				if (_equipmentHasTableau[i])
				{
					for (int j = 0; j < val4.MeshCount; j++)
					{
						Mesh currentMesh = val4.GetMeshAtIndex(j);
						Mesh obj = currentMesh;
						if (obj == null || obj.HasTag("dont_use_tableau"))
						{
							continue;
						}
						Mesh obj2 = currentMesh;
						if (obj2 != null && obj2.HasTag("banner_replacement_mesh") && _banner != null)
						{
							BannerTextureCreationData thumbnailCreationData = new BannerTextureCreationData(_banner, delegate(Texture t)
							{
								ApplyBannerTextureToMesh(currentMesh, t);
							}, null, BannerDebugInfo.CreateManual(GetType().Name), isTableauOrNineGrid: true, isLarge: true);
							ThumbnailCacheManager.Current.CreateTexture(thumbnailCreationData);
							break;
						}
					}
				}
				else if (_equipmentHasColors[i])
				{
					for (int num = 0; num < val4.MeshCount; num++)
					{
						Mesh meshAtIndex = val4.GetMeshAtIndex(num);
						if (!meshAtIndex.HasTag("no_team_color"))
						{
							meshAtIndex.Color = _clothColor1;
							meshAtIndex.Color2 = _clothColor2;
							Material val5 = meshAtIndex.GetMaterial().CreateCopy();
							val5.AddMaterialShaderFlag("use_double_colormap_with_mask_texture", false);
							meshAtIndex.SetMaterial(val5);
						}
					}
				}
				((NativeObject)val4).ManualInvalidate();
			}
			val.SetGlobalFrame(ref _initialSpawnFrame, true);
			SkinGenerationParams val6 = default(SkinGenerationParams);
			((SkinGenerationParams)(ref val6))._002Ector((int)_skinMeshesMask, _underwearType, (int)_bodyMeshType, (int)_hairCoverType, (int)_beardCoverType, (int)_bodyDeformType, true, _faceDirtAmount, flag ? 1 : 0, _race, false, false, 0);
			MBAgentVisuals.FillEntityWithBodyMeshesWithoutAgentVisuals(val, val6, _bodyProperties, val3);
			MBSkeletonExtensions.SetAgentActionChannel(val.Skeleton, 0, ref ActionIndexCache.act_inventory_idle, 0f, -0.2f, true, 0f);
			val.SetEnforcedMaximumLodLevel(0);
			val.CheckResources(true, true);
			if (_mountMeshName.Length > 0)
			{
				val2.Skeleton = Skeleton.CreateFromModel(_mountSkeletonName);
				MetaMesh copy = MetaMesh.GetCopy(_mountMeshName, true, false);
				if ((NativeObject)(object)copy != (NativeObject)null)
				{
					MountCreationKey mountCreationKey = MountCreationKey.FromString(_mountCreationKey);
					MountVisualCreator.SetHorseColors(copy, mountCreationKey);
					if (!string.IsNullOrEmpty(_mountMaterialName))
					{
						Material fromResource = Material.GetFromResource(_mountMaterialName);
						copy.SetMaterialToSubMeshesWithTag(fromResource, "horse_body");
						copy.SetFactorColorToSubMeshesWithTag(_mountManeMeshMultiplier, "horse_tail");
					}
					val2.AddMultiMeshToSkeleton(copy);
					((NativeObject)copy).ManualInvalidate();
					if (_mountHarnessMeshName.Length > 0)
					{
						MetaMesh copy2 = MetaMesh.GetCopy(_mountHarnessMeshName, false, true);
						if ((NativeObject)(object)copy2 != (NativeObject)null)
						{
							if (_mountReinsMeshName.Length > 0)
							{
								MetaMesh copy3 = MetaMesh.GetCopy(_mountReinsMeshName, false, true);
								if ((NativeObject)(object)copy3 != (NativeObject)null)
								{
									val2.AddMultiMeshToSkeleton(copy3);
									((NativeObject)copy3).ManualInvalidate();
								}
							}
							val2.AddMultiMeshToSkeleton(copy2);
							if (_mountHarnessHasColors)
							{
								for (int num2 = 0; num2 < copy2.MeshCount; num2++)
								{
									Mesh meshAtIndex2 = copy2.GetMeshAtIndex(num2);
									if (!meshAtIndex2.HasTag("no_team_color"))
									{
										meshAtIndex2.Color = _clothColor1;
										meshAtIndex2.Color2 = _clothColor2;
										Material val7 = meshAtIndex2.GetMaterial().CreateCopy();
										val7.AddMaterialShaderFlag("use_double_colormap_with_mask_texture", false);
										meshAtIndex2.SetMaterial(val7);
									}
								}
							}
							((NativeObject)copy2).ManualInvalidate();
						}
					}
				}
				string[] maneMeshNames = _maneMeshNames;
				for (int num3 = 0; num3 < maneMeshNames.Length; num3++)
				{
					MetaMesh copy4 = MetaMesh.GetCopy(maneMeshNames[num3], false, true);
					if (_mountManeMeshMultiplier != uint.MaxValue)
					{
						copy4.SetFactor1Linear(_mountManeMeshMultiplier);
					}
					val2.AddMultiMeshToSkeleton(copy4);
					((NativeObject)copy4).ManualInvalidate();
				}
				val2.SetGlobalFrame(ref _mountSpawnPoint, true);
				MBSkeletonExtensions.SetAnimationAtChannel(val2.Skeleton, _mountIdleAnimationName, 0, 1f, 0f, 0f);
				val2.SetEnforcedMaximumLodLevel(0);
				val2.CheckResources(true, true);
			}
		}
		_currentCharacters[_currentEntityToShowIndex].SetVisibilityExcludeParents(false);
		_currentMounts[_currentEntityToShowIndex].SetVisibilityExcludeParents(false);
		int num4 = (_currentEntityToShowIndex + 1) % 2;
		_currentCharacters[num4].SetVisibilityExcludeParents(true);
		_currentMounts[num4].SetVisibilityExcludeParents(true);
		_checkWhetherEntitiesAreReady = true;
	}

	internal void SetCamera()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		Camera val = Camera.CreateCamera();
		val.Frame = _tableauScene.FindEntityWithTag("camera_instance").GetGlobalFrame();
		val.SetFovVertical(MathF.PI / 4f, _cameraRatio, 0.2f, 200f);
		((SceneView)View).SetCamera(val);
		((NativeObject)val).ManualInvalidate();
	}

	public void RotateCharacter(bool value)
	{
		_isRotatingCharacter = value;
	}

	public void SetBannerCode(string value)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		if (string.IsNullOrEmpty(value))
		{
			_banner = null;
		}
		else
		{
			_banner = new Banner(value);
		}
	}

	internal void CharacterTableauContinuousRenderFunction(Texture sender, EventArgs e)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Expected O, but got Unknown
		Scene val = (Scene)sender.UserData;
		TableauView tableauView = sender.TableauView;
		if ((NativeObject)val == (NativeObject)null)
		{
			tableauView.SetContinuousRendering(false);
			tableauView.SetDeleteAfterRendering(true);
		}
	}
}
