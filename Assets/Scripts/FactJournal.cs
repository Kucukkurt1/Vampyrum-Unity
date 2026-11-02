using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FactJournal : MonoBehaviour
{
    public static FactJournal Instance { get; private set; }

    private readonly List<NightMurderRecord> murderRecords = new List<NightMurderRecord>();
    private readonly Dictionary<string, int> talkCounts = new Dictionary<string, int>();
    private readonly Dictionary<string, HashSet<string>> playedBeats = new Dictionary<string, HashSet<string>>();
    private readonly HashSet<string> globalFlags = new HashSet<string>();

    public IReadOnlyList<NightMurderRecord> MurderRecords => murderRecords;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
    }

    public void RecordMurder(NightMurderRecord record)
    {
        if (record == null)
            return;

        murderRecords.Add(record);
    }

    public NightMurderRecord GetLatestMurder() =>
        murderRecords.Count > 0 ? murderRecords[murderRecords.Count - 1] : null;

    public WitnessStatementRecord GetPendingWitnessStatement(string guestId)
    {
        NightMurderRecord latest = GetLatestMurder();
        if (latest?.Witnesses == null)
            return null;

        return latest.Witnesses.FirstOrDefault(
            witness => witness.WitnessGuestId == guestId && witness.PendingPlayerTalk);
    }

    public void MarkWitnessStatementDelivered(string guestId, DialogueOutcome outcome)
    {
        WitnessStatementRecord pending = GetPendingWitnessStatement(guestId);
        if (pending == null)
            return;

        pending.PendingPlayerTalk = false;
        pending.DeliveredOutcome = outcome;
    }

    public int GetTalkCount(string guestId) =>
        talkCounts.TryGetValue(guestId, out int count) ? count : 0;

    public void IncrementTalkCount(string guestId)
    {
        if (!talkCounts.ContainsKey(guestId))
            talkCounts[guestId] = 0;

        talkCounts[guestId]++;
    }

    public bool HasPlayedBeat(string guestId, string beatId)
    {
        if (!playedBeats.TryGetValue(guestId, out HashSet<string> beats))
            return false;

        return beats.Contains(beatId);
    }

    public void MarkBeatPlayed(string guestId, string beatId)
    {
        if (!playedBeats.TryGetValue(guestId, out HashSet<string> beats))
        {
            beats = new HashSet<string>();
            playedBeats[guestId] = beats;
        }

        beats.Add(beatId);
    }

    public bool HasFlag(string flag) =>
        !string.IsNullOrEmpty(flag) && globalFlags.Contains(flag);

    public void SetFlag(string flag)
    {
        if (!string.IsNullOrEmpty(flag))
            globalFlags.Add(flag);
    }
}
