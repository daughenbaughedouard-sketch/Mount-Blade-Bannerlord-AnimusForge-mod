using TaleWorlds.Core;
using TaleWorlds.MountAndBlade.View.Tableaus;
using TaleWorlds.MountAndBlade.View.Tableaus.Thumbnails;

namespace TaleWorlds.MountAndBlade.GauntletUI.TextureProviders.ImageIdentifiers;

public class CharacterImageTextureProvider : ImageIdentifierTextureProvider
{
	protected override void OnCreateImageWithId(string id, string additionalArgs)
	{
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Invalid comparison between Unknown and I4
		if (string.IsNullOrEmpty(id))
		{
			if (base.ThumbnailCreationData == null)
			{
				goto IL_0033;
			}
			if (base.ThumbnailCreationData is CharacterThumbnailCreationData characterThumbnailCreationData)
			{
				CharacterCode characterCode = characterThumbnailCreationData.CharacterCode;
				if (characterCode == null || characterCode.IsEmpty)
				{
					goto IL_0033;
				}
			}
		}
		CharacterCode val = CharacterCode.CreateFrom(id);
		if ((int)FaceGen.GetMaturityTypeWithAge(((BodyProperties)(ref val.BodyProperties)).Age) <= 1)
		{
			OnTextureCreated(null);
			return;
		}
		int customSizeX = -1;
		int customSizeY = -1;
		if (!string.IsNullOrEmpty(additionalArgs))
		{
			string[] array = additionalArgs.Split(new char[1] { ';' });
			for (int i = 0; i < array.Length; i++)
			{
				string[] array2 = array[i].Split(new char[1] { '=' });
				if (array2.Length != 2)
				{
					continue;
				}
				int result2;
				if (array2[0] == "customSizeX")
				{
					if (int.TryParse(array2[1], out var result))
					{
						customSizeX = result;
					}
				}
				else if (array2[0] == "customSizeY" && int.TryParse(array2[1], out result2))
				{
					customSizeY = result2;
				}
			}
		}
		base.ThumbnailCreationData = new CharacterThumbnailCreationData(val, base.OnTextureCreated, base.OnTextureCreationCancelled, base.IsBig, customSizeX, customSizeY);
		ThumbnailCacheManager.Current.CreateTexture(base.ThumbnailCreationData);
		return;
		IL_0033:
		OnTextureCreated(ThumbnailCacheManager.Current.GetCachedHeroSilhouetteTexture());
	}
}
