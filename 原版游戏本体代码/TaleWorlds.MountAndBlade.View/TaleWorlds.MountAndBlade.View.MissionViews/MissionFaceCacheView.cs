using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;

namespace TaleWorlds.MountAndBlade.View.MissionViews;

public class MissionFaceCacheView : MissionView
{
	private struct CacheRecord
	{
		public BodyProperties BodyProperties;

		public int CacheID;

		public FaceGenerationParams FaceParamsForSimilarity;

		public HairCoverTypes HairCover;

		public BeardCoverTypes BeardCover;
	}

	private int _totalFaceBudget = 250;

	private int _uniqueCacheIndex;

	private float _currentSimilarityThreshold;

	private float _currentRandomSwitchChance;

	private KeyValuePair<float, float>[] _comprasionThresholdsWrtEmptyBudget;

	private List<CacheRecord> _alreadyAssignedFaces = new List<CacheRecord>();

	private MBFastRandom _randomGenerator;

	public MissionFaceCacheView()
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Expected O, but got Unknown
		_totalFaceBudget = (NativeConfig.CharacterDetail + 1) * 100;
		_randomGenerator = new MBFastRandom((uint)(Time.ApplicationTime * 73f));
		_currentSimilarityThreshold = 25f;
		_comprasionThresholdsWrtEmptyBudget = new KeyValuePair<float, float>[5];
		_comprasionThresholdsWrtEmptyBudget[0] = new KeyValuePair<float, float>(0.2f, 100f);
		_comprasionThresholdsWrtEmptyBudget[1] = new KeyValuePair<float, float>(0.4f, 200f);
		_comprasionThresholdsWrtEmptyBudget[2] = new KeyValuePair<float, float>(0.6f, 450f);
		_comprasionThresholdsWrtEmptyBudget[3] = new KeyValuePair<float, float>(0.8f, 750f);
		_comprasionThresholdsWrtEmptyBudget[4] = new KeyValuePair<float, float>(1f, 5000f);
	}

	public override void OnPreMissionTick(float dt)
	{
	}

	public override void OnBehaviorInitialize()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected O, but got Unknown
		Mission.Current.OnComputeTroopBodyProperties += new ComputeTroopBodyPropertiesDelegate(GetRandomBodyPropertyForTroop);
	}

	public override void OnMissionScreenFinalize()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected O, but got Unknown
		Mission.Current.OnComputeTroopBodyProperties -= new ComputeTroopBodyPropertiesDelegate(GetRandomBodyPropertyForTroop);
		FaceGen.FlushFaceCache();
	}

	private float ComputeSimilarityOfFace(FaceGenerationParams f0, FaceGenerationParams f1, HairCoverTypes hairCover1, HairCoverTypes hairCover2, BeardCoverTypes beardCover1, BeardCoverTypes beardCover2)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		float num = 0f;
		if (hairCover1 != hairCover2)
		{
			num += 1000000f;
		}
		if (beardCover1 != beardCover2)
		{
			num += 1000000f;
		}
		if (f0.CurrentBeard != f1.CurrentBeard)
		{
			num += 10f;
		}
		if (f0.CurrentHair != f1.CurrentHair)
		{
			num += 10f;
		}
		if (f0.CurrentEyebrow != f1.CurrentEyebrow)
		{
			num += 10f;
		}
		if (f0.CurrentRace != f1.CurrentRace)
		{
			num += 10f;
		}
		if (f0.CurrentGender != f1.CurrentGender)
		{
			num += 1000f;
		}
		if (f0.CurrentFaceTexture != f1.CurrentFaceTexture)
		{
			num += 10f;
		}
		if (f0.CurrentMouthTexture != f1.CurrentMouthTexture)
		{
			num += 5f;
		}
		if (f0.CurrentFaceTattoo != f1.CurrentFaceTattoo)
		{
			num += 250f;
		}
		float num2 = 32.5f;
		for (int i = 0; i < f0.KeyWeights.Length; i++)
		{
			num += MathF.Abs(f0.KeyWeights[i] - f1.KeyWeights[i]) * num2;
		}
		return num;
	}

	private int CheckForSimilarFacesFromCache(FaceGenerationParams newFaceGen, HairCoverTypes hairCoverType, BeardCoverTypes beardCoverType)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		int num = -1;
		float num2 = 1E+09f;
		for (int i = 0; i < _uniqueCacheIndex; i++)
		{
			float num3 = ComputeSimilarityOfFace(newFaceGen, _alreadyAssignedFaces[i].FaceParamsForSimilarity, hairCoverType, _alreadyAssignedFaces[i].HairCover, beardCoverType, _alreadyAssignedFaces[i].BeardCover);
			if (num3 < _currentSimilarityThreshold && _randomGenerator.NextFloat() > _currentRandomSwitchChance)
			{
				return i;
			}
			if (num2 < num3 && (_randomGenerator.NextFloat() > _currentRandomSwitchChance || num == -1))
			{
				num = i;
				num2 = num3;
			}
		}
		if (_uniqueCacheIndex == _totalFaceBudget)
		{
			return num;
		}
		return -1;
	}

	private void UpdateFaceSimilarityThreshold()
	{
		float num = (float)_uniqueCacheIndex / (float)_totalFaceBudget;
		float num2 = 0.4f;
		float num3 = 0.7f;
		float num4 = (num - num2) / (num3 - num2);
		num4 = MathF.Clamp(num4, 0f, 1f);
		_currentRandomSwitchChance = 0.37f * num4;
		if (num < _comprasionThresholdsWrtEmptyBudget[0].Key)
		{
			_currentSimilarityThreshold = _comprasionThresholdsWrtEmptyBudget[0].Value;
			_currentRandomSwitchChance = 0f;
			return;
		}
		for (int i = 1; i < _comprasionThresholdsWrtEmptyBudget.Count(); i++)
		{
			if (_comprasionThresholdsWrtEmptyBudget[i].Key > num)
			{
				float value = _comprasionThresholdsWrtEmptyBudget[i - 1].Value;
				float value2 = _comprasionThresholdsWrtEmptyBudget[i].Value;
				float key = _comprasionThresholdsWrtEmptyBudget[i - 1].Key;
				float key2 = _comprasionThresholdsWrtEmptyBudget[i].Key;
				float num5 = (num - key) / (key2 - key);
				num5 = MathF.Clamp(num5, 0f, 1f);
				_currentSimilarityThreshold = value + (value2 - value) * num5;
				break;
			}
		}
	}

	private BodyProperties GetRandomBodyPropertyForTroop(AgentBuildData agentBuildData, BasicCharacterObject characterObject, Equipment equipment, int seed)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		if (characterObject.IsHero)
		{
			return characterObject.GetBodyProperties(equipment, seed);
		}
		HairCoverTypes hairCoverType = equipment.HairCoverType;
		BeardCoverTypes beardCoverType = equipment.BeardCoverType;
		bool earsAreHidden = equipment.EarsAreHidden;
		bool mouthIsHidden = equipment.MouthIsHidden;
		BodyProperties bodyProperties = characterObject.GetBodyProperties(equipment, seed);
		FaceGenerationParams val = default(FaceGenerationParams);
		MBBodyProperties.GetParamsFromKey(ref val, bodyProperties, earsAreHidden, mouthIsHidden);
		int num = CheckForSimilarFacesFromCache(val, hairCoverType, beardCoverType);
		if (num != -1)
		{
			DynamicBodyProperties dynamicProperties = ((BodyProperties)(ref bodyProperties)).DynamicProperties;
			CacheRecord cacheRecord = _alreadyAssignedFaces[num];
			BodyProperties result = new BodyProperties(dynamicProperties, ((BodyProperties)(ref cacheRecord.BodyProperties)).StaticProperties);
			agentBuildData.FaceCacheId = _alreadyAssignedFaces[num].CacheID;
			return result;
		}
		agentBuildData.FaceCacheId = _uniqueCacheIndex;
		CacheRecord item = new CacheRecord
		{
			BodyProperties = bodyProperties,
			CacheID = _uniqueCacheIndex,
			FaceParamsForSimilarity = val,
			HairCover = hairCoverType,
			BeardCover = beardCoverType
		};
		_alreadyAssignedFaces.Add(item);
		_uniqueCacheIndex++;
		MBDebug.Print($"GetRandomBodyPropertyForTroop::Unique troop index: {_uniqueCacheIndex}\n", 0, (DebugColor)12, 17592186044416uL);
		UpdateFaceSimilarityThreshold();
		return bodyProperties;
	}
}
