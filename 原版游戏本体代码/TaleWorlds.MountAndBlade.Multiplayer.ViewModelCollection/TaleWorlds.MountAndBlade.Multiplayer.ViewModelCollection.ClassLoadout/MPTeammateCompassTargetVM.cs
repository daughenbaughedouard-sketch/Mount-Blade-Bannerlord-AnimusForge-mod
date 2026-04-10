using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.ImageIdentifiers;
using TaleWorlds.MountAndBlade.ViewModelCollection.HUD.Compass;

namespace TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.ClassLoadout;

public class MPTeammateCompassTargetVM : CompassTargetVM
{
	public unsafe MPTeammateCompassTargetVM(TargetIconType iconType, uint color, uint color2, Banner banner, bool isAlly)
		: base(iconType, color, color2, banner, false, isAlly)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Expected O, but got Unknown
		((CompassTargetVM)this).IconType = ((object)(*(TargetIconType*)(&iconType))/*cast due to .constrained prefix*/).ToString();
		((CompassTargetVM)this).IsFlag = false;
		((CompassTargetVM)this).Banner = ((banner != null) ? new BannerImageIdentifierVM(banner, false) : new BannerImageIdentifierVM((Banner)null, false));
	}

	public unsafe void RefreshTargetIconType(TargetIconType targetIconType)
	{
		((CompassTargetVM)this).IconType = ((object)(*(TargetIconType*)(&targetIconType))/*cast due to .constrained prefix*/).ToString();
	}

	public void RefreshTeam(Banner banner, bool isAlly)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Expected O, but got Unknown
		((CompassTargetVM)this).Banner = ((banner != null) ? new BannerImageIdentifierVM(banner, false) : new BannerImageIdentifierVM((Banner)null, false));
		((CompassTargetVM)this).IsEnemy = !isAlly;
	}
}
