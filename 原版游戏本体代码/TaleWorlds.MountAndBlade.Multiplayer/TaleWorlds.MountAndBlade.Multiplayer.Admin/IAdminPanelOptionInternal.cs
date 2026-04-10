using System;

namespace TaleWorlds.MountAndBlade.Multiplayer.Admin;

internal interface IAdminPanelOptionInternal
{
	OptionType GetOptionType();

	MultiplayerOptionsAccessMode GetOptionAccessMode();

	void OnApplyChanges();

	void AddValueChangedCallback(Action callback);

	void RemoveValueChangedCallback(Action callback);

	void OnFinalize();
}
internal interface IAdminPanelOptionInternal<T> : IAdminPanelOptionInternal, IAdminPanelOption<T>, IAdminPanelOption
{
}
