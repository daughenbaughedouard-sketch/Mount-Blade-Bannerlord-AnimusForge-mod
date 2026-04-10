using TaleWorlds.PlayerServices.Avatar;

namespace TaleWorlds.MountAndBlade.View.Tableaus.Thumbnails;

public class AvatarThumbnailCreationData : ThumbnailCreationData
{
	public string AvatarID { get; private set; }

	public byte[] AvatarBytes { get; private set; }

	public uint Width { get; private set; }

	public uint Height { get; private set; }

	public ImageType ImageType { get; private set; }

	public AvatarThumbnailCreationData(string avatarID, byte[] avatarBytes, uint width, uint height, ImageType imageType)
		: base(avatarID, null, null)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		AvatarID = avatarID;
		AvatarBytes = avatarBytes;
		Width = width;
		Height = height;
		ImageType = imageType;
	}
}
