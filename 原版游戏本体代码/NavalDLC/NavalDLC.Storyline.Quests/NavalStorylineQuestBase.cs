using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace NavalDLC.Storyline.Quests;

public abstract class NavalStorylineQuestBase : QuestBase
{
	public sealed override bool IsRemainingTimeHidden => true;

	public override string SpecialQuestType => "NavalStoryline";

	public abstract NavalStorylineData.NavalStorylineStage Stage { get; }

	public abstract bool WillProgressStoryline { get; }

	protected abstract string MainPartyTemplateStringId { get; }

	public PartyTemplateObject Template
	{
		get
		{
			if (string.IsNullOrEmpty(MainPartyTemplateStringId))
			{
				return null;
			}
			return ((GameType)Campaign.Current).ObjectManager.GetObject<PartyTemplateObject>(MainPartyTemplateStringId);
		}
	}

	protected NavalStorylineQuestBase(string questId, Hero questGiver, CampaignTime duration, int rewardGold)
		: base(questId, questGiver, duration, rewardGold)
	{
	}//IL_0003: Unknown result type (might be due to invalid IL or missing references)


	protected sealed override void RegisterEvents()
	{
		NavalDLCEvents.OnNavalStorylineActivityChangedEvent.AddNonSerializedListener((object)this, (Action<bool>)OnNavalStorylineActivityChanged);
		NavalDLCEvents.IsNavalQuestPartyEvent.AddNonSerializedListener((object)this, (Action<PartyBase, NavalStorylinePartyData>)IsNavalQuestParty);
		RegisterEventsInternal();
	}

	private void IsNavalQuestParty(PartyBase partyBase, NavalStorylinePartyData data)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		if (partyBase == PartyBase.MainParty)
		{
			data.IsQuestParty = true;
			data.Template = Template;
			if (data.Template != null)
			{
				ExplainedNumber maxPartySizeLimitFromTemplate = NavalDLCHelpers.GetMaxPartySizeLimitFromTemplate(data.Template);
				data.PartySize = (int)((ExplainedNumber)(ref maxPartySizeLimitFromTemplate)).ResultNumber + 2;
			}
		}
		IsNavalQuestPartyInternal(partyBase, data);
	}

	protected virtual void IsNavalQuestPartyInternal(PartyBase partyBase, NavalStorylinePartyData data)
	{
	}

	private void OnNavalStorylineActivityChanged(bool activity)
	{
		if (((QuestBase)this).IsOngoing && !activity)
		{
			ResetQuest();
		}
		OnNavalStorylineActivityChangedInternal(activity);
	}

	protected virtual void OnNavalStorylineActivityChangedInternal(bool activity)
	{
	}

	protected abstract void RegisterEventsInternal();

	public void ResetQuest()
	{
		((QuestBase)this).CompleteQuestWithCancel((TextObject)null);
	}

	protected sealed override void OnStartQuest()
	{
		if (WillProgressStoryline)
		{
			NavalStorylineData.OnStorylineProgress(this);
		}
		OnStartQuestInternal();
	}

	protected sealed override void OnFinalize()
	{
		OnFinalizeInternal();
	}

	protected sealed override void InitializeQuestOnGameLoad()
	{
		InitializeQuestOnGameLoadInternal();
	}

	protected virtual void InitializeQuestOnGameLoadInternal()
	{
	}

	protected virtual void OnStartQuestInternal()
	{
	}

	protected virtual void OnFinalizeInternal()
	{
	}

	public sealed override void OnCanceled()
	{
		OnCanceledInternal();
	}

	protected virtual void OnCanceledInternal()
	{
	}

	public sealed override void OnFailed()
	{
		OnFailedInternal();
	}

	protected virtual void OnFailedInternal()
	{
	}

	protected sealed override void OnCompleteWithSuccess()
	{
		OnCompleteWithSuccessInternal();
	}

	protected virtual void OnCompleteWithSuccessInternal()
	{
	}
}
