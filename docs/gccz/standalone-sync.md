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
