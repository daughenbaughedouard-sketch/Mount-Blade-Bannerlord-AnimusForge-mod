using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.DotNet;
using TaleWorlds.Engine;
using TaleWorlds.Library;

namespace SandBox;

public class CampaignMapSiegePrefabEntityCache : ScriptComponentBehavior
{
	[EditableScriptComponentVariable(true, "")]
	private string _attackerBallistaPrefab = "ballista_a_mapicon";

	[EditableScriptComponentVariable(true, "")]
	private string _defenderBallistaPrefab = "ballista_b_mapicon";

	[EditableScriptComponentVariable(true, "")]
	private string _attackerFireBallistaPrefab = "ballista_a_fire_mapicon";

	[EditableScriptComponentVariable(true, "")]
	private string _defenderFireBallistaPrefab = "ballista_b_fire_mapicon";

	[EditableScriptComponentVariable(true, "")]
	private string _attackerMangonelPrefab = "mangonel_a_mapicon";

	[EditableScriptComponentVariable(true, "")]
	private string _defenderMangonelPrefab = "mangonel_b_mapicon";

	[EditableScriptComponentVariable(true, "")]
	private string _attackerFireMangonelPrefab = "mangonel_a_fire_mapicon";

	[EditableScriptComponentVariable(true, "")]
	private string _defenderFireMangonelPrefab = "mangonel_b_fire_mapicon";

	[EditableScriptComponentVariable(true, "")]
	private string _attackerTrebuchetPrefab = "trebuchet_a_mapicon";

	[EditableScriptComponentVariable(true, "")]
	private string _defenderTrebuchetPrefab = "trebuchet_b_mapicon";

	private MatrixFrame _attackerBallistaLaunchEntitialFrame;

	private MatrixFrame _defenderBallistaLaunchEntitialFrame;

	private MatrixFrame _attackerFireBallistaLaunchEntitialFrame;

	private MatrixFrame _defenderFireBallistaLaunchEntitialFrame;

	private MatrixFrame _attackerMangonelLaunchEntitialFrame;

	private MatrixFrame _defenderMangonelLaunchEntitialFrame;

	private MatrixFrame _attackerFireMangonelLaunchEntitialFrame;

	private MatrixFrame _defenderFireMangonelLaunchEntitialFrame;

	private MatrixFrame _attackerTrebuchetLaunchEntitialFrame;

	private MatrixFrame _defenderTrebuchetLaunchEntitialFrame;

	private Vec3 _attackerBallistaScale;

	private Vec3 _defenderBallistaScale;

	private Vec3 _attackerFireBallistaScale;

	private Vec3 _defenderFireBallistaScale;

	private Vec3 _attackerMangonelScale;

	private Vec3 _defenderMangonelScale;

	private Vec3 _attackerFireMangonelScale;

	private Vec3 _defenderFireMangonelScale;

	private Vec3 _attackerTrebuchetScale;

	private Vec3 _defenderTrebuchetScale;

	protected override void OnInit()
	{
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_016d: Unknown result type (might be due to invalid IL or missing references)
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		//IL_017f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0233: Unknown result type (might be due to invalid IL or missing references)
		//IL_0238: Unknown result type (might be due to invalid IL or missing references)
		//IL_0246: Unknown result type (might be due to invalid IL or missing references)
		//IL_024b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0254: Unknown result type (might be due to invalid IL or missing references)
		//IL_0259: Unknown result type (might be due to invalid IL or missing references)
		//IL_0299: Unknown result type (might be due to invalid IL or missing references)
		//IL_029e: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0304: Unknown result type (might be due to invalid IL or missing references)
		//IL_0312: Unknown result type (might be due to invalid IL or missing references)
		//IL_0317: Unknown result type (might be due to invalid IL or missing references)
		//IL_0320: Unknown result type (might be due to invalid IL or missing references)
		//IL_0325: Unknown result type (might be due to invalid IL or missing references)
		//IL_0365: Unknown result type (might be due to invalid IL or missing references)
		//IL_036a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0378: Unknown result type (might be due to invalid IL or missing references)
		//IL_037d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0386: Unknown result type (might be due to invalid IL or missing references)
		//IL_038b: Unknown result type (might be due to invalid IL or missing references)
		//IL_03cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_03de: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f1: Unknown result type (might be due to invalid IL or missing references)
		((ScriptComponentBehavior)this).OnInit();
		GameEntity val = GameEntity.Instantiate(((MapScene)(object)Campaign.Current.MapSceneWrapper).Scene, _attackerBallistaPrefab, true, true, "");
		_attackerBallistaLaunchEntitialFrame = val.GetChild(0).GetFirstChildEntityWithTag("projectile_position").GetGlobalFrame();
		MatrixFrame frame = val.GetChild(0).GetFrame();
		_attackerBallistaScale = ((Mat3)(ref frame.rotation)).GetScaleVector();
		GameEntity val2 = GameEntity.Instantiate(((MapScene)(object)Campaign.Current.MapSceneWrapper).Scene, _defenderBallistaPrefab, true, true, "");
		_defenderBallistaLaunchEntitialFrame = val2.GetChild(0).GetFirstChildEntityWithTag("projectile_position").GetGlobalFrame();
		frame = val2.GetChild(0).GetFrame();
		_defenderBallistaScale = ((Mat3)(ref frame.rotation)).GetScaleVector();
		GameEntity val3 = GameEntity.Instantiate(((MapScene)(object)Campaign.Current.MapSceneWrapper).Scene, _attackerFireBallistaPrefab, true, true, "");
		_attackerFireBallistaLaunchEntitialFrame = val3.GetChild(0).GetFirstChildEntityWithTag("projectile_position").GetGlobalFrame();
		frame = val3.GetChild(0).GetFrame();
		_attackerFireBallistaScale = ((Mat3)(ref frame.rotation)).GetScaleVector();
		GameEntity val4 = GameEntity.Instantiate(((MapScene)(object)Campaign.Current.MapSceneWrapper).Scene, _defenderFireBallistaPrefab, true, true, "");
		_defenderFireBallistaLaunchEntitialFrame = val4.GetChild(0).GetFirstChildEntityWithTag("projectile_position").GetGlobalFrame();
		frame = val4.GetChild(0).GetFrame();
		_defenderFireBallistaScale = ((Mat3)(ref frame.rotation)).GetScaleVector();
		GameEntity val5 = GameEntity.Instantiate(((MapScene)(object)Campaign.Current.MapSceneWrapper).Scene, _attackerMangonelPrefab, true, true, "");
		_attackerMangonelLaunchEntitialFrame = val5.GetChild(0).GetFirstChildEntityWithTag("projectile_position").GetGlobalFrame();
		frame = val5.GetChild(0).GetFrame();
		_attackerMangonelScale = ((Mat3)(ref frame.rotation)).GetScaleVector();
		GameEntity val6 = GameEntity.Instantiate(((MapScene)(object)Campaign.Current.MapSceneWrapper).Scene, _defenderMangonelPrefab, true, true, "");
		_defenderMangonelLaunchEntitialFrame = val6.GetChild(0).GetFirstChildEntityWithTag("projectile_position").GetGlobalFrame();
		frame = val6.GetChild(0).GetFrame();
		_defenderMangonelScale = ((Mat3)(ref frame.rotation)).GetScaleVector();
		GameEntity val7 = GameEntity.Instantiate(((MapScene)(object)Campaign.Current.MapSceneWrapper).Scene, _attackerFireMangonelPrefab, true, true, "");
		_attackerFireMangonelLaunchEntitialFrame = val7.GetChild(0).GetFirstChildEntityWithTag("projectile_position").GetGlobalFrame();
		frame = val7.GetChild(0).GetFrame();
		_attackerFireMangonelScale = ((Mat3)(ref frame.rotation)).GetScaleVector();
		GameEntity val8 = GameEntity.Instantiate(((MapScene)(object)Campaign.Current.MapSceneWrapper).Scene, _defenderFireMangonelPrefab, true, true, "");
		_defenderFireMangonelLaunchEntitialFrame = val8.GetChild(0).GetFirstChildEntityWithTag("projectile_position").GetGlobalFrame();
		frame = val8.GetChild(0).GetFrame();
		_defenderFireMangonelScale = ((Mat3)(ref frame.rotation)).GetScaleVector();
		GameEntity val9 = GameEntity.Instantiate(((MapScene)(object)Campaign.Current.MapSceneWrapper).Scene, _attackerTrebuchetPrefab, true, true, "");
		_attackerTrebuchetLaunchEntitialFrame = val9.GetChild(0).GetFirstChildEntityWithTag("projectile_position").GetGlobalFrame();
		frame = val9.GetChild(0).GetFrame();
		_attackerTrebuchetScale = ((Mat3)(ref frame.rotation)).GetScaleVector();
		GameEntity val10 = GameEntity.Instantiate(((MapScene)(object)Campaign.Current.MapSceneWrapper).Scene, _defenderTrebuchetPrefab, true, true, "");
		_defenderTrebuchetLaunchEntitialFrame = val10.GetChild(0).GetFirstChildEntityWithTag("projectile_position").GetGlobalFrame();
		frame = val10.GetChild(0).GetFrame();
		_defenderTrebuchetScale = ((Mat3)(ref frame.rotation)).GetScaleVector();
	}

	public MatrixFrame GetLaunchEntitialFrameForSiegeEngine(SiegeEngineType type, BattleSideEnum side)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Invalid comparison between Unknown and I4
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Invalid comparison between Unknown and I4
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		MatrixFrame result = MatrixFrame.Identity;
		if (type == DefaultSiegeEngineTypes.Onager)
		{
			result = _attackerMangonelLaunchEntitialFrame;
		}
		else if (type == DefaultSiegeEngineTypes.FireOnager)
		{
			result = _attackerFireMangonelLaunchEntitialFrame;
		}
		else if (type == DefaultSiegeEngineTypes.Catapult)
		{
			result = _defenderMangonelLaunchEntitialFrame;
		}
		else if (type == DefaultSiegeEngineTypes.FireCatapult)
		{
			result = _defenderFireMangonelLaunchEntitialFrame;
		}
		else if (type == DefaultSiegeEngineTypes.Ballista)
		{
			result = (((int)side == 1) ? _attackerBallistaLaunchEntitialFrame : _defenderBallistaLaunchEntitialFrame);
		}
		else if (type == DefaultSiegeEngineTypes.FireBallista)
		{
			result = (((int)side == 1) ? _attackerFireBallistaLaunchEntitialFrame : _defenderFireBallistaLaunchEntitialFrame);
		}
		else if (type == DefaultSiegeEngineTypes.Trebuchet)
		{
			result = _attackerTrebuchetLaunchEntitialFrame;
		}
		else if (type == DefaultSiegeEngineTypes.Bricole)
		{
			result = _defenderTrebuchetLaunchEntitialFrame;
		}
		return result;
	}

	public Vec3 GetScaleForSiegeEngine(SiegeEngineType type, BattleSideEnum side)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Invalid comparison between Unknown and I4
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Invalid comparison between Unknown and I4
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		Vec3 result = Vec3.Zero;
		if (type == DefaultSiegeEngineTypes.Onager)
		{
			result = _attackerMangonelScale;
		}
		else if (type == DefaultSiegeEngineTypes.FireOnager)
		{
			result = _attackerFireMangonelScale;
		}
		else if (type == DefaultSiegeEngineTypes.Catapult)
		{
			result = _defenderMangonelScale;
		}
		else if (type == DefaultSiegeEngineTypes.FireCatapult)
		{
			result = _defenderFireMangonelScale;
		}
		else if (type == DefaultSiegeEngineTypes.Ballista)
		{
			result = (((int)side == 1) ? _attackerBallistaScale : _defenderBallistaScale);
		}
		else if (type == DefaultSiegeEngineTypes.FireBallista)
		{
			result = (((int)side == 1) ? _attackerFireBallistaScale : _defenderFireBallistaScale);
		}
		else if (type == DefaultSiegeEngineTypes.Trebuchet)
		{
			result = _attackerTrebuchetScale;
		}
		else if (type == DefaultSiegeEngineTypes.Bricole)
		{
			result = _defenderTrebuchetScale;
		}
		return result;
	}
}
