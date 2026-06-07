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
