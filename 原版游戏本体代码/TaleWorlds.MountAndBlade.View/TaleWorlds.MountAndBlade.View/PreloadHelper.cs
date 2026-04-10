using System.Collections.Generic;
using System.Threading;
using TaleWorlds.Core;
using TaleWorlds.DotNet;
using TaleWorlds.Engine;
using TaleWorlds.Library;

namespace TaleWorlds.MountAndBlade.View;

public class PreloadHelper
{
	private readonly HashSet<(string, bool, bool)> _uniqueMetaMeshNames = new HashSet<(string, bool, bool)>();

	private readonly HashSet<string> _uniqueDynamicPhysicsShapeName = new HashSet<string>();

	private readonly HashSet<(MetaMesh, bool, bool)> _uniqueMetaMeshes = new HashSet<(MetaMesh, bool, bool)>();

	private readonly HashSet<ItemObject> _loadedItems = new HashSet<ItemObject>();

	public void PreloadCharacters(List<BasicCharacterObject> characters)
	{
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		Utilities.EnableGlobalEditDataCacher();
		foreach (BasicCharacterObject character in characters)
		{
			foreach (Equipment battleEquipment in character.BattleEquipments)
			{
				AddEquipment(battleEquipment);
			}
			if (Mission.Current != null && Mission.Current.DoesMissionRequireCivilianEquipment)
			{
				foreach (Equipment civilianEquipment in character.CivilianEquipments)
				{
					AddEquipment(civilianEquipment);
				}
			}
			if (Mission.Current == null)
			{
				continue;
			}
			List<EquipmentElement> extraEquipmentElementsForCharacter = Mission.Current.GetExtraEquipmentElementsForCharacter(character, true);
			if (extraEquipmentElementsForCharacter != null)
			{
				int count = extraEquipmentElementsForCharacter.Count;
				for (int i = 0; i < count; i++)
				{
					EquipmentElement val = extraEquipmentElementsForCharacter[i];
					ItemObject item = ((EquipmentElement)(ref val)).Item;
					AddItemObject(item);
				}
			}
		}
		Utilities.DisableGlobalEditDataCacher();
		PreloadMeshesAndPhysics();
	}

	public void WaitForMeshesToBeLoaded()
	{
		int num;
		do
		{
			num = 0;
			foreach (var uniqueMetaMesh in _uniqueMetaMeshes)
			{
				num += uniqueMetaMesh.Item1.CheckResources();
			}
			foreach (string item in _uniqueDynamicPhysicsShapeName)
			{
				num += (((NativeObject)(object)PhysicsShape.GetFromResource(item, true) == (NativeObject)null) ? 1 : 0);
			}
			Thread.Sleep(1);
		}
		while (num != 0);
	}

	public void PreloadEquipments(List<Equipment> equipments)
	{
		foreach (Equipment equipment in equipments)
		{
			AddEquipment(equipment);
		}
		PreloadMeshesAndPhysics();
	}

	public void PreloadItems(List<ItemObject> items)
	{
		Utilities.EnableGlobalEditDataCacher();
		for (int i = 0; i < items.Count; i++)
		{
			AddItemObject(items[i]);
		}
		Utilities.DisableGlobalEditDataCacher();
		PreloadMeshesAndPhysics();
	}

	private void AddEquipment(Equipment equipment)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < 12; i++)
		{
			EquipmentElement equipmentFromSlot = equipment.GetEquipmentFromSlot((EquipmentIndex)i);
			ItemObject item = ((EquipmentElement)(ref equipmentFromSlot)).Item;
			if (item != null)
			{
				AddItemObject(item);
			}
		}
	}

	private void AddItemObject(ItemObject item)
	{
		if (item == null)
		{
			return;
		}
		bool isUsingTableau = item.IsUsingTableau;
		bool isUsingTeamColor = item.IsUsingTeamColor;
		RegisterMetaMeshUsageIfValid(item.MultiMeshName, isUsingTableau, isUsingTeamColor);
		RegisterMetaMeshUsageIfValid(item.HolsterMeshName, isUsingTableau, isUsingTeamColor);
		if (item.WeaponComponent != null)
		{
			if (item.IsCraftedWeapon)
			{
				RegisterMetaMeshUsageIfValid(item.GetHolsterWithWeaponMeshIfExists(), isUsingTableau, isUsingTeamColor);
				RegisterMetaMeshUsageIfValid(item.GetHolsterMeshIfExists(), isUsingTableau, isUsingTeamColor);
				RegisterMetaMeshUsageIfValid(item.GetFlyingMeshIfExists(), isUsingTableau, isUsingTeamColor);
			}
			else
			{
				RegisterMetaMeshUsageIfValid(item.HolsterWithWeaponMeshName, isUsingTableau, isUsingTeamColor);
				RegisterMetaMeshUsageIfValid(item.HolsterMeshName, isUsingTableau, isUsingTeamColor);
				RegisterMetaMeshUsageIfValid(item.FlyingMeshName, isUsingTableau, isUsingTeamColor);
			}
		}
		else if (item.HasHorseComponent)
		{
			foreach (KeyValuePair<string, bool> additionalMeshesName in item.HorseComponent.AdditionalMeshesNameList)
			{
				RegisterMetaMeshUsageIfValid(additionalMeshesName.Key, isUsingTableau, isUsingTeamColor);
			}
		}
		else if (item.HasArmorComponent && !string.IsNullOrEmpty(item.ArmorComponent.ReinsMesh))
		{
			RegisterMetaMeshUsageIfValid(item.ArmorComponent.ReinsMesh, isUsingTableau, isUsingTeamColor);
			RegisterMetaMeshUsageIfValid(item.ArmorComponent.ReinsRopeMesh, isUsingTableau, isUsingTeamColor);
		}
		if (!_loadedItems.Contains(item))
		{
			RegisterMetaMeshUsageIfValid(item.GetMultiMesh(isFemale: false, useSlimVersion: false, needBatchedVersion: true), isUsingTableau, isUsingTeamColor);
			if (item.HasArmorComponent)
			{
				RegisterMetaMeshUsageIfValid(item.GetMultiMesh(isFemale: false, useSlimVersion: true, needBatchedVersion: true), isUsingTableau, isUsingTeamColor);
				RegisterMetaMeshUsageIfValid(item.GetMultiMesh(isFemale: true, useSlimVersion: false, needBatchedVersion: true), isUsingTableau, isUsingTeamColor);
				RegisterMetaMeshUsageIfValid(item.GetMultiMesh(isFemale: true, useSlimVersion: true, needBatchedVersion: true), isUsingTableau, isUsingTeamColor);
			}
			_loadedItems.Add(item);
		}
		RegisterPhysicsBodyUsageIfValid(_uniqueDynamicPhysicsShapeName, item.CollisionBodyName);
		RegisterPhysicsBodyUsageIfValid(_uniqueDynamicPhysicsShapeName, item.BodyName);
		RegisterPhysicsBodyUsageIfValid(_uniqueDynamicPhysicsShapeName, item.HolsterBodyName);
	}

	public void PreloadEntities(List<WeakGameEntity> entities)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < entities.Count; i++)
		{
			WeakGameEntity val = entities[i];
			for (int j = 0; j < ((WeakGameEntity)(ref val)).MultiMeshComponentCount; j++)
			{
				MetaMesh metaMesh = ((WeakGameEntity)(ref val)).GetMetaMesh(j);
				RegisterMetaMeshUsageIfValid(metaMesh, useTableau: false, useTeamColor: false);
			}
			PhysicsShape bodyShape = GameEntityPhysicsExtensions.GetBodyShape(val);
			if ((NativeObject)(object)bodyShape != (NativeObject)null)
			{
				RegisterPhysicsBodyUsageIfValid(_uniqueDynamicPhysicsShapeName, bodyShape.GetName());
			}
		}
	}

	public void PreloadMeshesAndPhysics()
	{
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		foreach (var uniqueMetaMesh in _uniqueMetaMeshes)
		{
			var (val, _, _) = uniqueMetaMesh;
			val.PreloadForRendering();
			val.PreloadShaders(uniqueMetaMesh.Item2, uniqueMetaMesh.Item3);
		}
		foreach (string item in _uniqueDynamicPhysicsShapeName)
		{
			MBDebug.Print("Preload physics: " + item, 0, (DebugColor)12, 17592186044416uL);
			PhysicsShape.AddPreloadQueueWithName(item, new Vec3(1f, 1f, 1f, -1f));
		}
		PhysicsShape.ProcessPreloadQueue();
	}

	public void Clear()
	{
		_uniqueMetaMeshNames.Clear();
		_uniqueDynamicPhysicsShapeName.Clear();
		_uniqueMetaMeshes.Clear();
		PhysicsShape.UnloadDynamicBodies();
	}

	private void RegisterMetaMeshUsageIfValid(string metaMeshName, bool useTableau, bool useTeamColor)
	{
		(string, bool, bool) item = (metaMeshName, useTableau, useTeamColor);
		if (!string.IsNullOrEmpty(metaMeshName) && !_uniqueMetaMeshNames.Contains(item))
		{
			RegisterMetaMeshUsageIfValid(MetaMesh.GetCopy(metaMeshName, false, true), useTableau, useTeamColor);
			_uniqueMetaMeshNames.Add(item);
		}
	}

	private void RegisterMetaMeshUsageIfValid(MetaMesh metaMesh, bool useTableau, bool useTeamColor)
	{
		if ((NativeObject)(object)metaMesh != (NativeObject)null)
		{
			(MetaMesh, bool, bool) item = (metaMesh, useTableau, useTeamColor);
			_uniqueMetaMeshes.Add(item);
		}
	}

	private void RegisterPhysicsBodyUsageIfValid(HashSet<string> uniquePhysicsShapeName, string physicsShape)
	{
		if (!string.IsNullOrWhiteSpace(physicsShape))
		{
			uniquePhysicsShapeName.Add(physicsShape);
		}
	}
}
