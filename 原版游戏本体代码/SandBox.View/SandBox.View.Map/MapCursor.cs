using Helpers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameState;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Engine.Screens;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ScreenSystem;

namespace SandBox.View.Map;

public class MapCursor
{
	private const string GameCursorValidDecalMaterialName = "map_cursor_valid_decal";

	private const string GameCursorInvalidDecalMaterialName = "map_cursor_invalid_decal";

	private const float CursorDecalBaseScale = 0.38f;

	private GameEntity _mapCursorDecalEntity;

	private Decal _mapCursorDecal;

	private MapScreen _mapScreen;

	private Material _gameCursorValidDecalMaterial;

	private Material _gameCursorInvalidDecalMaterial;

	private Vec3 _smoothRotationNormalStart;

	private Vec3 _smoothRotationNormalEnd;

	private Vec3 _smoothRotationNormalCurrent;

	private float _smoothRotationAlpha;

	private int _smallAtlasTextureIndex;

	private float _targetCircleRotationStartTime;

	private bool _gameCursorActive;

	private bool _anotherEntityHiglighted;

	private const float _navigationPositionCheckFrequency = 0.2f;

	private float _navigatablePositionCheckTimer;

	public void Initialize(MapScreen parentMapScreen)
	{
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		_targetCircleRotationStartTime = 0f;
		_smallAtlasTextureIndex = 0;
		_mapScreen = parentMapScreen;
		Scene scene = (Campaign.Current.MapSceneWrapper as MapScene).Scene;
		_gameCursorValidDecalMaterial = Material.GetFromResource("map_cursor_valid_decal");
		_gameCursorInvalidDecalMaterial = Material.GetFromResource("map_cursor_invalid_decal");
		_mapCursorDecalEntity = GameEntity.CreateEmpty(scene, true, true, true);
		_mapCursorDecalEntity.Name = "tCursor";
		_mapCursorDecal = Decal.CreateDecal((string)null);
		_mapCursorDecal.SetMaterial(_gameCursorValidDecalMaterial);
		_mapCursorDecalEntity.AddComponent((GameEntityComponent)(object)_mapCursorDecal);
		scene.AddDecalInstance(_mapCursorDecal, "editor_set", true);
		MatrixFrame frame = _mapCursorDecalEntity.GetFrame();
		Vec3 val = new Vec3(0.38f, 0.38f, 0.38f, -1f);
		((MatrixFrame)(ref frame)).Scale(ref val);
		_mapCursorDecal.SetFrame(frame);
	}

	public void BeforeTick(float dt)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		//IL_01da: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0238: Unknown result type (might be due to invalid IL or missing references)
		//IL_023d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0242: Unknown result type (might be due to invalid IL or missing references)
		//IL_0247: Unknown result type (might be due to invalid IL or missing references)
		//IL_027c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0281: Unknown result type (might be due to invalid IL or missing references)
		//IL_029f: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
		SceneLayer sceneLayer = _mapScreen.SceneLayer;
		Camera camera = _mapScreen.MapCameraView.Camera;
		float cameraDistance = _mapScreen.MapCameraView.CameraDistance;
		Vec3 clippedMouseNear = Vec3.Zero;
		Vec3 clippedMouseFar = Vec3.Zero;
		Vec2 val = sceneLayer.SceneView.ScreenPointToViewportPoint(new Vec2(0.5f, 0.5f));
		camera.ViewportPointToWorldRay(ref clippedMouseNear, ref clippedMouseFar, val);
		PathFaceRecord currentFace = default(PathFaceRecord);
		_mapScreen.GetCursorIntersectionPoint(ref clippedMouseNear, ref clippedMouseFar, out var _, out var intersectionPoint, ref currentFace, out var isOnland, (BodyFlags)79617);
		Vec3 val2 = default(Vec3);
		sceneLayer.SceneView.ProjectedMousePositionOnGround(ref intersectionPoint, ref val2, false, (BodyFlags)0, false);
		if (_mapCursorDecalEntity != (GameEntity)null)
		{
			_smallAtlasTextureIndex = GetCircleIndex();
			if (_navigatablePositionCheckTimer > 0.2f)
			{
				bool flag = false;
				if (!_anotherEntityHiglighted)
				{
					NavigationType val3 = default(NavigationType);
					flag = NavigationHelper.CanPlayerNavigateToPosition(new CampaignVec2(((Vec3)(ref intersectionPoint)).AsVec2, isOnland), ref val3);
				}
				_mapCursorDecal.SetMaterial((flag || _anotherEntityHiglighted) ? _gameCursorValidDecalMaterial : _gameCursorInvalidDecalMaterial);
				_mapCursorDecal.SetVectorArgument(0.166f, 1f, 0.166f * (float)_smallAtlasTextureIndex, 0f);
				SetAlpha(_anotherEntityHiglighted ? 0.2f : 1f);
			}
			MatrixFrame frame = _mapCursorDecalEntity.GetFrame();
			frame.origin = intersectionPoint;
			bool flag2 = !((Vec3)(ref _smoothRotationNormalStart)).IsNonZero;
			Vec3 val4 = ((cameraDistance > 160f) ? Vec3.Up : val2);
			if (!((Vec3)(ref _smoothRotationNormalEnd)).NearlyEquals(ref val4, 1E-05f))
			{
				_smoothRotationNormalStart = (flag2 ? val4 : _smoothRotationNormalCurrent);
				_smoothRotationNormalEnd = val4;
				((Vec3)(ref _smoothRotationNormalStart)).Normalize();
				((Vec3)(ref _smoothRotationNormalEnd)).Normalize();
				_smoothRotationAlpha = 0f;
			}
			_smoothRotationNormalCurrent = Vec3.Lerp(_smoothRotationNormalStart, _smoothRotationNormalEnd, _smoothRotationAlpha);
			_smoothRotationAlpha += 12f * dt;
			_smoothRotationAlpha = MathF.Clamp(_smoothRotationAlpha, 0f, 1f);
			((Vec3)(ref _smoothRotationNormalCurrent)).Normalize();
			frame.rotation.f = camera.Frame.rotation.f;
			frame.rotation.f.z = 0f;
			((Vec3)(ref frame.rotation.f)).Normalize();
			frame.rotation.u = _smoothRotationNormalCurrent;
			((Vec3)(ref frame.rotation.u)).Normalize();
			frame.rotation.s = Vec3.CrossProduct(frame.rotation.u, frame.rotation.f);
			float num = (cameraDistance + 80f) * (cameraDistance + 80f) / 10000f;
			num = MathF.Clamp(num, 0.2f, 38f);
			Vec3 val5 = Vec3.One * num;
			((MatrixFrame)(ref frame)).Scale(ref val5);
			_mapCursorDecalEntity.SetGlobalFrame(ref frame, true);
			_anotherEntityHiglighted = false;
		}
		if (_navigatablePositionCheckTimer > 0.2f)
		{
			_navigatablePositionCheckTimer = 0f;
		}
		_navigatablePositionCheckTimer += dt;
	}

	public void SetVisible(bool value)
	{
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		if (value)
		{
			if (!_gameCursorActive || _mapScreen.GetMouseVisible())
			{
				_mapScreen.SetMouseVisible(value: false);
				_mapCursorDecalEntity.SetVisibilityExcludeParents(true);
				if (_mapScreen.CurrentVisualOfTooltip != null)
				{
					_mapScreen.RemoveMapTooltip();
				}
				Vec2 resolution = Input.Resolution;
				Input.SetMousePosition((int)(((Vec2)(ref resolution)).X / 2f), (int)(((Vec2)(ref resolution)).Y / 2f));
				_gameCursorActive = true;
			}
		}
		else
		{
			bool flag = !(GameStateManager.Current.ActiveState is MapState) || (!((ScreenLayer)_mapScreen.SceneLayer).Input.IsKeyDown((InputKey)225) && !((ScreenLayer)_mapScreen.SceneLayer).Input.IsKeyDown((InputKey)226));
			if (_gameCursorActive || _mapScreen.GetMouseVisible() != flag)
			{
				_mapScreen.SetMouseVisible(flag);
				_mapCursorDecalEntity.SetVisibilityExcludeParents(false);
				_gameCursorActive = false;
			}
		}
	}

	protected internal void OnMapTerrainClick()
	{
		_targetCircleRotationStartTime = MBCommon.GetApplicationTime();
	}

	protected internal void OnAnotherEntityHighlighted()
	{
		_anotherEntityHiglighted = true;
	}

	protected internal void SetAlpha(float alpha)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		Color val = Color.FromUint(_mapCursorDecal.GetFactor1());
		Color val2 = default(Color);
		((Color)(ref val2))._002Ector(val.Red, val.Green, val.Blue, alpha);
		_mapCursorDecal.SetFactor1(((Color)(ref val2)).ToUnsignedInteger());
	}

	private int GetCircleIndex()
	{
		int num = (int)((MBCommon.GetApplicationTime() - _targetCircleRotationStartTime) / 0.033f);
		if (num >= 10)
		{
			return 0;
		}
		int num2 = num % 10;
		if (num2 >= 5)
		{
			num2 = 10 - num2 - 1;
		}
		return num2;
	}
}
