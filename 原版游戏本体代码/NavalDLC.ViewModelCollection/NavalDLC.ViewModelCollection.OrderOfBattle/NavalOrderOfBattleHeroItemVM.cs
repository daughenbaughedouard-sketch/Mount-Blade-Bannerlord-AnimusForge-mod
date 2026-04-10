using System;
using System.Collections.Generic;
using System.Linq;
using SandBox.ViewModelCollection;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Extensions;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.ImageIdentifiers;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;

namespace NavalDLC.ViewModelCollection.OrderOfBattle;

public class NavalOrderOfBattleHeroItemVM : ViewModel
{
	public readonly IAgentOriginBase AgentOrigin;

	private readonly Action<NavalOrderOfBattleHeroItemVM, bool> _onSelected;

	private List<TooltipProperty> _cachedTooltipProperties;

	private readonly TextObject _perkDefinitionText = new TextObject("{=jCdZY3i4}{PERK_NAME} ({SKILL_LEVEL} - {SKILL})", (Dictionary<string, object>)null);

	private readonly TextObject _captainPerksText = new TextObject("{=pgXuyHxH}Captain Perks", (Dictionary<string, object>)null);

	private readonly TextObject _infantryInfluenceText = new TextObject("{=SSLUHH6j}Infantry Influence", (Dictionary<string, object>)null);

	private readonly TextObject _rangedInfluenceText = new TextObject("{=0DMM0agr}Ranged Influence", (Dictionary<string, object>)null);

	private readonly TextObject _noPerksText = new TextObject("{=7yaDnyKb}There is no additional perk influence.", (Dictionary<string, object>)null);

	private readonly PerkObjectComparer _perkComparer = new PerkObjectComparer();

	private bool _isDisabled;

	private bool _isSelected;

	private bool _isMainHero;

	private CharacterImageIdentifierVM _imageIdentifier;

	private BasicTooltipViewModel _tooltip;

	[DataSourceProperty]
	public bool IsDisabled
	{
		get
		{
			return _isDisabled;
		}
		set
		{
			if (value != _isDisabled)
			{
				_isDisabled = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsDisabled");
			}
		}
	}

	[DataSourceProperty]
	public bool IsSelected
	{
		get
		{
			return _isSelected;
		}
		set
		{
			if (value != _isSelected)
			{
				_isSelected = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsSelected");
			}
		}
	}

	[DataSourceProperty]
	public bool IsMainHero
	{
		get
		{
			return _isMainHero;
		}
		set
		{
			if (value != _isMainHero)
			{
				_isMainHero = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsMainHero");
			}
		}
	}

	[DataSourceProperty]
	public CharacterImageIdentifierVM ImageIdentifier
	{
		get
		{
			return _imageIdentifier;
		}
		set
		{
			if (value != _imageIdentifier)
			{
				_imageIdentifier = value;
				((ViewModel)this).OnPropertyChangedWithValue<CharacterImageIdentifierVM>(value, "ImageIdentifier");
			}
		}
	}

	[DataSourceProperty]
	public BasicTooltipViewModel Tooltip
	{
		get
		{
			return _tooltip;
		}
		set
		{
			if (value != _tooltip)
			{
				_tooltip = value;
				((ViewModel)this).OnPropertyChangedWithValue<BasicTooltipViewModel>(value, "Tooltip");
			}
		}
	}

	public NavalOrderOfBattleHeroItemVM(IAgentOriginBase agentOrigin, Action<NavalOrderOfBattleHeroItemVM, bool> onSelected)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Expected O, but got Unknown
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Expected O, but got Unknown
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Expected O, but got Unknown
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Expected O, but got Unknown
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Expected O, but got Unknown
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Expected O, but got Unknown
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Expected O, but got Unknown
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Expected O, but got Unknown
		_onSelected = onSelected;
		AgentOrigin = agentOrigin;
		ImageIdentifier = new CharacterImageIdentifierVM(CharacterCode.CreateFrom(agentOrigin.Troop));
		IsMainHero = agentOrigin.Troop.IsPlayerCharacter;
		Tooltip = new BasicTooltipViewModel((Func<List<TooltipProperty>>)(() => _cachedTooltipProperties));
		((ViewModel)this).RefreshValues();
	}

	public override void RefreshValues()
	{
		((ViewModel)this).RefreshValues();
		_cachedTooltipProperties = GetTooltip();
	}

	public void ExecuteSelect()
	{
		if (!IsDisabled)
		{
			_onSelected?.Invoke(this, arg2: true);
		}
	}

	public void ExecuteToggleSelect()
	{
		if (!IsDisabled)
		{
			_onSelected?.Invoke(this, !IsSelected);
		}
	}

	public void ExecuteDeselect()
	{
		if (!IsDisabled)
		{
			_onSelected?.Invoke(this, arg2: false);
		}
	}

	private List<TooltipProperty> GetTooltip()
	{
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Expected O, but got Unknown
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Expected O, but got Unknown
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Expected O, but got Unknown
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Expected O, but got Unknown
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Expected O, but got Unknown
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Expected O, but got Unknown
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Expected O, but got Unknown
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b0: Expected O, but got Unknown
		//IL_01db: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ec: Expected O, but got Unknown
		//IL_01fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_021b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0249: Unknown result type (might be due to invalid IL or missing references)
		//IL_024e: Unknown result type (might be due to invalid IL or missing references)
		//IL_025a: Expected O, but got Unknown
		//IL_027b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0280: Unknown result type (might be due to invalid IL or missing references)
		//IL_028c: Expected O, but got Unknown
		//IL_029a: Unknown result type (might be due to invalid IL or missing references)
		//IL_029f: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ab: Expected O, but got Unknown
		//IL_03e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ea: Expected O, but got Unknown
		//IL_02ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f8: Expected O, but got Unknown
		//IL_045c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0461: Unknown result type (might be due to invalid IL or missing references)
		//IL_046d: Expected O, but got Unknown
		//IL_0311: Unknown result type (might be due to invalid IL or missing references)
		//IL_0318: Invalid comparison between Unknown and I4
		//IL_032a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0331: Invalid comparison between Unknown and I4
		//IL_031c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0323: Invalid comparison between Unknown and I4
		//IL_03a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b0: Expected O, but got Unknown
		BasicCharacterObject troop = AgentOrigin.Troop;
		BasicCharacterObject obj = ((troop is CharacterObject) ? troop : null);
		Hero val = ((obj != null) ? ((CharacterObject)obj).HeroObject : null);
		List<TooltipProperty> list = new List<TooltipProperty>
		{
			new TooltipProperty(((val != null) ? ((object)val.Name).ToString() : null) ?? ((object)AgentOrigin.Troop.Name).ToString(), string.Empty, 0, false, (TooltipPropertyFlags)4096)
		};
		if (IsMainHero)
		{
			list.Add(new TooltipProperty(string.Empty, ((object)new TextObject("{=9y7LtTLf}Main hero is always assigned to the first formation.", (Dictionary<string, object>)null)).ToString(), 0, false, (TooltipPropertyFlags)0));
			list.Add(new TooltipProperty(string.Empty, string.Empty, 0, false, (TooltipPropertyFlags)0));
		}
		else if (IsDisabled)
		{
			list.Add(new TooltipProperty(string.Empty, ((object)new TextObject("{=3XlyBbSE}You cannot move heroes when you are not the general.", (Dictionary<string, object>)null)).ToString(), 0, false, (TooltipPropertyFlags)0));
			list.Add(new TooltipProperty(string.Empty, string.Empty, 0, false, (TooltipPropertyFlags)0));
		}
		if (((val != null) ? val.PartyBelongedTo : null) != null)
		{
			list.Add(new TooltipProperty(((object)GameTexts.FindText("str_party", (string)null)).ToString(), ((object)val.PartyBelongedTo.Name).ToString(), 0, false, (TooltipPropertyFlags)0));
		}
		if (val != null)
		{
			foreach (SkillObject item in (List<SkillObject>)(object)Skills.All)
			{
				if (((MBObjectBase)item).StringId == "Mariner" || ((MBObjectBase)item).StringId == "Boatswain" || ((MBObjectBase)item).StringId == "Shipmaster")
				{
					list.Add(new TooltipProperty(((object)((PropertyObject)item).Name).ToString(), val.GetSkillValue(item).ToString(), 0, false, (TooltipPropertyFlags)0)
					{
						OnlyShowWhenNotExtended = true
					});
				}
			}
			list.Add(new TooltipProperty(string.Empty, string.Empty, 0, false, (TooltipPropertyFlags)1024)
			{
				OnlyShowWhenNotExtended = true
			});
			List<PerkObject> first = default(List<PerkObject>);
			float captainRatingForTroopUsages = Campaign.Current.Models.BattleCaptainModel.GetCaptainRatingForTroopUsages(val, FormationClassExtensions.GetTroopUsageFlags((FormationClass)0), ref first);
			List<PerkObject> second = default(List<PerkObject>);
			float captainRatingForTroopUsages2 = Campaign.Current.Models.BattleCaptainModel.GetCaptainRatingForTroopUsages(val, FormationClassExtensions.GetTroopUsageFlags((FormationClass)1), ref second);
			list.Add(new TooltipProperty(((object)_infantryInfluenceText).ToString(), ((int)(captainRatingForTroopUsages * 100f)).ToString(), 0, false, (TooltipPropertyFlags)0)
			{
				OnlyShowWhenNotExtended = true
			});
			list.Add(new TooltipProperty(((object)_rangedInfluenceText).ToString(), ((int)(captainRatingForTroopUsages2 * 100f)).ToString(), 0, false, (TooltipPropertyFlags)0)
			{
				OnlyShowWhenNotExtended = true
			});
			list.Add(new TooltipProperty(string.Empty, string.Empty, 0, false, (TooltipPropertyFlags)0)
			{
				OnlyShowWhenNotExtended = true
			});
			List<PerkObject> list2 = first.Union(second).ToList();
			list2.Sort((IComparer<PerkObject>?)_perkComparer);
			if (list2.Count != 0)
			{
				list.Add(new TooltipProperty(((object)_captainPerksText).ToString(), string.Empty, 0, true, (TooltipPropertyFlags)4096));
				foreach (PerkObject item2 in list2)
				{
					if ((int)item2.PrimaryRole == 13 || (int)item2.SecondaryRole == 13)
					{
						TextObject val2 = (((int)item2.PrimaryRole == 13) ? item2.PrimaryDescription : item2.SecondaryDescription);
						string genericImageText = HyperlinkTexts.GetGenericImageText(CampaignUIHelper.GetSkillMeshId(item2.Skill, true), 2);
						_perkDefinitionText.SetTextVariable("PERK_NAME", ((PropertyObject)item2).Name).SetTextVariable("SKILL", genericImageText).SetTextVariable("SKILL_LEVEL", item2.RequiredSkillValue, 2);
						list.Add(new TooltipProperty(((object)_perkDefinitionText).ToString(), ((object)val2).ToString(), 0, true, (TooltipPropertyFlags)0));
					}
				}
			}
			else
			{
				list.Add(new TooltipProperty(((object)_noPerksText).ToString(), string.Empty, 0, true, (TooltipPropertyFlags)0));
			}
			if (Input.IsGamepadActive)
			{
				GameTexts.SetVariable("EXTEND_KEY", ((object)GameKeyTextExtensions.GetHotKeyGameText(Game.Current.GameTextManager, "MapHotKeyCategory", "MapFollowModifier")).ToString());
			}
			else
			{
				GameTexts.SetVariable("EXTEND_KEY", ((object)Game.Current.GameTextManager.FindText("str_game_key_text", "anyalt")).ToString());
			}
			list.Add(new TooltipProperty(string.Empty, ((object)GameTexts.FindText("str_map_tooltip_info", (string)null)).ToString(), -1, false, (TooltipPropertyFlags)0)
			{
				OnlyShowWhenNotExtended = true
			});
		}
		return list;
	}

	public bool GetCanBeUnassignedOrMoved()
	{
		if (!IsDisabled)
		{
			return !IsMainHero;
		}
		return false;
	}
}
