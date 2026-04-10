using SandBox.BoardGames.Objects;
using TaleWorlds.Engine;
using TaleWorlds.Library;

namespace SandBox.BoardGames.Tiles;

public class TilePuluc : Tile1D
{
	public Vec3 PosLeft { get; private set; }

	public Vec3 PosLeftMid { get; private set; }

	public Vec3 PosRight { get; private set; }

	public Vec3 PosRightMid { get; private set; }

	public TilePuluc(GameEntity entity, BoardGameDecal decal, int x)
		: base(entity, decal, x)
	{
		UpdateTilePosition();
	}

	public void UpdateTilePosition()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		MatrixFrame globalFrame = base.Entity.GetGlobalFrame();
		MetaMesh tileMesh = base.Entity.GetFirstScriptOfType<Tile>().TileMesh;
		Vec3 val = tileMesh.GetBoundingBox().max - tileMesh.GetBoundingBox().min;
		ref Mat3 rotation = ref globalFrame.rotation;
		MatrixFrame frame = tileMesh.Frame;
		Mat3 val2 = ((Mat3)(ref rotation)).TransformToParent(ref frame.rotation);
		Vec3 val3 = new Vec3(0f, val.y / 6f, 0f, -1f);
		Vec3 val4 = ((Mat3)(ref val2)).TransformToParent(ref val3);
		val3 = new Vec3(0f, val.y / 3f, 0f, -1f);
		Vec3 val5 = ((Mat3)(ref val2)).TransformToParent(ref val3);
		Vec3 globalPosition = base.Entity.GlobalPosition;
		PosLeft = globalPosition + val5;
		PosLeftMid = globalPosition + val4;
		PosRight = globalPosition - val5;
		PosRightMid = globalPosition - val4;
	}
}
