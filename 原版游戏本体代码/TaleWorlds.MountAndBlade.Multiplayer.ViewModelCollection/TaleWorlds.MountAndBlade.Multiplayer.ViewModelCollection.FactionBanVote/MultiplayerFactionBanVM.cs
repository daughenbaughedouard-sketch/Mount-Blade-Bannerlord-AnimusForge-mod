using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using NetworkMessages.FromClient;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.FactionBanVote;

public class MultiplayerFactionBanVM : ViewModel
{
	private MBBindingList<MultiplayerFactionBanVoteVM> _banList;

	private MBBindingList<MultiplayerFactionBanVoteVM> _selectList;

	private string _selectTitle;

	private string _banTitle;

	[DataSourceProperty]
	public MBBindingList<MultiplayerFactionBanVoteVM> SelectList
	{
		get
		{
			return _selectList;
		}
		set
		{
			if (value != _selectList)
			{
				_selectList = value;
				((ViewModel)this).OnPropertyChangedWithValue<MBBindingList<MultiplayerFactionBanVoteVM>>(value, "SelectList");
			}
		}
	}

	[DataSourceProperty]
	public MBBindingList<MultiplayerFactionBanVoteVM> BanList
	{
		get
		{
			return _banList;
		}
		set
		{
			if (value != _banList)
			{
				_banList = value;
				((ViewModel)this).OnPropertyChangedWithValue<MBBindingList<MultiplayerFactionBanVoteVM>>(value, "BanList");
			}
		}
	}

	[DataSourceProperty]
	public string SelectTitle
	{
		get
		{
			return _selectTitle;
		}
		set
		{
			if (value != _selectTitle)
			{
				_selectTitle = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "SelectTitle");
			}
		}
	}

	[DataSourceProperty]
	public string BanTitle
	{
		get
		{
			return _banTitle;
		}
		set
		{
			if (value != _banTitle)
			{
				_banTitle = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "BanTitle");
			}
		}
	}

	public MultiplayerFactionBanVM()
	{
		SelectTitle = "SELECT FACTION";
		BanTitle = "BAN FACTION";
		_banList = new MBBindingList<MultiplayerFactionBanVoteVM>();
		foreach (BasicCultureObject availableCulture in MultiplayerClassDivisions.AvailableCultures)
		{
			((Collection<MultiplayerFactionBanVoteVM>)(object)_banList).Add(new MultiplayerFactionBanVoteVM(availableCulture, OnBanFaction));
		}
		_selectList = new MBBindingList<MultiplayerFactionBanVoteVM>();
		foreach (BasicCultureObject availableCulture2 in MultiplayerClassDivisions.AvailableCultures)
		{
			((Collection<MultiplayerFactionBanVoteVM>)(object)_selectList).Add(new MultiplayerFactionBanVoteVM(availableCulture2, OnSelectFaction));
		}
		foreach (MultiplayerFactionBanVoteVM item in (Collection<MultiplayerFactionBanVoteVM>)(object)_selectList)
		{
			if (item.IsEnabled)
			{
				item.IsSelected = true;
				break;
			}
		}
	}

	public override void RefreshValues()
	{
		((ViewModel)this).RefreshValues();
	}

	public override void OnFinalize()
	{
		((ViewModel)this).OnFinalize();
	}

	private void OnSelectFaction(MultiplayerFactionBanVoteVM vote)
	{
		VoteForCulture((CultureVoteTypes)1, vote.Culture);
	}

	private void OnBanFaction(MultiplayerFactionBanVoteVM vote)
	{
		VoteForCulture((CultureVoteTypes)0, vote.Culture);
	}

	private void Refresh()
	{
		foreach (MultiplayerFactionBanVoteVM item in (Collection<MultiplayerFactionBanVoteVM>)(object)_banList)
		{
			item.IsSelected = false;
			item.IsEnabled = false;
		}
		MissionPeer component = PeerExtensions.GetComponent<MissionPeer>(GameNetwork.MyPeer);
		bool flag = false;
		foreach (MultiplayerFactionBanVoteVM item2 in (Collection<MultiplayerFactionBanVoteVM>)(object)_selectList)
		{
			if (flag)
			{
				item2.IsSelected = true;
				flag = false;
				break;
			}
			if (component.VotedForBan == item2.Culture)
			{
				item2.IsEnabled = false;
				if (item2.IsSelected)
				{
					item2.IsSelected = false;
					flag = true;
				}
			}
		}
		if (flag)
		{
			MultiplayerFactionBanVoteVM multiplayerFactionBanVoteVM = ((IEnumerable<MultiplayerFactionBanVoteVM>)_selectList).FirstOrDefault((MultiplayerFactionBanVoteVM s) => s.IsEnabled);
			if (multiplayerFactionBanVoteVM != null)
			{
				multiplayerFactionBanVoteVM.IsSelected = true;
			}
		}
	}

	private static void VoteForCulture(CultureVoteTypes voteType, BasicCultureObject culture)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Expected O, but got Unknown
		MissionPeer component = PeerExtensions.GetComponent<MissionPeer>(GameNetwork.MyPeer);
		if (GameNetwork.IsServer)
		{
			component.HandleVoteChange(voteType, culture);
		}
		else if (GameNetwork.IsClient)
		{
			GameNetwork.BeginModuleEventAsClient();
			GameNetwork.WriteMessage((GameNetworkMessage)new CultureVoteClient(voteType, culture));
			GameNetwork.EndModuleEventAsClient();
		}
	}
}
