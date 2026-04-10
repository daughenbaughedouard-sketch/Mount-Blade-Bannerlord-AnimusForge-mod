using TaleWorlds.Core;

namespace TaleWorlds.MountAndBlade.View.Scripts;

public class PopupSceneSwitchItemSequence : PopupSceneSequence
{
	public enum BodyPartIndex
	{
		None,
		Weapon0,
		Weapon1,
		Weapon2,
		Weapon3,
		ExtraWeaponSlot,
		Head,
		Body,
		Leg,
		Gloves,
		Cape,
		Horse,
		HorseHarness
	}

	public string InitialItem;

	public string PositiveItem;

	public string NegativeItem;

	public BodyPartIndex InitialBodyPart;

	public BodyPartIndex PositiveBodyPart;

	public BodyPartIndex NegativeBodyPart;

	public override void OnInitialState()
	{
		AttachItem(InitialItem, InitialBodyPart);
	}

	public override void OnPositiveState()
	{
		AttachItem(PositiveItem, PositiveBodyPart);
	}

	public override void OnNegativeState()
	{
		AttachItem(NegativeItem, NegativeBodyPart);
	}

	private EquipmentIndex StringToEquipmentIndex(BodyPartIndex part)
	{
		return (EquipmentIndex)(part switch
		{
			BodyPartIndex.None => -1, 
			BodyPartIndex.Weapon0 => 0, 
			BodyPartIndex.Weapon1 => 1, 
			BodyPartIndex.Weapon2 => 2, 
			BodyPartIndex.Weapon3 => 3, 
			BodyPartIndex.ExtraWeaponSlot => 4, 
			BodyPartIndex.Head => 5, 
			BodyPartIndex.Body => 6, 
			BodyPartIndex.Leg => 7, 
			BodyPartIndex.Gloves => 8, 
			BodyPartIndex.Cape => 9, 
			BodyPartIndex.Horse => 10, 
			BodyPartIndex.HorseHarness => 11, 
			_ => -1, 
		});
	}

	private void AttachItem(string itemName, BodyPartIndex bodyPart)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Invalid comparison between Unknown and I4
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		if (_agentVisuals == null)
		{
			return;
		}
		EquipmentIndex val = StringToEquipmentIndex(bodyPart);
		if ((int)val != -1)
		{
			AgentVisualsData copyAgentVisualsData = _agentVisuals.GetCopyAgentVisualsData();
			Equipment val2 = _agentVisuals.GetEquipment().Clone(false);
			if (itemName == "")
			{
				val2.AddEquipmentToSlotWithoutAgent(val, default(EquipmentElement));
			}
			else
			{
				val2.AddEquipmentToSlotWithoutAgent(val, new EquipmentElement(Game.Current.ObjectManager.GetObject<ItemObject>(itemName), (ItemModifier)null, (ItemObject)null, false));
			}
			copyAgentVisualsData.RightWieldedItemIndex(0).LeftWieldedItemIndex(-1).Equipment(val2);
			_agentVisuals.Refresh(needBatchedVersionForWeaponMeshes: false, copyAgentVisualsData);
		}
	}
}
