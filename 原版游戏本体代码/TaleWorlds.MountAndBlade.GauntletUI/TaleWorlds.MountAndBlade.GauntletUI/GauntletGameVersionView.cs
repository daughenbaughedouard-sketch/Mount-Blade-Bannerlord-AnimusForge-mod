using System;
using System.Collections.Generic;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.ViewModelCollection;
using TaleWorlds.ScreenSystem;

namespace TaleWorlds.MountAndBlade.GauntletUI;

public class GauntletGameVersionView : GlobalLayer
{
	private static readonly List<Tuple<string, string>> _versionTexts = new List<Tuple<string, string>>();

	private GameVersionVM _dataSource;

	private bool _isEnabled = true;

	public static GauntletGameVersionView Current { get; private set; }

	public GauntletGameVersionView()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Expected O, but got Unknown
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Expected O, but got Unknown
		_dataSource = new GameVersionVM((Func<List<string>>)CollectAllVersionTexts);
		GauntletLayer val = new GauntletLayer("MainMenuGameVersion", 15001, false);
		val.LoadMovie("GameVersion", (ViewModel)(object)_dataSource);
		((GlobalLayer)this).Layer = (ScreenLayer)(object)val;
	}

	public static void Initialize()
	{
		if (Current == null)
		{
			Current = new GauntletGameVersionView();
			ScreenManager.AddGlobalLayer((GlobalLayer)(object)Current, false);
		}
	}

	public static void Refresh()
	{
		GauntletGameVersionView current = Current;
		if (current != null)
		{
			GameVersionVM dataSource = current._dataSource;
			if (dataSource != null)
			{
				((ViewModel)dataSource).RefreshValues();
			}
		}
	}

	public static void AddModuleVersionInfo(string title, string versionStr)
	{
		_versionTexts.Add(new Tuple<string, string>(title, versionStr));
	}

	public static void RemoveModuleVersionInfo(string title)
	{
		_versionTexts.RemoveAll((Tuple<string, string> x) => x.Item1 == title);
	}

	private static List<string> CollectAllVersionTexts()
	{
		List<string> list = new List<string>();
		foreach (Tuple<string, string> versionText in _versionTexts)
		{
			list.Add(versionText.Item1 + ": " + versionText.Item2);
		}
		return list;
	}

	protected override void OnTick(float dt)
	{
		((GlobalLayer)this).OnTick(dt);
		bool enabled = ScreenManager.TopScreen is GauntletInitialScreen || ScreenManager.TopScreen is GauntletOptionsScreen;
		SetEnabled(enabled);
	}

	private void SetEnabled(bool isEnabled)
	{
		if (_isEnabled != isEnabled)
		{
			_isEnabled = isEnabled;
			ScreenManager.SetSuspendLayer(((GlobalLayer)this).Layer, !_isEnabled);
			if (_isEnabled)
			{
				Refresh();
			}
		}
	}
}
