using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GuestManager : MonoBehaviour
{
    public static GuestManager Instance { get; private set; }

    [Header("Kapasite (RoomSlot yoksa yedek)")]
    [SerializeField] private string[] fallbackRooms = { "Oda 1", "Oda 2", "Oda 3", "Oda 4" };

    [Header("Gunluk Varis")]
    [SerializeField] private int initialArrivalCount = 3;
    [SerializeField] private int dailyArrivalMin = 1;
    [SerializeField] private int dailyArrivalMax = 2;

    [Header("Vampir Havuzu (rastgele adaylar)")]
    [SerializeField] private int randomVampireCountMin = 2;
    [SerializeField] private int randomVampireCountMax = 4;

    private readonly List<GuestProfile> guests = new List<GuestProfile>();
    private readonly Dictionary<string, RoomSlot> guestRoomSlots = new Dictionary<string, RoomSlot>();
    private int arrivalCounter;
    private int lastNightRefunds;
    private bool runInitialized;

    public int LastNightRefunds => lastNightRefunds;

    public IReadOnlyList<GuestProfile> AllGuests => guests;
    public int MaxCapacity => RoomSlotRegistry.Count > 0 ? RoomSlotRegistry.Count : fallbackRooms.Length;
    public int OccupiedRoomCount => GetStayingGuests().Count;

    public event Action OnGuestsChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
        EnsureRunInitialized();
        SpawnInitialArrivals();
    }

    public void SpawnInitialArrivals()
    {
        for (int i = 0; i < initialArrivalCount; i++)
            SpawnRandomGuest();
    }

    public void SpawnDailyArrivals()
    {
        int count = UnityEngine.Random.Range(dailyArrivalMin, dailyArrivalMax + 1);
        for (int i = 0; i < count; i++)
            SpawnRandomGuest();
    }

    public List<GuestProfile> GetPendingGuests() =>
        guests.Where(g => g.IsAlive && g.IsPending).ToList();

    public List<GuestProfile> GetStayingGuests() =>
        guests.Where(g => g.IsAlive && g.IsAccepted && g.IsStaying).ToList();

    public List<GuestProfile> GetAttackersTonight() =>
        guests.Where(g => g.CanAttackTonight).ToList();

    public List<GuestProfile> GetPotentialVictimsTonight() =>
        guests.Where(g => g.CanBeKilledTonight).ToList();

    public int CountActiveVampires() =>
        guests.Count(g => g.IsAlive && g.IsVampire && g.IsAccepted && g.IsStaying && !g.IsInDungeon);

    public bool HasFreeRoom()
    {
        if (RoomSlotRegistry.Count > 0)
            return RoomSlotRegistry.GetFirstFree(GetOccupiedRoomIds()) != null;

        return OccupiedRoomCount < fallbackRooms.Length;
    }

    public bool AcceptGuest(string guestId)
    {
        GuestProfile guest = FindGuest(guestId);
        if (guest == null || !guest.IsPending || !guest.IsAlive || !HasFreeRoom())
            return false;

        GameManager.EnsureInstance();
        guest.UcretGunluk = ReputationSystem.ScaleGuestPay(guest.UcretGunluk);
        int prepayment = guest.PrepaymentTotal;
        if (GameManager.Instance != null)
            GameManager.Instance.AddGold(prepayment);

        guest.Decision = GuestDecision.Accepted;
        guest.IsStaying = true;
        guest.OnOdemeAlindi = prepayment;
        guest.KalinanGun = 0;

        RoomSlot roomSlot = AssignRoom(guest);
        if (roomSlot != null && NpcLifeCycleManager.Instance != null)
            NpcLifeCycleManager.Instance.RegisterAcceptedGuest(guest, roomSlot);

        OnGuestsChanged?.Invoke();
        return true;
    }

    public bool RejectGuest(string guestId)
    {
        GuestProfile guest = FindGuest(guestId);
        if (guest == null || !guest.IsPending || !guest.IsAlive)
            return false;

        guest.Decision = GuestDecision.Rejected;
        guest.IsStaying = false;
        ClearGuestRoom(guest);
        OnGuestsChanged?.Invoke();
        return true;
    }

    public void BeginNightEconomy()
    {
        lastNightRefunds = 0;
    }

    public DayIncomeResult CollectNightlyHostingCosts()
    {
        var result = new DayIncomeResult();

        foreach (GuestProfile guest in GetStayingGuests())
        {
            result.TotalHostingCost += guest.MaliyetGunluk;
            result.Lines.Add($"Barindirma ({guest.DisplayName}): -{guest.MaliyetGunluk} altin");

            guest.KalinanGun++;

            if (guest.KalinanGun >= guest.PlanlananGun)
            {
                guest.IsStaying = false;
                ReleaseGuestRoom(guest);
                result.Lines.Add($"{guest.DisplayName} konaklamasini tamamladi ve ayrildi.");
            }
        }

        return result;
    }

    public void AppendRefundsToReport(DayIncomeResult result)
    {
        if (lastNightRefunds <= 0)
            return;

        result.TotalRefunds = lastNightRefunds;
        result.Lines.Add($"Iade (erken olum/ayrilis): +{lastNightRefunds} altin");
    }

    public void KillGuest(string guestId)
    {
        GuestProfile guest = FindGuest(guestId);
        if (guest == null || !guest.IsAlive)
            return;

        int refund = guest.CalculateRefund();
        if (refund > 0 && GameManager.Instance != null)
        {
            GameManager.Instance.AddGold(refund);
            lastNightRefunds += refund;
        }

        guest.IsAlive = false;
        guest.IsStaying = false;
        ReleaseGuestRoom(guest);
        OnGuestsChanged?.Invoke();
    }

    public void ClearNightProtection()
    {
        foreach (GuestProfile guest in guests)
            guest.IsProtectedTonight = false;
    }

    public GuestProfile FindGuest(string guestId) =>
        guests.FirstOrDefault(g => g.Id == guestId);

    private void EnsureRunInitialized()
    {
        if (runInitialized)
            return;

        GameManager.EnsureInstance();
        int runSeed = GameManager.Instance != null ? GameManager.Instance.RunSeed : Environment.TickCount;
        var seedRandom = new System.Random(runSeed);
        int targetCount = seedRandom.Next(randomVampireCountMin, randomVampireCountMax + 1);
        NpcRunVampirePool.Initialize(runSeed, targetCount);
        runInitialized = true;
    }

    private void SpawnRandomGuest()
    {
        EnsureRunInitialized();

        IReadOnlyList<NpcCharacterDefinition> pool = NpcCatalog.GetActiveCharacters();
        var available = new List<NpcCharacterDefinition>();
        foreach (NpcCharacterDefinition character in pool)
        {
            bool alreadyHere = guests.Any(g =>
                g.CatalogId == character.id && g.IsAlive && (g.IsPending || g.IsStaying));
            if (!alreadyHere)
                available.Add(character);
        }

        if (available.Count == 0)
            return;

        NpcCharacterDefinition template = available[UnityEngine.Random.Range(0, available.Count)];
        arrivalCounter++;

        int runSeed = GameManager.Instance != null ? GameManager.Instance.RunSeed : Environment.TickCount;
        int rollSeed = runSeed ^ template.id ^ arrivalCounter;
        var economyRandom = new System.Random(rollSeed);
        template.RollEconomy(economyRandom, out int planlananGun, out int ucretGunluk, out int maliyetGunluk);

        var guest = new GuestProfile
        {
            Id = $"{template.id}_{CurrentDaySuffix()}_{arrivalCounter}",
            CatalogId = template.id,
            DisplayName = template.DisplayName,
            Description = template.aciklama,
            UcretGunluk = ucretGunluk,
            MaliyetGunluk = maliyetGunluk,
            PlanlananGun = planlananGun,
            IsVampire = NpcRunVampirePool.IsVampireThisRun(template),
            Decision = GuestDecision.Pending,
            IsStaying = false,
            RoomName = "",
            RoomSlotId = ""
        };

        guests.Add(guest);
        OnGuestsChanged?.Invoke();
    }

    private RoomSlot AssignRoom(GuestProfile guest)
    {
        if (RoomSlotRegistry.Count > 0)
        {
            RoomSlot slot = RoomSlotRegistry.GetFirstFree(GetOccupiedRoomIds());
            if (slot == null)
                return null;

            guest.RoomName = slot.RoomDisplayName;
            guest.RoomSlotId = slot.RoomId;
            guestRoomSlots[guest.Id] = slot;
            return slot;
        }

        guest.RoomName = GetNextFallbackRoom();
        guest.RoomSlotId = guest.RoomName;
        return null;
    }

    private void ClearGuestRoom(GuestProfile guest)
    {
        guest.RoomName = "";
        guest.RoomSlotId = "";
        guestRoomSlots.Remove(guest.Id);
    }

    private void ReleaseGuestRoom(GuestProfile guest)
    {
        if (NpcLifeCycleManager.Instance != null)
            NpcLifeCycleManager.Instance.UnregisterGuest(guest.Id);

        ClearGuestRoom(guest);
    }

    private IEnumerable<string> GetOccupiedRoomIds()
    {
        foreach (GuestProfile guest in GetStayingGuests())
        {
            if (!string.IsNullOrEmpty(guest.RoomSlotId))
                yield return guest.RoomSlotId;
        }
    }

    private string GetNextFallbackRoom()
    {
        HashSet<string> usedRooms = new HashSet<string>(
            GetStayingGuests().Select(g => g.RoomName));

        foreach (string room in fallbackRooms)
        {
            if (!usedRooms.Contains(room))
                return room;
        }

        return fallbackRooms[0];
    }

    private static string CurrentDaySuffix()
    {
        if (GameManager.Instance != null)
            return GameManager.Instance.CurrentDay.ToString();

        return "0";
    }
}
