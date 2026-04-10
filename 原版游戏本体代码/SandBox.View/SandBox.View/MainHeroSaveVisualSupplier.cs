using System.Collections.Generic;
using System.Globalization;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace SandBox.View;

public class MainHeroSaveVisualSupplier : IMainHeroVisualSupplier
{
	string IMainHeroVisualSupplier.GetMainHeroVisualCode()
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_0194: Unknown result type (might be due to invalid IL or missing references)
		//IL_019e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01db: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0205: Unknown result type (might be due to invalid IL or missing references)
		//IL_0212: Unknown result type (might be due to invalid IL or missing references)
		//IL_023d: Unknown result type (might be due to invalid IL or missing references)
		//IL_024a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0275: Unknown result type (might be due to invalid IL or missing references)
		//IL_0282: Unknown result type (might be due to invalid IL or missing references)
		//IL_0289: Unknown result type (might be due to invalid IL or missing references)
		//IL_035b: Unknown result type (might be due to invalid IL or missing references)
		//IL_035f: Invalid comparison between Unknown and I4
		//IL_0292: Unknown result type (might be due to invalid IL or missing references)
		//IL_0294: Unknown result type (might be due to invalid IL or missing references)
		//IL_0299: Unknown result type (might be due to invalid IL or missing references)
		//IL_036c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0371: Unknown result type (might be due to invalid IL or missing references)
		//IL_06d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0397: Unknown result type (might be due to invalid IL or missing references)
		//IL_039c: Unknown result type (might be due to invalid IL or missing references)
		//IL_03af: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_03de: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0405: Unknown result type (might be due to invalid IL or missing references)
		//IL_0412: Unknown result type (might be due to invalid IL or missing references)
		//IL_042b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0438: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0475: Unknown result type (might be due to invalid IL or missing references)
		//IL_047a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0485: Unknown result type (might be due to invalid IL or missing references)
		//IL_0492: Unknown result type (might be due to invalid IL or missing references)
		//IL_04a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0505: Unknown result type (might be due to invalid IL or missing references)
		//IL_0512: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_04bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_057e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0525: Unknown result type (might be due to invalid IL or missing references)
		//IL_0532: Unknown result type (might be due to invalid IL or missing references)
		//IL_0541: Unknown result type (might be due to invalid IL or missing references)
		//IL_054e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0562: Unknown result type (might be due to invalid IL or missing references)
		//IL_056f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0304: Unknown result type (might be due to invalid IL or missing references)
		//IL_031d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0336: Unknown result type (might be due to invalid IL or missing references)
		//IL_034f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0355: Unknown result type (might be due to invalid IL or missing references)
		//IL_0358: Unknown result type (might be due to invalid IL or missing references)
		//IL_0359: Unknown result type (might be due to invalid IL or missing references)
		//IL_0682: Unknown result type (might be due to invalid IL or missing references)
		//IL_05eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_05f1: Invalid comparison between Unknown and I4
		//IL_06a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_06ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_0606: Unknown result type (might be due to invalid IL or missing references)
		//IL_060c: Invalid comparison between Unknown and I4
		//IL_063e: Unknown result type (might be due to invalid IL or missing references)
		Hero mainHero = Hero.MainHero;
		CharacterObject characterObject = mainHero.CharacterObject;
		Monster baseMonsterFromRace = FaceGen.GetBaseMonsterFromRace(((BasicCharacterObject)characterObject).Race);
		MBStringBuilder val = default(MBStringBuilder);
		((MBStringBuilder)(ref val)).Initialize(1024, "GetMainHeroVisualCode");
		((MBStringBuilder)(ref val)).Append<string>("4|");
		MBActionSet actionSet = MBActionSet.GetActionSet(baseMonsterFromRace.ActionSetCode);
		((MBStringBuilder)(ref val)).Append<string>(((MBActionSet)(ref actionSet)).GetSkeletonName());
		((MBStringBuilder)(ref val)).Append<string>("|");
		Equipment battleEquipment = mainHero.BattleEquipment;
		((MBStringBuilder)(ref val)).Append<string>(((object)MBEquipmentMissionExtensions.GetSkinMeshesMask(battleEquipment)/*cast due to .constrained prefix*/).ToString());
		((MBStringBuilder)(ref val)).Append<string>("|");
		((MBStringBuilder)(ref val)).Append<string>(mainHero.IsFemale.ToString());
		((MBStringBuilder)(ref val)).Append<string>("|");
		((MBStringBuilder)(ref val)).Append<string>(((BasicCharacterObject)mainHero.CharacterObject).Race.ToString());
		((MBStringBuilder)(ref val)).Append<string>("|");
		((MBStringBuilder)(ref val)).Append<string>(((object)battleEquipment.GetUnderwearType(mainHero.IsFemale)/*cast due to .constrained prefix*/).ToString());
		((MBStringBuilder)(ref val)).Append<string>("|");
		((MBStringBuilder)(ref val)).Append<string>(((object)battleEquipment.BodyMeshType/*cast due to .constrained prefix*/).ToString());
		((MBStringBuilder)(ref val)).Append<string>("|");
		((MBStringBuilder)(ref val)).Append<string>(((object)battleEquipment.HairCoverType/*cast due to .constrained prefix*/).ToString());
		((MBStringBuilder)(ref val)).Append<string>("|");
		((MBStringBuilder)(ref val)).Append<string>(((object)battleEquipment.BeardCoverType/*cast due to .constrained prefix*/).ToString());
		((MBStringBuilder)(ref val)).Append<string>("|");
		((MBStringBuilder)(ref val)).Append<string>(((object)battleEquipment.BodyDeformType/*cast due to .constrained prefix*/).ToString());
		((MBStringBuilder)(ref val)).Append<string>("|");
		((MBStringBuilder)(ref val)).Append<string>(((BasicCharacterObject)characterObject).FaceDirtAmount.ToString(CultureInfo.InvariantCulture));
		((MBStringBuilder)(ref val)).Append<string>("|");
		((MBStringBuilder)(ref val)).Append<string>(((object)mainHero.BodyProperties/*cast due to .constrained prefix*/).ToString());
		((MBStringBuilder)(ref val)).Append<string>("|");
		((MBStringBuilder)(ref val)).Append<string>((mainHero.MapFaction != null) ? mainHero.MapFaction.Color.ToString() : "0xFFFFFFFF");
		((MBStringBuilder)(ref val)).Append<string>("|");
		((MBStringBuilder)(ref val)).Append<string>((mainHero.MapFaction != null) ? mainHero.MapFaction.Color2.ToString() : "0xFFFFFFFF");
		((MBStringBuilder)(ref val)).Append<string>("|");
		EquipmentElement val3;
		for (EquipmentIndex val2 = (EquipmentIndex)5; (int)val2 < 10; val2 = (EquipmentIndex)(val2 + 1))
		{
			val3 = battleEquipment[val2];
			ItemObject item = ((EquipmentElement)(ref val3)).Item;
			string text = ((item != null) ? item.MultiMeshName : "");
			bool flag = item != null && item.IsUsingTeamColor;
			bool flag2 = item != null && item.IsUsingTableau;
			bool flag3 = item != null && item.HasArmorComponent && item.ArmorComponent.MultiMeshHasGenderVariations;
			((MBStringBuilder)(ref val)).Append<string>(text + "|");
			((MBStringBuilder)(ref val)).Append<string>(flag + "|");
			((MBStringBuilder)(ref val)).Append<string>(flag3 + "|");
			((MBStringBuilder)(ref val)).Append<string>(flag2 + "|");
		}
		val3 = mainHero.BattleEquipment[(EquipmentIndex)10];
		if (!((EquipmentElement)(ref val3)).IsEmpty && ((ActionIndexCache)(ref ActionIndexCache.act_inventory_idle)).Index != -1)
		{
			val3 = mainHero.BattleEquipment[(EquipmentIndex)10];
			ItemObject item2 = ((EquipmentElement)(ref val3)).Item;
			val3 = mainHero.BattleEquipment[(EquipmentIndex)11];
			ItemObject item3 = ((EquipmentElement)(ref val3)).Item;
			HorseComponent horseComponent = item2.HorseComponent;
			MBActionSet actionSet2 = MBActionSet.GetActionSet(item2.HorseComponent.Monster.ActionSetCode);
			((MBStringBuilder)(ref val)).Append<string>(((MBActionSet)(ref actionSet2)).GetSkeletonName());
			((MBStringBuilder)(ref val)).Append<string>("|");
			((MBStringBuilder)(ref val)).Append<string>(item2.MultiMeshName);
			((MBStringBuilder)(ref val)).Append<string>("|");
			MountCreationKey randomMountKey = MountCreationKey.GetRandomMountKey(item2, ((BasicCharacterObject)characterObject).GetMountKeySeed());
			((MBStringBuilder)(ref val)).Append<MountCreationKey>(randomMountKey);
			((MBStringBuilder)(ref val)).Append<string>("|");
			if (((List<MaterialProperty>)(object)horseComponent.HorseMaterialNames).Count > 0)
			{
				int index = MathF.Min((int)randomMountKey.MaterialIndex, ((List<MaterialProperty>)(object)horseComponent.HorseMaterialNames).Count - 1);
				MaterialProperty val4 = ((List<MaterialProperty>)(object)horseComponent.HorseMaterialNames)[index];
				((MBStringBuilder)(ref val)).Append<string>(((MaterialProperty)(ref val4)).Name);
				((MBStringBuilder)(ref val)).Append<string>("|");
				uint num = uint.MaxValue;
				int num2 = MathF.Min((int)randomMountKey.MeshMultiplierIndex, val4.MeshMultiplier.Count - 1);
				if (num2 != -1)
				{
					num = val4.MeshMultiplier[num2].Item1;
				}
				((MBStringBuilder)(ref val)).Append(num);
			}
			else
			{
				((MBStringBuilder)(ref val)).Append<string>("|");
			}
			((MBStringBuilder)(ref val)).Append<string>("|");
			((MBStringBuilder)(ref val)).Append<string>(((MBActionSet)(ref actionSet2)).GetAnimationName(ref ActionIndexCache.act_inventory_idle));
			((MBStringBuilder)(ref val)).Append<string>("|");
			if (item3 != null)
			{
				((MBStringBuilder)(ref val)).Append<string>(item3.MultiMeshName);
				((MBStringBuilder)(ref val)).Append<string>("|");
				((MBStringBuilder)(ref val)).Append<bool>(item3.IsUsingTeamColor);
				((MBStringBuilder)(ref val)).Append<string>("|");
				((MBStringBuilder)(ref val)).Append<string>(item3.ArmorComponent.ReinsMesh);
				((MBStringBuilder)(ref val)).Append<string>("|");
			}
			else
			{
				((MBStringBuilder)(ref val)).Append<string>("|||");
			}
			List<string> list = new List<string>();
			foreach (KeyValuePair<string, bool> additionalMeshesName in horseComponent.AdditionalMeshesNameList)
			{
				if (additionalMeshesName.Key.Length <= 0)
				{
					continue;
				}
				string text2 = additionalMeshesName.Key;
				if (item3 == null || !additionalMeshesName.Value)
				{
					list.Add(text2);
					continue;
				}
				ArmorComponent armorComponent = item3.ArmorComponent;
				if (armorComponent != null && (int)armorComponent.ManeCoverType == 3)
				{
					continue;
				}
				ArmorComponent armorComponent2 = item3.ArmorComponent;
				if (armorComponent2 != null && (int)armorComponent2.ManeCoverType > 0)
				{
					string text3 = text2;
					HorseHarnessCoverTypes? obj;
					if (item3 == null)
					{
						obj = null;
					}
					else
					{
						ArmorComponent armorComponent3 = item3.ArmorComponent;
						obj = ((armorComponent3 != null) ? new HorseHarnessCoverTypes?(armorComponent3.ManeCoverType) : ((HorseHarnessCoverTypes?)null));
					}
					text2 = text3 + "_" + obj;
				}
				list.Add(text2);
			}
			((MBStringBuilder)(ref val)).Append(list.Count);
			foreach (string item4 in list)
			{
				((MBStringBuilder)(ref val)).Append<string>("|");
				((MBStringBuilder)(ref val)).Append<string>(item4);
			}
		}
		else
		{
			((MBStringBuilder)(ref val)).Append<string>("|||||||||0");
		}
		return ((MBStringBuilder)(ref val)).ToStringAndRelease();
	}
}
