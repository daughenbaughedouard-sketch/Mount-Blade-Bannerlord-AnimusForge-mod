using System;
using TaleWorlds.Library;

namespace SandBox.ViewModelCollection.Map.Cheat;

public class CheatGroupItemVM : CheatItemBaseVM
{
	public readonly GameplayCheatGroup CheatGroup;

	private readonly Action<CheatGroupItemVM> _onSelectCheatGroup;

	public CheatGroupItemVM(GameplayCheatGroup cheatGroup, Action<CheatGroupItemVM> onSelectCheatGroup)
	{
		CheatGroup = cheatGroup;
		_onSelectCheatGroup = onSelectCheatGroup;
		((ViewModel)this).RefreshValues();
	}

	public override void RefreshValues()
	{
		((ViewModel)this).RefreshValues();
		base.Name = ((object)CheatGroup.GetName())?.ToString();
	}

	public override void ExecuteAction()
	{
		_onSelectCheatGroup?.Invoke(this);
	}
}
