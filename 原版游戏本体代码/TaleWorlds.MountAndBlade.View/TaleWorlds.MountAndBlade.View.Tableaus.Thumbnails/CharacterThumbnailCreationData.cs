using System;
using TaleWorlds.Core;
using TaleWorlds.Engine;

namespace TaleWorlds.MountAndBlade.View.Tableaus.Thumbnails;

public class CharacterThumbnailCreationData : ThumbnailCreationData
{
	public CharacterCode CharacterCode { get; private set; }

	public bool IsBig { get; private set; }

	public int CustomSizeX { get; private set; }

	public int CustomSizeY { get; private set; }

	public CharacterThumbnailCreationData(CharacterCode characterCode, Action<Texture> setAction, Action cancelAction, bool isBig, int customSizeX = -1, int customSizeY = -1)
		: base("", setAction, cancelAction)
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		characterCode.BodyProperties = new BodyProperties(new DynamicBodyProperties((float)(int)((BodyProperties)(ref characterCode.BodyProperties)).Age, (float)(int)((BodyProperties)(ref characterCode.BodyProperties)).Weight, (float)(int)((BodyProperties)(ref characterCode.BodyProperties)).Build), ((BodyProperties)(ref characterCode.BodyProperties)).StaticProperties);
		base.RenderId = characterCode.CreateNewCodeString();
		base.RenderId += (isBig ? "1" : "0");
		if (customSizeX > 0)
		{
			base.RenderId += $"_x:{customSizeX}";
		}
		if (customSizeY > 0)
		{
			base.RenderId += $"_y:{customSizeY}";
		}
		CharacterCode = characterCode;
		IsBig = isBig;
		CustomSizeX = customSizeX;
		CustomSizeY = customSizeY;
	}
}
