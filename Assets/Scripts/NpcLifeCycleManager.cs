using System.Collections.Generic;
using UnityEngine;

public class NpcLifeCycleManager : MonoBehaviour
{
    public static NpcLifeCycleManager Instance { get; private set; }

    [Header("Gorsel")]
    [SerializeField] private GameObject npcVisualPrefab;
    [SerializeField] private NpcSchedule defaultSchedule;

    [Header("Simulasyon")]
    [SerializeField] private int simulatedHour = 8;
    [SerializeField] private float hourDurationSeconds = 8f;

    private readonly Dictionary<string, NpcAgent> agentsByGuestId = new Dictionary<string, NpcAgent>();
    private float hourTimer;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
    }

    private void Update()
    {
        if (agentsByGuestId.Count == 0)
            return;

        hourTimer += Time.deltaTime;
        if (hourTimer < hourDurationSeconds)
            return;

        hourTimer = 0f;
        simulatedHour = (simulatedHour + 1) % 24;
        DispatchScheduleForHour(simulatedHour);
    }

    public void RegisterAcceptedGuest(GuestProfile guest, RoomSlot roomSlot)
    {
        if (guest == null || roomSlot == null)
            return;

        if (agentsByGuestId.ContainsKey(guest.Id))
            return;

        GameObject visual = CreateVisual(roomSlot.BedAnchor.position, guest.DisplayName);
        var walker = visual.GetComponent<NpcWalker>();
        if (walker == null)
            walker = visual.AddComponent<NpcWalker>();

        var agent = new NpcAgent
        {
            GuestId = guest.Id,
            RoomSlot = roomSlot,
            Walker = walker,
            Visual = visual
        };

        agentsByGuestId[guest.Id] = agent;
        MoveAgentToRoom(agent);
    }

    public void UnregisterGuest(string guestId)
    {
        if (!agentsByGuestId.TryGetValue(guestId, out NpcAgent agent))
            return;

        if (agent.Visual != null)
            Destroy(agent.Visual);

        agentsByGuestId.Remove(guestId);
    }

    private GameObject CreateVisual(Vector2 position, string displayName)
    {
        GameObject visual;
        if (npcVisualPrefab != null)
        {
            visual = Instantiate(npcVisualPrefab, position, Quaternion.identity, transform);
        }
        else
        {
            visual = new GameObject($"NPC_{displayName}");
            visual.transform.SetParent(transform);
            visual.transform.position = position;

            var spriteRenderer = visual.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = CreatePlaceholderSprite();
            spriteRenderer.sortingOrder = 10;
        }

        visual.name = $"NPC_{displayName}";
        return visual;
    }

    private static Sprite CreatePlaceholderSprite()
    {
        var texture = new Texture2D(8, 8);
        Color[] pixels = new Color[64];
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = new Color(0.85f, 0.75f, 0.55f, 1f);

        texture.SetPixels(pixels);
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, 8, 8), new Vector2(0.5f, 0.5f), 16f);
    }

    private void DispatchScheduleForHour(int hour)
    {
        string locationId = defaultSchedule != null
            ? defaultSchedule.GetLocationForHour(hour)
            : GetFallbackLocationForHour(hour);

        foreach (NpcAgent agent in agentsByGuestId.Values)
            MoveAgentToLocation(agent, locationId);
    }

    private static string GetFallbackLocationForHour(int hour)
    {
        if (hour >= 8 && hour < 10)
            return "yemekhane";

        if (hour >= 10 && hour < 19)
            return "salon";

        if (hour >= 19 && hour < 21)
            return "yemekhane";

        return "oda";
    }

    private void MoveAgentToRoom(NpcAgent agent)
    {
        if (agent?.Walker == null || agent.RoomSlot == null)
            return;

        agent.Walker.MoveTo(agent.RoomSlot.BedAnchor.position);
    }

    private void MoveAgentToLocation(NpcAgent agent, string locationId)
    {
        if (agent?.Walker == null)
            return;

        if (locationId == "oda" && agent.RoomSlot != null)
        {
            MoveAgentToRoom(agent);
            return;
        }

        LocationAnchor anchor = LocationAnchorRegistry.Find(locationId);
        if (anchor == null)
        {
            MoveAgentToRoom(agent);
            return;
        }

        agent.Walker.MoveTo(anchor.Position);
    }

    private class NpcAgent
    {
        public string GuestId;
        public RoomSlot RoomSlot;
        public NpcWalker Walker;
        public GameObject Visual;
    }
}
