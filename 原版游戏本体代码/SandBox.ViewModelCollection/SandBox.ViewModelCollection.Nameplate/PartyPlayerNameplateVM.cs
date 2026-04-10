using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.Core.ViewModelCollection.ImageIdentifiers;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace SandBox.ViewModelCollection.Nameplate;

public class PartyPlayerNameplateVM : PartyNameplateVM
{
	private float _latestMainHeroAge = -1f;

	private bool _isPartyHeroVisualDirty;

	private Action _resetCamera;

	private CharacterImageIdentifierVM _mainHeroVisualBind;

	private bool _isPrisonerBind;

	private bool _isMainParty;

	private bool _isPrisoner;

	private CharacterImageIdentifierVM _mainHeroVisual;

	[DataSourceProperty]
	public bool IsMainParty
	{
		get
		{
			return _isMainParty;
		}
		set
		{
			if (value != _isMainParty)
			{
				_isMainParty = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsMainParty");
			}
		}
	}

	[DataSourceProperty]
	public bool IsPrisoner
	{
		get
		{
			return _isPrisoner;
		}
		set
		{
			if (value != _isPrisoner)
			{
				_isPrisoner = value;
				((ViewModel)this).OnPropertyChangedWithValue(value, "IsPrisoner");
			}
		}
	}

	[DataSourceProperty]
	public CharacterImageIdentifierVM MainHeroVisual
	{
		get
		{
			return _mainHeroVisual;
		}
		set
		{
			if (value != _mainHeroVisual)
			{
				_mainHeroVisual = value;
				((ViewModel)this).OnPropertyChangedWithValue<CharacterImageIdentifierVM>(value, "MainHeroVisual");
			}
		}
	}

	public PartyPlayerNameplateVM()
	{
		IsMainParty = true;
	}

	public void InitializePlayerNameplate(Action resetCamera)
	{
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Expected O, but got Unknown
		_isPartyHeroVisualDirty = true;
		_resetCamera = resetCamera;
		int isPrisonerBind;
		if (IsMainParty && base.Party.LeaderHero == null)
		{
			Hero mainHero = Hero.MainHero;
			isPrisonerBind = ((mainHero != null && mainHero.IsAlive) ? 1 : 0);
		}
		else
		{
			isPrisonerBind = 0;
		}
		_isPrisonerBind = (byte)isPrisonerBind != 0;
		MainHeroVisual = new CharacterImageIdentifierVM(CampaignUIHelper.GetCharacterCode(Hero.MainHero.CharacterObject, false));
	}

	public override void Clear()
	{
		base.Clear();
		base.IsInSettlement = true;
		base.IsVisibleOnMap = false;
		MainHeroVisual = null;
	}

	public override void RefreshDynamicProperties(bool forceUpdate)
	{
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Expected O, but got Unknown
		base.RefreshDynamicProperties(forceUpdate);
		if ((IsMainParty && MathF.Abs(Hero.MainHero.Age - _latestMainHeroAge) >= 1f) || forceUpdate)
		{
			_latestMainHeroAge = Hero.MainHero.Age;
			_isPartyHeroVisualDirty = true;
		}
		if (_isPartyHeroVisualDirty || forceUpdate)
		{
			_mainHeroVisualBind = new CharacterImageIdentifierVM(SandBoxUIHelper.GetCharacterCode(Hero.MainHero.CharacterObject));
			_isPartyHeroVisualDirty = false;
		}
		int isPrisonerBind;
		if (IsMainParty && base.Party.LeaderHero == null)
		{
			Hero mainHero = Hero.MainHero;
			isPrisonerBind = ((mainHero != null && mainHero.IsAlive) ? 1 : 0);
		}
		else
		{
			isPrisonerBind = 0;
		}
		_isPrisonerBind = (byte)isPrisonerBind != 0;
	}

	public override void RefreshBinding()
	{
		base.RefreshBinding();
		IsPrisoner = _isPrisonerBind;
	}

	public override void RefreshPosition()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		CampaignVec2 val = base.Party.Position + base.Party.EventPositionAdder;
		Vec3 val2 = ((CampaignVec2)(ref val)).AsVec3();
		Vec3 val3 = val2 + new Vec3(0f, 0f, 0.8f, -1f);
		_latestX = 0f;
		_latestY = 0f;
		_latestW = 0f;
		MBWindowManager.WorldToScreenInsideUsableArea(_mapCamera, val2, ref _latestX, ref _latestY, ref _latestW);
		_partyPositionBind = new Vec2(_latestX, _latestY);
		Vec3 position = _mapCamera.Position;
		_isHighBind = ((Vec3)(ref position)).Distance(val2) >= 110f;
		_isBehindBind = _latestW < 0f;
		MBWindowManager.WorldToScreenInsideUsableArea(_mapCamera, val3, ref _latestX, ref _latestY, ref _latestW);
		_headPositionBind = new Vec2(_latestX, _latestY);
		base.DistanceToCamera = ((Vec3)(ref val2)).Distance(_mapCamera.Position);
	}

	public void ExecuteSetCameraPosition()
	{
		_resetCamera();
	}
}
