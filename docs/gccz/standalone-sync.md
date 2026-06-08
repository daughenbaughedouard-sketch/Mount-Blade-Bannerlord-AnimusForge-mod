# GCCZ standalone sync note

This fused `new-` worktree is the compile-ready AF + GCCZ integration. The standalone GCCZ source/notes are maintained in `G:\AFMOD\GCCZ`.

Current synchronized GCCZ docs created from this fused tree:

- `G:\AFMOD\GCCZ\docs\migration\current-fused-inventory.md`
- `G:\AFMOD\GCCZ\docs\bridge\af-bridge-surface.md`
- `G:\AFMOD\GCCZ\ModuleData\siege_intervention_aftermath.notes.md`

Do not silently edit only one side. When GCCZ source/rules/bridge details change in `G:\AFMOD\GCCZ`, mirror the compile-ready integration or at least this handoff note in `G:\AFMOD\new-`.

## 2026-06-08 source extraction seed

Standalone GCCZ now has a dependency-free source seed under `G:\AFMOD\GCCZ\src\AnimusForge.SiegeAftermathIntervention`:

- action kind vocabulary
- intervention outcome enum
- action-tag normalization catalog

Verification run: `G:\AFMOD\.dotnet-sdk\dotnet.exe build G:\AFMOD\GCCZ\src\AnimusForge.SiegeAftermathIntervention\AnimusForge.SiegeAftermathIntervention.csproj` completed with 0 warnings and 0 errors before generated `bin/obj` cleanup.

No compile-affecting `.cs` file was added to this fused `new-` tree in this step; this is intentionally a standalone isolation seed before AF bridge routing is changed.

## 2026-06-08 outcome-rule seed

Standalone GCCZ now also contains a dependency-free outcome-rule core:

- `SiegeInterventionActionRuleDecision`
- `SiegeInterventionActionRules`

This mirrors the fused runtime invariant that mercy-track actions can override reversible 搜掠 but cannot downgrade 血洗/殖民, while destructive actions remain policy-gated and 殖民 requires allied-soldier context. No fused runtime `.cs` file was changed in this step.

## 2026-06-08 standalone tests

Standalone GCCZ now includes `G:\AFMOD\GCCZ\tests\AnimusForge.SiegeAftermathIntervention.Tests`, a no-third-party console test project for the extracted tag catalog and outcome-rule core. This keeps future refactors anchored to the existing fused behavior before any AF bridge routing is changed.

## 2026-06-08 passive rule snippet

Standalone GCCZ now carries `G:\AFMOD\GCCZ\ModuleData\siege_intervention_aftermath.rule.json`, extracted from this fused tree's `AnimusForge\ModuleData\RuleBehaviorPrompts.json`. Tests assert it remains passive (`TriggerKeywords: []`) and contains the nine current canonical action tags. No fused runtime rule was modified in this step.

## 2026-06-08 fused runtime bridge seed

`new-` now contains `AnimusForge.SiegeAftermathIntervention\` as the fused AF-side independent GCCZ source area. `SiegeAiInterventionBehavior.cs` has a first thin bridge to the extracted core for action-tag classification, canonical tag normalization, mercy-track detection, and irreversible destructive-outcome locking. Bannerlord side effects remain in the AF adapter for now.

Build note: the first fused runtime bridge was verified with local dependency/output folder `G:\AFMOD\new-\bin\Debug\net472` rather than the absent `F:\SteamLibrary` game directory. `原版游戏本体代码1.3.x` remains read-only source reference only.


Follow-up isolation: canonical tag order and alias table now live in `SiegeActionTagCatalog`, removing duplicate switch helpers from the AF adapter.


Follow-up isolation: postprocess-rule filtering now lives in `SiegePostprocessRuleFilter`; `SiegeAiInterventionBehavior` passes only runtime booleans and no longer duplicates destructive/mercy/安兵 tag classification.


Follow-up isolation: fallback postprocess rules now live in `SiegePostprocessRuleCatalog`; fused AF maps them to `PostprocessRuleEntry` and no longer stores rule wording in `SiegeAiInterventionBehavior`.


Follow-up isolation: postprocess context text now lives in `SiegePostprocessContextBuilder`; fused AF gathers live facts into `SiegePostprocessContextFacts` and delegates formatting to GCCZ core.


Follow-up isolation: postprocess tag normalization now lives in `SiegePostprocessTagNormalizer`; this fused tree mirrors the standalone source file and routes `NormalizeSiegeInterventionPostprocessTagsForExternal(...)` through that core.


Follow-up isolation: shared civilian relief-pool context now lives in mirrored `SiegeSharedReliefPoolFacts` and `SiegeSharedReliefPoolFormatter` source files. The fused tree routes `DescribeSharedCivilianReliefPoolForContext()` and `HasSharedCivilianReliefPool()` through that core while preserving AF-side inventory effects.


Follow-up isolation: negative-outcome shared-pool refund UI and memory wording now also live in mirrored `SiegeSharedReliefPoolFormatter`. The fused tree routes refund text through that core while preserving AF-side item/gold return side effects.


Follow-up isolation: shared-pool applied-effect UI now also lives in mirrored `SiegeSharedReliefPoolFormatter`. The fused tree routes the displayed effect message through that core while preserving AF-side pool description and display.


Follow-up isolation: shared-pool capture UI now also lives in mirrored `SiegeSharedReliefPoolFormatter`. The fused tree routes captured-transfer wording through the core while preserving AF-side Bannerlord gold/item summary collection and display side effects.


Follow-up isolation: newly applied shared-pool settlement-effect deltas now live in mirrored `SiegeSharedReliefPoolEffectCalculator` and `SiegeSharedReliefPoolEffectDeltas`. The fused tree routes relief material delta formulas through that core while preserving AF-side town food-stock mutation and settlement application.


Follow-up isolation: outcome message de-duplication now lives in mirrored `SiegeOutcomeMessageDeduplicator`. The fused tree routes reset/show-once decisions through that core while preserving AF-side `InformationMessage` display.


Follow-up isolation: postprocess current-outcome wording now lives in mirrored `SiegePostprocessOutcomeFacts` and `SiegePostprocessOutcomeTextBuilder`. The fused tree routes postprocess context outcome text through that core while preserving AF-side live state collection.


Follow-up isolation: civilian gather runtime context now lives in mirrored `SiegeCivilianGatherContextFacts` and `SiegeCivilianGatherContextBuilder`. The fused tree routes 民众召集状态 wording through that core while preserving AF-side live agent counting and gather/formation flags.


Follow-up isolation: civilian gather UI/memory now lives in mirrored `SiegeCivilianGatherUiProfile`. The fused tree routes prepared-count, messenger, queue, ready wording, immediate messenger speech prompt, and fallback names through that core while preserving AF-side mission-agent tracking, messenger/formation state, `ShoutBehavior` triggering, and side effects.


Follow-up isolation: intervention memory context formatting now lives in mirrored `SiegeInterventionMemoryContextBuilder`. The fused tree routes prompt memory wording through that core while preserving AF-side event collection, de-duplication, trimming, and logging.


Follow-up isolation: single memory-event formatting now lives in mirrored `SiegeInterventionMemoryEventFormatter`. The fused tree routes kind/detail fallback, action-tag stripping, and whitespace normalization through that core while preserving AF-side sequencing, duplicate checks, trimming, and logging.


Follow-up isolation: completed intervention summary now lives in mirrored `SiegeCompletedInterventionSummaryFacts` and `SiegeCompletedInterventionSummaryBuilder`. The fused tree routes completion-summary wording through that core while preserving AF-side fact collection and menu transitions.


Follow-up isolation: civilian loot-accounting UI now lives in mirrored `SiegeLootAccountingProfile`. The fused tree routes exit-settlement and per-target civilian gold wording through the core while preserving AF-side Bannerlord gold transfer, target eligibility, random amount calculation, and display side effects.


Follow-up isolation: market/civilian-spoils loot UI now also lives in mirrored `SiegeLootAccountingProfile`. The fused tree routes market gold, market inventory, and civilian-spoils wording through the core while preserving AF-side town gold/inventory mutation, pending loot roster construction, random stack selection, and display side effects.


Follow-up isolation: direct aftermath loot status UI now also lives in mirrored `SiegeLootAccountingProfile`. The fused tree routes direct devastate/plunder settlement notices and credited loot summary wording through the core while preserving AF-side direct loot-screen timing/state flags and display side effects.


Follow-up isolation: market-loot settlement reasons and capture ratios now also live in mirrored `SiegeLootAccountingProfile`. The fused tree routes plunder/massacre market-loot labels and percentage constants through that core while preserving AF-side town gold/inventory mutation and one-time guards.


Follow-up isolation: scene-entry tooltip and missing-scene UI now live in mirrored `SiegeInterventionEntryProfile`. The fused tree routes entry wording through that core while preserving AF-side settlement/location/menu checks and display side effects.


Follow-up isolation: scene-entry troop-selection instructions and selection-result UI now also live in mirrored `SiegeInterventionEntryProfile`. The fused tree routes entry instructions, decision-policy text, failure text, and selected/fallback troop-selection messages through the core while preserving AF-side menu callbacks and selected-roster storage.


Follow-up isolation: mission-entry battle-equipment and allied-summon UI now also live in mirrored `SiegeInterventionEntryProfile`. The fused tree routes battle-equipment, no-healthy-troop, and summoned-troop messages through the core while preserving AF-side equipment mutation, troop picking, agent spawning, and formation side effects.


Follow-up isolation: pending native aftermath selection now lives in mirrored `SiegeAftermathResolutionKind` and `SiegeAftermathSelectionPolicy`. The fused tree routes native aftermath severity and replacement decisions through that core while preserving AF-side enum mapping and side effects.


Follow-up isolation: action-tag routing now lives in mirrored `SiegeActionRoutingFacts`, `SiegeActionRoutingDecision`, and `SiegeActionRoutingPolicy`. The fused tree routes destructive/mercy-track detection plus soldier relief downgrade/capping decisions through that core while preserving AF-side effects.


Follow-up isolation: postprocess action effect trigger wording now lives in mirrored `SiegePostprocessActionEffectProfile`. The fused tree routes normalized mercy replacement, gather source, and trigger source/detail strings through that core while preserving AF-side regex matching and live target checks.


Follow-up isolation: mercy-track transition UI now lives in mirrored `SiegeMercyTrackTransitionProfile`. The fused tree routes blocked post-destruction action and reversible-plunder-stop wording through the core while preserving AF-side destructive-lock checks, plunder-state clearing, logging, and display side effects.


Follow-up isolation: relief/appeasement profile selection now lives in mirrored `SiegeReliefChoiceProfile`. The fused tree routes relief deltas, message/memory wording, soldier appeasement reason, and shared-pool effect reason through that core while preserving AF-side side effects.


Follow-up isolation: relief validation messages now also live in mirrored `SiegeReliefChoiceProfile`. The fused tree routes invalid-target and missing-shared-material UI text through that core while preserving AF-side validation and `InformationMessage` display.


Follow-up isolation: civic profile selection now lives in mirrored `SiegeCivicChoiceProfile`. The fused tree routes 安民宣抚/归心盟誓 deltas, notable effects, message/memory wording, gather source, soldier appeasement reason, and shared-pool effect reason through that core while preserving AF-side side effects.


Follow-up isolation: mercy profile selection now lives in mirrored `SiegeMercyChoiceProfile`. The fused tree routes stop-plunder reason, soldier appeasement reason, shared-pool effect reason, message text, and memory text through that core while preserving AF-side side effects.


Follow-up isolation: destructive profile selection now lives in mirrored `SiegeDestructiveChoiceProfile`. The fused tree routes 搜掠/血洗 aftermath kind, assembly source, UI message text, memory wording, public-trust delta, and trigger-source classification through that core while preserving AF-side mission/combat/settlement side effects.


Follow-up isolation: plunder finalized trust penalty now also lives in mirrored `SiegeDestructiveChoiceProfile`. The fused tree routes the finalized trust delta/reason through that core while preserving AF-side settlement mutation.


Follow-up isolation: destructive same-culture/policy validation messages now also live in mirrored `SiegeDestructiveChoiceProfile`. The fused tree routes blocked 搜掠/血洗 UI text through that core while preserving AF-side policy validation and display.


Follow-up isolation: same-culture destructive-policy scene-entry and postprocess-batch messages now also live in mirrored `SiegeDestructiveChoiceProfile`. The fused tree routes that wording through the core while preserving AF-side `TextObject`/`InformationMessage` display and live policy checks.


Follow-up isolation: direct player-attack bloodbath trigger UI and pending-aftermath detail wording now also live in mirrored `SiegeDestructiveChoiceProfile`. The fused tree routes weapon-attack/hit messages, trigger sources, and trigger details through the core while preserving AF-side input/damage detection and combat side effects.


Follow-up isolation: cultural repopulation request handling now lives in mirrored `SiegeCulturalRepopulationProfile`. The fused tree routes 屠民迁殖 request wording and devastate aftermath kind through that core while preserving AF-side validation, culture resolution, and settlement/notable mutation.


Follow-up isolation: cultural repopulation completion UI now also lives in mirrored `SiegeCulturalRepopulationProfile`. The fused tree routes completion text/color and notable-result wording through that core while preserving AF-side settlement/village/notable mutation.


Follow-up isolation: cultural repopulation policy/target validation messages now also live in mirrored `SiegeCulturalRepopulationProfile`. The fused tree routes blocked-policy and invalid-target UI text through that core while preserving AF-side policy and allied-soldier validation.


Follow-up isolation: soldier appeasement now lives in mirrored `SiegeSoldierAppeasementProfile`. The fused tree routes 安兵 success wording plus fallback morale-penalty text/amount through that core while preserving AF-side target validation, party morale mutation, UI, and memory side effects.


Follow-up isolation: soldier appeasement need-warning now also lives in mirrored `SiegeSoldierAppeasementProfile`. The fused tree routes the initial 军心 warning UI and memory wording through that core while preserving AF-side random gating and state changes.


Follow-up isolation: soldier appeasement target validation now also lives in mirrored `SiegeSoldierAppeasementProfile`. The fused tree routes the invalid-target UI text through that core while preserving AF-side allied-soldier validation and display.


Follow-up isolation: final completion and encounter-exit UI now lives in mirrored `SiegeInterventionCompletionUiProfile`. The fused tree routes completed-menu fallbacks, massacre-victory message/quick text, completed-aftermath labels/text, loot-settlement summary, and leave-encounter quick text through the core while preserving AF-side enum mapping, state checks, and display side effects.
