using System;
using TaleWorlds.DotNet;
using TaleWorlds.Engine;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.GauntletUI;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.View.Tableaus;
using TaleWorlds.MountAndBlade.View.Tableaus.Thumbnails;
using TaleWorlds.ScreenSystem;
using TaleWorlds.TwoDimension;

namespace TaleWorlds.MountAndBlade.GauntletUI.TextureProviders.ImageIdentifiers;

public abstract class ImageIdentifierTextureProvider : TextureProvider, IDisposable
{
	private bool _textureRequiresRefreshing;

	private bool _handleNewlyCreatedTexture;

	private Texture _texture;

	private Texture _providedTexture;

	private string _imageId;

	private string _additionalArgs;

	private bool _isBig;

	private bool _isReleased;

	protected ThumbnailCreationData ThumbnailCreationData { get; set; }

	public bool IsReleased
	{
		get
		{
			return _isReleased;
		}
		set
		{
			if (_isReleased != value)
			{
				_isReleased = value;
				if (_isReleased)
				{
					ReleaseCache();
					_isReleased = false;
				}
			}
			_textureRequiresRefreshing = true;
			_handleNewlyCreatedTexture = true;
		}
	}

	public bool IsBig
	{
		get
		{
			return _isBig;
		}
		set
		{
			if (_isBig != value)
			{
				_isBig = value;
				_textureRequiresRefreshing = true;
			}
		}
	}

	public string ImageId
	{
		get
		{
			return _imageId;
		}
		set
		{
			if (_imageId != value)
			{
				_imageId = value;
				_textureRequiresRefreshing = true;
			}
		}
	}

	public string AdditionalArgs
	{
		get
		{
			return _additionalArgs;
		}
		set
		{
			if (_additionalArgs != value)
			{
				_additionalArgs = value;
				_textureRequiresRefreshing = true;
			}
		}
	}

	public ImageIdentifierTextureProvider()
	{
		_textureRequiresRefreshing = true;
	}

	~ImageIdentifierTextureProvider()
	{
		try
		{
			OnDisposed();
		}
		finally
		{
			((object)this).Finalize();
		}
	}

	protected abstract void OnCreateImageWithId(string id, string additionalArgs);

	public override void Tick(float dt)
	{
		((TextureProvider)this).Tick(dt);
		CheckTexture();
	}

	private void ReleaseCache()
	{
		if (ThumbnailCreationData != null)
		{
			if (!ThumbnailCreationData.IsProcessed)
			{
				Debug.FailedAssert("Created thumbnail data but trying to release it before its processed", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.GauntletUI\\TextureProviders\\ImageIdentifiers\\ImageIdentifierTextureProvider.cs", "ReleaseCache", 50);
				return;
			}
			ThumbnailCacheManager.Current.DestroyTexture(ThumbnailCreationData);
			ThumbnailCreationData = null;
		}
	}

	public override void Clear(bool clearNextFrame)
	{
		((TextureProvider)this).Clear(clearNextFrame);
		_providedTexture = null;
		_textureRequiresRefreshing = true;
		ReleaseCache();
	}

	protected virtual bool GetCanForceCheckTexture()
	{
		return false;
	}

	protected virtual void OnCheckTexture()
	{
		CreateImageWithId(ImageId, AdditionalArgs);
	}

	protected override Texture OnGetTextureForRender(TwoDimensionContext twoDimensionContext, string name)
	{
		return _providedTexture;
	}

	protected void ForceRefreshTextures()
	{
		_textureRequiresRefreshing = true;
	}

	private void CheckTexture()
	{
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Expected O, but got Unknown
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Expected O, but got Unknown
		if (_textureRequiresRefreshing || GetCanForceCheckTexture())
		{
			_texture = null;
			ReleaseCache();
			OnCheckTexture();
			_textureRequiresRefreshing = false;
		}
		if (!_handleNewlyCreatedTexture)
		{
			return;
		}
		Texture val = null;
		Texture providedTexture = _providedTexture;
		ITexture obj = ((providedTexture != null) ? providedTexture.PlatformTexture : null);
		EngineTexture val2;
		if ((val2 = (EngineTexture)(object)((obj is EngineTexture) ? obj : null)) != null)
		{
			val = val2.Texture;
		}
		if ((NativeObject)(object)_texture != (NativeObject)(object)val)
		{
			if ((NativeObject)(object)_texture != (NativeObject)null)
			{
				EngineTexture val3 = new EngineTexture(_texture);
				_providedTexture = new Texture((ITexture)(object)val3);
			}
			else
			{
				_providedTexture = null;
			}
		}
		_handleNewlyCreatedTexture = false;
	}

	public void CreateImageWithId(string id, string additionalArgs)
	{
		if (ScreenManager.IsLateTickInProgress)
		{
			Debug.FailedAssert("Trying to create a render request on late tick. ", "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.GauntletUI\\TextureProviders\\ImageIdentifiers\\ImageIdentifierTextureProvider.cs", "CreateImageWithId", 131);
		}
		OnCreateImageWithId(id, additionalArgs ?? string.Empty);
	}

	protected void OnTextureCreated(Texture texture)
	{
		_texture = texture;
		_textureRequiresRefreshing = false;
		_handleNewlyCreatedTexture = true;
	}

	protected void OnTextureCreationCancelled()
	{
		_texture = null;
	}

	private void OnDisposed()
	{
		_ = ThumbnailCreationData;
		ReleaseCache();
	}

	void IDisposable.Dispose()
	{
		OnDisposed();
	}
}
