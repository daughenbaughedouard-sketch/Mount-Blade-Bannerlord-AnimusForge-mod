using System.Collections.Generic;
using System.Xml;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.ObjectSystem;

namespace TaleWorlds.MountAndBlade.Network.Gameplay.Perks.Effects;

public class RandomEquipmentEffect : MPRandomOnSpawnPerkEffect
{
	protected static string StringType = "RandomEquipmentOnSpawn";

	private MBList<List<(EquipmentIndex, EquipmentElement)>> _groups;

	protected RandomEquipmentEffect()
	{
	}

	protected override void Deserialize(XmlNode node)
	{
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		((MPOnSpawnPerkEffectBase)this).Deserialize(node);
		_groups = new MBList<List<(EquipmentIndex, EquipmentElement)>>();
		foreach (XmlNode childNode in node.ChildNodes)
		{
			if (childNode.NodeType == XmlNodeType.Comment || childNode.NodeType == XmlNodeType.SignificantWhitespace || !(childNode.Name == "Group"))
			{
				continue;
			}
			List<(EquipmentIndex, EquipmentElement)> list = new List<(EquipmentIndex, EquipmentElement)>();
			foreach (XmlNode childNode2 in childNode.ChildNodes)
			{
				if (childNode2.NodeType != XmlNodeType.Comment && childNode2.NodeType != XmlNodeType.SignificantWhitespace)
				{
					EquipmentElement item = default(EquipmentElement);
					EquipmentIndex item2 = (EquipmentIndex)(-1);
					XmlAttribute xmlAttribute = childNode2.Attributes?["item"];
					if (xmlAttribute != null)
					{
						ItemObject val = MBObjectManager.Instance.GetObject<ItemObject>(xmlAttribute.Value);
						((EquipmentElement)(ref item))._002Ector(val, (ItemModifier)null, (ItemObject)null, false);
					}
					XmlAttribute xmlAttribute2 = childNode2.Attributes?["slot"];
					if (xmlAttribute2 != null)
					{
						item2 = Equipment.GetEquipmentIndexFromOldEquipmentIndexName(xmlAttribute2.Value);
					}
					list.Add((item2, item));
				}
			}
			if (list.Count > 0)
			{
				((List<List<(EquipmentIndex, EquipmentElement)>>)(object)_groups).Add(list);
			}
		}
	}

	public override List<(EquipmentIndex, EquipmentElement)> GetAlternativeEquipments(bool isPlayer, List<(EquipmentIndex, EquipmentElement)> alternativeEquipments, bool getAll)
	{
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Invalid comparison between Unknown and I4
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Invalid comparison between Unknown and I4
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Invalid comparison between Unknown and I4
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Invalid comparison between Unknown and I4
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Invalid comparison between Unknown and I4
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Invalid comparison between Unknown and I4
		if (getAll)
		{
			foreach (List<(EquipmentIndex, EquipmentElement)> item in (List<List<(EquipmentIndex, EquipmentElement)>>)(object)_groups)
			{
				if ((int)((MPOnSpawnPerkEffectBase)this).EffectTarget == 2 || (isPlayer ? ((int)((MPOnSpawnPerkEffectBase)this).EffectTarget == 0) : ((int)((MPOnSpawnPerkEffectBase)this).EffectTarget == 1)))
				{
					if (alternativeEquipments == null)
					{
						alternativeEquipments = new List<(EquipmentIndex, EquipmentElement)>(item);
					}
					else
					{
						alternativeEquipments.AddRange(item);
					}
				}
			}
		}
		else if ((int)((MPOnSpawnPerkEffectBase)this).EffectTarget == 2 || (isPlayer ? ((int)((MPOnSpawnPerkEffectBase)this).EffectTarget == 0) : ((int)((MPOnSpawnPerkEffectBase)this).EffectTarget == 1)))
		{
			if (alternativeEquipments == null)
			{
				alternativeEquipments = new List<(EquipmentIndex, EquipmentElement)>(Extensions.GetRandomElement<List<(EquipmentIndex, EquipmentElement)>>(_groups));
			}
			else
			{
				alternativeEquipments.AddRange(Extensions.GetRandomElement<List<(EquipmentIndex, EquipmentElement)>>(_groups));
			}
		}
		return alternativeEquipments;
	}
}
