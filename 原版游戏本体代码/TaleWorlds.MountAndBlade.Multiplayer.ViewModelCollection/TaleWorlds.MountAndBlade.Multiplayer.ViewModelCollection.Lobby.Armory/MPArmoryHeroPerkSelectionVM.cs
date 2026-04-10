using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Selector;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.ClassLoadout;

namespace TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.Lobby.Armory;

public class MPArmoryHeroPerkSelectionVM : ViewModel
{
	private readonly Action<HeroPerkVM, MPPerkVM> _onPerkSelection;

	private readonly Action _forceRefreshCharacter;

	private List<string> _availableGameModes = new List<string> { "Skirmish", "Captain", "Siege", "TeamDeathmatch", "Duel" };

	private MBBindingList<HeroPerkVM> _perks;

	private SelectorVM<SelectorItemVM> _gameModes;

	public MPHeroClass CurrentHeroClass { get; private set; }

	public List<IReadOnlyPerkObject> CurrentSelectedPerks { get; private set; }

	[DataSourceProperty]
	public MBBindingList<HeroPerkVM> Perks
	{
		get
		{
			return _perks;
		}
		set
		{
			if (value != _perks)
			{
				_perks = value;
				((ViewModel)this).OnPropertyChangedWithValue<MBBindingList<HeroPerkVM>>(value, "Perks");
			}
		}
	}

	[DataSourceProperty]
	public SelectorVM<SelectorItemVM> GameModes
	{
		get
		{
			return _gameModes;
		}
		set
		{
			if (value != _gameModes)
			{
				_gameModes = value;
				((ViewModel)this).OnPropertyChangedWithValue<SelectorVM<SelectorItemVM>>(value, "GameModes");
			}
		}
	}

	public MPArmoryHeroPerkSelectionVM(Action<HeroPerkVM, MPPerkVM> onPerkSelection, Action forceRefreshCharacter)
	{
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Expected O, but got Unknown
		_onPerkSelection = onPerkSelection;
		_forceRefreshCharacter = forceRefreshCharacter;
		Perks = new MBBindingList<HeroPerkVM>();
		GameModes = new SelectorVM<SelectorItemVM>(0, (Action<SelectorVM<SelectorItemVM>>)OnGameModeSelectionChanged);
		foreach (string availableGameMode in _availableGameModes)
		{
			GameModes.AddItem(new SelectorItemVM(GameTexts.FindText("str_multiplayer_official_game_type_name", availableGameMode)));
		}
		GameModes.SelectedIndex = 0;
		((ViewModel)this).RefreshValues();
	}

	public override void RefreshValues()
	{
		((ViewModel)this).RefreshValues();
		((ViewModel)GameModes).RefreshValues();
		GameModes.SelectedIndex = 0;
		Perks.ApplyActionOnAllItems((Action<HeroPerkVM>)delegate(HeroPerkVM x)
		{
			((ViewModel)x).RefreshValues();
		});
	}

	public void RefreshPerksListWithHero(MPHeroClass heroClass)
	{
		Perks = new MBBindingList<HeroPerkVM>();
		CurrentHeroClass = heroClass;
		MBBindingList<HeroPerkVM> val = new MBBindingList<HeroPerkVM>();
		List<HeroPerkVM> list = new List<HeroPerkVM>();
		List<List<IReadOnlyPerkObject>> allPerksForHeroClass = MultiplayerClassDivisions.GetAllPerksForHeroClass(CurrentHeroClass, _availableGameModes[GameModes.SelectedIndex]);
		for (int i = 0; i < allPerksForHeroClass.Count; i++)
		{
			if (allPerksForHeroClass[i].Count > 0)
			{
				IReadOnlyPerkObject perk = allPerksForHeroClass[i][0];
				HeroPerkVM item = new HeroPerkVM(OnPerkSelection, perk, allPerksForHeroClass[i], i);
				((Collection<HeroPerkVM>)(object)val).Add(item);
				list.Add(item);
			}
		}
		Perks = val;
		if (CurrentSelectedPerks == null)
		{
			CurrentSelectedPerks = new List<IReadOnlyPerkObject>();
		}
		else
		{
			CurrentSelectedPerks.Clear();
		}
		foreach (HeroPerkVM item2 in list)
		{
			OnPerkSelection(item2, item2.SelectedPerkItem);
		}
	}

	private void OnGameModeSelectionChanged(SelectorVM<SelectorItemVM> selector)
	{
		if (GameModes.SelectedIndex == -1)
		{
			GameModes.SelectedIndex = 0;
		}
		if (CurrentHeroClass != null)
		{
			RefreshPerksListWithHero(CurrentHeroClass);
			_forceRefreshCharacter?.Invoke();
		}
	}

	private void OnPerkSelection(HeroPerkVM heroPerk, MPPerkVM candidate)
	{
		CurrentSelectedPerks = ((IEnumerable<HeroPerkVM>)Perks).Select((HeroPerkVM x) => x.SelectedPerk).ToList();
		_onPerkSelection?.Invoke(heroPerk, candidate);
	}
}
