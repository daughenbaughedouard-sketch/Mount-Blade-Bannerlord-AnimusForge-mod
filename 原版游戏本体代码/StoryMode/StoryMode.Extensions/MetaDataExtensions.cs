using System;
using TaleWorlds.SaveSystem;

namespace StoryMode.Extensions;

public static class MetaDataExtensions
{
	public static bool HasStoryMode(this MetaData metaData)
	{
		bool result = false;
		string text = default(string);
		if (metaData != null && metaData.TryGetValue("Modules", ref text))
		{
			string[] array = text.Split(new char[1] { ';' });
			for (int i = 0; i < array.Length; i++)
			{
				if (string.Equals(array[i], "StoryMode", StringComparison.OrdinalIgnoreCase))
				{
					result = true;
					break;
				}
			}
		}
		return result;
	}

	public static bool AreAchievementsDisabled(this MetaData metaData)
	{
		string s = default(string);
		if (metaData != null && metaData.TryGetValue("AchievementsDisabled", ref s) && int.TryParse(s, out var result))
		{
			return result == 1;
		}
		return false;
	}
}
