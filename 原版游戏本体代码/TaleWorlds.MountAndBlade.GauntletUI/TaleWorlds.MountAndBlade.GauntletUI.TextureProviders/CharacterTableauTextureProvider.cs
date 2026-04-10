using TaleWorlds.DotNet;
using TaleWorlds.Engine;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.GauntletUI;
using TaleWorlds.MountAndBlade.View.Tableaus;
using TaleWorlds.TwoDimension;

namespace TaleWorlds.MountAndBlade.GauntletUI.TextureProviders;

public class CharacterTableauTextureProvider : TextureProvider
{
	private CharacterTableau _characterTableau;

	private Texture _texture;

	private Texture _providedTexture;

	private bool _isHidden;

	public float CustomAnimationProgressRatio => _characterTableau.GetCustomAnimationProgressRatio();

	public string BannerCodeText
	{
		set
		{
			_characterTableau.SetBannerCode(value);
		}
	}

	public string BodyProperties
	{
		set
		{
			_characterTableau.SetBodyProperties(value);
		}
	}

	public int StanceIndex
	{
		set
		{
			_characterTableau.SetStanceIndex(value);
		}
	}

	public bool IsFemale
	{
		set
		{
			_characterTableau.SetIsFemale(value);
		}
	}

	public int Race
	{
		set
		{
			_characterTableau.SetRace(value);
		}
	}

	public bool IsBannerShownInBackground
	{
		set
		{
			_characterTableau.SetIsBannerShownInBackground(value);
		}
	}

	public bool IsEquipmentAnimActive
	{
		set
		{
			_characterTableau.SetIsEquipmentAnimActive(value);
		}
	}

	public string EquipmentCode
	{
		set
		{
			_characterTableau.SetEquipmentCode(value);
		}
	}

	public string IdleAction
	{
		set
		{
			_characterTableau.SetIdleAction(value);
		}
	}

	public string IdleFaceAnim
	{
		set
		{
			_characterTableau.SetIdleFaceAnim(value);
		}
	}

	public bool CurrentlyRotating
	{
		set
		{
			_characterTableau.RotateCharacter(value);
		}
	}

	public string MountCreationKey
	{
		set
		{
			_characterTableau.SetMountCreationKey(value);
		}
	}

	public uint ArmorColor1
	{
		set
		{
			_characterTableau.SetArmorColor1(value);
		}
	}

	public uint ArmorColor2
	{
		set
		{
			_characterTableau.SetArmorColor2(value);
		}
	}

	public string CharStringId
	{
		set
		{
			_characterTableau.SetCharStringID(value);
		}
	}

	public bool TriggerCharacterMountPlacesSwap
	{
		set
		{
			_characterTableau.TriggerCharacterMountPlacesSwap();
		}
	}

	public float CustomRenderScale
	{
		set
		{
			_characterTableau.SetCustomRenderScale(value);
		}
	}

	public bool IsPlayingCustomAnimations
	{
		get
		{
			return _characterTableau?.IsRunningCustomAnimation ?? false;
		}
		set
		{
			if (value)
			{
				_characterTableau.StartCustomAnimation();
			}
			else
			{
				_characterTableau.StopCustomAnimation();
			}
		}
	}

	public bool ShouldLoopCustomAnimation
	{
		get
		{
			return _characterTableau.ShouldLoopCustomAnimation;
		}
		set
		{
			_characterTableau.ShouldLoopCustomAnimation = value;
		}
	}

	public int LeftHandWieldedEquipmentIndex
	{
		set
		{
			_characterTableau.SetLeftHandWieldedEquipmentIndex(value);
		}
	}

	public int RightHandWieldedEquipmentIndex
	{
		set
		{
			_characterTableau.SetRightHandWieldedEquipmentIndex(value);
		}
	}

	public float CustomAnimationWaitDuration
	{
		set
		{
			_characterTableau.CustomAnimationWaitDuration = value;
		}
	}

	public string CustomAnimation
	{
		set
		{
			_characterTableau.SetCustomAnimation(value);
		}
	}

	public bool IsHidden
	{
		get
		{
			return _isHidden;
		}
		set
		{
			if (_isHidden != value)
			{
				_isHidden = value;
			}
		}
	}

	public CharacterTableauTextureProvider()
	{
		_characterTableau = new CharacterTableau();
	}

	public override void Clear(bool clearNextFrame)
	{
		_characterTableau.OnFinalize();
		((TextureProvider)this).Clear(clearNextFrame);
	}

	private void CheckTexture()
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Expected O, but got Unknown
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Expected O, but got Unknown
		if ((NativeObject)(object)_texture != (NativeObject)(object)_characterTableau.Texture)
		{
			_texture = _characterTableau.Texture;
			if ((NativeObject)(object)_texture != (NativeObject)null)
			{
				EngineTexture val = new EngineTexture(_texture);
				_providedTexture = new Texture((ITexture)(object)val);
			}
			else
			{
				_providedTexture = null;
			}
		}
	}

	protected override Texture OnGetTextureForRender(TwoDimensionContext twoDimensionContext, string name)
	{
		CheckTexture();
		return _providedTexture;
	}

	public override void SetTargetSize(int width, int height)
	{
		((TextureProvider)this).SetTargetSize(width, height);
		_characterTableau.SetTargetSize(width, height);
	}

	public override void Tick(float dt)
	{
		((TextureProvider)this).Tick(dt);
		CheckTexture();
		_characterTableau.OnTick(dt);
	}
}
