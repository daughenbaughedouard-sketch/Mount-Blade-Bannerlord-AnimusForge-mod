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

`SiegePostprocessContextBuilder` owns dependency-free formatting for postprocess runtime facts. The AF adapter now only gathers live objects and passes `SiegePostprocessContextFacts`.


## Intervention memory context

`SiegeInterventionMemoryContextBuilder` owns dependency-free formatting for the per-scene GCCZ memory context appended to AF prompts plus the max retained memory-event count. AF adapters still own event collection, de-duplication, trim application, and logging.


## Intervention memory event formatter

`SiegeInterventionMemoryEventFormatter` owns dependency-free formatting for one GCCZ memory event: kind fallback, detail fallback, action-tag stripping, and whitespace normalization. AF adapters still own sequencing, duplicate checks, trim application, and logging.


## Loot accounting profile

`SiegeLootAccountingProfile` owns dependency-free loot UI wording, market-loot ratios, and civilian/hero gold amount constants for GCCZ 搜掠/血洗 accounting. AF adapters still own Bannerlord gold/item mutation, target eligibility, random sampling, and display side effects.
