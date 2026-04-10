using System;
using System.Collections.Generic;
using System.Xml;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.ObjectSystem;

namespace SandBox.Objects;

public class InstrumentData : MBObjectBase
{
	private MBList<(HumanBone, string)> _instrumentEntities;

	public MBReadOnlyList<(HumanBone, string)> InstrumentEntities => (MBReadOnlyList<(HumanBone, string)>)(object)_instrumentEntities;

	public string SittingAction { get; private set; }

	public string StandingAction { get; private set; }

	public string Tag { get; private set; }

	public bool IsDataWithoutInstrument { get; private set; }

	public InstrumentData()
	{
	}

	public InstrumentData(string stringId)
		: base(stringId)
	{
	}

	public void InitializeInstrumentData(string sittingAction, string standingAction, bool isDataWithoutInstrument)
	{
		SittingAction = sittingAction;
		StandingAction = standingAction;
		_instrumentEntities = new MBList<(HumanBone, string)>(0);
		IsDataWithoutInstrument = isDataWithoutInstrument;
		Tag = string.Empty;
	}

	public override void Deserialize(MBObjectManager objectManager, XmlNode node)
	{
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		((MBObjectBase)this).Deserialize(objectManager, node);
		SittingAction = Convert.ToString(node.Attributes["sittingAction"].Value);
		StandingAction = Convert.ToString(node.Attributes["standingAction"].Value);
		Tag = Convert.ToString(node.Attributes["tag"]?.Value);
		_instrumentEntities = new MBList<(HumanBone, string)>();
		if (node.HasChildNodes)
		{
			foreach (XmlNode childNode in node.ChildNodes)
			{
				if (!(childNode.Name == "Entities"))
				{
					continue;
				}
				foreach (XmlNode childNode2 in childNode.ChildNodes)
				{
					if (!(childNode2.Name == "Entity"))
					{
						continue;
					}
					if (childNode2.Attributes?["name"] != null && childNode2.Attributes["bone"] != null)
					{
						string item = Convert.ToString(childNode2.Attributes["name"].Value);
						if (Enum.TryParse<HumanBone>(childNode2.Attributes["bone"].Value, out HumanBone result))
						{
							((List<(HumanBone, string)>)(object)_instrumentEntities).Add((result, item));
						}
						else
						{
							Debug.FailedAssert("Couldn't parse bone xml node for instrument.", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\SandBox\\Objects\\InstrumentData.cs", "Deserialize", 62);
						}
					}
					else
					{
						Debug.FailedAssert("Couldn't find required attributes of entity xml node in Instrument", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\SandBox\\Objects\\InstrumentData.cs", "Deserialize", 67);
					}
				}
			}
		}
		((List<(HumanBone, string)>)(object)_instrumentEntities).Capacity = ((List<(HumanBone, string)>)(object)_instrumentEntities).Count;
	}
}
