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
