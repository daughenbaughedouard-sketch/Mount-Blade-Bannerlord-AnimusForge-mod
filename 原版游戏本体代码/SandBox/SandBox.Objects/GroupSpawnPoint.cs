using SandBox.Objects.Usables;
using TaleWorlds.Library;

namespace SandBox.Objects;

public class GroupSpawnPoint : UsablePlace
{
	public float Delay = -1f;

	public int SpawnCount = 1;

	public bool IsInstant
	{
		get
		{
			if (!(Delay < 0f))
			{
				return MBMath.ApproximatelyEqualsTo(Delay, 0f, 1E-05f);
			}
			return true;
		}
	}
}
