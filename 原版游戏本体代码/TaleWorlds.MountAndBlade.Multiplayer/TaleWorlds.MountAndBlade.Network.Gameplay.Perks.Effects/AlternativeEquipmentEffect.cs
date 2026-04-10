using System.Collections.Generic;
using System.Xml;
using TaleWorlds.Core;
using TaleWorlds.ObjectSystem;

namespace TaleWorlds.MountAndBlade.Network.Gameplay.Perks.Effects;

public class AlternativeEquipmentEffect : MPOnSpawnPerkEffect
{
	protected static string StringType = "AlternativeEquipmentOnSpawn";

	private EquipmentElement _item;

	private EquipmentIndex _index;

	protected AlternativeEquipmentEffect()
	{
	}

	protected override void Deserialize(XmlNode node)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		((MPOnSpawnPerkEffectBase)this).Deserialize(node);
		_item = default(EquipmentElement);
		_index = (EquipmentIndex)(-1);
		XmlNode xmlNode = node?.Attributes?["item"];
		if (xmlNode != null)
		{
			ItemObject val = MBObjectManager.Instance.GetObject<ItemObject>(xmlNode.Value);
			_item = new EquipmentElement(val, (ItemModifier)null, (ItemObject)null, false);
		}
		XmlNode xmlNode2 = node?.Attributes?["slot"];
		if (xmlNode2 != null)
		{
			_index = Equipment.GetEquipmentIndexFromOldEquipmentIndexName(xmlNode2.Value);
		}
	}

	public override List<(EquipmentIndex, EquipmentElement)> GetAlternativeEquipments(bool isPlayer, List<(EquipmentIndex, EquipmentElement)> alternativeEquipments, bool getAll)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Invalid comparison between Unknown and I4
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Invalid comparison between Unknown and I4
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Invalid comparison between Unknown and I4
		if ((int)((MPOnSpawnPerkEffectBase)this).EffectTarget == 2 || (isPlayer ? ((int)((MPOnSpawnPerkEffectBase)this).EffectTarget == 0) : ((int)((MPOnSpawnPerkEffectBase)this).EffectTarget == 1)))
		{
			if (alternativeEquipments == null)
			{
				alternativeEquipments = new List<(EquipmentIndex, EquipmentElement)> { (_index, _item) };
			}
			else
			{
				alternativeEquipments.Add((_index, _item));
			}
		}
		return alternativeEquipments;
	}
}
