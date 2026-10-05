using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Threading;
using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Library;

namespace AnimusForge;

/// <summary>
/// Field diagnostic and safety net for the reported symptom
/// "the player's main party headcount grew to 10,000+ within about 10 seconds and kept growing".
///
/// Cost model:
/// - The sampler is called once per application tick but returns immediately unless
///   250 ms have passed, and it only reads cached counters (TroopRoster.TotalManCount,
///   PartyBase.PartySizeLimit). No roster enumeration and no allocation while dormant.
/// - The Harmony prefix on TroopRoster.AddToCounts exits after a single volatile read
///   while the guard is dormant, so ordinary roster traffic pays one branch only.
/// - Stack capture, roster dumps and counter reconciliation only run after abnormal
///   growth is detected, and every capture structure is bounded.
///
/// The guard never changes gameplay while the main party grows normally. It exists so a
/// field report without logs still produces the exact call site responsible for the
/// runaway, and so the runaway cannot keep doubling the save.
/// </summary>
internal static class MainPartyRosterRunawayGuard
{
	private const string LogSource = "RosterRunaway";
	private const string HarmonyId = "AnimusForge.mainparty.roster.runaway.guard";

	private const double SampleIntervalSeconds = 0.25;
	private const double RecorderWindowSeconds = 20.0;
	private const double RecorderExtendSeconds = 10.0;
	private const double MaxRecorderWindowSeconds = 90.0;
	private const double ProgressLogIntervalSeconds = 5.0;

	/// <summary>Men per second that count as runaway once the roster is above the ceiling.</summary>
	private const double RunawayMenPerSecond = 60.0;

	/// <summary>Consecutive growing samples required, so one-shot transfers never arm the guard.</summary>
	private const int RequiredGrowingSamples = 3;

	/// <summary>Absolute lower bound of the runaway ceiling.</summary>
	private const int CeilingFloor = 1200;

	/// <summary>Runaway ceiling as a multiple of the main party size limit.</summary>
	private const int CeilingLimitMultiplier = 4;

	/// <summary>Past this multiple of the ceiling, further runaway additions are clamped.</summary>
	private const int BlockCeilingMultiplier = 2;

	/// <summary>Set to false to keep the guard diagnostic only.</summary>
	private const bool ClampRunawayAdds = true;

	private const int MaxCapturedSites = 48;
	private const int MaxDuplicateHeroLogs = 12;
	private const int StackFrameLimit = 10;
	private const int StackLineLength = 220;
	private const int RosterDumpLimit = 40;

	private static readonly FieldInfo TroopRosterTotalRegularsField = typeof(TroopRoster).GetField("_totalRegulars", BindingFlags.Instance | BindingFlags.NonPublic);
	private static readonly FieldInfo TroopRosterTotalWoundedRegularsField = typeof(TroopRoster).GetField("_totalWoundedRegulars", BindingFlags.Instance | BindingFlags.NonPublic);
	private static readonly FieldInfo TroopRosterTotalHeroesField = typeof(TroopRoster).GetField("_totalHeroes", BindingFlags.Instance | BindingFlags.NonPublic);
	private static readonly FieldInfo TroopRosterTotalWoundedHeroesField = typeof(TroopRoster).GetField("_totalWoundedHeroes", BindingFlags.Instance | BindingFlags.NonPublic);

	private static readonly object CaptureLock = new object();
	private static readonly Dictionary<string, int> SiteHits = new Dictionary<string, int>(StringComparer.Ordinal);
	private static readonly Dictionary<string, string> SiteSamples = new Dictionary<string, string>(StringComparer.Ordinal);
	private static readonly List<string> SiteOrder = new List<string>();

	private static bool _patched;
	private static bool _patchFailed;

	private static int _armed;
	private static int _detectionCount;
	private static double _lastSampleSeconds;
	private static double _recorderDeadlineSeconds;
	private static double _armedAtSeconds;
	private static double _nextProgressLogSeconds;
	private static int _lastSampleCount;
	private static int _growingSamples;
	private static int _ceiling;
	private static int _blockCeiling;
	private static int _runStartCount;
	private static int _peakCount;
	private static long _positiveAdds;
	private static long _clampedMen;
	private static long _clampEvents;
	private static long _blockedHeroMen;
	private static long _duplicateHeroAdds;
	private static long _repairedHeroStacks;
	private static long _repairedHeroMen;
	private static long _heroRepairGeneration;
	private static int _heroRepairRequested;
	private static int _foldedSites;
	private static int _campaignGeneration = -1;
	private static TroopRoster _mainMemberRoster;
	private static TroopRoster _mainPrisonRoster;

	internal static void EnsurePatched(Harmony harmony)
	{
		if (_patched || harmony == null)
		{
			return;
		}
		try
		{
			var target = AccessTools.Method(
				typeof(TroopRoster),
				"AddToCounts",
				new[]
				{
					typeof(CharacterObject),
					typeof(int),
					typeof(bool),
					typeof(int),
					typeof(int),
					typeof(bool),
					typeof(int)
				});
			if (target == null)
			{
				_patchFailed = true;
				Logger.LogImmediate(LogSource, "main party roster runaway guard could not resolve TroopRoster.AddToCounts; diagnostics stay sampler-only.");
				return;
			}
			var prefix = new HarmonyMethod(typeof(MainPartyRosterRunawayGuard), nameof(AddToCountsPrefix))
			{
				priority = Priority.First
			};
			harmony.Patch(target, prefix: prefix);
			_patched = true;
			Logger.LogImmediate(LogSource, "main party roster runaway guard enabled on " + (target.DeclaringType?.FullName ?? "TroopRoster"));
		}
		catch (Exception ex)
		{
			_patchFailed = true;
			Logger.LogImmediate(LogSource, "main party roster runaway guard patch failed: " + ex);
		}
	}

	/// <summary>
	/// Prefixed to TroopRoster.AddToCounts.
	///
	/// Two independent jobs:
	/// 1. Always-on invariant: a hero owns exactly one slot in a party roster, so a
	///    positive add on a hero that is already present is always corruption. Vanilla
	///    PlayerCaptivity.EndCaptivityInternal() unconditionally calls
	///    PartyBase.MainParty.AddElementToMemberRoster(PlayerCharacter, 1), so every
	///    re-triggered captivity release inflates the main party headcount by one and
	///    the field report "party grew to 10,000+ and keeps growing" is exactly that.
	/// 2. Runaway recorder: only after abnormal growth was detected.
	/// </summary>
	public static bool AddToCountsPrefix(TroopRoster __instance, CharacterObject character, ref int count)
	{
		if (count <= 0 || character == null || __instance == null)
		{
			return true;
		}
		try
		{
			if (character.IsHero
				&& (ReferenceEquals(__instance, _mainMemberRoster) || ReferenceEquals(__instance, _mainPrisonRoster))
				&& __instance.FindIndexOfTroop(character) >= 0)
			{
				BlockDuplicateHeroAdd(__instance, character, count);
				if (count <= 0)
				{
					return true;
				}
			}
		}
		catch
		{
			// A watchdog must never break the game's own roster writes.
		}
		if (Volatile.Read(ref _armed) == 0)
		{
			return true;
		}
		try
		{
			if (!ReferenceEquals(__instance, _mainMemberRoster) && !ReferenceEquals(__instance, _mainPrisonRoster))
			{
				return true;
			}
			CaptureSite(__instance, character, count);
			if (!ClampRunawayAdds || _blockCeiling <= 0)
			{
				return true;
			}
			int current = __instance.TotalManCount;
			if (current < _blockCeiling)
			{
				return true;
			}
			int allowed = Math.Max(0, _blockCeiling - current);
			if (allowed >= count)
			{
				return true;
			}
			int removed = count - allowed;
			Interlocked.Add(ref _clampedMen, removed);
			long clampEvents = Interlocked.Increment(ref _clampEvents);
			if (clampEvents == 1)
			{
				Logger.LogImmediate(LogSource, "runaway clamp engaged roster=" + DescribeRosterKind(__instance)
					+ " current=" + current + " requested=" + count + " allowed=" + allowed
					+ " ceiling=" + _blockCeiling + " troop=" + SafeCharacterId(character));
			}
			count = allowed;
		}
		catch
		{
			// A watchdog must never break the game's own roster writes.
		}
		return true;
	}

	private static void BlockDuplicateHeroAdd(TroopRoster roster, CharacterObject character, int count)
	{
		Interlocked.Add(ref _blockedHeroMen, count);
		long events = Interlocked.Increment(ref _duplicateHeroAdds);
		_heroRepairRequested = 1;
		if (events <= MaxDuplicateHeroLogs)
		{
			string stack = "";
			try
			{
				stack = BuildStackText(new StackTrace(2, true));
			}
			catch
			{
			}
			Logger.LogImmediate(LogSource, "blocked duplicate hero add #" + events
				+ " roster=" + DescribeRosterKind(roster)
				+ " hero=" + SafeCharacterId(character)
				+ " requested=" + count
				+ " existing=" + SafeHeroStackNumber(roster, character)
				+ " roster_total=" + roster.TotalManCount
				+ stack);
		}
		else if (events == MaxDuplicateHeroLogs + 1)
		{
			Logger.LogImmediate(LogSource, "further duplicate hero add logs suppressed for this session; total events=" + events);
		}
	}

	private static int SafeHeroStackNumber(TroopRoster roster, CharacterObject character)
	{
		try
		{
			int index = roster.FindIndexOfTroop(character);
			return index >= 0 ? roster.GetElementNumber(index) : 0;
		}
		catch
		{
			return 0;
		}
	}

	/// <summary>
	/// Repairs hero stacks that an already re-triggered captivity release inflated
	/// (a hero stack above one is never legal) and rebuilds the cached counters so the
	/// party head shown in the UI becomes correct again. Runs once per campaign/load and
	/// again whenever a duplicate hero add is blocked, so existing saves heal on load.
	/// </summary>
	private static void TryRepairHeroStacks(TroopRoster memberRoster, TroopRoster prisonRoster)
	{
		bool dueForLoad = _heroRepairGeneration != _campaignGeneration;
		bool requested = Interlocked.Exchange(ref _heroRepairRequested, 0) != 0;
		if (!dueForLoad && !requested)
		{
			return;
		}
		_heroRepairGeneration = _campaignGeneration;
		RepairHeroStacksInRoster(memberRoster, "member");
		RepairHeroStacksInRoster(prisonRoster, "prisoner");
	}

	private static void RepairHeroStacksInRoster(TroopRoster roster, string label)
	{
		if (roster == null)
		{
			return;
		}
		try
		{
			int cachedBefore = roster.TotalManCount;
			List<CharacterObject> inflated = null;
			List<int> inflatedNumbers = null;
			List<TroopRosterElement> snapshot = SnapshotRoster(roster);
			for (int i = 0; i < snapshot.Count; i++)
			{
				TroopRosterElement element = snapshot[i];
				CharacterObject character = element.Character;
				if (character == null || !character.IsHero || element.Number <= 1)
				{
					continue;
				}
				if (inflated == null)
				{
					inflated = new List<CharacterObject>();
					inflatedNumbers = new List<int>();
				}
				inflated.Add(character);
				inflatedNumbers.Add(element.Number);
			}
			if (inflated == null)
			{
				return;
			}
			for (int i = 0; i < inflated.Count; i++)
			{
				CharacterObject character = inflated[i];
				int number = inflatedNumbers[i];
				try
				{
					roster.AddToCounts(character, -(number - 1), false, 0, 0, true, -1);
					Interlocked.Increment(ref _repairedHeroStacks);
					Interlocked.Add(ref _repairedHeroMen, number - 1);
				}
				catch (Exception ex)
				{
					Logger.LogImmediate(LogSource, "hero stack repair failed roster=" + label
						+ " hero=" + SafeCharacterId(character) + " error=" + ex.Message);
				}
			}
			int afterRemoval = roster.TotalManCount;
			RebuildCachedTotals(roster, "hero_stack_repair_" + label);
			Logger.LogImmediate(LogSource, "HERO STACK REPAIR roster=" + label
				+ " fixed_stacks=" + inflated.Count
				+ " removed_men=" + Interlocked.Read(ref _repairedHeroMen)
				+ " cached_before=" + cachedBefore
				+ " after_removal=" + afterRemoval
				+ " after_rebuild=" + roster.TotalManCount
				+ " details=" + BuildRepairDetail(inflated, inflatedNumbers));
		}
		catch (Exception ex)
		{
			Logger.LogImmediate(LogSource, "hero stack repair pass failed roster=" + label + " error=" + ex);
		}
	}

	private static string BuildRepairDetail(List<CharacterObject> characters, List<int> numbers)
	{
		StringBuilder builder = new StringBuilder();
		for (int i = 0; i < characters.Count && i < 12; i++)
		{
			if (builder.Length > 0)
			{
				builder.Append(", ");
			}
			builder.Append(SafeCharacterId(characters[i])).Append(':').Append(numbers[i]).Append("->1");
		}
		if (characters.Count > 12)
		{
			builder.Append(", +").Append(characters.Count - 12).Append(" more");
		}
		return builder.ToString();
	}

	private static List<TroopRosterElement> SnapshotRoster(TroopRoster roster)
	{
		List<TroopRosterElement> snapshot = new List<TroopRosterElement>();
		var elements = roster.GetTroopRoster();
		for (int i = 0; i < elements.Count; i++)
		{
			snapshot.Add(elements[i]);
		}
		return snapshot;
	}

	/// <summary>
	/// Recomputes the roster's cached counters from its elements. The game reports
	/// TotalManCount as _totalRegulars + _totalHeroes, so a corrupted cache keeps showing
	/// an inflated party size even after the roster elements themselves are repaired.
	/// </summary>
	private static void RebuildCachedTotals(TroopRoster roster, string label)
	{
		if (roster == null
			|| TroopRosterTotalRegularsField == null
			|| TroopRosterTotalWoundedRegularsField == null
			|| TroopRosterTotalHeroesField == null
			|| TroopRosterTotalWoundedHeroesField == null)
		{
			return;
		}
		try
		{
			int totalRegulars = 0;
			int totalWoundedRegulars = 0;
			int totalHeroes = 0;
			int totalWoundedHeroes = 0;
			List<TroopRosterElement> snapshot = SnapshotRoster(roster);
			for (int i = 0; i < snapshot.Count; i++)
			{
				TroopRosterElement element = snapshot[i];
				CharacterObject character = element.Character;
				if (character == null || element.Number <= 0)
				{
					continue;
				}
				if (character.IsHero)
				{
					totalHeroes++;
					if (Math.Max(0, element.WoundedNumber) > 0 || (character.HeroObject != null && character.HeroObject.IsWounded))
					{
						totalWoundedHeroes++;
					}
					continue;
				}
				totalRegulars += Math.Max(0, element.Number);
				totalWoundedRegulars += Math.Max(0, element.WoundedNumber);
			}
			int beforeRegulars = GetIntFieldValue(TroopRosterTotalRegularsField, roster);
			int beforeHeroes = GetIntFieldValue(TroopRosterTotalHeroesField, roster);
			if (beforeRegulars == totalRegulars && beforeHeroes == totalHeroes)
			{
				return;
			}
			TroopRosterTotalRegularsField.SetValue(roster, totalRegulars);
			TroopRosterTotalWoundedRegularsField.SetValue(roster, totalWoundedRegulars);
			TroopRosterTotalHeroesField.SetValue(roster, totalHeroes);
			TroopRosterTotalWoundedHeroesField.SetValue(roster, totalWoundedHeroes);
			try
			{
				roster.UpdateVersion();
			}
			catch
			{
			}
			Logger.LogImmediate(LogSource, "roster cache rebuilt label=" + label
				+ " regulars " + beforeRegulars + "->" + totalRegulars
				+ " heroes " + beforeHeroes + "->" + totalHeroes
				+ " total=" + roster.TotalManCount);
		}
		catch (Exception ex)
		{
			Logger.LogImmediate(LogSource, "roster cache rebuild failed label=" + label + " error=" + ex.Message);
		}
	}

	private static int GetIntFieldValue(FieldInfo field, object target)
	{
		try
		{
			object value = field?.GetValue(target);
			return value is int result ? result : 0;
		}
		catch
		{
			return 0;
		}
	}

	internal static void OnApplicationTick()
	{
		try
		{
			if (Campaign.Current == null)
			{
				ResetForCampaignExit();
				return;
			}
			double now = NowSeconds();
			if (now - _lastSampleSeconds < SampleIntervalSeconds)
			{
				return;
			}
			MobileParty mainParty = MobileParty.MainParty;
			TroopRoster roster = mainParty?.MemberRoster;
			if (roster == null)
			{
				_lastSampleSeconds = now;
				return;
			}
			_mainMemberRoster = roster;
			_mainPrisonRoster = mainParty.PrisonRoster;
			EnsureCampaignGeneration();
			TryRepairHeroStacks(roster, _mainPrisonRoster);

			int count = roster.TotalManCount;
			int partyLimit = PartyBase.MainParty?.PartySizeLimit ?? 0;
			int ceiling = Math.Max(CeilingFloor, partyLimit * CeilingLimitMultiplier);
			double elapsed = now - _lastSampleSeconds;
			int growth = count - _lastSampleCount;
			double perSecond = elapsed > 0.001 ? growth / elapsed : 0.0;
			_ceiling = ceiling;
			_blockCeiling = ceiling * BlockCeilingMultiplier;

			if (growth > 0)
			{
				_growingSamples++;
			}
			else
			{
				_growingSamples = 0;
			}

			bool armed = Volatile.Read(ref _armed) != 0;
			if (armed)
			{
				if (count > _peakCount)
				{
					_peakCount = count;
				}
				if (growth > 0)
				{
					_recorderDeadlineSeconds = now + RecorderExtendSeconds;
				}
				if (now >= _nextProgressLogSeconds)
				{
					_nextProgressLogSeconds = now + ProgressLogIntervalSeconds;
					Logger.LogImmediate(LogSource, "runaway in progress count=" + count + " growth=" + growth
						+ " per_second=" + perSecond.ToString("0.0") + " captured_adds=" + Interlocked.Read(ref _positiveAdds)
						+ " clamped_men=" + Interlocked.Read(ref _clampedMen));
				}
				if (now - _armedAtSeconds >= MaxRecorderWindowSeconds)
				{
					// Keep the log bounded even if the runaway never stops; the detector
					// re-arms on the next growing sample and starts a fresh capture window.
					FlushReport("window_cap");
				}
				else if (now >= _recorderDeadlineSeconds)
				{
					FlushReport("quiet");
				}
			}
			else if (count > ceiling && perSecond >= RunawayMenPerSecond && _growingSamples >= RequiredGrowingSamples)
			{
				Arm(count, growth, perSecond, ceiling, partyLimit, mainParty);
			}

			_lastSampleSeconds = now;
			_lastSampleCount = count;
		}
		catch (Exception ex)
		{
			try
			{
				Logger.Log(LogSource, "sampler failed: " + ex.GetType().Name + ": " + ex.Message);
			}
			catch
			{
			}
		}
	}

	private static void Arm(int count, int growth, double perSecond, int ceiling, int partyLimit, MobileParty mainParty)
	{
		Interlocked.Exchange(ref _armed, 1);
		_detectionCount++;
		_runStartCount = count - Math.Max(0, growth);
		_peakCount = count;
		Volatile.Write(ref _positiveAdds, 0L);
		Volatile.Write(ref _clampedMen, 0L);
		Volatile.Write(ref _clampEvents, 0L);
		double now = NowSeconds();
		_armedAtSeconds = now;
		_recorderDeadlineSeconds = now + RecorderWindowSeconds;
		_nextProgressLogSeconds = now + ProgressLogIntervalSeconds;
		lock (CaptureLock)
		{
			SiteHits.Clear();
			SiteSamples.Clear();
			SiteOrder.Clear();
			_foldedSites = 0;
		}
		Logger.LogImmediate(LogSource, "MAIN PARTY ROSTER RUNAWAY DETECTED #" + _detectionCount
			+ " count=" + count
			+ " growth=" + growth
			+ " per_second=" + perSecond.ToString("0.0")
			+ " ceiling=" + ceiling
			+ " party_limit=" + partyLimit
			+ " prisoners=" + (mainParty?.PrisonRoster?.TotalManCount ?? 0)
			+ " day=" + SafeNowDay()
			+ " settlement=" + (mainParty?.CurrentSettlement?.StringId ?? Settlement.CurrentSettlement?.StringId ?? "null")
			+ " mission=" + (TaleWorlds.MountAndBlade.Mission.Current != null)
			+ " encounter=" + (PlayerEncounter.Current != null)
			+ " patch=" + (_patched ? "on" : (_patchFailed ? "failed" : "off")));
		Logger.LogImmediate(LogSource, "roster element reconciliation: " + BuildReconciliation(mainParty?.MemberRoster)
			+ " | " + BuildRosterDump(mainParty?.MemberRoster));
		try
		{
			InformationManager.DisplayMessage(new InformationMessage(
				"【AnimusForge】检测到主队人数异常暴涨（" + count + " 人，约 " + perSecond.ToString("0") + " 人/秒）。已开始记录来源；超过 " + (ceiling * BlockCeilingMultiplier) + " 人后会拦截继续增长。"));
		}
		catch
		{
		}
	}

	private static void FlushReport(string reason)
	{
		Interlocked.Exchange(ref _armed, 0);
		List<string> lines = new List<string>();
		lock (CaptureLock)
		{
			foreach (string signature in SiteOrder)
			{
				if (!SiteHits.TryGetValue(signature, out int hits))
				{
					continue;
				}
				SiteSamples.TryGetValue(signature, out string sample);
				lines.Add("  hits=" + hits + " site=" + signature + "\n" + (sample ?? ""));
			}
		}
		Logger.LogImmediate(LogSource, "runaway report reason=" + reason
			+ " detections=" + _detectionCount
			+ " start_count=" + _runStartCount
			+ " peak_count=" + _peakCount
			+ " captured_additions=" + Interlocked.Read(ref _positiveAdds)
			+ " distinct_sites=" + lines.Count
			+ " folded_sites=" + _foldedSites
			+ " clamped_events=" + Interlocked.Read(ref _clampEvents)
			+ " clamped_men=" + Interlocked.Read(ref _clampedMen));
		if (lines.Count == 0)
		{
			Logger.LogImmediate(LogSource, "runaway report captured no positive addition to the main party roster; growth came from a source that never called TroopRoster.AddToCounts with a positive count (check cached totals, save/load restore, or another mod).");
			return;
		}
		StringBuilder builder = new StringBuilder();
		builder.Append("runaway call sites (most frequent first is authoritative):");
		for (int i = 0; i < lines.Count; i++)
		{
			builder.Append("\n[").Append(i + 1).Append("]").Append(lines[i]);
		}
		Logger.LogImmediate(LogSource, builder.ToString());
	}

	private static void CaptureSite(TroopRoster roster, CharacterObject character, int count)
	{
		Interlocked.Increment(ref _positiveAdds);
		StackTrace stack;
		try
		{
			stack = new StackTrace(2, true);
		}
		catch
		{
			return;
		}
		string signature = BuildSignature(stack);
		lock (CaptureLock)
		{
			if (SiteHits.TryGetValue(signature, out int hits))
			{
				SiteHits[signature] = hits + 1;
				return;
			}
			if (SiteOrder.Count >= MaxCapturedSites)
			{
				_foldedSites++;
				return;
			}
			SiteHits[signature] = 1;
			SiteOrder.Add(signature);
			SiteSamples[signature] = "    troop=" + SafeCharacterId(character)
				+ " added=" + count
				+ " roster=" + DescribeRosterKind(roster)
				+ " roster_total=" + roster.TotalManCount
				+ "\n" + BuildStackText(stack);
		}
	}

	private static string BuildSignature(StackTrace stack)
	{
		StringBuilder builder = new StringBuilder();
		int used = 0;
		for (int i = 0; i < stack.FrameCount && used < 4; i++)
		{
			var method = stack.GetFrame(i)?.GetMethod();
			if (method == null)
			{
				continue;
			}
			Type declaring = method.DeclaringType;
			if (declaring == typeof(MainPartyRosterRunawayGuard))
			{
				continue;
			}
			if (used > 0)
			{
				builder.Append(" <- ");
			}
			builder.Append(declaring?.FullName ?? "?").Append('.').Append(method.Name);
			used++;
		}
		return builder.Length == 0 ? "<unknown>" : builder.ToString();
	}

	private static string BuildStackText(StackTrace stack)
	{
		StringBuilder builder = new StringBuilder();
		int used = 0;
		for (int i = 0; i < stack.FrameCount && used < StackFrameLimit; i++)
		{
			StackFrame frame = stack.GetFrame(i);
			var method = frame?.GetMethod();
			if (method == null)
			{
				continue;
			}
			Type declaring = method.DeclaringType;
			if (declaring == typeof(MainPartyRosterRunawayGuard))
			{
				continue;
			}
			string line = "      at " + (declaring?.FullName ?? "?") + "." + method.Name;
			try
			{
				int fileLine = frame.GetFileLineNumber();
				if (fileLine > 0)
				{
					line += " (line " + fileLine + ")";
				}
			}
			catch
			{
			}
			if (line.Length > StackLineLength)
			{
				line = line.Substring(0, StackLineLength);
			}
			builder.Append('\n').Append(line);
			used++;
		}
		return builder.ToString();
	}

	private static string BuildReconciliation(TroopRoster roster)
	{
		try
		{
			if (roster == null)
			{
				return "roster_unavailable";
			}
			int elements = 0;
			int regulars = 0;
			int heroes = 0;
			int woundedRegulars = 0;
			var list = roster.GetTroopRoster();
			for (int i = 0; i < list.Count; i++)
			{
				TroopRosterElement element = list[i];
				if (element.Character == null || element.Number <= 0)
				{
					continue;
				}
				elements++;
				if (element.Character.IsHero)
				{
					heroes += element.Number;
				}
				else
				{
					regulars += element.Number;
					woundedRegulars += Math.Max(0, element.WoundedNumber);
				}
			}
			int cached = roster.TotalManCount;
			int actual = regulars + heroes;
			return "cached_total=" + cached
				+ " element_total=" + actual
				+ " regulars=" + regulars
				+ " heroes=" + heroes
				+ " wounded_regulars=" + woundedRegulars
				+ " stacks=" + elements
				+ " reconciled=" + (cached == actual);
		}
		catch (Exception ex)
		{
			return "reconciliation_failed " + ex.GetType().Name + ": " + ex.Message;
		}
	}

	private static string BuildRosterDump(TroopRoster roster)
	{
		try
		{
			if (roster == null)
			{
				return "roster_dump=unavailable";
			}
			StringBuilder builder = new StringBuilder("roster_dump=");
			var list = roster.GetTroopRoster();
			int written = 0;
			for (int i = 0; i < list.Count && written < RosterDumpLimit; i++)
			{
				TroopRosterElement element = list[i];
				if (element.Character == null || element.Number == 0)
				{
					continue;
				}
				if (written > 0)
				{
					builder.Append(' ');
				}
				builder.Append(SafeCharacterId(element.Character))
					.Append(':').Append(element.Number);
				if (element.WoundedNumber > 0)
				{
					builder.Append("(w").Append(element.WoundedNumber).Append(')');
				}
				written++;
			}
			if (list.Count > written)
			{
				builder.Append(" ...+").Append(list.Count - written).Append(" stacks");
			}
			return builder.ToString();
		}
		catch (Exception ex)
		{
			return "roster_dump_failed " + ex.GetType().Name + ": " + ex.Message;
		}
	}

	private static void EnsureCampaignGeneration()
	{
		int generation = SaveRuntimeGuard.CurrentGeneration > int.MaxValue ? int.MaxValue : (int)SaveRuntimeGuard.CurrentGeneration;
		if (generation == _campaignGeneration)
		{
			return;
		}
		_campaignGeneration = generation;
		bool wasArmed = Volatile.Read(ref _armed) != 0;
		if (wasArmed)
		{
			FlushReport("campaign_generation_changed");
		}
		_lastSampleSeconds = 0.0;
		_lastSampleCount = 0;
		_growingSamples = 0;
		_peakCount = 0;
		_runStartCount = 0;
	}

	private static void ResetForCampaignExit()
	{
		if (Volatile.Read(ref _armed) != 0)
		{
			FlushReport("campaign_exited");
		}
		_campaignGeneration = -1;
		_mainMemberRoster = null;
		_mainPrisonRoster = null;
		_lastSampleSeconds = 0.0;
		_lastSampleCount = 0;
		_growingSamples = 0;
	}

	private static string DescribeRosterKind(TroopRoster roster)
	{
		if (ReferenceEquals(roster, _mainMemberRoster))
		{
			return "member";
		}
		if (ReferenceEquals(roster, _mainPrisonRoster))
		{
			return "prisoner";
		}
		return "other";
	}

	private static double NowSeconds()
	{
		return (double)Stopwatch.GetTimestamp() / Stopwatch.Frequency;
	}

	private static string SafeCharacterId(CharacterObject character)
	{
		try
		{
			return character?.StringId ?? (character?.Name?.ToString() ?? "unknown");
		}
		catch
		{
			return "unknown";
		}
	}

	private static string SafeNowDay()
	{
		try
		{
			return CampaignTime.Now.ToDays.ToString("0.00");
		}
		catch
		{
			return "n/a";
		}
	}
}
