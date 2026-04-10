using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using psai.net;

namespace SandBox.View;

public class CampaignMusicHandler : IMusicHandler
{
	private const float MinRestDurationInSeconds = 30f;

	private const float MaxRestDurationInSeconds = 120f;

	private float _restTimer;

	bool IMusicHandler.IsPausable => false;

	private CampaignMusicHandler()
	{
	}

	public static void Create()
	{
		CampaignMusicHandler campaignMusicHandler = new CampaignMusicHandler();
		MBMusicManager.Current.OnCampaignMusicHandlerInit((IMusicHandler)(object)campaignMusicHandler);
	}

	void IMusicHandler.OnUpdated(float dt)
	{
		CheckMusicMode();
		TickCampaignMusic(dt);
	}

	private void CheckMusicMode()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		if ((int)MBMusicManager.Current.CurrentMode == 0)
		{
			MBMusicManager.Current.ActivateCampaignMode();
		}
	}

	private void TickCampaignMusic(float dt)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Invalid comparison between Unknown and I4
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		PsaiInfo psaiInfo = PsaiCore.Instance.GetPsaiInfo();
		bool flag = (int)((PsaiInfo)(ref psaiInfo)).psaiState == 2;
		if (_restTimer <= 0f)
		{
			_restTimer += dt;
			if (_restTimer > 0f)
			{
				MBMusicManager.Current.StartThemeWithConstantIntensity(MBMusicManager.Current.GetCampaignMusicTheme((BasicCultureObject)(object)GetNearbyCulture(), GetMoodOfMainParty() < MusicParameters.CampaignDarkModeThreshold, IsPlayerInAnArmy(), GetIsMainPartyAtSea()), false);
				Debug.Print("Campaign music play started.", 0, (DebugColor)9, 64uL);
			}
		}
		else if (!flag)
		{
			MBMusicManager.Current.ForceStopThemeWithFadeOut();
			_restTimer = 0f - (30f + MBRandom.RandomFloat * 90f);
			Debug.Print("Campaign music rest started.", 0, (DebugColor)9, 64uL);
		}
	}

	private CultureObject GetNearbyCulture()
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		CultureObject result = null;
		float num = float.MaxValue;
		foreach (Settlement item in (List<Settlement>)(object)Campaign.Current.Settlements)
		{
			if (item.IsTown || item.IsVillage)
			{
				CampaignVec2 position = item.Position;
				float num2 = ((CampaignVec2)(ref position)).DistanceSquared(MobileParty.MainParty.Position);
				if (item.IsVillage)
				{
					num2 *= 1.05f;
				}
				if (num > num2)
				{
					result = item.Culture;
					num = num2;
				}
			}
		}
		return result;
	}

	private bool IsPlayerInAnArmy()
	{
		return MobileParty.MainParty.Army != null;
	}

	private float GetMoodOfMainParty()
	{
		return MathF.Clamp(MobileParty.MainParty.Morale / 100f, 0f, 1f);
	}

	private bool GetIsMainPartyAtSea()
	{
		return MobileParty.MainParty.IsCurrentlyAtSea;
	}
}
