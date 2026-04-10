using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem.CharacterCreationContent;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.ViewModelCollection.EscapeMenu;
using TaleWorlds.ScreenSystem;

namespace SandBox.View.CharacterCreation;

public abstract class CharacterCreationStageViewBase : ICharacterCreationStageListener
{
	protected readonly ControlCharacterCreationStage _affirmativeAction;

	protected readonly ControlCharacterCreationStage _negativeAction;

	protected readonly ControlCharacterCreationStage _refreshAction;

	protected readonly ControlCharacterCreationStageReturnInt _getTotalStageCountAction;

	protected readonly ControlCharacterCreationStageReturnInt _getCurrentStageIndexAction;

	protected readonly ControlCharacterCreationStageReturnInt _getFurthestIndexAction;

	protected readonly ControlCharacterCreationStageWithInt _goToIndexAction;

	protected readonly Vec3 _cameraPosition = new Vec3(6.45f, 4.35f, 1.6f, -1f);

	private bool _isEscapeOpen;

	protected CharacterCreationStageViewBase(ControlCharacterCreationStage affirmativeAction, ControlCharacterCreationStage negativeAction, ControlCharacterCreationStage refreshAction, ControlCharacterCreationStageReturnInt getCurrentStageIndexAction, ControlCharacterCreationStageReturnInt getTotalStageCountAction, ControlCharacterCreationStageReturnInt getFurthestIndexAction, ControlCharacterCreationStageWithInt goToIndexAction)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		_affirmativeAction = affirmativeAction;
		_negativeAction = negativeAction;
		_refreshAction = refreshAction;
		_getTotalStageCountAction = getTotalStageCountAction;
		_getCurrentStageIndexAction = getCurrentStageIndexAction;
		_getFurthestIndexAction = getFurthestIndexAction;
		_goToIndexAction = goToIndexAction;
	}

	public virtual void SetGenericScene(Scene scene)
	{
	}

	protected virtual void OnRefresh()
	{
		_refreshAction.Invoke();
	}

	public abstract IEnumerable<ScreenLayer> GetLayers();

	public abstract void NextStage();

	public abstract void PreviousStage();

	void ICharacterCreationStageListener.OnStageFinalize()
	{
		OnFinalize();
	}

	protected virtual void OnFinalize()
	{
	}

	public virtual void Tick(float dt)
	{
	}

	public abstract int GetVirtualStageCount();

	public virtual void GoToIndex(int index)
	{
		_goToIndexAction.Invoke(index);
	}

	public abstract void LoadEscapeMenuMovie();

	public abstract void ReleaseEscapeMenuMovie();

	public void HandleEscapeMenu(CharacterCreationStageViewBase view, ScreenLayer screenLayer)
	{
		if (screenLayer.Input.IsHotKeyReleased("ToggleEscapeMenu"))
		{
			if (_isEscapeOpen)
			{
				RemoveEscapeMenu(view);
			}
			else
			{
				OpenEscapeMenu(view);
			}
		}
	}

	private void OpenEscapeMenu(CharacterCreationStageViewBase view)
	{
		view.LoadEscapeMenuMovie();
		_isEscapeOpen = true;
	}

	private void RemoveEscapeMenu(CharacterCreationStageViewBase view)
	{
		view.ReleaseEscapeMenuMovie();
		_isEscapeOpen = false;
	}

	public List<EscapeMenuItemVM> GetEscapeMenuItems(CharacterCreationStageViewBase view)
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Expected O, but got Unknown
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Expected O, but got Unknown
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Expected O, but got Unknown
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Expected O, but got Unknown
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Expected O, but got Unknown
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Expected O, but got Unknown
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Expected O, but got Unknown
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Expected O, but got Unknown
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Expected O, but got Unknown
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Expected O, but got Unknown
		//IL_0184: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bb: Expected O, but got Unknown
		//IL_01b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c0: Expected O, but got Unknown
		//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fe: Expected O, but got Unknown
		//IL_01f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0203: Expected O, but got Unknown
		//IL_020a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0241: Expected O, but got Unknown
		//IL_023c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0246: Expected O, but got Unknown
		TextObject characterCreationDisabledReason = GameTexts.FindText("str_pause_menu_disabled_hint", "CharacterCreation");
		return new List<EscapeMenuItemVM>
		{
			new EscapeMenuItemVM(new TextObject("{=5Saniypu}Resume", (Dictionary<string, object>)null), (Action<object>)delegate
			{
				RemoveEscapeMenu(view);
			}, (object)null, (Func<Tuple<bool, TextObject>>)(() => new Tuple<bool, TextObject>(item1: false, null)), true),
			new EscapeMenuItemVM(new TextObject("{=PXT6aA4J}Campaign Options", (Dictionary<string, object>)null), (Action<object>)delegate
			{
			}, (object)null, (Func<Tuple<bool, TextObject>>)(() => new Tuple<bool, TextObject>(item1: true, characterCreationDisabledReason)), false),
			new EscapeMenuItemVM(new TextObject("{=NqarFr4P}Options", (Dictionary<string, object>)null), (Action<object>)delegate
			{
			}, (object)null, (Func<Tuple<bool, TextObject>>)(() => new Tuple<bool, TextObject>(item1: true, characterCreationDisabledReason)), false),
			new EscapeMenuItemVM(new TextObject("{=bV75iwKa}Save", (Dictionary<string, object>)null), (Action<object>)delegate
			{
			}, (object)null, (Func<Tuple<bool, TextObject>>)(() => new Tuple<bool, TextObject>(item1: true, characterCreationDisabledReason)), false),
			new EscapeMenuItemVM(new TextObject("{=e0KdfaNe}Save As", (Dictionary<string, object>)null), (Action<object>)delegate
			{
			}, (object)null, (Func<Tuple<bool, TextObject>>)(() => new Tuple<bool, TextObject>(item1: true, characterCreationDisabledReason)), false),
			new EscapeMenuItemVM(new TextObject("{=9NuttOBC}Load", (Dictionary<string, object>)null), (Action<object>)delegate
			{
			}, (object)null, (Func<Tuple<bool, TextObject>>)(() => new Tuple<bool, TextObject>(item1: true, characterCreationDisabledReason)), false),
			new EscapeMenuItemVM(new TextObject("{=AbEh2y8o}Save And Exit", (Dictionary<string, object>)null), (Action<object>)delegate
			{
			}, (object)null, (Func<Tuple<bool, TextObject>>)(() => new Tuple<bool, TextObject>(item1: true, characterCreationDisabledReason)), false),
			new EscapeMenuItemVM(new TextObject("{=RamV6yLM}Exit to Main Menu", (Dictionary<string, object>)null), (Action<object>)delegate
			{
				RemoveEscapeMenu(view);
				view.OnFinalize();
				MBGameManager.EndGame();
			}, (object)null, (Func<Tuple<bool, TextObject>>)(() => new Tuple<bool, TextObject>(item1: false, null)), false)
		};
	}
}
