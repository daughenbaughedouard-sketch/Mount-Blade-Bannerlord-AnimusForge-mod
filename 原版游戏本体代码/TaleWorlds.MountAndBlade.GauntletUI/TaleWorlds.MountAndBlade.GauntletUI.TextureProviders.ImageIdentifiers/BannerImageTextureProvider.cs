using TaleWorlds.Core;
using TaleWorlds.GauntletUI;
using TaleWorlds.MountAndBlade.View.Tableaus;
using TaleWorlds.MountAndBlade.View.Tableaus.Thumbnails;

namespace TaleWorlds.MountAndBlade.GauntletUI.TextureProviders.ImageIdentifiers;

public class BannerImageTextureProvider : ImageIdentifierTextureProvider
{
	protected override void OnCreateImageWithId(string id, string additionalArgs)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Expected O, but got Unknown
		if (string.IsNullOrEmpty(id))
		{
			OnTextureCreated(null);
			return;
		}
		Banner banner = new Banner(id);
		BannerDebugInfo debugInfo = BannerDebugInfo.CreateWidget(((TextureProvider)this).SourceInfo ?? ((object)this).GetType().Name);
		if (additionalArgs == "ninegrid")
		{
			base.ThumbnailCreationData = new BannerThumbnailCreationData(banner, base.OnTextureCreated, base.OnTextureCreationCancelled, debugInfo, isTableauOrNineGrid: true, isLarge: true);
		}
		else
		{
			base.ThumbnailCreationData = new BannerThumbnailCreationData(banner, base.OnTextureCreated, base.OnTextureCreationCancelled, debugInfo, isTableauOrNineGrid: false, isLarge: false);
		}
		ThumbnailCacheManager.Current.CreateTexture(base.ThumbnailCreationData);
	}
}
