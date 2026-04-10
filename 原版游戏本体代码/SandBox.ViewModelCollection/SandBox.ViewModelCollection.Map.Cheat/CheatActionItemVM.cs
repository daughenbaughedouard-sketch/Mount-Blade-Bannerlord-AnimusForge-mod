using System;
using TaleWorlds.Library;

namespace SandBox.ViewModelCollection.Map.Cheat;

public class CheatActionItemVM : CheatItemBaseVM
{
	public readonly GameplayCheatItem Cheat;

	private readonly Action<CheatActionItemVM> _onCheatExecuted;

	public CheatActionItemVM(GameplayCheatItem cheat, Action<CheatActionItemVM> onCheatExecuted)
	{
		_onCheatExecuted = onCheatExecuted;
		Cheat = cheat;
		((ViewModel)this).RefreshValues();
	}

	public override void RefreshValues()
	{
		((ViewModel)this).RefreshValues();
		base.Name = ((object)Cheat?.GetName()).ToString();
	}

	public override void ExecuteAction()
	{
		Cheat?.ExecuteCheat();
		_onCheatExecuted?.Invoke(this);
	}
}
