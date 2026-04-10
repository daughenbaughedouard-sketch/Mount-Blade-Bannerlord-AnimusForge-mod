using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.DotNet;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.View.Screens;
using TaleWorlds.MountAndBlade.View.Tableaus.Thumbnails;

namespace TaleWorlds.MountAndBlade.View.MissionViews.Singleplayer;

public class FormationIndicatorMissionView : MissionView
{
	public class Indicator
	{
		public MissionScreen missionScreen;

		public bool indicatorVisible;

		public MatrixFrame indicatorFrame;

		public bool firstTime = true;

		public GameEntity indicatorEntity;

		public Vec3 nextIndicatorPosition;

		public Vec3 prevIndicatorPosition;

		public float indicatorAlpha = 1f;

		private float _drawIndicatorElapsedTime;

		private const float IndicatorExpireTime = 0.5f;

		private bool _isSeenByPlayer = true;

		internal bool _isMovingTooFast;

		private Vec3? GetCurrentPosition()
		{
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0067: Unknown result type (might be due to invalid IL or missing references)
			if (Mission.Current.MainAgent != null)
			{
				return Mission.Current.MainAgent.AgentVisuals.GetGlobalFrame().origin + new Vec3(0f, 0f, 1f, -1f);
			}
			if ((NativeObject)(object)missionScreen.CombatCamera != (NativeObject)null)
			{
				return missionScreen.CombatCamera.Position;
			}
			return null;
		}

		public void DetermineIndicatorState(float dt, Vec3 position)
		{
			//IL_0042: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0065: Unknown result type (might be due to invalid IL or missing references)
			//IL_006b: Unknown result type (might be due to invalid IL or missing references)
			//IL_008b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0090: Unknown result type (might be due to invalid IL or missing references)
			//IL_0059: Unknown result type (might be due to invalid IL or missing references)
			//IL_005a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0093: Unknown result type (might be due to invalid IL or missing references)
			//IL_0098: Unknown result type (might be due to invalid IL or missing references)
			//IL_009c: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
			//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ab: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d9: Unknown result type (might be due to invalid IL or missing references)
			//IL_0111: Unknown result type (might be due to invalid IL or missing references)
			//IL_012a: Unknown result type (might be due to invalid IL or missing references)
			//IL_012f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0134: Unknown result type (might be due to invalid IL or missing references)
			//IL_0149: Unknown result type (might be due to invalid IL or missing references)
			//IL_014e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0268: Unknown result type (might be due to invalid IL or missing references)
			//IL_026d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0271: Unknown result type (might be due to invalid IL or missing references)
			//IL_0272: Unknown result type (might be due to invalid IL or missing references)
			//IL_029e: Unknown result type (might be due to invalid IL or missing references)
			//IL_02a0: Unknown result type (might be due to invalid IL or missing references)
			Mission current = Mission.Current;
			Vec3? currentPosition = GetCurrentPosition();
			if (!currentPosition.HasValue)
			{
				indicatorVisible = false;
				return;
			}
			if (firstTime)
			{
				prevIndicatorPosition = position;
				nextIndicatorPosition = position;
				firstTime = false;
			}
			Vec3 val;
			if (((Vec3)(ref nextIndicatorPosition)).Distance(prevIndicatorPosition) / 0.5f > 30f)
			{
				val = position;
				_isMovingTooFast = true;
			}
			else
			{
				val = Vec3.Lerp(prevIndicatorPosition, nextIndicatorPosition, MBMath.ClampFloat(_drawIndicatorElapsedTime / 0.5f, 0f, 1f));
			}
			Vec3 value = currentPosition.Value;
			float num = ((Vec3)(ref value)).Distance(val);
			if (_drawIndicatorElapsedTime < 0.5f)
			{
				_drawIndicatorElapsedTime += dt;
			}
			else
			{
				prevIndicatorPosition = nextIndicatorPosition;
				nextIndicatorPosition = position;
				_isSeenByPlayer = num < 60f && (num < 15f || current.Scene.CheckPointCanSeePoint(currentPosition.Value, position, (float?)null) || current.Scene.CheckPointCanSeePoint(currentPosition.Value + new Vec3(0f, 0f, 2f, -1f), position + new Vec3(0f, 0f, 2f, -1f), (float?)null));
				_drawIndicatorElapsedTime = 0f;
			}
			if (!_isSeenByPlayer)
			{
				float num2 = MBMath.ClampFloat(num * 0.02f, 1f, 10f) * 3f;
				MatrixFrame identity = MatrixFrame.Identity;
				identity.origin = val;
				identity.origin.z += num2 * 0.75f;
				((Mat3)(ref identity.rotation)).ApplyScaleLocal(num2);
				indicatorFrame = identity;
				if (_isMovingTooFast)
				{
					if (indicatorEntity != (GameEntity)null && indicatorEntity.IsVisibleIncludeParents())
					{
						indicatorVisible = false;
						return;
					}
					_isMovingTooFast = false;
					indicatorVisible = true;
				}
				else
				{
					indicatorVisible = true;
				}
			}
			else
			{
				indicatorVisible = false;
				if (!_isMovingTooFast && indicatorEntity != (GameEntity)null && indicatorEntity.IsVisibleIncludeParents())
				{
					float num3 = MBMath.ClampFloat(num * 0.02f, 1f, 10f) * 3f;
					MatrixFrame identity2 = MatrixFrame.Identity;
					identity2.origin = val;
					identity2.origin.z += num3 * 0.75f;
					((Mat3)(ref identity2.rotation)).ApplyScaleLocal(num3);
					indicatorFrame = identity2;
				}
			}
		}
	}

	private Indicator[,] _indicators;

	private Mission mission;

	private bool _isEnabled;

	public override void AfterStart()
	{
		((MissionBehavior)this).AfterStart();
		mission = Mission.Current;
		_indicators = new Indicator[((List<Team>)(object)mission.Teams).Count, 9];
		for (int i = 0; i < ((List<Team>)(object)mission.Teams).Count; i++)
		{
			for (int j = 0; j < 9; j++)
			{
				_indicators[i, j] = new Indicator
				{
					missionScreen = base.MissionScreen
				};
			}
		}
	}

	private GameEntity CreateBannerEntity(Formation formation)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Expected I4, but got Unknown
		GameEntity obj = GameEntity.CreateEmpty(mission.Scene, true, true, true);
		obj.EntityFlags = (EntityFlags)(obj.EntityFlags | 0x200);
		uint color = 4278190080u;
		uint color2 = ((!formation.Team.IsPlayerAlly) ? 2144798212u : 2130747904u);
		obj.AddMultiMesh(MetaMesh.GetCopy("billboard_unit_mesh", true, false), true);
		obj.GetFirstMesh().Color = uint.MaxValue;
		Material formationMaterial = Material.GetFromResource("formation_icon").CreateCopy();
		if (formation.Team != null)
		{
			Banner val = ((formation.Captain != null) ? formation.Captain.Origin.Banner : formation.Team.Banner);
			if (val != null)
			{
				BannerVisualExtensions.GetTableauTextureLarge(setAction: delegate(Texture tex)
				{
					formationMaterial.SetTexture((MBTextureType)1, tex);
				}, banner: val, debugInfo: BannerDebugInfo.CreateManual(((object)this).GetType().Name));
			}
			else
			{
				Texture fromResource = Texture.GetFromResource("plain_red");
				formationMaterial.SetTexture((MBTextureType)1, fromResource);
			}
		}
		else
		{
			Texture fromResource2 = Texture.GetFromResource("plain_red");
			formationMaterial.SetTexture((MBTextureType)1, fromResource2);
		}
		int num = formation.FormationIndex % 4;
		obj.GetFirstMesh().SetMaterial(formationMaterial);
		obj.GetFirstMesh().Color = color2;
		obj.GetFirstMesh().Color2 = color;
		obj.GetFirstMesh().SetVectorArgument(0f, 1f, (float)num, 1f);
		return obj;
	}

	private int GetFormationTeamIndex(Formation formation)
	{
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Expected I4, but got Unknown
		if (((List<Team>)(object)mission.Teams).Count > 2 && (formation.Team == mission.AttackerAllyTeam || formation.Team == mission.DefenderAllyTeam))
		{
			return (((List<Team>)(object)mission.Teams).Count == 3) ? 2 : ((formation.Team == mission.DefenderAllyTeam) ? 2 : 3);
		}
		return (int)formation.Team.Side;
	}

	public override void OnMissionScreenTick(float dt)
	{
		//IL_0344: Unknown result type (might be due to invalid IL or missing references)
		//IL_034e: Expected I4, but got Unknown
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Expected I4, but got Unknown
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Expected I4, but got Unknown
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Expected I4, but got Unknown
		//IL_02a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ab: Invalid comparison between Unknown and I4
		//IL_02c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cc: Expected I4, but got Unknown
		((MissionBehavior)this).OnMissionTick(dt);
		bool flag;
		if (base.Input.IsGameKeyDown(5))
		{
			flag = false;
			_isEnabled = false;
		}
		else
		{
			flag = false;
		}
		IEnumerable<Formation> enumerable = ((IEnumerable<Team>)mission.Teams).SelectMany((Team t) => (IEnumerable<Formation>)t.FormationsIncludingEmpty);
		if (flag)
		{
			IEnumerable<Formation> enumerable2 = ((IEnumerable<Team>)mission.Teams).SelectMany((Team t) => ((IEnumerable<Formation>)t.FormationsIncludingEmpty).Where((Formation f) => f.CountOfUnits > 0));
			foreach (Formation item in enumerable2)
			{
				int formationTeamIndex = GetFormationTeamIndex(item);
				Indicator indicator = _indicators[formationTeamIndex, (int)item.FormationIndex];
				WorldPosition cachedMedianPosition = item.CachedMedianPosition;
				indicator.DetermineIndicatorState(dt, ((WorldPosition)(ref cachedMedianPosition)).GetGroundVec3());
				if (indicator.indicatorEntity == (GameEntity)null)
				{
					if (indicator.indicatorVisible)
					{
						GameEntity val = CreateBannerEntity(item);
						val.SetFrame(ref indicator.indicatorFrame, true);
						val.SetVisibilityExcludeParents(true);
						_indicators[formationTeamIndex, (int)item.FormationIndex].indicatorEntity = val;
						val.SetAlpha(0f);
						_indicators[formationTeamIndex, (int)item.FormationIndex].indicatorAlpha = 0f;
					}
					continue;
				}
				if (indicator.indicatorEntity.IsVisibleIncludeParents() != indicator.indicatorVisible)
				{
					if (!indicator.indicatorVisible)
					{
						if (indicator.indicatorAlpha > 0f)
						{
							indicator.indicatorAlpha -= 0.01f;
							if (indicator.indicatorAlpha < 0f)
							{
								indicator.indicatorAlpha = 0f;
							}
							indicator.indicatorEntity.SetAlpha(indicator.indicatorAlpha);
						}
						else
						{
							indicator.indicatorEntity.SetVisibilityExcludeParents(indicator.indicatorVisible);
						}
					}
					else
					{
						indicator.indicatorEntity.SetVisibilityExcludeParents(indicator.indicatorVisible);
					}
				}
				if (indicator.indicatorVisible && indicator.indicatorAlpha < 1f)
				{
					indicator.indicatorAlpha += 0.01f;
					if (indicator.indicatorAlpha > 1f)
					{
						indicator.indicatorAlpha = 1f;
					}
					indicator.indicatorEntity.SetAlpha(indicator.indicatorAlpha);
				}
				if (!indicator._isMovingTooFast && indicator.indicatorEntity.IsVisibleIncludeParents())
				{
					indicator.indicatorEntity.SetFrame(ref indicator.indicatorFrame, true);
				}
			}
			{
				foreach (Formation item2 in enumerable.Except(enumerable2))
				{
					if ((int)item2.FormationIndex >= 8)
					{
						break;
					}
					Indicator indicator2 = _indicators[GetFormationTeamIndex(item2), (int)item2.FormationIndex];
					if (indicator2.indicatorEntity != (GameEntity)null && indicator2.indicatorEntity.IsVisibleIncludeParents())
					{
						indicator2.indicatorEntity.SetVisibilityExcludeParents(false);
						indicator2.indicatorVisible = false;
					}
				}
				return;
			}
		}
		if (!_isEnabled)
		{
			return;
		}
		foreach (Formation item3 in enumerable)
		{
			int formationTeamIndex2 = GetFormationTeamIndex(item3);
			Indicator indicator3 = _indicators[formationTeamIndex2, (int)item3.FormationIndex];
			if (indicator3 != null && indicator3.indicatorEntity != (GameEntity)null)
			{
				indicator3.indicatorAlpha = 0f;
				indicator3.indicatorEntity.SetVisibilityExcludeParents(false);
			}
		}
		_isEnabled = false;
	}
}
