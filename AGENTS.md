# Fused AF + Siege Worktree Rules

Use the installed Codex skill `af-siege-fusion` for this workspace.

## Role

`G:\AFMOD\new-` is the fused AnimusForge + 攻城处置 worktree used to build v0.8.x test DLLs. It is not the upstream AF checkout and not the standalone siege source package.

## Required source relationship

- Upstream AF source: `G:\AFMOD\Mount-Blade-Bannerlord-AnimusForge-mod`
- Standalone 攻城处置 source: `G:\AFMOD\GCCZ`
- Fused integration: `G:\AFMOD\new-`

Every 攻城处置 update must be mirrored with `G:\AFMOD\GCCZ`. If this fused tree changes `SubModule.cs`, `ShoutBehavior.cs`, menu patches, or `RuleBehaviorPrompts.json`, record the reusable patch/handoff detail back in `G:\AFMOD\GCCZ`.

## Fusion guardrails

- Start from GitHub-latest AF source, then apply GCCZ siege code.
- Do not overwrite AF v0.8 `Patch_GameMenu_ActivateGameMenu.cs`; merge siege branches while preserving existing encounter/lord redirect logic.
- Merge only `siege_intervention_aftermath` into v0.8 `RuleBehaviorPrompts.json`; do not replace the full file.
- Register `SiegeAiInterventionBehavior` in the fused `SubModule.cs`.
- Build and verify before covering game DLLs; back up game files first and preserve ONNX.
- Keep local git checkpoints.
