using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.Diamond;

namespace TaleWorlds.MountAndBlade.Multiplayer;

public sealed class MapCheckHelpers
{
	private static readonly double TimeoutDuration = 1.8;

	private static string MapListEndpoint(GameServerEntry serverEntry)
	{
		return $"http://{serverEntry.Address}:{serverEntry.Port}/maps/list";
	}

	public static async Task<(bool isRefusedToJoin, string notExistingMap)> CheckMaps(GameServerEntry serverEntry)
	{
		if (CheckCurrentlyPlayedMap(serverEntry))
		{
			return await CheckMapDownloaderMaps(serverEntry);
		}
		return (isRefusedToJoin: true, notExistingMap: serverEntry.Map);
	}

	private static bool CheckCurrentlyPlayedMap(GameServerEntry serverEntry)
	{
		UniqueSceneId val = default(UniqueSceneId);
		bool flag = UniqueSceneId.TryParse(serverEntry.UniqueMapId, ref val);
		if (!flag || !DoesSceneExist(serverEntry.Map, val.UniqueToken, val.Revision))
		{
			return !flag;
		}
		return true;
	}

	private static async Task<(bool isRefusedToJoin, string notExistingMap)> CheckMapDownloaderMaps(GameServerEntry serverEntry)
	{
		_ = 1;
		try
		{
			Stopwatch watch = Stopwatch.StartNew();
			CancellationTokenSource cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(TimeoutDuration));
			Task<string> downloadTask = HttpHelper.DownloadStringTaskAsync(MapListEndpoint(serverEntry));
			if (await Task.WhenAny(new Task[2]
			{
				downloadTask,
				Task.Delay(-1, cancellationTokenSource.Token)
			}) == downloadTask)
			{
				foreach (MapListItemResponse map in JsonConvert.DeserializeObject<MapListResponse>(await downloadTask).Maps)
				{
					if (!DoesSceneExist(map.Name, map.UniqueToken, map.Revision))
					{
						return (isRefusedToJoin: true, notExistingMap: map.Name);
					}
				}
				return (isRefusedToJoin: false, notExistingMap: null);
			}
			watch.Stop();
			long elapsedMilliseconds = watch.ElapsedMilliseconds;
			Debug.Print($"Getting map list timeout to host: {serverEntry.Address}:{serverEntry.Port} in {elapsedMilliseconds} milliseconds.", 0, (DebugColor)12, 17592186044416uL);
			return (isRefusedToJoin: false, notExistingMap: null);
		}
		catch (Exception ex)
		{
			Debug.Print("Exception getting map list from custom servers: " + ex.Message, 0, (DebugColor)12, 17592186044416uL);
			return (isRefusedToJoin: false, notExistingMap: null);
		}
	}

	private static bool DoesSceneExist(string mapId, string uniqueMapId, string revision)
	{
		string text = default(string);
		if (!Utilities.TryGetFullFilePathOfScene(mapId, ref text))
		{
			return false;
		}
		if (uniqueMapId == null)
		{
			return true;
		}
		UniqueSceneId val = default(UniqueSceneId);
		if (!Utilities.TryGetUniqueIdentifiersForSceneFile(text, ref val))
		{
			return false;
		}
		if (val.UniqueToken == uniqueMapId)
		{
			return val.Revision == revision;
		}
		return false;
	}

	private MapCheckHelpers()
	{
	}
}
