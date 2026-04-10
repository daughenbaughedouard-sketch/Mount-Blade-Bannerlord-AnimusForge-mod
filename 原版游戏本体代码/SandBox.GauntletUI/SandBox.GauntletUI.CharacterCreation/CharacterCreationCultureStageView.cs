using System;
using System.Collections.Generic;
using SandBox.View.CharacterCreation;
using SandBox.View.Missions;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterCreationContent;
using TaleWorlds.CampaignSystem.ViewModelCollection.CharacterCreation;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.Engine;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.ViewModelCollection.EscapeMenu;
using TaleWorlds.ObjectSystem;
using TaleWorlds.ScreenSystem;
using TaleWorlds.TwoDimension;

namespace SandBox.GauntletUI.CharacterCreation;

[CharacterCreationStageView(typeof(CharacterCreationCultureStage))]
public class CharacterCreationCultureStageView : CharacterCreationStageViewBase
{
	private const string CultureParameterId = "MissionCulture";

	private readonly GauntletMovieIdentifier _movie;

	private GauntletLayer GauntletLayer;

	private CharacterCreationCultureStageVM _dataSource;

	private SpriteCategory _characterCreationCategory;

	private SpriteCategory _bannerEditorCategory;

	private readonly CharacterCreationManager _characterCreationManager;

	private EscapeMenuVM _escapeMenuDatasource;

	private GauntletMovieIdentifier _escapeMenuMovie;

	public CharacterCreationCultureStageView(CharacterCreationManager characterCreationManager, ControlCharacterCreationStage affirmativeAction, TextObject affirmativeActionText, ControlCharacterCreationStage negativeAction, TextObject negativeActionText, ControlCharacterCreationStage onRefresh, ControlCharacterCreationStageReturnInt getCurrentStageIndexAction, ControlCharacterCreationStageReturnInt getTotalStageCountAction, ControlCharacterCreationStageReturnInt getFurthestIndexAction, ControlCharacterCreationStageWithInt goToIndexAction)
		: base(affirmativeAction, negativeAction, onRefresh, getCurrentStageIndexAction, getTotalStageCountAction, getFurthestIndexAction, goToIndexAction)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Expected O, but got Unknown
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Expected O, but got Unknown
		_characterCreationManager = characterCreationManager;
		GauntletLayer = new GauntletLayer("CharacterCreationCulture", 1, true)
		{
			IsFocusLayer = true
		};
		((ScreenLayer)GauntletLayer).InputRestrictions.SetInputRestrictions(true, (InputUsageMask)7);
		((ScreenLayer)GauntletLayer).Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("GenericPanelGameKeyCategory"));
		ScreenManager.TrySetFocus((ScreenLayer)(object)GauntletLayer);
		_dataSource = new CharacterCreationCultureStageVM(_characterCreationManager, (Action)NextStage, affirmativeActionText, (Action)PreviousStage, negativeActionText, (Action<CultureObject>)OnCultureSelected);
		_movie = GauntletLayer.LoadMovie("CharacterCreationCultureStage", (ViewModel)(object)_dataSource);
		_dataSource.SetCancelInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("Exit"));
		_dataSource.SetDoneInputKey(HotKeyManager.GetCategory("GenericPanelGameKeyCategory").GetHotKey("Confirm"));
		_characterCreationCategory = UIResourceManager.LoadSpriteCategory("ui_charactercreation");
		if (_characterCreationManager.GetStage<CharacterCreationBannerEditorStage>() != null)
		{
			_bannerEditorCategory = UIResourceManager.LoadSpriteCategory("ui_bannericons");
		}
	}

	protected override void OnFinalize()
	{
		base.OnFinalize();
		GauntletLayer = null;
		CharacterCreationCultureStageVM dataSource = _dataSource;
		if (dataSource != null)
		{
			((ViewModel)dataSource).OnFinalize();
		}
		_dataSource = null;
		_characterCreationCategory.Unload();
	}

	private void HandleLayerInput()
	{
		if (((ScreenLayer)GauntletLayer).Input.IsHotKeyReleased("Exit"))
		{
			UISoundsHelper.PlayUISound("event:/ui/panels/next");
			((CharacterCreationStageBaseVM)_dataSource).OnPreviousStage();
		}
		else if (((ScreenLayer)GauntletLayer).Input.IsHotKeyReleased("Confirm") && ((CharacterCreationStageBaseVM)_dataSource).CanAdvance)
		{
			UISoundsHelper.PlayUISound("event:/ui/panels/next");
			((CharacterCreationStageBaseVM)_dataSource).OnNextStage();
		}
	}

	public override void Tick(float dt)
	{
		base.Tick(dt);
		if (_dataSource.IsActive)
		{
			HandleEscapeMenu(this, (ScreenLayer)(object)GauntletLayer);
			HandleLayerInput();
		}
	}

	public override void NextStage()
	{
		_characterCreationManager.CharacterCreationContent.SetMainCharacterName(((object)NameGenerator.Current.GenerateFirstNameForPlayer(_dataSource.CurrentSelectedCulture.Culture, Hero.MainHero.IsFemale)).ToString());
		_affirmativeAction.Invoke();
	}

	private void OnCultureSelected(CultureObject culture)
	{
		MissionSoundParametersView.SoundParameterMissionCulture soundParameterMissionCulture = MissionSoundParametersView.SoundParameterMissionCulture.None;
		if (((MBObjectBase)culture).StringId == "aserai")
		{
			soundParameterMissionCulture = MissionSoundParametersView.SoundParameterMissionCulture.Aserai;
		}
		else if (((MBObjectBase)culture).StringId == "khuzait")
		{
			soundParameterMissionCulture = MissionSoundParametersView.SoundParameterMissionCulture.Khuzait;
		}
		else if (((MBObjectBase)culture).StringId == "vlandia")
		{
			soundParameterMissionCulture = MissionSoundParametersView.SoundParameterMissionCulture.Vlandia;
		}
		else if (((MBObjectBase)culture).StringId == "sturgia")
		{
			soundParameterMissionCulture = MissionSoundParametersView.SoundParameterMissionCulture.Sturgia;
		}
		else if (((MBObjectBase)culture).StringId == "battania")
		{
			soundParameterMissionCulture = MissionSoundParametersView.SoundParameterMissionCulture.Battania;
		}
		else if (((MBObjectBase)culture).StringId == "empire")
		{
			soundParameterMissionCulture = MissionSoundParametersView.SoundParameterMissionCulture.Empire;
		}
		else if (((MBObjectBase)culture).StringId == "nord")
		{
			soundParameterMissionCulture = MissionSoundParametersView.SoundParameterMissionCulture.Nord;
		}
		SoundManager.SetGlobalParameter("MissionCulture", (float)soundParameterMissionCulture);
	}

	public override void PreviousStage()
	{
		Game.Current.GameStateManager.PopState(0);
	}

	public override int GetVirtualStageCount()
	{
		return 1;
	}

	public override IEnumerable<ScreenLayer> GetLayers()
	{
		return new List<ScreenLayer> { (ScreenLayer)(object)GauntletLayer };
	}

	public override void LoadEscapeMenuMovie()
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Expected O, but got Unknown
		_escapeMenuDatasource = new EscapeMenuVM((IEnumerable<EscapeMenuItemVM>)GetEscapeMenuItems(this), (TextObject)null);
		_escapeMenuMovie = GauntletLayer.LoadMovie("EscapeMenu", (ViewModel)(object)_escapeMenuDatasource);
	}

	public override void ReleaseEscapeMenuMovie()
	{
		GauntletLayer.ReleaseMovie(_escapeMenuMovie);
		_escapeMenuDatasource = null;
		_escapeMenuMovie = null;
	}
}
