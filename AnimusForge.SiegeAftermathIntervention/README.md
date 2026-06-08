# AnimusForge.SiegeAftermathIntervention

Standalone GCCZ source area.

Current first extraction slice:

- `SiegeInterventionOutcome` mirrors the existing fused outcome state names.
- `SiegeInterventionActionKind` mirrors the existing postprocess action vocabulary.
- `SiegeActionTagCatalog` preserves current English/Chinese action tag parsing and normalizes to the Chinese canonical tags already used by `SiegeAiInterventionBehavior`.

This slice has no Bannerlord, Harmony, or AF dependencies. It is safe to build independently and later bridge from the fused `AnimusForge` namespace.

Second extraction slice:

- `SiegeInterventionActionRules` preserves the current outcome-routing invariants: mercy/relief/inspire/oath can override reversible plunder, but cannot downgrade massacre or cultural repopulation; destructive actions can be policy-blocked; cultural repopulation requires allied-soldier context.
- `SiegeInterventionActionRuleDecision` returns a dependency-free decision for future AF adapters to translate into UI messages, memory records, and Bannerlord effects.


## Tag order and aliases

`SiegeActionTagCatalog` now owns the canonical tag order and alias table, so AF bridge code does not need duplicate switch statements for action tag normalization.


## Postprocess-rule filtering

`SiegePostprocessRuleFilter` owns dependency-free filtering for active scene postprocess tags: destructive policy gate, irreversible outcome downgrade gate, and pending/completed soldier appeasement visibility. AF adapters pass runtime booleans and keep side effects outside the core.


## Fallback postprocess rule catalog

`SiegePostprocessRuleCatalog` owns the dependency-free fallback postprocess rule definitions. Fused AF maps these definitions to `PostprocessRuleEntry` instead of keeping rule wording inside `SiegeAiInterventionBehavior`.


## Postprocess context builder

`SiegePostprocessContextBuilder` owns dependency-free formatting for postprocess runtime facts plus speaker identity labels for allied soldiers, civilians, and other scene NPCs. The AF adapter now only gathers live objects and passes `SiegePostprocessContextFacts`.


## Shared civilian relief pool

`SiegeSharedReliefPoolFacts` and `SiegeSharedReliefPoolFormatter` own dependency-free checks, context wording, refund UI/memory wording, returned-gold source construction, applied-effect wording, transfer summaries, and amount formatting for the AF give-item/give-gold pool reserved for civilian relief. Bannerlord item objects, inventory/gold mutation, and display side effects stay in the AF adapter.


## Intervention memory context

`SiegeInterventionMemoryContextBuilder` owns dependency-free formatting for the per-scene GCCZ memory context appended to AF prompts plus the max retained memory-event count. AF adapters still own event collection, de-duplication, trim application, and logging.


## Intervention memory event formatter

`SiegeInterventionMemoryEventFormatter` owns dependency-free formatting for one GCCZ memory event: kind fallback, detail fallback, action-tag stripping, and whitespace normalization. AF adapters still own sequencing, duplicate checks, trim application, and logging.


## Loot accounting profile

`SiegeLootAccountingProfile` owns dependency-free loot UI wording, market-loot ratios, and civilian/hero gold amount constants for GCCZ 搜掠/血洗 accounting. AF adapters still own Bannerlord gold/item mutation, target eligibility, random sampling, and display side effects.


## Plunder interaction profile

`SiegePlunderInteractionProfile` owns dependency-free runtime parameters and source codes for GCCZ 搜掠 soldier assignment, approach distance, concurrent interactions, talk duration, allied assignment restore, and target follow operations. AF adapters still own live mission-agent selection, movement, timing application, and side effects.


## Intervention entry profile

`SiegeInterventionEntryProfile` owns dependency-free scene-entry tooltip, missing-scene UI wording, and troop-selection mission-entry, scene-cleanup, and auto-enter summon source codes for the GCCZ intervention menu. AF adapters still resolve Bannerlord settlements, locations, menu args, and display side effects.


## Mercy choice profile

`SiegeMercyChoiceProfile` owns dependency-free stop-plunder reason, soldier appeasement reason, shared-pool effect reason, message, memory text, loyalty bonus, and destructive-lock display action name for the simple 宽恕 choice. AF adapters still apply Bannerlord aftermath, shared-pool, UI, memory, and settlement side effects.


## Settlement effect profile

`SiegeSettlementEffectProfile` owns dependency-free reason codes for GCCZ settlement-effect mutations, including the positive public-trust reason used by relief/civic/mercy-track adjustments. AF adapters still own Bannerlord settlement, town, and reward-system side effects.


## Destructive choice profile

`SiegeDestructiveChoiceProfile` owns dependency-free aftermath kind, assembly source, message text, memory text, massacre source classification, player-attack trigger wording, player-attack damage source, player-hit bridge sources, policy validation text, and public-trust deltas for 搜掠 and 血洗. AF adapters still apply Bannerlord aftermath, troop, mission, UI, settlement, damage, and memory side effects.


## Massacre interaction profile

`SiegeMassacreInteractionProfile` owns dependency-free runtime parameters and source codes for GCCZ 血洗 civilian hide distance, hide refresh timing, soldier follow refresh, soldier target refresh, occupation follow, combat preparation, allied combat drive, and all-targets-down victory completion. AF adapters still own live mission-agent routing, order timing application, hide-point projection, and combat side effects.


## Cultural repopulation profile

`SiegeCulturalRepopulationProfile` owns dependency-free aftermath kind, massacre trigger wording, request memory text, pending UI message, completion UI message, target-culture labels, validation text, and apply source codes for 屠民迁殖. AF adapters still resolve Bannerlord cultures, mutate settlements/villages/notables, and run mission/combat side effects.


## Civilian gather interaction profile

`SiegeCivilianGatherInteractionProfile` owns dependency-free runtime parameters and source codes for GCCZ 民众召集 messenger speech, follow refresh, fallback timing, approach distance, soldier messenger ratio/source codes, messenger speed, formation-control batching, gather-mark/seed/fallback/messenger-return/formation-queue source construction, target waiting, messenger movement, follower preparation, interaction release, fake-talk follower completion, fallback follower marking, and formation-control reasons/order readiness. AF adapters still own live mission-agent selection, `ShoutBehavior` triggering, movement, formation control, and side effects.


## Civilian assembly profile

`SiegeCivilianAssemblyProfile` owns dependency-free runtime parameters and source codes for GCCZ civilian assembly target counts, scene caps, extra-spawn gating, forward offset, grid spacing, columns, mission-start assembly, and control-tick assembly. AF adapters still own scene capacity checks, spawn gating, formation slot projection, and mission side effects.


## Soldier cordon profile

`SiegeSoldierCordonProfile` owns dependency-free runtime parameters and source codes for GCCZ soldier cordon radius, padding, teleport threshold, movement tolerance, settle tolerance, order/look refresh timing, allied control tick, default infantry follow, spawn follow, and spawn-batch order-controller priming. AF adapters still own live soldier selection, target-slot projection, movement orders, and look-at side effects.

## Direct aftermath source profile

`SiegeDirectAftermathSourceProfile` owns dependency-free source codes for direct AF aftermath campaign tick scripts, native-menu intercepts, external pump fallbacks, direct-script phase transitions, and direct loot-screen defer reasons. AF adapters still own campaign tick timing, loot-screen state, and encounter transitions.

## Aftermath transition source profile

`SiegeAftermathTransitionSourceProfile` owns dependency-free source codes for mission-end aftermath finalization, post-mission encounter finish retries, done-menu continue finish, native menu initialization, campaign-tick native menu detection, and native devastate summary continuation. AF adapters still own mission lifecycle, menu switching, loot-screen timing, and encounter side effects.

## Native bridge source profile

`SiegeNativeBridgeSourceProfile` owns dependency-free source codes for native flee suppression, order UI readiness, order-team resolution, control-tick order-controller priming, order-controller binding, and injected native order views. AF adapters still own Harmony patches, mission views, and live agent/order side effects.

## Aftermath menu profile

`SiegeAftermathMenuProfile` owns dependency-free menu identifiers for GCCZ aftermath entry, native settlement-taken routing, and contextual summary routing. AF adapters still own Bannerlord menu registration, switching, and live menu side effects.
