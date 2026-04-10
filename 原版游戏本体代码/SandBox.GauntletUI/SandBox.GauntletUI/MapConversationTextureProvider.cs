using SandBox.View.Map;
using TaleWorlds.DotNet;
using TaleWorlds.Engine;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.GauntletUI;
using TaleWorlds.TwoDimension;

namespace SandBox.GauntletUI;

public class MapConversationTextureProvider : TextureProvider
{
	private MapConversationTableau _mapConversationTableau;

	private Texture _texture;

	private Texture _providedTexture;

	public object Data
	{
		set
		{
			_mapConversationTableau.SetData(value);
		}
	}

	public bool IsEnabled
	{
		set
		{
			_mapConversationTableau.SetEnabled(value);
		}
	}

	public MapConversationTextureProvider()
	{
		_mapConversationTableau = new MapConversationTableau();
	}

	public override void Clear(bool clearNextFrame)
	{
		_mapConversationTableau.OnFinalize(clearNextFrame);
		((TextureProvider)this).Clear(clearNextFrame);
	}

	private void CheckTexture()
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Expected O, but got Unknown
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Expected O, but got Unknown
		if ((NativeObject)(object)_texture != (NativeObject)(object)_mapConversationTableau.Texture)
		{
			_texture = _mapConversationTableau.Texture;
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
		_mapConversationTableau.SetTargetSize(width, height);
	}

	public override void Tick(float dt)
	{
		((TextureProvider)this).Tick(dt);
		CheckTexture();
		_mapConversationTableau.OnTick(dt);
	}
}
