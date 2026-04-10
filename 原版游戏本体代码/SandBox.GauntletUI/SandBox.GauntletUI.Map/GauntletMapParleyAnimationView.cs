using System;
using System.Collections.Generic;
using SandBox.View;
using SandBox.View.Map;
using SandBox.View.Map.Managers;
using SandBox.View.Map.Visuals;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.View;

namespace SandBox.GauntletUI.Map;

[OverrideView(typeof(MapParleyAnimationView))]
public class GauntletMapParleyAnimationView : MapParleyAnimationView
{
	private readonly PartyBase _parleyedParty;

	private CampaignTimeControlMode _previousTimeControlMode;

	private const float _animationDuration = 1f;

	private float _remainingAnimationDuration;

	private readonly IParleyCampaignBehavior _behavior;

	private GameEntity _playerBannerEntity;

	private GameEntity _targetBannerEntity;

	private Vec3 _bannerTargetPosition;

	private MapEntityVisual<PartyBase> _mainPartyVisual;

	private MapEntityVisual<PartyBase> _parleyedPartyVisual;

	public GauntletMapParleyAnimationView(PartyBase parleyedParty)
	{
		_parleyedParty = parleyedParty;
		_behavior = Campaign.Current.GetCampaignBehavior<IParleyCampaignBehavior>();
		foreach (EntityVisualManagerBase<PartyBase> component in SandBoxViewSubModule.SandBoxViewVisualManager.GetComponents<EntityVisualManagerBase<PartyBase>>())
		{
			MapEntityVisual<PartyBase> visualOfEntity = component.GetVisualOfEntity(PartyBase.MainParty);
			MapEntityVisual<PartyBase> visualOfEntity2 = component.GetVisualOfEntity(_parleyedParty);
			if (visualOfEntity != null)
			{
				_mainPartyVisual = visualOfEntity;
			}
			if (visualOfEntity2 != null)
			{
				_parleyedPartyVisual = visualOfEntity2;
			}
		}
	}

	protected override void CreateLayout()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		base.CreateLayout();
		_remainingAnimationDuration = 1f;
		CreateBanners();
		MBInformationManager.AddQuickInformation(new TextObject("{=LZbHWkCB}Parleying with {PARTY_NAME}", (Dictionary<string, object>)null).SetTextVariable("PARTY_NAME", _parleyedParty.Name), -750, (BasicCharacterObject)null, (Equipment)null, "");
		_previousTimeControlMode = Campaign.Current.TimeControlMode;
		Campaign.Current.TimeControlMode = (CampaignTimeControlMode)0;
		Campaign.Current.SetTimeControlModeLock(true);
	}

	private void CreateBanners()
	{
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		_playerBannerEntity = CreateAnimationBannerEntity(PartyBase.MainParty, _mainPartyVisual);
		_targetBannerEntity = CreateAnimationBannerEntity(_parleyedParty, _parleyedPartyVisual);
		if (_parleyedParty.IsSettlement)
		{
			_bannerTargetPosition = _targetBannerEntity.GetFrame().origin;
		}
		else
		{
			_bannerTargetPosition = Vec3.Lerp(_playerBannerEntity.GetFrame().origin, _targetBannerEntity.GetFrame().origin, 0.5f);
		}
		RotateBannersTowardsEachother(_playerBannerEntity, _targetBannerEntity, _bannerTargetPosition);
		float num = 0.7f;
		Vec3 scaleVector = default(Vec3);
		((Vec3)(ref scaleVector))._002Ector(num, num, num, -1f);
		ScaleBanner(_playerBannerEntity, scaleVector);
		ScaleBanner(_targetBannerEntity, scaleVector);
	}

	private GameEntity CreateAnimationBannerEntity(PartyBase party, MapEntityVisual<PartyBase> partyVisual)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		GameEntity obj = GameEntity.CreateEmpty(base.MapScreen.MapScene, false, true, true);
		MetaMesh copy = MetaMesh.GetCopy("map_banner", true, false);
		obj.AddMultiMesh(copy, true);
		MatrixFrame identity = MatrixFrame.Identity;
		identity.origin = partyVisual.GetVisualPosition();
		obj.SetFrame(ref identity, true);
		return obj;
	}

	private void RotateBannersTowardsEachother(GameEntity playerBanner, GameEntity targetBanner, Vec3 bannerTargetPosition)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		MatrixFrame frame = playerBanner.GetFrame();
		MatrixFrame frame2 = targetBanner.GetFrame();
		Vec3 f = bannerTargetPosition - frame.origin;
		frame.rotation.f = f;
		((Mat3)(ref frame.rotation)).OrthonormalizeAccordingToForwardAndKeepUpAsZAxis();
		((Mat3)(ref frame.rotation)).RotateAboutUp(MathF.PI);
		frame2.rotation = frame.rotation;
		((Mat3)(ref frame2.rotation)).RotateAboutUp(MathF.PI);
		playerBanner.SetFrame(ref frame, true);
		targetBanner.SetFrame(ref frame2, true);
	}

	private void ScaleBanner(GameEntity bannerEntity, Vec3 scaleVector)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		MatrixFrame frame = bannerEntity.GetFrame();
		((MatrixFrame)(ref frame)).Scale(ref scaleVector);
		bannerEntity.SetFrame(ref frame, true);
	}

	private void DestroyAnimationBannerEntities()
	{
		GameEntity playerBannerEntity = _playerBannerEntity;
		if (playerBannerEntity != null)
		{
			playerBannerEntity.Remove(0);
		}
		GameEntity targetBannerEntity = _targetBannerEntity;
		if (targetBannerEntity != null)
		{
			targetBannerEntity.Remove(0);
		}
		_playerBannerEntity = null;
		_targetBannerEntity = null;
	}

	protected override void OnFrameTick(float dt)
	{
		base.OnFrameTick(dt);
		Tick(dt);
	}

	protected override void OnIdleTick(float dt)
	{
		base.OnIdleTick(dt);
		Tick(dt);
	}

	private void Tick(float dt)
	{
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		if (_remainingAnimationDuration <= 0f)
		{
			base.MapScreen.RemoveMapView(this);
			IParleyCampaignBehavior behavior = _behavior;
			if (behavior != null)
			{
				behavior.StartParley(_parleyedParty);
			}
			return;
		}
		float num = MathF.Clamp((1f - _remainingAnimationDuration) / 1f, 0f, 1f);
		Vec3 visualPosition = _mainPartyVisual.GetVisualPosition();
		Vec3 visualPosition2 = _parleyedPartyVisual.GetVisualPosition();
		MatrixFrame frame = _playerBannerEntity.GetFrame();
		MatrixFrame frame2 = _targetBannerEntity.GetFrame();
		frame.origin = Vec3.Lerp(visualPosition, _bannerTargetPosition, num);
		frame2.origin = Vec3.Lerp(visualPosition2, _bannerTargetPosition, num);
		_playerBannerEntity.SetFrame(ref frame, true);
		_targetBannerEntity.SetFrame(ref frame2, true);
		_remainingAnimationDuration -= dt;
	}

	protected override void OnFinalize()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		base.OnFinalize();
		DestroyAnimationBannerEntities();
		Campaign.Current.SetTimeControlModeLock(false);
		Campaign.Current.TimeControlMode = _previousTimeControlMode;
	}
}
