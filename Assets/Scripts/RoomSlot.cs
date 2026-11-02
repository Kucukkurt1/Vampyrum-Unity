using System.Collections.Generic;
using UnityEngine;

public class RoomSlot : MonoBehaviour
{
    [SerializeField] private string roomId = "oda_1";
    [SerializeField] private string roomDisplayName = "Oda 1";
    [SerializeField] private Transform bedAnchor;
    [SerializeField] private Transform doorAnchor;

    public string RoomId => roomId;
    public string RoomDisplayName => roomDisplayName;
    public Transform BedAnchor => bedAnchor != null ? bedAnchor : transform;
    public Transform DoorAnchor => doorAnchor != null ? doorAnchor : transform;

    private void OnEnable() => RoomSlotRegistry.Register(this);

    private void OnDisable() => RoomSlotRegistry.Unregister(this);
}

public static class RoomSlotRegistry
{
    private static readonly List<RoomSlot> slots = new List<RoomSlot>();

    public static void Register(RoomSlot slot)
    {
        if (slot == null || slots.Contains(slot))
            return;

        slots.Add(slot);
        slots.Sort((a, b) => string.CompareOrdinal(a.RoomId, b.RoomId));
    }

    public static void Unregister(RoomSlot slot)
    {
        if (slot != null)
            slots.Remove(slot);
    }

    public static IReadOnlyList<RoomSlot> All => slots;

    public static RoomSlot FindById(string id) =>
        slots.Find(slot => slot.RoomId == id);

    public static RoomSlot GetFirstFree(IEnumerable<string> occupiedRoomIds)
    {
        var occupied = new HashSet<string>(occupiedRoomIds);
        foreach (RoomSlot slot in slots)
        {
            if (!occupied.Contains(slot.RoomId))
                return slot;
        }

        return null;
    }

    public static int Count => slots.Count;
}
