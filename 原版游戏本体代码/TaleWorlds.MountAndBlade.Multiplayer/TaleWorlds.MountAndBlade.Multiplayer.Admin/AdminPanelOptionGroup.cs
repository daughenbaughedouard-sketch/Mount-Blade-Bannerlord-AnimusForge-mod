using System.Collections.Generic;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace TaleWorlds.MountAndBlade.Multiplayer.Admin;

internal class AdminPanelOptionGroup : IAdminPanelOptionGroup, IAdminPanelTickable
{
	private readonly bool _requiresRestart;

	private readonly string _uniqueId;

	private readonly TextObject _nameTextObj;

	private readonly MBList<IAdminPanelOption> _options;

	private readonly MBList<IAdminPanelAction> _actions;

	private readonly MBList<IAdminPanelTickable> _tickableOptions;

	string IAdminPanelOptionGroup.UniqueId => _uniqueId;

	TextObject IAdminPanelOptionGroup.Name => _nameTextObj;

	MBReadOnlyList<IAdminPanelOption> IAdminPanelOptionGroup.Options => (MBReadOnlyList<IAdminPanelOption>)(object)_options;

	MBReadOnlyList<IAdminPanelAction> IAdminPanelOptionGroup.Actions => (MBReadOnlyList<IAdminPanelAction>)(object)_actions;

	bool IAdminPanelOptionGroup.RequiresRestart => _requiresRestart;

	public AdminPanelOptionGroup(string uniqueId, TextObject name, bool requiresRestart = false)
	{
		_uniqueId = uniqueId;
		_nameTextObj = name;
		_requiresRestart = requiresRestart;
		_options = new MBList<IAdminPanelOption>();
		_actions = new MBList<IAdminPanelAction>();
		_tickableOptions = new MBList<IAdminPanelTickable>();
	}

	public void AddOption(IAdminPanelOption option)
	{
		((List<IAdminPanelOption>)(object)_options).Add(option);
		if (option is IAdminPanelTickable item)
		{
			((List<IAdminPanelTickable>)(object)_tickableOptions).Add(item);
		}
	}

	public void AddAction(IAdminPanelAction action)
	{
		((List<IAdminPanelAction>)(object)_actions).Add(action);
	}

	void IAdminPanelTickable.OnTick(float dt)
	{
		for (int i = 0; i < ((List<IAdminPanelTickable>)(object)_tickableOptions).Count; i++)
		{
			((List<IAdminPanelTickable>)(object)_tickableOptions)[i].OnTick(dt);
		}
	}

	void IAdminPanelOptionGroup.OnFinalize()
	{
		for (int i = 0; i < ((List<IAdminPanelOption>)(object)_options).Count; i++)
		{
			if (((List<IAdminPanelOption>)(object)_options)[i] is IAdminPanelOptionInternal adminPanelOptionInternal)
			{
				adminPanelOptionInternal.OnFinalize();
			}
		}
		for (int j = 0; j < ((List<IAdminPanelAction>)(object)_actions).Count; j++)
		{
			if (((List<IAdminPanelAction>)(object)_actions)[j] is IAdminPanelActionInternal adminPanelActionInternal)
			{
				adminPanelActionInternal.OnFinalize();
			}
		}
	}
}
