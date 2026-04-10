using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.ObjectSystem;

namespace TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.FlagMarker.Targets;

public class MissionSiegeEngineMarkerTargetVM : MissionMarkerTargetVM
{
	private readonly GameEntity _siegeEngine;

	public readonly BattleSideEnum Side;

	private string _siegeEngineID;

	public override Vec3 WorldPosition
	{
		get
		{
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			if (!(_siegeEngine != (GameEntity)null))
			{
				return Vec3.One;
			}
			return _siegeEngine.GlobalPosition;
		}
	}

	protected override float HeightOffset => 2.5f;

	[DataSourceProperty]
	public string SiegeEngineID
	{
		get
		{
			return _siegeEngineID;
		}
		set
		{
			if (value != _siegeEngineID)
			{
				_siegeEngineID = value;
				((ViewModel)this).OnPropertyChangedWithValue<string>(value, "SiegeEngineID");
			}
		}
	}

	public MissionSiegeEngineMarkerTargetVM(SiegeWeapon siegeEngine)
		: base(MissionMarkerType.SiegeEngine)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Invalid comparison between Unknown and I4
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Invalid comparison between Unknown and I4
		_siegeEngine = GameEntity.CreateFromWeakEntity(((ScriptComponentBehavior)siegeEngine).GameEntity);
		Side = siegeEngine.Side;
		SiegeEngineID = ((MBObjectBase)siegeEngine.GetSiegeEngineType()).StringId;
		RefreshColor(((int)Side == 1) ? Mission.Current.AttackerTeam.Color : Mission.Current.DefenderTeam.Color, ((int)Side == 1) ? Mission.Current.AttackerTeam.Color2 : Mission.Current.DefenderTeam.Color2);
	}
}
