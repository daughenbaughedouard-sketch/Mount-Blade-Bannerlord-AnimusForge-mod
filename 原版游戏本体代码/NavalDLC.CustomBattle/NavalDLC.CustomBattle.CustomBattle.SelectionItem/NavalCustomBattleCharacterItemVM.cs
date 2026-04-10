using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Selector;

namespace NavalDLC.CustomBattle.CustomBattle.SelectionItem;

public class NavalCustomBattleCharacterItemVM : SelectorItemVM
{
	public BasicCharacterObject Character { get; private set; }

	public NavalCustomBattleCharacterItemVM(BasicCharacterObject character)
		: base(((object)character.Name).ToString())
	{
		Character = character;
	}
}
