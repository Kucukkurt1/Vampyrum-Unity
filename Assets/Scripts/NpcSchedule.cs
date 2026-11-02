using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GuestDailySchedule", menuName = "Vampyrum/NPC Schedule")]
public class NpcSchedule : ScriptableObject
{
    [SerializeField] private List<NpcScheduleEntry> entries = new List<NpcScheduleEntry>();

    public IReadOnlyList<NpcScheduleEntry> Entries => entries;

    public string GetLocationForHour(int hour)
    {
        if (entries == null || entries.Count == 0)
            return "yemekhane";

        NpcScheduleEntry selected = entries[0];
        foreach (NpcScheduleEntry entry in entries)
        {
            if (entry.hour <= hour)
                selected = entry;
            else
                break;
        }

        return selected.locationId;
    }
}

[Serializable]
public class NpcScheduleEntry
{
    public int hour;
    public string locationId = "yemekhane";
}
