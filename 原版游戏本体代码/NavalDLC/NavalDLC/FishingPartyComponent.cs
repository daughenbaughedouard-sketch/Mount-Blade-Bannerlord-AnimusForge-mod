using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Party.PartyComponents;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;

namespace NavalDLC;

public class FishingPartyComponent : VillagerPartyComponent
{
	[SaveableField(1)]
	private bool _isFishing;

	[SaveableField(2)]
	private bool _isRoaming;

	public bool IsFishing
	{
		get
		{
			return _isFishing;
		}
		set
		{
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			if (_isFishing != value)
			{
				_isFishing = value;
				if (_isFishing)
				{
					FishingWaitStartTime = CampaignTime.Now;
				}
				else
				{
					FishingWaitStartTime = CampaignTime.Never;
				}
			}
		}
	}

	public bool IsRoaming
	{
		get
		{
			return _isRoaming;
		}
		set
		{
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			if (_isRoaming != value)
			{
				_isRoaming = value;
				if (_isRoaming)
				{
					RoamingStartTime = CampaignTime.Now;
				}
				else
				{
					RoamingStartTime = CampaignTime.Never;
				}
			}
		}
	}

	[SaveableProperty(3)]
	public CampaignTime FishingWaitStartTime { get; private set; }

	[SaveableProperty(4)]
	public CampaignTime RoamingStartTime { get; private set; }

	public override TextObject Name
	{
		get
		{
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Expected O, but got Unknown
			if (base._cachedName == (TextObject)null)
			{
				base._cachedName = new TextObject("{=a9TivyGv}Fishers of {VILLAGE_NAME}", (Dictionary<string, object>)null);
				base._cachedName.SetTextVariable("VILLAGE_NAME", ((SettlementComponent)((VillagerPartyComponent)this).Village).Name);
			}
			return base._cachedName;
		}
	}

	public static MobileParty CreateFishingParty(string stringId, Village village)
	{
		return MobileParty.CreateParty(stringId, (PartyComponent)(object)new FishingPartyComponent(village));
	}

	protected FishingPartyComponent(Village village)
		: base(village, (InitializationArgs)null)
	{
	}

	protected override void OnMobilePartySetOnCreation()
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		((PartyComponent)this).MobileParty.Aggressiveness = 0f;
		((PartyComponent)this).MobileParty.InitializePartyTrade(0);
		PartyTemplateObject fishingPartyTemplate = ((SettlementComponent)((VillagerPartyComponent)this).Village).Settlement.Culture.FishingPartyTemplate;
		CampaignVec2 portPosition = ((SettlementComponent)((VillagerPartyComponent)this).Village).Settlement.PortPosition;
		((PartyComponent)this).MobileParty.InitializeMobilePartyAroundPosition(fishingPartyTemplate, portPosition, 1f, 0f);
		((PartyComponent)this).Party.SetVisualAsDirty();
		((PartyComponent)this).MobileParty.SetLandNavigationAccess(false);
	}

	protected override void OnInitialize()
	{
		if (!NavalDLCManager.Instance.FishingParties.TryGetValue(((VillagerPartyComponent)this).Village, out var value))
		{
			value = new List<FishingPartyComponent>();
			NavalDLCManager.Instance.FishingParties.Add(((VillagerPartyComponent)this).Village, value);
		}
		value.Add(this);
	}

	protected override void OnFinalize()
	{
		if (NavalDLCManager.Instance.FishingParties.TryGetValue(((VillagerPartyComponent)this).Village, out var value))
		{
			value.Remove(this);
		}
		else
		{
			Debug.FailedAssert("parties.Contains(fishingParty)", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\NavalDLC\\FishingPartyComponent.cs", "OnFinalize", 136);
		}
	}
}
