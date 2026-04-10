using System;
using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.Lobby.Friends;
using TaleWorlds.PlayerServices;

namespace TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.Lobby.Clan;

public class MPLobbyClanAnnouncementVM : ViewModel
{
	private DateTime _announcedDate;

	private PlayerId _senderId;

	private int _id;

	private bool _canBeDeleted;

	private string _messageText;

	private string _details;

	private MPLobbyPlayerBaseVM _senderPlayer;

	[DataSourceProperty]
	public bool CanBeDeleted
	{
		get
		{
			return _canBeDeleted;
		}
		set
		{
			if (value != _canBeDeleted)
			{
				_canBeDeleted = value;
				((ViewModel)this).OnPropertyChanged("CanBeDeleted");
			}
		}
	}

	[DataSourceProperty]
	public string MessageText
	{
		get
		{
			return _messageText;
		}
		set
		{
			if (value != _messageText)
			{
				_messageText = value;
				((ViewModel)this).OnPropertyChanged("MessageText");
			}
		}
	}

	[DataSourceProperty]
	public string Details
	{
		get
		{
			return _details;
		}
		set
		{
			if (value != _details)
			{
				_details = value;
				((ViewModel)this).OnPropertyChanged("Details");
			}
		}
	}

	[DataSourceProperty]
	public MPLobbyPlayerBaseVM SenderPlayer
	{
		get
		{
			return _senderPlayer;
		}
		set
		{
			if (value != _senderPlayer)
			{
				_senderPlayer = value;
				((ViewModel)this).OnPropertyChanged("SenderPlayer");
			}
		}
	}

	public MPLobbyClanAnnouncementVM(PlayerId senderId, string message, DateTime date, int id, bool canBeDeleted)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		_id = id;
		_senderId = senderId;
		_announcedDate = date;
		SenderPlayer = new MPLobbyPlayerBaseVM(senderId);
		MessageText = message;
		CanBeDeleted = canBeDeleted;
		((ViewModel)this).RefreshValues();
	}

	public override void RefreshValues()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected O, but got Unknown
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Expected O, but got Unknown
		((ViewModel)this).RefreshValues();
		string text = ((object)new TextObject("{=oMiNaY1E}Posted By", (Dictionary<string, object>)null)).ToString();
		GameTexts.SetVariable("STR1", text);
		GameTexts.SetVariable("STR2", SenderPlayer.Name);
		string text2 = ((object)GameTexts.FindText("str_STR1_space_STR2", (string)null)).ToString();
		string dateFormattedByLanguage = LocalizedTextManager.GetDateFormattedByLanguage(BannerlordConfig.Language, _announcedDate);
		GameTexts.SetVariable("STR1", text2);
		GameTexts.SetVariable("STR2", dateFormattedByLanguage);
		Details = ((object)new TextObject("{=QvDxB57o}{STR1} | {STR2}", (Dictionary<string, object>)null)).ToString();
	}

	private void ExecuteDeleteAnnouncement()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Expected O, but got Unknown
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected O, but got Unknown
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Expected O, but got Unknown
		string? text = ((object)new TextObject("{=P1MybNr7}Delete Announcement", (Dictionary<string, object>)null)).ToString();
		string text2 = ((object)new TextObject("{=CW2JkWzC}Are you sure want to delete this announcement?", (Dictionary<string, object>)null)).ToString();
		InformationManager.ShowInquiry(new InquiryData(text, text2, true, true, ((object)GameTexts.FindText("str_yes", (string)null)).ToString(), ((object)GameTexts.FindText("str_no", (string)null)).ToString(), (Action)DeleteAnnouncement, (Action)null, "", 0f, (Action)null, (Func<ValueTuple<bool, string>>)null, (Func<ValueTuple<bool, string>>)null), false, false);
	}

	private void DeleteAnnouncement()
	{
		NetworkMain.GameClient.RemoveClanAnnouncement(_id);
	}
}
