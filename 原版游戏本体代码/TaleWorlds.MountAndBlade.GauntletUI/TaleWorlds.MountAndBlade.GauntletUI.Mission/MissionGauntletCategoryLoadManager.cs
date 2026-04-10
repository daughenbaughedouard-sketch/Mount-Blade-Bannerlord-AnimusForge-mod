using TaleWorlds.Core;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TaleWorlds.TwoDimension;

namespace TaleWorlds.MountAndBlade.GauntletUI.Mission;

[DefaultView]
public class MissionGauntletCategoryLoadManager : MissionView, IMissionListener
{
	private SpriteCategory _fullBackgroundCategory;

	private SpriteCategory _mapBarCategory;

	private SpriteCategory _encyclopediaCategory;

	private MissionGauntletOptionsUIHandler _optionsView;

	public override void AfterStart()
	{
		((MissionBehavior)this).AfterStart();
		if (_fullBackgroundCategory == null)
		{
			_fullBackgroundCategory = UIResourceManager.GetSpriteCategory("ui_fullbackgrounds");
		}
		if (_encyclopediaCategory == null)
		{
			_encyclopediaCategory = UIResourceManager.GetSpriteCategory("ui_encyclopedia");
		}
		if (_mapBarCategory == null)
		{
			SpriteCategory spriteCategory = UIResourceManager.GetSpriteCategory("ui_mapbar");
			if (spriteCategory != null && spriteCategory.IsLoaded)
			{
				_mapBarCategory = spriteCategory;
			}
		}
		if (_optionsView == null)
		{
			_optionsView = ((MissionBehavior)this).Mission.GetMissionBehavior<MissionGauntletOptionsUIHandler>();
			((MissionBehavior)this).Mission.AddListener((IMissionListener)(object)this);
		}
		HandleCategoryLoadingUnloading();
	}

	public override void OnMissionScreenFinalize()
	{
		base.OnMissionScreenFinalize();
		_optionsView = null;
		((MissionBehavior)this).Mission.RemoveListener((IMissionListener)(object)this);
		LoadUnloadAllCategories(load: true);
	}

	public override void OnMissionTick(float dt)
	{
		((MissionBehavior)this).OnMissionTick(dt);
		HandleCategoryLoadingUnloading();
	}

	private void HandleCategoryLoadingUnloading()
	{
		bool load = true;
		if (((MissionBehavior)this).Mission != null)
		{
			load = IsBackgroundsUsedInMission(((MissionBehavior)this).Mission);
		}
		LoadUnloadAllCategories(load);
	}

	private void LoadUnloadAllCategories(bool load)
	{
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Invalid comparison between Unknown and I4
		if (load)
		{
			if (!_fullBackgroundCategory.IsLoaded)
			{
				_fullBackgroundCategory.Load((ITwoDimensionResourceContext)(object)UIResourceManager.ResourceContext, UIResourceManager.ResourceDepot);
			}
			if (!_encyclopediaCategory.IsLoaded)
			{
				_encyclopediaCategory.Load((ITwoDimensionResourceContext)(object)UIResourceManager.ResourceContext, UIResourceManager.ResourceDepot);
			}
			SpriteCategory mapBarCategory = _mapBarCategory;
			if (mapBarCategory != null && !mapBarCategory.IsLoaded)
			{
				_mapBarCategory.Load((ITwoDimensionResourceContext)(object)UIResourceManager.ResourceContext, UIResourceManager.ResourceDepot);
			}
			return;
		}
		if (_fullBackgroundCategory.IsLoaded)
		{
			_fullBackgroundCategory.Unload();
		}
		if (_encyclopediaCategory.IsLoaded)
		{
			Mission mission = ((MissionBehavior)this).Mission;
			if (mission == null || (int)mission.Mode != 1)
			{
				_encyclopediaCategory.Unload();
			}
		}
		SpriteCategory mapBarCategory2 = _mapBarCategory;
		if (mapBarCategory2 != null && mapBarCategory2.IsLoaded)
		{
			_mapBarCategory.Unload();
		}
	}

	private bool IsBackgroundsUsedInMission(Mission mission)
	{
		if (!mission.IsInventoryAccessAllowed && !mission.IsCharacterWindowAccessAllowed && !mission.IsClanWindowAccessAllowed && !mission.IsKingdomWindowAccessAllowed && !mission.IsQuestScreenAccessAllowed && !mission.IsPartyWindowAccessAllowed)
		{
			return mission.IsEncyclopediaWindowAccessAllowed;
		}
		return true;
	}

	void IMissionListener.OnEquipItemsFromSpawnEquipmentBegin(Agent agent, CreationType creationType)
	{
	}

	void IMissionListener.OnEquipItemsFromSpawnEquipment(Agent agent, CreationType creationType)
	{
	}

	void IMissionListener.OnEndMission()
	{
	}

	void IMissionListener.OnMissionModeChange(MissionMode oldMissionMode, bool atStart)
	{
		HandleCategoryLoadingUnloading();
	}

	void IMissionListener.OnConversationCharacterChanged()
	{
	}

	void IMissionListener.OnResetMission()
	{
	}

	void IMissionListener.OnDeploymentPlanMade(Team team, bool isFirstPlan)
	{
	}
}
