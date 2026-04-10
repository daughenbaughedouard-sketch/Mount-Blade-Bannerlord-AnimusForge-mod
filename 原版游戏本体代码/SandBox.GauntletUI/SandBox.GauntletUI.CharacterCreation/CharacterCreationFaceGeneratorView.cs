using System.Collections.Generic;
using SandBox.View.CharacterCreation;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterCreationContent;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.GauntletUI.BodyGenerator;
using TaleWorlds.MountAndBlade.ViewModelCollection.EscapeMenu;
using TaleWorlds.ObjectSystem;
using TaleWorlds.ScreenSystem;

namespace SandBox.GauntletUI.CharacterCreation;

[CharacterCreationStageView(typeof(CharacterCreationFaceGeneratorStage))]
public class CharacterCreationFaceGeneratorView : CharacterCreationStageViewBase
{
	private BodyGeneratorView _faceGeneratorView;

	private readonly CharacterCreationManager _characterCreationManager;

	private EscapeMenuVM _escapeMenuDatasource;

	private GauntletMovieIdentifier _escapeMenuMovie;

	public CharacterCreationFaceGeneratorView(CharacterCreationManager characterCreationManager, ControlCharacterCreationStage affirmativeAction, TextObject affirmativeActionText, ControlCharacterCreationStage negativeAction, TextObject negativeActionText, ControlCharacterCreationStage onRefresh, ControlCharacterCreationStageReturnInt getCurrentStageIndexAction, ControlCharacterCreationStageReturnInt getTotalStageCountAction, ControlCharacterCreationStageReturnInt getFurthestIndexAction, ControlCharacterCreationStageWithInt goToIndexAction)
		: base(affirmativeAction, negativeAction, onRefresh, getTotalStageCountAction, getCurrentStageIndexAction, getFurthestIndexAction, goToIndexAction)
	{
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Expected O, but got Unknown
		//IL_009b: Expected O, but got Unknown
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Expected O, but got Unknown
		_characterCreationManager = characterCreationManager;
		MBObjectManager objectManager = Game.Current.ObjectManager;
		CharacterObject playerCharacter = CharacterObject.PlayerCharacter;
		object obj;
		if (playerCharacter == null)
		{
			obj = null;
		}
		else
		{
			CultureObject culture = playerCharacter.Culture;
			obj = ((culture != null) ? ((MBObjectBase)culture).StringId : null);
		}
		MBEquipmentRoster obj2 = objectManager.GetObject<MBEquipmentRoster>("player_char_creation_show_" + (string?)obj);
		Equipment val = ((obj2 != null) ? obj2.DefaultEquipment : null);
		_faceGeneratorView = new BodyGeneratorView(new ControlCharacterCreationStage(NextStage), affirmativeActionText, new ControlCharacterCreationStage(PreviousStage), negativeActionText, (BasicCharacterObject)(object)CharacterObject.PlayerCharacter, false, (IFaceGeneratorCustomFilter)null, val, getCurrentStageIndexAction, getTotalStageCountAction, getFurthestIndexAction, goToIndexAction, _characterCreationManager.FaceGenHistory);
	}

	protected override void OnFinalize()
	{
		base.OnFinalize();
		_faceGeneratorView.OnFinalize();
		_faceGeneratorView = null;
	}

	public override IEnumerable<ScreenLayer> GetLayers()
	{
		return new List<ScreenLayer>
		{
			(ScreenLayer)(object)_faceGeneratorView.SceneLayer,
			(ScreenLayer)(object)_faceGeneratorView.GauntletLayer
		};
	}

	public override void PreviousStage()
	{
		_negativeAction.Invoke();
	}

	public override void NextStage()
	{
		_affirmativeAction.Invoke();
	}

	public override void Tick(float dt)
	{
		_faceGeneratorView.OnTick(dt);
	}

	public override int GetVirtualStageCount()
	{
		return 1;
	}

	public override void GoToIndex(int index)
	{
		_goToIndexAction.Invoke(index);
	}

	public override void LoadEscapeMenuMovie()
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Expected O, but got Unknown
		_escapeMenuDatasource = new EscapeMenuVM((IEnumerable<EscapeMenuItemVM>)GetEscapeMenuItems(this), (TextObject)null);
		_escapeMenuMovie = _faceGeneratorView.GauntletLayer.LoadMovie("EscapeMenu", (ViewModel)(object)_escapeMenuDatasource);
	}

	public override void ReleaseEscapeMenuMovie()
	{
		_faceGeneratorView.GauntletLayer.ReleaseMovie(_escapeMenuMovie);
		_escapeMenuDatasource = null;
		_escapeMenuMovie = null;
	}
}
