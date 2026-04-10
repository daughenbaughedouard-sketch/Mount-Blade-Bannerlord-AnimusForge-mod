using System;
using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.DotNet;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.View.Tableaus.Thumbnails;

namespace TaleWorlds.MountAndBlade.View.MissionViews;

public class MissionAgentLabelView : MissionView
{
	private const float _highlightedLabelScaleFactor = 20f;

	private const float _labelBannerWidth = 0.4f;

	private const float _labelBlackBorderWidth = 0.44f;

	private readonly Vec3 _meshOffset = new Vec3(0f, 0f, 2f, -1f);

	private const float _nearDistance = 1.5f;

	private const float _farDistance = 8f;

	private readonly List<Agent> _closeAgentsWithMeshes;

	private readonly Dictionary<Agent, MetaMesh> _agentMeshes;

	private readonly Dictionary<Texture, Material> _labelMaterials;

	private bool _isSuspendingView;

	private bool _isResumingView;

	private bool _isOrderFlagVisible;

	private bool _alwaysShowFriendlyTroopBanners;

	private bool _indicatorsActive;

	private bool IndicatorsActive
	{
		get
		{
			return _indicatorsActive;
		}
		set
		{
			if (_indicatorsActive != value)
			{
				_indicatorsActive = value;
				UpdateAllAgentMeshVisibilities();
			}
		}
	}

	private OrderController PlayerOrderController
	{
		get
		{
			Team playerTeam = ((MissionBehavior)this).Mission.PlayerTeam;
			if (playerTeam == null)
			{
				return null;
			}
			return playerTeam.PlayerOrderController;
		}
	}

	private SiegeWeaponController PlayerSiegeWeaponController
	{
		get
		{
			Team playerTeam = ((MissionBehavior)this).Mission.PlayerTeam;
			if (playerTeam == null)
			{
				return null;
			}
			return playerTeam.PlayerOrderController.SiegeWeaponController;
		}
	}

	public MissionAgentLabelView()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		_agentMeshes = new Dictionary<Agent, MetaMesh>();
		_labelMaterials = new Dictionary<Texture, Material>();
		_closeAgentsWithMeshes = new List<Agent>();
	}

	public override void OnBehaviorInitialize()
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Expected O, but got Unknown
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Expected O, but got Unknown
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Expected O, but got Unknown
		((MissionBehavior)this).OnBehaviorInitialize();
		((MissionBehavior)this).Mission.Teams.OnPlayerTeamChanged += Mission_OnPlayerTeamChanged;
		((MissionBehavior)this).Mission.OnMainAgentChanged += new OnMainAgentChangedDelegate(OnMainAgentChanged);
		ManagedOptions.OnManagedOptionChanged = (OnManagedOptionChangedDelegate)Delegate.Combine((Delegate?)(object)ManagedOptions.OnManagedOptionChanged, (Delegate?)new OnManagedOptionChangedDelegate(OnManagedOptionChanged));
		base.MissionScreen.OnSpectateAgentFocusIn += HandleSpectateAgentFocusIn;
		base.MissionScreen.OnSpectateAgentFocusOut += HandleSpectateAgentFocusOut;
	}

	public override void AfterStart()
	{
		if (PlayerOrderController != null)
		{
			PlayerOrderController.OnSelectedFormationsChanged += OrderController_OnSelectedFormationsChanged;
			((MissionBehavior)this).Mission.PlayerTeam.OnFormationsChanged += PlayerTeam_OnFormationsChanged;
		}
		BannerBearerLogic missionBehavior = ((MissionBehavior)this).Mission.GetMissionBehavior<BannerBearerLogic>();
		if (missionBehavior != null)
		{
			missionBehavior.OnBannerBearerAgentUpdated += BannerBearerLogic_OnBannerBearerAgentUpdated;
		}
		UpdateAlwaysShowFriendlyTroopBanners();
	}

	public override void OnMissionTick(float dt)
	{
		bool isOrderFlagVisible = _isOrderFlagVisible;
		UpdateIsOrderFlagVisible();
		if (!_isOrderFlagVisible && isOrderFlagVisible)
		{
			UpdateAllAgentMeshVisibilities();
			SetHighlightForAgents(highlight: false, useSiegeMachineUsers: false, useAllTeamAgents: false);
			SetHighlightForAgents(highlight: false, useSiegeMachineUsers: true, useAllTeamAgents: false);
		}
		if (_isOrderFlagVisible && !isOrderFlagVisible)
		{
			UpdateAllAgentMeshVisibilities();
			SetHighlightForAgents(highlight: true, useSiegeMachineUsers: false, useAllTeamAgents: false);
			SetHighlightForAgents(highlight: true, useSiegeMachineUsers: true, useAllTeamAgents: false);
		}
		UpdateProximityBannerTransparencies();
		IndicatorsActive = _alwaysShowFriendlyTroopBanners || base.Input.IsGameKeyDown(5);
	}

	private void UpdateProximityBannerTransparencies()
	{
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < _closeAgentsWithMeshes.Count; i++)
		{
			Agent agent = _closeAgentsWithMeshes[i];
			SetBannerHighlightVisibility(agent, IsAgentListeningToOrders(agent));
		}
		_closeAgentsWithMeshes.Clear();
		Mission mission = ((MissionBehavior)this).Mission;
		Vec3 position = base.MissionScreen.CombatCamera.Position;
		ProximityMapSearchStruct val = AgentProximityMap.BeginSearch(mission, ((Vec3)(ref position)).AsVec2, 8f, false);
		while (((ProximityMapSearchStruct)(ref val)).LastFoundAgent != null)
		{
			if (_agentMeshes.ContainsKey(((ProximityMapSearchStruct)(ref val)).LastFoundAgent))
			{
				_closeAgentsWithMeshes.Add(((ProximityMapSearchStruct)(ref val)).LastFoundAgent);
			}
			AgentProximityMap.FindNext(((MissionBehavior)this).Mission, ref val);
		}
		for (int j = 0; j < _closeAgentsWithMeshes.Count; j++)
		{
			Agent agent2 = _closeAgentsWithMeshes[j];
			SetBannerHighlightVisibility(agent2, IsAgentListeningToOrders(agent2));
		}
	}

	public override void OnRemoveBehavior()
	{
		UnregisterEvents();
		base.OnRemoveBehavior();
	}

	public override void OnMissionScreenFinalize()
	{
		UnregisterEvents();
		base.OnMissionScreenFinalize();
	}

	private void UnregisterEvents()
	{
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Expected O, but got Unknown
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Expected O, but got Unknown
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Expected O, but got Unknown
		if (((MissionBehavior)this).Mission != null)
		{
			((MissionBehavior)this).Mission.Teams.OnPlayerTeamChanged -= Mission_OnPlayerTeamChanged;
			((MissionBehavior)this).Mission.OnMainAgentChanged -= new OnMainAgentChangedDelegate(OnMainAgentChanged);
		}
		ManagedOptions.OnManagedOptionChanged = (OnManagedOptionChangedDelegate)Delegate.Remove((Delegate?)(object)ManagedOptions.OnManagedOptionChanged, (Delegate?)new OnManagedOptionChangedDelegate(OnManagedOptionChanged));
		if (base.MissionScreen != null)
		{
			base.MissionScreen.OnSpectateAgentFocusIn -= HandleSpectateAgentFocusIn;
			base.MissionScreen.OnSpectateAgentFocusOut -= HandleSpectateAgentFocusOut;
		}
		if (PlayerOrderController != null)
		{
			PlayerOrderController.OnSelectedFormationsChanged -= OrderController_OnSelectedFormationsChanged;
			if (((MissionBehavior)this).Mission != null)
			{
				((MissionBehavior)this).Mission.PlayerTeam.OnFormationsChanged -= PlayerTeam_OnFormationsChanged;
			}
		}
		BannerBearerLogic missionBehavior = ((MissionBehavior)this).Mission.GetMissionBehavior<BannerBearerLogic>();
		if (missionBehavior != null)
		{
			missionBehavior.OnBannerBearerAgentUpdated -= BannerBearerLogic_OnBannerBearerAgentUpdated;
		}
	}

	public override void OnAgentRemoved(Agent affectedAgent, Agent affectorAgent, AgentState agentState, KillingBlow killingBlow)
	{
		RemoveAgentLabel(affectedAgent);
	}

	public override void OnAgentBuild(Agent agent, Banner banner)
	{
		InitAgentLabel(agent, banner);
	}

	public override void OnAssignPlayerAsSergeantOfFormation(Agent agent)
	{
		SetBannerHighlightVisibility(agent, highlightVisibility: true);
	}

	public override void OnClearScene()
	{
		_agentMeshes.Clear();
		_labelMaterials.Clear();
		_closeAgentsWithMeshes.Clear();
	}

	private void PlayerTeam_OnFormationsChanged(Team team, Formation formation)
	{
		UpdateIsOrderFlagVisible();
		if (_isOrderFlagVisible)
		{
			DehighlightAllAgents();
			SetHighlightForAgents(highlight: true, useSiegeMachineUsers: false, useAllTeamAgents: false);
		}
	}

	private void Mission_OnPlayerTeamChanged(Team previousTeam, Team currentTeam)
	{
		DehighlightAllAgents();
		_isOrderFlagVisible = false;
		if (((previousTeam != null) ? previousTeam.PlayerOrderController : null) != null)
		{
			previousTeam.PlayerOrderController.OnSelectedFormationsChanged -= OrderController_OnSelectedFormationsChanged;
			previousTeam.PlayerOrderController.SiegeWeaponController.OnSelectedSiegeWeaponsChanged -= PlayerSiegeWeaponController_OnSelectedSiegeWeaponsChanged;
		}
		if (PlayerOrderController != null)
		{
			PlayerOrderController.OnSelectedFormationsChanged += OrderController_OnSelectedFormationsChanged;
			PlayerSiegeWeaponController.OnSelectedSiegeWeaponsChanged += PlayerSiegeWeaponController_OnSelectedSiegeWeaponsChanged;
		}
		SetHighlightForAgents(highlight: true, useSiegeMachineUsers: false, useAllTeamAgents: true);
		foreach (Agent item in (List<Agent>)(object)((MissionBehavior)this).Mission.Agents)
		{
			UpdateVisibilityOfAgentMesh(item);
		}
	}

	private void OrderController_OnSelectedFormationsChanged()
	{
		UpdateAllAgentMeshVisibilities();
		DehighlightAllAgents();
		UpdateIsOrderFlagVisible();
		if (_isOrderFlagVisible)
		{
			SetHighlightForAgents(highlight: true, useSiegeMachineUsers: false, useAllTeamAgents: false);
		}
	}

	private void PlayerSiegeWeaponController_OnSelectedSiegeWeaponsChanged()
	{
		DehighlightAllAgents();
		SetHighlightForAgents(highlight: true, useSiegeMachineUsers: true, useAllTeamAgents: false);
	}

	private void BannerBearerLogic_OnBannerBearerAgentUpdated(Agent agent, bool isBannerBearer)
	{
		RemoveAgentLabel(agent);
		InitAgentLabel(agent);
	}

	private void RemoveAgentLabel(Agent agent)
	{
		if (agent.IsHuman && _agentMeshes.ContainsKey(agent))
		{
			if ((NativeObject)(object)agent.AgentVisuals != (NativeObject)null)
			{
				agent.AgentVisuals.ReplaceMeshWithMesh(_agentMeshes[agent], (MetaMesh)null, (BodyMeshTypes)13);
			}
			_agentMeshes.Remove(agent);
		}
		if (_closeAgentsWithMeshes.Contains(agent))
		{
			_closeAgentsWithMeshes.Remove(agent);
		}
	}

	private void InitAgentLabel(Agent agent, Banner peerBanner = null)
	{
		if (!agent.IsHuman)
		{
			return;
		}
		Banner val = peerBanner ?? agent.Origin.Banner;
		if (val == null)
		{
			return;
		}
		Texture val2 = null;
		MetaMesh copy = MetaMesh.GetCopy("troop_banner_selection", false, true);
		Material tableauMaterial = Material.GetFromResource("agent_label_with_tableau");
		val2 = val.GetTableauTextureSmall(BannerDebugInfo.CreateManual(((object)this).GetType().Name), null);
		if (!((NativeObject)(object)copy != (NativeObject)null) || !((NativeObject)(object)tableauMaterial != (NativeObject)null))
		{
			return;
		}
		Texture fromResource = Texture.GetFromResource("banner_top_of_head");
		if (_labelMaterials.TryGetValue(val2 ?? fromResource, out var value))
		{
			tableauMaterial = value;
		}
		else
		{
			tableauMaterial = tableauMaterial.CreateCopy();
			Action<Texture> setAction = delegate(Texture tex)
			{
				tableauMaterial.SetTexture((MBTextureType)0, tex);
			};
			val2 = val.GetTableauTextureSmall(BannerDebugInfo.CreateManual(((object)this).GetType().Name), setAction);
			tableauMaterial.SetTexture((MBTextureType)1, fromResource);
			_labelMaterials.Add(val2, tableauMaterial);
		}
		copy.SetMaterial(tableauMaterial);
		copy.SetVectorArgument(0.5f, 0.5f, 0.25f, 0.25f);
		agent.AgentVisuals.AddMultiMesh(copy, (BodyMeshTypes)13);
		_agentMeshes.Add(agent, copy);
		UpdateVisibilityOfAgentMesh(agent);
		SetBannerHighlightVisibility(agent, highlightVisibility: false);
	}

	private void UpdateVisibilityOfAgentMesh(Agent agent)
	{
		if (agent.IsActive() && _agentMeshes.ContainsKey(agent))
		{
			bool flag = IsMeshVisibleForAgent(agent);
			_agentMeshes[agent].SetVisibilityMask((VisibilityMaskFlags)(flag ? 1 : 0));
		}
	}

	private bool IsMeshVisibleForAgent(Agent agent)
	{
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Invalid comparison between Unknown and I4
		if ((_isResumingView || (!base.IsViewSuspended && !_isSuspendingView)) && IsAllyInAllyTeam(agent) && base.MissionScreen.LastFollowedAgent != agent && BannerlordConfig.FriendlyTroopsBannerOpacity > 0f && !base.MissionScreen.IsPhotoModeEnabled)
		{
			if (!IndicatorsActive && (int)((MissionBehavior)this).Mission.Mode != 6)
			{
				return IsAgentListeningToOrders(agent);
			}
			return true;
		}
		return false;
	}

	public override void OnMissionModeChange(MissionMode oldMissionMode, bool atStart)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		((MissionBehavior)this).OnMissionModeChange(oldMissionMode, atStart);
		UpdateAllAgentMeshVisibilities();
	}

	private void OnUpdateOpacityValueOfAgentMesh(Agent agent)
	{
		if (agent.IsActive() && _agentMeshes.ContainsKey(agent))
		{
			SetBannerHighlightVisibility(agent, IsAgentListeningToOrders(agent));
		}
	}

	private bool IsAllyInAllyTeam(Agent agent)
	{
		if (((agent != null) ? agent.Team : null) != null && ((MissionBehavior)this).Mission != null && agent != ((MissionBehavior)this).Mission.MainAgent)
		{
			Team val = null;
			Team val2;
			if (GameNetwork.IsSessionActive)
			{
				object obj;
				if (!GameNetwork.IsMyPeerReady)
				{
					obj = null;
				}
				else
				{
					NetworkCommunicator myPeer = GameNetwork.MyPeer;
					if (myPeer == null)
					{
						obj = null;
					}
					else
					{
						MissionPeer component = PeerExtensions.GetComponent<MissionPeer>(myPeer);
						obj = ((component != null) ? component.Team : null);
					}
				}
				val2 = (Team)obj;
			}
			else
			{
				val2 = ((MissionBehavior)this).Mission.PlayerTeam;
				val = ((MissionBehavior)this).Mission.PlayerAllyTeam;
			}
			if (agent.Team != val2)
			{
				return agent.Team == val;
			}
			return true;
		}
		return false;
	}

	private void OnMainAgentChanged(Agent oldAgent)
	{
		UpdateAllAgentMeshVisibilities();
	}

	private void HandleSpectateAgentFocusIn(Agent agent)
	{
		UpdateVisibilityOfAgentMesh(agent);
	}

	private void HandleSpectateAgentFocusOut(Agent agent)
	{
		UpdateVisibilityOfAgentMesh(agent);
	}

	private void OnManagedOptionChanged(ManagedOptionsType optionType)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Invalid comparison between Unknown and I4
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Invalid comparison between Unknown and I4
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Invalid comparison between Unknown and I4
		if ((int)optionType == 13)
		{
			UpdateAlwaysShowFriendlyTroopBanners();
			UpdateAllAgentMeshVisibilities();
		}
		if ((int)optionType == 12 || (int)optionType == 13)
		{
			UpdateAllAgentMeshVisibilities();
		}
	}

	private void UpdateAlwaysShowFriendlyTroopBanners()
	{
		float config = ManagedOptions.GetConfig((ManagedOptionsType)13);
		_alwaysShowFriendlyTroopBanners = config == 2f || (config == 1f && GameNetwork.IsMultiplayer);
	}

	private void UpdateAllAgentMeshVisibilities()
	{
		foreach (Agent item in (List<Agent>)(object)((MissionBehavior)this).Mission.Agents)
		{
			if (item.IsHuman)
			{
				UpdateVisibilityOfAgentMesh(item);
				if (IsMeshVisibleForAgent(item))
				{
					OnUpdateOpacityValueOfAgentMesh(item);
				}
			}
		}
	}

	private bool IsAgentListeningToOrders(Agent agent)
	{
		UpdateIsOrderFlagVisible();
		if (!_isOrderFlagVisible)
		{
			return false;
		}
		if (PlayerOrderController != null && agent.Formation != null && PlayerOrderController.IsFormationListening(agent.Formation))
		{
			return true;
		}
		if (PlayerSiegeWeaponController != null && agent.IsUsingGameObject)
		{
			UsableMissionObject currentlyUsedGameObject = agent.CurrentlyUsedGameObject;
			for (int i = 0; i < ((List<SiegeWeapon>)(object)PlayerSiegeWeaponController.SelectedWeapons).Count; i++)
			{
				SiegeWeapon val = ((List<SiegeWeapon>)(object)PlayerSiegeWeaponController.SelectedWeapons)[i];
				for (int j = 0; j < ((List<StandingPoint>)(object)((UsableMachine)val).StandingPoints).Count; j++)
				{
					if ((object)currentlyUsedGameObject == ((List<StandingPoint>)(object)((UsableMachine)val).StandingPoints)[j])
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	private void SetBannerHighlightVisibility(Agent agent, bool highlightVisibility)
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		if (!_agentMeshes.TryGetValue(agent, out var value))
		{
			Debug.FailedAssert("Trying to update the banner of an agent that isn't present in _agentMeshes!", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.View\\MissionViews\\MissionAgentLabelView.cs", "SetBannerHighlightVisibility", 499);
			return;
		}
		float num = (highlightVisibility ? 1f : (-1f));
		Vec3 val = agent.Position + _meshOffset;
		float num2 = ((Vec3)(ref val)).Distance(base.MissionScreen.CombatCamera.Position);
		if (num2 < 1.5f)
		{
			num = 0f;
		}
		else if (num2 < 8f)
		{
			num *= (num2 - 1.5f) / 6.5f;
		}
		value.SetVectorArgument2(20f, 0.4f, 0.44f, num * BannerlordConfig.FriendlyTroopsBannerOpacity);
	}

	private void UpdateIsOrderFlagVisible()
	{
		_isOrderFlagVisible = PlayerOrderController != null && base.MissionScreen.OrderFlag != null && base.MissionScreen.OrderFlag.IsVisible;
	}

	private void SetHighlightForAgents(bool highlight, bool useSiegeMachineUsers, bool useAllTeamAgents)
	{
		if (PlayerOrderController == null)
		{
			bool flag = ((MissionBehavior)this).Mission.PlayerTeam == null;
			Debug.Print($"PlayerOrderController is null and playerTeamIsNull: {flag}", 0, (DebugColor)12, 17179869184uL);
		}
		if (useSiegeMachineUsers)
		{
			foreach (SiegeWeapon item in (List<SiegeWeapon>)(object)PlayerSiegeWeaponController.SelectedWeapons)
			{
				foreach (StandingPoint item2 in (List<StandingPoint>)(object)((UsableMachine)item).StandingPoints)
				{
					Agent userAgent = ((UsableMissionObject)item2).UserAgent;
					if (userAgent != null)
					{
						SetBannerHighlightVisibility(userAgent, highlight);
					}
				}
			}
			return;
		}
		if (useAllTeamAgents)
		{
			if (PlayerOrderController.Owner != null)
			{
				Team val = PlayerOrderController.Owner.Team;
				if (val == null)
				{
					Debug.Print("PlayerOrderController.Owner.Team is null, overriding with Mission.Current.PlayerTeam", 0, (DebugColor)12, 17179869184uL);
					val = Mission.Current.PlayerTeam;
				}
				{
					foreach (Agent item3 in (List<Agent>)(object)val.ActiveAgents)
					{
						SetBannerHighlightVisibility(item3, highlight);
					}
					return;
				}
			}
			Debug.Print("PlayerOrderController.Owner is null", 0, (DebugColor)12, 17179869184uL);
			return;
		}
		foreach (Formation item4 in (List<Formation>)(object)PlayerOrderController.SelectedFormations)
		{
			item4.ApplyActionOnEachUnit((Action<Agent>)delegate(Agent agent)
			{
				SetBannerHighlightVisibility(agent, highlight);
			}, (Agent)null);
		}
	}

	private void DehighlightAllAgents()
	{
		foreach (KeyValuePair<Agent, MetaMesh> agentMesh in _agentMeshes)
		{
			SetBannerHighlightVisibility(agentMesh.Key, highlightVisibility: false);
		}
	}

	public override void OnAgentTeamChanged(Team prevTeam, Team newTeam, Agent agent)
	{
		UpdateVisibilityOfAgentMesh(agent);
	}

	public override void OnPhotoModeActivated()
	{
		base.OnPhotoModeActivated();
		UpdateAllAgentMeshVisibilities();
	}

	public override void OnPhotoModeDeactivated()
	{
		base.OnPhotoModeDeactivated();
		UpdateAllAgentMeshVisibilities();
	}

	protected override void OnSuspendView()
	{
		base.OnSuspendView();
		_isSuspendingView = true;
		UpdateAllAgentMeshVisibilities();
		_isSuspendingView = false;
	}

	protected override void OnResumeView()
	{
		base.OnResumeView();
		_isResumingView = true;
		UpdateAllAgentMeshVisibilities();
		_isResumingView = false;
	}
}
