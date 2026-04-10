using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using TaleWorlds.DotNet;
using TaleWorlds.Engine;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.GauntletUI;
using TaleWorlds.Library;
using TaleWorlds.TwoDimension;

namespace TaleWorlds.MountAndBlade.GauntletUI.TextureProviders;

public class OnlineImageTextureProvider : TextureProvider
{
	private Dictionary<string, PlatformFilePath> _onlineImageCache;

	private readonly string DataFolder = "Online Images";

	private readonly PlatformDirectoryPath _onlineImageCacheFolderPath;

	private Texture _texture;

	private bool _requiresRetry;

	private int _retryCount;

	private const int _maxRetryCount = 10;

	private string _onlineSourceUrl;

	public string OnlineSourceUrl
	{
		set
		{
			_onlineSourceUrl = value;
			RefreshOnlineImage();
		}
	}

	public OnlineImageTextureProvider()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		_onlineImageCache = new Dictionary<string, PlatformFilePath>();
		_onlineImageCacheFolderPath = new PlatformDirectoryPath((PlatformFileType)1, DataFolder);
		PopulateOnlineImageCache();
	}

	public override void Tick(float dt)
	{
		((TextureProvider)this).Tick(dt);
		if (_requiresRetry)
		{
			if (10 < _retryCount)
			{
				_requiresRetry = false;
				return;
			}
			_retryCount++;
			RefreshOnlineImage();
		}
	}

	private async void RefreshOnlineImage()
	{
		if (string.IsNullOrEmpty(_onlineSourceUrl))
		{
			_requiresRetry = false;
		}
		else
		{
			if (_retryCount >= 10)
			{
				return;
			}
			try
			{
				string guidOfRequestedURL = ToGuid(_onlineSourceUrl).ToString();
				if (!_onlineImageCache.ContainsKey(guidOfRequestedURL))
				{
					PlatformFilePath pathOfTheDownloadedImage = new PlatformFilePath(_onlineImageCacheFolderPath, guidOfRequestedURL + ".png");
					byte[] array = await HttpHelper.DownloadDataTaskAsync(_onlineSourceUrl);
					if (array != null)
					{
						FileHelper.SaveFile(pathOfTheDownloadedImage, array);
						_onlineImageCache.Add(guidOfRequestedURL, pathOfTheDownloadedImage);
					}
				}
				if (_onlineImageCache.TryGetValue(guidOfRequestedURL, out var value))
				{
					Texture val = Texture.CreateTextureFromPath(value);
					if ((NativeObject)(object)val == (NativeObject)null)
					{
						_onlineImageCache.Remove(guidOfRequestedURL);
						Debug.Print($"RETRYING TO DOWNLOAD: {_onlineSourceUrl} | RETRY COUNT: {_retryCount}", 0, (DebugColor)3, 17592186044416uL);
						_requiresRetry = true;
					}
					else
					{
						OnTextureCreated(val);
						_requiresRetry = false;
					}
				}
				else
				{
					Debug.Print($"RETRYING TO DOWNLOAD: {_onlineSourceUrl} | RETRY COUNT: {_retryCount}", 0, (DebugColor)3, 17592186044416uL);
					_requiresRetry = true;
				}
			}
			catch (Exception ex)
			{
				Debug.FailedAssert("Error while trying to get image online: " + ex.Message, "C:\\BuildAgent\\work\\mb3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.GauntletUI\\TextureProviders\\OnlineImageTextureProvider.cs", "RefreshOnlineImage", 115);
				Debug.Print($"RETRYING TO DOWNLOAD: {_onlineSourceUrl} | RETRY COUNT: {_retryCount}", 0, (DebugColor)3, 17592186044416uL);
				_requiresRetry = true;
			}
		}
	}

	protected override Texture OnGetTextureForRender(TwoDimensionContext twoDimensionContext, string name)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Expected O, but got Unknown
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Expected O, but got Unknown
		if ((NativeObject)(object)_texture != (NativeObject)null)
		{
			return new Texture((ITexture)new EngineTexture(_texture));
		}
		return null;
	}

	private void PopulateOnlineImageCache()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		PlatformFilePath[] files = FileHelper.GetFiles(_onlineImageCacheFolderPath, "*.png", SearchOption.AllDirectories);
		for (int i = 0; i < files.Length; i++)
		{
			PlatformFilePath value = files[i];
			string fileNameWithoutExtension = ((PlatformFilePath)(ref value)).GetFileNameWithoutExtension();
			_onlineImageCache.Add(fileNameWithoutExtension, value);
		}
	}

	private static Guid ToGuid(string src)
	{
		byte[] bytes = Encoding.UTF8.GetBytes(src);
		byte[] array = new SHA1CryptoServiceProvider().ComputeHash(bytes);
		Array.Resize(ref array, 16);
		return new Guid(array);
	}

	private void OnTextureCreated(Texture texture)
	{
		_texture = texture;
	}
}
