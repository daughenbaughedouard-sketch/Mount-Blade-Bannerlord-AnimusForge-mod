using System;
using System.Collections.Generic;
using System.Linq;
using SandBox.Conversation.MissionLogics;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TaleWorlds.MountAndBlade.View.Tableaus.Thumbnails;

namespace SandBox.View.Missions;

public class MissionConversationPrepareView : MissionView
{
	public const string BannerTagId = "banner_with_faction_color";

	private ConversationMissionLogic _conversationMissionLogic;

	public override void OnBehaviorInitialize()
	{
		((MissionBehavior)this).OnBehaviorInitialize();
		_conversationMissionLogic = ((MissionBehavior)this).Mission.GetMissionBehavior<ConversationMissionLogic>();
	}

	public override void AfterStart()
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		((MissionBehavior)this).AfterStart();
		if (_conversationMissionLogic == null)
		{
			return;
		}
		GameEntity val = ((MissionBehavior)this).Mission.Scene.FindEntityWithTag("banner_with_faction_color");
		if (!(val != (GameEntity)null))
		{
			return;
		}
		if (((BasicCharacterObject)_conversationMissionLogic.OtherSideConversationData.Character).IsHero)
		{
			PartyBase party = _conversationMissionLogic.OtherSideConversationData.Party;
			object obj = ((party != null) ? party.Banner : null);
			if (obj == null)
			{
				PartyBase party2 = _conversationMissionLogic.PlayerConversationData.Party;
				obj = ((party2 != null) ? party2.Banner : null);
			}
			Banner val2 = (Banner)obj;
			if (val2 != null)
			{
				SetOwnerBanner(val, val2);
			}
		}
		else
		{
			val.Remove(112);
		}
	}

	private void SetOwnerBanner(GameEntity bannerEntity, Banner ownerBanner)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		BannerDebugInfo val = BannerDebugInfo.CreateManual(((object)this).GetType().Name);
		BannerVisualExtensions.GetTableauTextureLarge(ownerBanner, ref val, (Action<Texture>)delegate(Texture tex)
		{
			OnTextureRendered(tex, bannerEntity);
		});
	}

	private void OnTextureRendered(Texture tex, GameEntity bannerEntity)
	{
		List<Mesh> list = bannerEntity.GetAllMeshesWithTag("banner_with_faction_color").ToList();
		if (Extensions.IsEmpty<Mesh>((IEnumerable<Mesh>)list))
		{
			list.Add(bannerEntity.GetFirstMesh());
		}
		foreach (Mesh item in list)
		{
			Material val = item.GetMaterial().CreateCopy();
			val.SetTexture((MBTextureType)1, tex);
			uint num = (uint)val.GetShader().GetMaterialShaderFlagMask("use_tableau_blending", true);
			ulong shaderFlags = val.GetShaderFlags();
			val.SetShaderFlags(shaderFlags | num);
			item.SetMaterial(val);
		}
	}
}
