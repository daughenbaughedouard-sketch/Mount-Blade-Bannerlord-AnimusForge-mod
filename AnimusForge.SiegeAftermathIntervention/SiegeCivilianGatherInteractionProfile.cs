namespace AnimusForge.SiegeAftermathIntervention;

/// <summary>
/// Dependency-free runtime parameters and source codes for GCCZ civilian-gather interactions.
/// AF adapters still own live mission-agent selection, messenger speech triggering, formation control, and side effects.
/// </summary>
public static class SiegeCivilianGatherInteractionProfile
{
    public const float SpeechRallySettleTolerance = 0.8f;

    public const float TalkMinSeconds = 1.0f;

    public const float TalkMaxSeconds = 3.0f;

    public const float FallbackSeconds = 75.0f;

    public const float ApproachDistance = 3.2f;

    public const float FollowRefreshSeconds = 1.25f;

    public const float FormationSettleDistance = 5.5f;

    public const float SoldierMessengerRatio = 0.20f;

    public const float MessengerMoveSpeedLimit = 1.9f;

    public const float FormationControlInitialDelaySeconds = 0.8f;

    public const float FormationControlBatchIntervalSeconds = 0.12f;

    public const int FormationControlBatchSize = 8;

    public const int MessengerSpeechMinCount = 2;

    public const int MessengerSpeechMaxCount = 3;

    public const string TargetWaitSource = "gather_target_wait";

    public const string MessengerMoveSource = "gather_messenger_move";

    public const string FollowPrepareSource = "gather_follow_prepare_once";

    public const string InvalidOrAlreadyFollowerReleaseSource = "gather_interaction_invalid_or_target_already_c";

    public const string FakeTalkFollowerSource = "gather_fake_talk";

    public const string InteractionTimeoutReleaseSource = "gather_interaction_timeout";

    public const string FallbackFollowerSource = "gather_120s_fallback";

    public const string FallbackElapsedFormationSource = "gather_120s_elapsed";

    public const string AllGatheredAndSettledFormationSource = "all_civilians_gathered_and_settled";

    public const string TargetBecameFollowerReleaseSource = "gather_target_became_c";
}
