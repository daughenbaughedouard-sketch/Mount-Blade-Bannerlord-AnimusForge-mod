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


## Intervention memory context

`SiegeInterventionMemoryContextBuilder` owns dependency-free formatting for the per-scene GCCZ memory context appended to AF prompts plus the max retained memory-event count. AF adapters still own event collection, de-duplication, trim application, and logging.


## Intervention memory event formatter

`SiegeInterventionMemoryEventFormatter` owns dependency-free formatting for one GCCZ memory event: kind fallback, detail fallback, action-tag stripping, and whitespace normalization. AF adapters still own sequencing, duplicate checks, trim application, and logging.


## Loot accounting profile

`SiegeLootAccountingProfile` owns dependency-free loot UI wording, market-loot ratios, and civilian/hero gold amount constants for GCCZ 搜掠/血洗 accounting. AF adapters still own Bannerlord gold/item mutation, target eligibility, random sampling, and display side effects.


## Plunder interaction profile

`SiegePlunderInteractionProfile` owns dependency-free runtime parameters for GCCZ 搜掠 soldier assignment, approach distance, concurrent interactions, and talk duration. AF adapters still own live mission-agent selection, movement, timing application, and side effects.


## Mercy choice profile

`SiegeMercyChoiceProfile` owns dependency-free stop-plunder reason, soldier appeasement reason, shared-pool effect reason, message, memory text, loyalty bonus, and destructive-lock display action name for the simple 宽恕 choice. AF adapters still apply Bannerlord aftermath, shared-pool, UI, memory, and settlement side effects.


## Massacre interaction profile

`SiegeMassacreInteractionProfile` owns dependency-free runtime parameters for GCCZ 血洗 civilian hide distance, hide refresh timing, soldier follow refresh, and soldier target refresh. AF adapters still own live mission-agent routing, order timing application, hide-point projection, and combat side effects.


## Civilian gather interaction profile

`SiegeCivilianGatherInteractionProfile` owns dependency-free runtime parameters for GCCZ 民众召集 messenger speech, follow refresh, fallback timing, approach distance, soldier messenger ratio, messenger speed, and formation-control batching. AF adapters still own live mission-agent selection, `ShoutBehavior` triggering, movement, formation control, and side effects.


## Civilian assembly profile

`SiegeCivilianAssemblyProfile` owns dependency-free runtime parameters for GCCZ civilian assembly target counts, scene caps, extra-spawn gating, forward offset, grid spacing, and columns. AF adapters still own scene capacity checks, spawn gating, formation slot projection, and mission side effects.


## Soldier cordon profile

`SiegeSoldierCordonProfile` owns dependency-free runtime parameters for GCCZ soldier cordon radius, padding, teleport threshold, movement tolerance, settle tolerance, and order/look refresh timing. AF adapters still own live soldier selection, target-slot projection, movement orders, and look-at side effects.
