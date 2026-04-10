using TaleWorlds.DotNet;
using TaleWorlds.Engine;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.GauntletUI;
using TaleWorlds.MountAndBlade.View.Tableaus;
using TaleWorlds.TwoDimension;

namespace TaleWorlds.MountAndBlade.GauntletUI.TextureProviders;

public class BrightnessDemoTextureProvider : TextureProvider
{
	private BrightnessDemoTableau _sceneTableau;

	private Texture _texture;

	private Texture _providedTexture;

	private EngineTexture wrappedTexture;

	public int DemoType
	{
		set
		{
			_sceneTableau.SetDemoType(value);
		}
	}

	public BrightnessDemoTextureProvider()
	{
		_sceneTableau = new BrightnessDemoTableau();
	}

	private void CheckTexture()
	{
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Expected O, but got Unknown
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Expected O, but got Unknown
		if (_sceneTableau != null)
		{
			if ((NativeObject)(object)_texture != (NativeObject)(object)_sceneTableau.Texture)
			{
				_texture = _sceneTableau.Texture;
				if ((NativeObject)(object)_texture != (NativeObject)null)
				{
					wrappedTexture = new EngineTexture(_texture);
					_providedTexture = new Texture((ITexture)(object)wrappedTexture);
				}
				else
				{
					_providedTexture = null;
				}
			}
		}
		else
		{
			_providedTexture = null;
		}
	}

	public override void Tick(float dt)
	{
		((TextureProvider)this).Tick(dt);
		CheckTexture();
		_sceneTableau?.OnTick(dt);
	}

	public override void Clear(bool clearNextFrame)
	{
		_sceneTableau?.OnFinalize();
		((TextureProvider)this).Clear(clearNextFrame);
	}

	public override void SetTargetSize(int width, int height)
	{
		((TextureProvider)this).SetTargetSize(width, height);
		_sceneTableau.SetTargetSize(width, height);
	}

	protected override Texture OnGetTextureForRender(TwoDimensionContext twoDimensionContext, string name)
	{
		CheckTexture();
		return _providedTexture;
	}
}
