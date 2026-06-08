# GCCZ fused runtime integration

This fused `new-` worktree now has a real GCCZ source isolation area:

- `G:\AFMOD\new-\AnimusForge.SiegeAftermathIntervention\`

The first runtime bridge is intentionally narrow:

- copied the tested standalone GCCZ tag/outcome rule core from `G:\AFMOD\GCCZ\src\AnimusForge.SiegeAftermathIntervention`;
- `SiegeAiInterventionBehavior.cs` now uses that core for action-tag classification, canonical tag normalization, mercy-track detection, and irreversible destructive-outcome locking;
- existing AF/Bannerlord side effects remain in `SiegeAiInterventionBehavior.cs` for now, so this is a safe first bridge rather than a wholesale rewrite.

Next extraction targets:

1. Move fallback postprocess rule construction into the isolated GCCZ area.
2. Move shared relief-pool accounting into dependency-light GCCZ state classes.
3. Keep `SiegeAiInterventionBehavior.cs` as an AF adapter around mission/menu/settlement side effects.

## Local build verification

The fused project can be built without the absent `F:\SteamLibrary` game directory by pointing all required reference directories at the existing local dependency/output folder:

```powershell
$dep='G:\AFMOD\new-\bin\Debug\net472'
G:\AFMOD\.dotnet-sdk\dotnet.exe build G:\AFMOD\new-\AnimusForge.csproj -v:minimal /p:BannerlordApi=1.3 /p:VersionedDepsDir="$dep" /p:BannerlordBinDir="$dep" /p:NativeBinDir="$dep" /p:SandBoxBinDir="$dep" /p:AnimusForgeBinDir="$dep" /p:HarmonyPath="$dep\Bannerlord.Harmony.dll" /p:UIExtenderExPath="$dep\Bannerlord.UIExtenderEx.dll" /p:MBOptionScreenPath="$dep\Bannerlord.MBOptionScreen.v1.3.6.dll" /p:Mcmv5Path="$dep\MCMv5.dll"
```

Verified after the first runtime bridge: 0 warnings, 0 errors.

`G:\AFMOD\new-\原版游戏本体代码1.3.x` is treated as read-only Bannerlord 1.3.x source reference; it is not a dependency DLL directory and must not be edited for GCCZ integration.


Follow-up isolation: canonical tag order and alias table now live in `SiegeActionTagCatalog`, removing duplicate switch helpers from the AF adapter.


Follow-up isolation: postprocess-rule filtering now lives in `SiegePostprocessRuleFilter`; `SiegeAiInterventionBehavior` passes only runtime booleans and no longer duplicates destructive/mercy/安兵 tag classification.


Follow-up isolation: fallback postprocess rules now live in `SiegePostprocessRuleCatalog`; fused AF maps them to `PostprocessRuleEntry` and no longer stores rule wording in `SiegeAiInterventionBehavior`.


Follow-up isolation: postprocess context text now lives in `SiegePostprocessContextBuilder`; fused AF gathers live facts into `SiegePostprocessContextFacts` and delegates formatting to GCCZ core.


Follow-up isolation: postprocess tag normalization now lives in `SiegePostprocessTagNormalizer`; `SiegeAiInterventionBehavior` collects allowed runtime rule tags and delegates alias matching, canonical order, duplicate removal, and mood preservation to the isolated GCCZ source area.


Follow-up isolation: shared civilian relief-pool context now uses `SiegeSharedReliefPoolFacts` and `SiegeSharedReliefPoolFormatter`; the fused AF adapter keeps ItemObject/inventory/UI side effects and delegates material-pool checks plus context wording to the isolated GCCZ source area.


Follow-up isolation: outcome message de-duplication now uses `SiegeOutcomeMessageDeduplicator`; `SiegeAiInterventionBehavior` keeps Bannerlord UI display and delegates reset/show-once decisions to the isolated GCCZ source area.


Follow-up isolation: postprocess current-outcome wording now uses `SiegePostprocessOutcomeFacts` and `SiegePostprocessOutcomeTextBuilder`; the fused AF adapter passes live state flags and pending aftermath name, then delegates wording to the isolated GCCZ source area.


Follow-up isolation: pending native aftermath selection now uses `SiegeAftermathResolutionKind` and `SiegeAftermathSelectionPolicy`; the fused AF adapter maps TaleWorlds aftermath enum values and delegates severity/replacement checks to the isolated GCCZ source area.


Follow-up isolation: action-tag routing now uses `SiegeActionRoutingFacts`, `SiegeActionRoutingDecision`, and `SiegeActionRoutingPolicy`; the fused AF adapter passes raw action text and live target/material/lock facts, then delegates destructive/mercy-track detection and soldier relief routing to the isolated GCCZ source area.


Follow-up isolation: relief/appeasement profile selection now uses `SiegeReliefChoiceProfile`; the fused AF adapter delegates relief deltas, message/memory wording, soldier appeasement reason, and shared-pool effect reason to the isolated GCCZ source area before applying Bannerlord side effects.


Follow-up isolation: civic profile selection now uses `SiegeCivicChoiceProfile`; the fused AF adapter delegates 安民宣抚/归心盟誓 deltas, notable effects, message/memory wording, gather source, soldier appeasement reason, and shared-pool effect reason to the isolated GCCZ source area before applying Bannerlord side effects.


Follow-up isolation: mercy profile selection now uses `SiegeMercyChoiceProfile`; the fused AF adapter delegates stop-plunder reason, soldier appeasement reason, shared-pool effect reason, message text, and memory text to the isolated GCCZ source area before applying Bannerlord side effects.


Follow-up isolation: destructive profile selection now uses `SiegeDestructiveChoiceProfile`; the fused AF adapter maps the standalone aftermath kind back to TaleWorlds' native aftermath enum, then preserves the existing mission, settlement trust, UI, memory, and massacre-combat side effects.


Follow-up isolation: soldier appeasement now uses `SiegeSoldierAppeasementProfile`; the fused AF adapter delegates 安兵 success wording and fallback morale-penalty text/amount to the isolated GCCZ source area before applying Bannerlord party morale, UI, and memory side effects.
