using System;
using System.Collections.Generic;
using SandBox.View.Map;
using SandBox.View.Map.Visuals;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace SandBox.View;

public class SandBoxViewVisualManager
{
	private EntitySystem<CampaignEntityVisualComponent> _components;

	private static readonly Comparison<CampaignEntityVisualComponent> _comparisonDelegate = (CampaignEntityVisualComponent x, CampaignEntityVisualComponent y) => x.Priority.CompareTo(y.Priority);

	public SandBoxViewVisualManager()
	{
		_components = new EntitySystem<CampaignEntityVisualComponent>();
	}

	public static void VisualTick(MapScreen screen, float realDt, float dt)
	{
		foreach (CampaignEntityVisualComponent item in (List<CampaignEntityVisualComponent>)(object)SandBoxViewSubModule.SandBoxViewVisualManager.GetComponents())
		{
			item.OnVisualTick(screen, realDt, dt);
		}
	}

	public static void OnTick(float realDt, float dt)
	{
		foreach (CampaignEntityVisualComponent item in (List<CampaignEntityVisualComponent>)(object)SandBoxViewSubModule.SandBoxViewVisualManager.GetComponents())
		{
			item.OnTick(realDt, dt);
		}
	}

	public static void ClearVisualMemory()
	{
		foreach (CampaignEntityVisualComponent item in (List<CampaignEntityVisualComponent>)(object)SandBoxViewSubModule.SandBoxViewVisualManager.GetComponents())
		{
			item.ClearVisualMemory();
		}
	}

	public static void OnFrameTick(float dt)
	{
		foreach (CampaignEntityVisualComponent item in (List<CampaignEntityVisualComponent>)(object)SandBoxViewSubModule.SandBoxViewVisualManager.GetComponents())
		{
			item.OnFrameTick(dt);
		}
	}

	public static bool OnMouseClick(MapEntityVisual visualOfSelectedEntity, Vec3 intersectionPoint, PathFaceRecord mouseOverFaceIndex, bool isDoubleClick)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		bool flag = false;
		foreach (CampaignEntityVisualComponent item in (List<CampaignEntityVisualComponent>)(object)SandBoxViewSubModule.SandBoxViewVisualManager.GetComponents())
		{
			flag |= item.OnMouseClick(visualOfSelectedEntity, intersectionPoint, mouseOverFaceIndex, isDoubleClick);
		}
		return flag;
	}

	public static void OnGameLoadFinished()
	{
		foreach (CampaignEntityVisualComponent item in (List<CampaignEntityVisualComponent>)(object)SandBoxViewSubModule.SandBoxViewVisualManager.GetComponents())
		{
			item.OnGameLoadFinished();
		}
	}

	public TComponent GetEntityComponent<TComponent>() where TComponent : CampaignEntityVisualComponent
	{
		EntitySystem<CampaignEntityVisualComponent> components = _components;
		if (components == null)
		{
			return null;
		}
		return components.GetComponent<TComponent>();
	}

	public TComponent AddEntityComponent<TComponent>() where TComponent : CampaignEntityVisualComponent, new()
	{
		TComponent result = _components.AddComponent<TComponent>();
		SortComponents();
		return result;
	}

	public void RemoveEntityComponent<TComponent>() where TComponent : CampaignEntityVisualComponent
	{
		_components.RemoveComponent<TComponent>();
	}

	public void Finalize<TComponent>(TComponent component) where TComponent : CampaignEntityVisualComponent
	{
		_components.Finalize((CampaignEntityVisualComponent)component);
	}

	public void RemoveEntityComponent<TComponent>(TComponent component) where TComponent : CampaignEntityVisualComponent
	{
		_components.RemoveComponent((CampaignEntityVisualComponent)component);
	}

	public List<TComponent> GetComponents<TComponent>() where TComponent : CampaignEntityVisualComponent
	{
		return _components.GetComponents<TComponent>();
	}

	public MBList<CampaignEntityVisualComponent> GetComponents()
	{
		return _components.GetComponents();
	}

	private void SortComponents()
	{
		_components.SortComponents<CampaignEntityVisualComponent>(_comparisonDelegate);
	}
}
