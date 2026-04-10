using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.DotNet;
using TaleWorlds.Engine;
using TaleWorlds.Library;

[ScriptComponentParams("ship_visual_only", "")]
internal class ShipBurningSystem : ScriptComponentBehavior
{
	private const string RailingParentTag = "railing_parent";

	private bool _fireStarted;

	private BurningSystem _railingFire;

	private BurningSystem _shipDeckFire;

	private BurningSystem _deckUpgradeFire;

	private BurningSystem _mastFire;

	private List<BurningNode> _railingNodes = new List<BurningNode>();

	private List<BurningNode> _shipDeckNodes = new List<BurningNode>();

	private List<BurningNode> _deckUpgradeNodes = new List<BurningNode>();

	private List<BurningNode> _mastNodes = new List<BurningNode>();

	private List<BurningSoundNode> _soundNodes = new List<BurningSoundNode>();

	private List<Light> _burningLights = new List<Light>();

	private MBFastRandom _randomGenerator;

	private List<BurningNode> _temporaryBurningNodes = new List<BurningNode>();

	[EditableScriptComponentVariable(true, "Start Fire")]
	private SimpleButton _startFire = new SimpleButton();

	[EditableScriptComponentVariable(true, "Stop Fire")]
	private SimpleButton _stopFire = new SimpleButton();

	[EditableScriptComponentVariable(true, "Spread Rate")]
	private float _spreadRate = 1f;

	[EditableScriptComponentVariable(true, "Fire Start Random Count")]
	private int _fireStartRandomCount = 2;

	[EditableScriptComponentVariable(true, "All Fire Mode")]
	private bool _allFireMode;

	[EditableScriptComponentVariable(true, "Small Hit Debug")]
	private bool _hitDebug;

	[EditableScriptComponentVariable(true, "Min Fire Progress For Light")]
	private float _minFireProgressLight = 0.5f;

	[EditableScriptComponentVariable(true, "Max Fire Progress For Light")]
	private float _maxFireProgressLight = 1f;

	[EditableScriptComponentVariable(true, "Max Light Intensity")]
	private float _maxLightIntensity = 5000f;

	public void DummyFunc()
	{
		Debug.Print(((object)_stopFire).ToString(), 0, (DebugColor)12, 17592186044416uL);
		Debug.Print(((object)_stopFire).ToString(), 0, (DebugColor)12, 17592186044416uL);
		Debug.Print(((object)_startFire).ToString(), 0, (DebugColor)12, 17592186044416uL);
		Debug.Print(_allFireMode.ToString(), 0, (DebugColor)12, 17592186044416uL);
		Debug.Print(_hitDebug.ToString(), 0, (DebugColor)12, 17592186044416uL);
	}

	protected override void OnInit()
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected O, but got Unknown
		FetchEntities();
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		_randomGenerator = new MBFastRandom((uint)((ulong)((WeakGameEntity)(ref gameEntity)).Pointer & 0xFFFFFFFFu));
	}

	protected override void OnTickParallel(float dt)
	{
		if (_fireStarted)
		{
			TickFire(dt);
		}
		HandleTemporaryBurningNodes(dt);
	}

	public override TickRequirement GetTickRequirement()
	{
		return (TickRequirement)4;
	}

	private void TickFire(float dt)
	{
		//IL_0184: Unknown result type (might be due to invalid IL or missing references)
		//IL_0189: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0191: Unknown result type (might be due to invalid IL or missing references)
		//IL_0194: Unknown result type (might be due to invalid IL or missing references)
		//IL_0199: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_021e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0223: Unknown result type (might be due to invalid IL or missing references)
		//IL_0226: Unknown result type (might be due to invalid IL or missing references)
		//IL_022b: Unknown result type (might be due to invalid IL or missing references)
		//IL_022e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0233: Unknown result type (might be due to invalid IL or missing references)
		//IL_023d: Unknown result type (might be due to invalid IL or missing references)
		//IL_024b: Unknown result type (might be due to invalid IL or missing references)
		//IL_024d: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0352: Unknown result type (might be due to invalid IL or missing references)
		//IL_0357: Unknown result type (might be due to invalid IL or missing references)
		//IL_035a: Unknown result type (might be due to invalid IL or missing references)
		//IL_035f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0362: Unknown result type (might be due to invalid IL or missing references)
		//IL_0367: Unknown result type (might be due to invalid IL or missing references)
		//IL_0371: Unknown result type (might be due to invalid IL or missing references)
		//IL_037f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0381: Unknown result type (might be due to invalid IL or missing references)
		float num = 0f;
		int num2 = 0;
		if (_railingFire != null)
		{
			_railingFire.Tick(dt);
			num += _railingFire.AverageFireProgress;
			num2++;
		}
		if (_shipDeckFire != null)
		{
			_shipDeckFire.Tick(dt);
			num += _shipDeckFire.AverageFireProgress;
			num2++;
		}
		if (_deckUpgradeFire != null)
		{
			_deckUpgradeFire.Tick(dt);
			num += _deckUpgradeFire.AverageFireProgress;
			num2++;
		}
		if (_mastFire != null)
		{
			_mastFire.Tick(dt);
			num += _mastFire.AverageFireProgress;
			num2++;
		}
		WeakGameEntity val;
		if (num2 > 0)
		{
			num /= (float)num2;
			if (num < _minFireProgressLight)
			{
				foreach (Light burningLight in _burningLights)
				{
					val = ((GameEntityComponent)burningLight).GetEntity();
					((WeakGameEntity)(ref val)).SetVisibilityExcludeParents(false);
				}
			}
			else
			{
				float num3 = (num - _minFireProgressLight) / (_maxFireProgressLight - _minFireProgressLight);
				num3 = MathF.Clamp(num3, 0f, 1f) * _maxLightIntensity;
				foreach (Light burningLight2 in _burningLights)
				{
					val = ((GameEntityComponent)burningLight2).GetEntity();
					((WeakGameEntity)(ref val)).SetVisibilityExcludeParents(true);
					burningLight2.Intensity = num3;
				}
			}
		}
		foreach (BurningNode railingNode in _railingNodes)
		{
			val = ((ScriptComponentBehavior)railingNode).GameEntity;
			MatrixFrame globalFrame = ((WeakGameEntity)(ref val)).GetGlobalFrame();
			val = ((ScriptComponentBehavior)this).GameEntity;
			float waterLevelAtPosition = ((WeakGameEntity)(ref val)).GetWaterLevelAtPosition(((Vec3)(ref globalFrame.origin)).AsVec2, true, false);
			if (globalFrame.origin.z < waterLevelAtPosition)
			{
				_railingFire.SetFlameProgressOfAdvancedNode(railingNode, 0f);
				railingNode.CurrentFireProgress = 0f;
				railingNode.BurningTimer = 3f;
			}
		}
		foreach (BurningNode shipDeckNode in _shipDeckNodes)
		{
			val = ((ScriptComponentBehavior)shipDeckNode).GameEntity;
			MatrixFrame globalFrame2 = ((WeakGameEntity)(ref val)).GetGlobalFrame();
			val = ((ScriptComponentBehavior)this).GameEntity;
			float waterLevelAtPosition2 = ((WeakGameEntity)(ref val)).GetWaterLevelAtPosition(((Vec3)(ref globalFrame2.origin)).AsVec2, true, false);
			if (globalFrame2.origin.z < waterLevelAtPosition2)
			{
				_shipDeckFire.SetFlameProgressOfAdvancedNode(shipDeckNode, 0f);
				shipDeckNode.CurrentFireProgress = 0f;
				shipDeckNode.BurningTimer = 3f;
			}
		}
		foreach (BurningNode deckUpgradeNode in _deckUpgradeNodes)
		{
			val = ((ScriptComponentBehavior)deckUpgradeNode).GameEntity;
			MatrixFrame globalFrame3 = ((WeakGameEntity)(ref val)).GetGlobalFrame();
			val = ((ScriptComponentBehavior)this).GameEntity;
			float waterLevelAtPosition3 = ((WeakGameEntity)(ref val)).GetWaterLevelAtPosition(((Vec3)(ref globalFrame3.origin)).AsVec2, true, false);
			if (globalFrame3.origin.z < waterLevelAtPosition3)
			{
				_deckUpgradeFire.SetFlameProgressOfAdvancedNode(deckUpgradeNode, 0f);
				deckUpgradeNode.CurrentFireProgress = 0f;
				deckUpgradeNode.BurningTimer = 3f;
			}
		}
		foreach (BurningNode mastNode in _mastNodes)
		{
			val = ((ScriptComponentBehavior)mastNode).GameEntity;
			MatrixFrame globalFrame4 = ((WeakGameEntity)(ref val)).GetGlobalFrame();
			val = ((ScriptComponentBehavior)this).GameEntity;
			float waterLevelAtPosition4 = ((WeakGameEntity)(ref val)).GetWaterLevelAtPosition(((Vec3)(ref globalFrame4.origin)).AsVec2, true, false);
			if (globalFrame4.origin.z < waterLevelAtPosition4)
			{
				_mastFire.SetFlameProgressOfAdvancedNode(mastNode, 0f);
				mastNode.CurrentFireProgress = 0f;
				mastNode.BurningTimer = 3f;
			}
		}
	}

	private void FillFireSystemWithNodes(ref List<BurningNode> nodes, ref BurningSystem fire)
	{
		nodes.Sort((BurningNode x, BurningNode y) => x.NodeIndex.CompareTo(x.NodeIndex));
		fire = new BurningSystem(null, 1f / _spreadRate);
		fire.AddAdvancedNode(nodes[0], nodes[nodes.Count - 1], nodes[1]);
		for (int num = 1; num < nodes.Count - 1; num++)
		{
			fire.AddAdvancedNode(nodes[num], nodes[num - 1], nodes[num + 1]);
			foreach (BurningSoundNode soundNode in _soundNodes)
			{
				soundNode.AddBurningNode(nodes[num]);
			}
		}
		fire.AddAdvancedNode(nodes[nodes.Count - 1], nodes[nodes.Count - 2], nodes[0]);
		for (int num2 = 0; num2 < _fireStartRandomCount; num2++)
		{
			int index = MBRandom.RandomInt(nodes.Count);
			fire.SetFlameProgressOfAdvancedNode(nodes[index], 0.05f + MBRandom.RandomFloat * 0.1f);
		}
	}

	private void FetchEntities()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_026a: Unknown result type (might be due to invalid IL or missing references)
		//IL_026f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0278: Unknown result type (might be due to invalid IL or missing references)
		//IL_027d: Unknown result type (might be due to invalid IL or missing references)
		//IL_027f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_0194: Unknown result type (might be due to invalid IL or missing references)
		//IL_0199: Unknown result type (might be due to invalid IL or missing references)
		//IL_0209: Unknown result type (might be due to invalid IL or missing references)
		//IL_020e: Unknown result type (might be due to invalid IL or missing references)
		//IL_029b: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a0: Unknown result type (might be due to invalid IL or missing references)
		_railingNodes.Clear();
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		WeakGameEntity firstChildEntityWithTag = ((WeakGameEntity)(ref gameEntity)).GetFirstChildEntityWithTag("railing_parent");
		if (firstChildEntityWithTag != (GameEntity)null)
		{
			foreach (WeakGameEntity child in ((WeakGameEntity)(ref firstChildEntityWithTag)).GetChildren())
			{
				WeakGameEntity current = child;
				BurningNode firstScriptOfType = ((WeakGameEntity)(ref current)).GetFirstScriptOfType<BurningNode>();
				if (firstScriptOfType != null)
				{
					_railingNodes.Add(firstScriptOfType);
				}
			}
		}
		_shipDeckNodes.Clear();
		gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		WeakGameEntity firstChildEntityWithTag2 = ((WeakGameEntity)(ref gameEntity)).GetFirstChildEntityWithTag("ship_deck_parent");
		if (firstChildEntityWithTag2 != (GameEntity)null)
		{
			foreach (WeakGameEntity child2 in ((WeakGameEntity)(ref firstChildEntityWithTag2)).GetChildren())
			{
				WeakGameEntity current2 = child2;
				BurningNode firstScriptOfType2 = ((WeakGameEntity)(ref current2)).GetFirstScriptOfType<BurningNode>();
				if (firstScriptOfType2 != null)
				{
					_shipDeckNodes.Add(firstScriptOfType2);
				}
			}
		}
		_deckUpgradeNodes.Clear();
		gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		WeakGameEntity firstChildEntityWithTag3 = ((WeakGameEntity)(ref gameEntity)).GetFirstChildEntityWithTag("deck_upgrade_parent");
		if (firstChildEntityWithTag3 != (GameEntity)null)
		{
			foreach (WeakGameEntity child3 in ((WeakGameEntity)(ref firstChildEntityWithTag3)).GetChildren())
			{
				WeakGameEntity current3 = child3;
				BurningNode firstScriptOfType3 = ((WeakGameEntity)(ref current3)).GetFirstScriptOfType<BurningNode>();
				if (firstScriptOfType3 != null)
				{
					_deckUpgradeNodes.Add(firstScriptOfType3);
				}
			}
		}
		_mastNodes.Clear();
		gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		WeakGameEntity firstChildEntityWithTag4 = ((WeakGameEntity)(ref gameEntity)).GetFirstChildEntityWithTag("mast_parent");
		if (firstChildEntityWithTag4 != (GameEntity)null)
		{
			foreach (WeakGameEntity child4 in ((WeakGameEntity)(ref firstChildEntityWithTag4)).GetChildren())
			{
				WeakGameEntity current4 = child4;
				BurningNode firstScriptOfType4 = ((WeakGameEntity)(ref current4)).GetFirstScriptOfType<BurningNode>();
				if (firstScriptOfType4 != null)
				{
					_mastNodes.Add(firstScriptOfType4);
				}
			}
		}
		_burningLights.Clear();
		gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		WeakGameEntity firstChildEntityWithTag5 = ((WeakGameEntity)(ref gameEntity)).GetFirstChildEntityWithTag("light_parent");
		if (firstChildEntityWithTag5 != (GameEntity)null)
		{
			foreach (WeakGameEntity child5 in ((WeakGameEntity)(ref firstChildEntityWithTag5)).GetChildren())
			{
				WeakGameEntity current5 = child5;
				GameEntityComponent componentAtIndex = ((WeakGameEntity)(ref current5)).GetComponentAtIndex(0, (ComponentType)1);
				Light val = (Light)(object)((componentAtIndex is Light) ? componentAtIndex : null);
				if ((NativeObject)(object)val != (NativeObject)null)
				{
					_burningLights.Add(val);
					if (!_allFireMode)
					{
						((WeakGameEntity)(ref current5)).SetVisibilityExcludeParents(false);
					}
				}
			}
		}
		_soundNodes.Clear();
		gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		WeakGameEntity firstChildEntityWithTag6 = ((WeakGameEntity)(ref gameEntity)).GetFirstChildEntityWithTag("sound_parent");
		if (!(firstChildEntityWithTag6 != (GameEntity)null))
		{
			return;
		}
		foreach (WeakGameEntity child6 in ((WeakGameEntity)(ref firstChildEntityWithTag6)).GetChildren())
		{
			WeakGameEntity current6 = child6;
			BurningSoundNode firstScriptOfType5 = ((WeakGameEntity)(ref current6)).GetFirstScriptOfType<BurningSoundNode>();
			if (firstScriptOfType5 != null)
			{
				_soundNodes.Add(firstScriptOfType5);
			}
		}
	}

	private void HandleTemporaryBurningNodes(float dt)
	{
		float num = 0.05f;
		for (int i = 0; i < _temporaryBurningNodes.Count; i++)
		{
			BurningNode burningNode = _temporaryBurningNodes[i];
			burningNode.CurrentFireProgress -= dt * num;
			if (burningNode.CurrentFireProgress == 0f)
			{
				_temporaryBurningNodes[i] = _temporaryBurningNodes[_temporaryBurningNodes.Count - 1];
				_temporaryBurningNodes.Remove(_temporaryBurningNodes[_temporaryBurningNodes.Count - 1]);
				i--;
			}
		}
	}

	private void RegisterBlowAux(Vec3 collisionPosition, List<BurningNode> nodes, BurningSystem fire)
	{
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		float num = 6f;
		float num2 = num * num;
		float num3 = 2f;
		float num4 = 0.75f;
		float num5 = 0.35f;
		foreach (BurningNode node in nodes)
		{
			if (!(node.CurrentFireProgress < 1f))
			{
				continue;
			}
			WeakGameEntity gameEntity = ((ScriptComponentBehavior)node).GameEntity;
			MatrixFrame globalFrame = ((WeakGameEntity)(ref gameEntity)).GetGlobalFrame();
			float num6 = ((Vec3)(ref globalFrame.origin)).DistanceSquared(collisionPosition);
			if (num6 < num2)
			{
				float num7 = MathF.Sqrt(num6);
				float num8 = 1f - MathF.Clamp((num7 - num3) / num, 0f, 1f);
				float num9 = _randomGenerator.NextFloatRanged(num5, num4) * num8;
				if (fire != null)
				{
					fire.SetFlameProgressOfAdvancedNode(node, node.CurrentFireProgress);
				}
				else if (node.CurrentFireProgress == 0f)
				{
					_temporaryBurningNodes.Add(node);
				}
				node.CurrentFireProgress += num9;
			}
		}
	}

	public void RegisterBlow(Vec3 collisionPosition)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		RegisterBlowAux(collisionPosition, _railingNodes, _railingFire);
		RegisterBlowAux(collisionPosition, _shipDeckNodes, _shipDeckFire);
		RegisterBlowAux(collisionPosition, _deckUpgradeNodes, _deckUpgradeFire);
		RegisterBlowAux(collisionPosition, _mastNodes, _mastFire);
	}

	public void StartFire()
	{
		_fireStarted = true;
		if (_railingNodes.Count > 2)
		{
			FillFireSystemWithNodes(ref _railingNodes, ref _railingFire);
		}
		if (_shipDeckNodes.Count > 2)
		{
			FillFireSystemWithNodes(ref _shipDeckNodes, ref _shipDeckFire);
		}
		if (_deckUpgradeNodes.Count > 2)
		{
			FillFireSystemWithNodes(ref _deckUpgradeNodes, ref _deckUpgradeFire);
		}
		if (_mastNodes.Count > 2)
		{
			FillFireSystemWithNodes(ref _mastNodes, ref _mastFire);
		}
		foreach (BurningSoundNode soundNode in _soundNodes)
		{
			soundNode.StartFire();
		}
	}
}
