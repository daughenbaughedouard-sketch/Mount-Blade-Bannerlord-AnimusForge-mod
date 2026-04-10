using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Avatar.PlayerServices;
using TaleWorlds.MountAndBlade.View.Tableaus;
using TaleWorlds.MountAndBlade.View.Tableaus.Thumbnails;
using TaleWorlds.PlayerServices;
using TaleWorlds.PlayerServices.Avatar;

namespace TaleWorlds.MountAndBlade.GauntletUI.TextureProviders.ImageIdentifiers;

public class PlayerAvatarImageTextureProvider : ImageIdentifierTextureProvider
{
	private const float AvatarFallbackWaitTime = 1f;

	private const float AvatarFailWaitTime = 5f;

	private AvatarData _receivedAvatarData;

	private bool _isUsingFallbackAvatar;

	private float _timeSinceAvatarFail;

	public PlayerAvatarImageTextureProvider()
	{
		_timeSinceAvatarFail = 5f;
	}

	public override void Tick(float dt)
	{
		_timeSinceAvatarFail += dt;
		base.Tick(dt);
	}

	protected override void OnCreateImageWithId(string id, string additionalArgs)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		PlayerId playerId = PlayerId.FromString(id);
		int num = -1;
		if (!string.IsNullOrEmpty(additionalArgs) && int.TryParse(additionalArgs, out var result))
		{
			num = result;
		}
		else
		{
			NetworkCommunicator? obj = ((IEnumerable<NetworkCommunicator>)GameNetwork.NetworkPeers).FirstOrDefault((Func<NetworkCommunicator, bool>)((NetworkCommunicator np) => np.VirtualPlayer.Id == playerId));
			num = ((obj != null) ? obj.ForcedAvatarIndex : (-1));
		}
		AvatarDataResponse playerAvatar = AvatarServices.GetPlayerAvatar(playerId, num);
		if (playerAvatar != null)
		{
			_receivedAvatarData = playerAvatar.AvatarData;
			_isUsingFallbackAvatar = playerAvatar.IsFallBack;
			if (_isUsingFallbackAvatar)
			{
				_timeSinceAvatarFail = 0f;
			}
		}
		else
		{
			_timeSinceAvatarFail = 0f;
		}
	}

	protected override bool GetCanForceCheckTexture()
	{
		return TextureNeedsRefresh();
	}

	protected override void OnCheckTexture()
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Invalid comparison between Unknown and I4
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Invalid comparison between Unknown and I4
		if (_receivedAvatarData != null)
		{
			if ((int)_receivedAvatarData.Status == 1)
			{
				AvatarData receivedAvatarData = _receivedAvatarData;
				_receivedAvatarData = null;
				OnAvatarLoaded(base.ImageId + "." + base.AdditionalArgs, receivedAvatarData);
			}
			else if ((int)_receivedAvatarData.Status == 2)
			{
				_receivedAvatarData = null;
				OnTextureCreated(null);
				ForceRefreshTextures();
				_timeSinceAvatarFail = 0f;
			}
		}
		else if (_isUsingFallbackAvatar && _timeSinceAvatarFail > 1f)
		{
			CreateImageWithId(base.ImageId, base.AdditionalArgs);
		}
		else if (_timeSinceAvatarFail > 5f)
		{
			CreateImageWithId(base.ImageId, base.AdditionalArgs);
		}
	}

	private void OnAvatarLoaded(string avatarID, AvatarData avatarData)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		if (avatarData != null)
		{
			AvatarThumbnailCreationData thumbnailCreationData = new AvatarThumbnailCreationData(avatarID, avatarData.Image, avatarData.Width, avatarData.Height, avatarData.Type);
			OnTextureCreated(ThumbnailCacheManager.Current.CreateTexture(thumbnailCreationData).Texture);
		}
	}

	private bool TextureNeedsRefresh()
	{
		if (_receivedAvatarData != null || (_isUsingFallbackAvatar && _timeSinceAvatarFail > 1f) || _timeSinceAvatarFail > 5f)
		{
			return true;
		}
		return false;
	}
}
