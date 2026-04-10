using System;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.KillFeed.Personal;

public class MPPersonalKillNotificationItemVM : ViewModel
{
	private enum ItemTypes
	{
		NormalDamage,
		FriendlyFireDamage,
		FriendlyFireKill,
		MountDamage,
		NormalKill,
		Assist,
		GoldChange,
		HeadshotKill
	}

	private Action<MPPersonalKillNotificationItemVM> _onRemoveItem;

	private ItemTypes _itemTypeAsEnum;

	private string _message;

	private int _amount;

	private int _itemType;

	private ItemTypes ItemTypeAsEnum
	{
		get
		{
			return _itemTypeAsEnum;
		}
		set
		{
			_itemType = (int)value;
			_itemTypeAsEnum = value;
		}
	}

	[DataSourceProperty]
	public string Message
	{
		get
		{
			return _message;
		}
		set
		{
			if (value != _message)
			{
				_message = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "Message");
			}
		}
	}

	[DataSourceProperty]
	public int ItemType
	{
		get
		{
			return _itemType;
		}
		set
		{
			if (value != _itemType)
			{
				_itemType = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "ItemType");
			}
		}
	}

	[DataSourceProperty]
	public int Amount
	{
		get
		{
			return _amount;
		}
		set
		{
			if (value != _amount)
			{
				_amount = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "Amount");
			}
		}
	}

	public MPPersonalKillNotificationItemVM(int amount, bool isFatal, bool isMountDamage, bool isFriendlyFire, bool isHeadshot, string killedAgentName, Action<MPPersonalKillNotificationItemVM> onRemoveItem)
	{
		_onRemoveItem = onRemoveItem;
		Amount = amount;
		if (isFriendlyFire)
		{
			ItemTypeAsEnum = ((!isFatal) ? ItemTypes.FriendlyFireDamage : ItemTypes.FriendlyFireKill);
			Message = killedAgentName;
		}
		else if (isMountDamage)
		{
			ItemTypeAsEnum = ItemTypes.MountDamage;
			Message = ((object)GameTexts.FindText("str_damage_delivered_message", (string)null)).ToString();
		}
		else if (isFatal)
		{
			ItemTypeAsEnum = (isHeadshot ? ItemTypes.HeadshotKill : ItemTypes.NormalKill);
			Message = killedAgentName;
		}
		else
		{
			ItemTypeAsEnum = ItemTypes.NormalDamage;
			Message = ((object)GameTexts.FindText("str_damage_delivered_message", (string)null)).ToString();
		}
	}

	public MPPersonalKillNotificationItemVM(int amount, GoldGainFlags reasonType, Action<MPPersonalKillNotificationItemVM> onRemoveItem)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Invalid comparison between Unknown and I4
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Invalid comparison between Unknown and I4
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Invalid comparison between Unknown and I4
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Invalid comparison between Unknown and I4
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Invalid comparison between Unknown and I4
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Invalid comparison between Unknown and I4
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Expected I4, but got Unknown
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Invalid comparison between Unknown and I4
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Invalid comparison between Unknown and I4
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Invalid comparison between Unknown and I4
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Invalid comparison between Unknown and I4
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Invalid comparison between Unknown and I4
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Invalid comparison between Unknown and I4
		_onRemoveItem = onRemoveItem;
		ItemTypeAsEnum = ItemTypes.GoldChange;
		if ((int)reasonType <= 64)
		{
			if ((int)reasonType <= 8)
			{
				switch (reasonType - 1)
				{
				case 0:
					goto IL_00a7;
				case 1:
					goto IL_00c2;
				case 3:
					goto IL_00dd;
				case 2:
					goto IL_01df;
				}
				if ((int)reasonType != 8)
				{
					goto IL_01df;
				}
				Message = ((object)GameTexts.FindText("str_gold_gain_second_assist", (string)null)).ToString();
			}
			else if ((int)reasonType != 16)
			{
				if ((int)reasonType != 32)
				{
					if ((int)reasonType != 64)
					{
						goto IL_01df;
					}
					Message = ((object)GameTexts.FindText("str_gold_gain_tenth_kill", (string)null)).ToString();
				}
				else
				{
					Message = ((object)GameTexts.FindText("str_gold_gain_fifth_kill", (string)null)).ToString();
				}
			}
			else
			{
				Message = ((object)GameTexts.FindText("str_gold_gain_third_assist", (string)null)).ToString();
			}
		}
		else if ((int)reasonType <= 256)
		{
			if ((int)reasonType != 128)
			{
				if ((int)reasonType != 256)
				{
					goto IL_01df;
				}
				Message = ((object)GameTexts.FindText("str_gold_gain_default_assist", (string)null)).ToString();
			}
			else
			{
				Message = ((object)GameTexts.FindText("str_gold_gain_default_kill", (string)null)).ToString();
			}
		}
		else if ((int)reasonType != 512)
		{
			if ((int)reasonType != 1024)
			{
				if ((int)reasonType != 2048)
				{
					goto IL_01df;
				}
				Message = ((object)GameTexts.FindText("str_gold_gain_perk_bonus", (string)null)).ToString();
			}
			else
			{
				Message = ((object)GameTexts.FindText("str_gold_gain_objective_destroyed", (string)null)).ToString();
			}
		}
		else
		{
			Message = ((object)GameTexts.FindText("str_gold_gain_objective_completed", (string)null)).ToString();
		}
		goto IL_0200;
		IL_0200:
		Amount = amount;
		return;
		IL_00c2:
		Message = ((object)GameTexts.FindText("str_gold_gain_first_melee_kill", (string)null)).ToString();
		goto IL_0200;
		IL_00a7:
		Message = ((object)GameTexts.FindText("str_gold_gain_first_ranged_kill", (string)null)).ToString();
		goto IL_0200;
		IL_00dd:
		Message = ((object)GameTexts.FindText("str_gold_gain_first_assist", (string)null)).ToString();
		goto IL_0200;
		IL_01df:
		Debug.FailedAssert("Undefined gold change type", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection\\KillFeed\\Personal\\MPPersonalKillNotificationItemVM.cs", ".ctor", 117);
		Message = "";
		goto IL_0200;
	}

	public MPPersonalKillNotificationItemVM(string victimAgentName, Action<MPPersonalKillNotificationItemVM> onRemoveItem)
	{
		_onRemoveItem = onRemoveItem;
		Amount = -1;
		Message = victimAgentName;
		ItemTypeAsEnum = ItemTypes.Assist;
	}

	public void ExecuteRemove()
	{
		_onRemoveItem(this);
	}
}
