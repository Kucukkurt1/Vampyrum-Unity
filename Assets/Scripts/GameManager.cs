using System;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GamePhase
{
    Morning,
    Day,
    Night
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Baslangic")]
    [SerializeField] private int startingGold = 100;
    [SerializeField] private int startingDay = 1;

    [Header("Gunluk Sabit Giderler")]
    [SerializeField] private int dailyMaintenanceCost = 5;
    [SerializeField] private int dailyTaxCost = 3;

    [Header("Gece Simulasyonu")]
    [SerializeField] [Range(0f, 1f)] private float vampireAttackChance = 0.4f;
    [SerializeField] private int vampireGameOverThreshold = 3;

    public int CurrentDay { get; private set; }
    public int Gold { get; private set; }
    public int ClockHour { get; private set; } = 8;
    public int ClockMinute { get; private set; }
    public string ClockText => $"{ClockHour:00}:{ClockMinute:00}";
    public float ClockStepDuration => SecondsPerTenMinutes;
    public float ClockStepElapsed => clockAccumulator;
    public GamePhase CurrentPhase { get; private set; } = GamePhase.Day;
    public bool IsGameOver { get; private set; }
    public string LastGameOverReason { get; private set; } = "";
    public string LastMorningReport { get; private set; } = "";
    public NightSimulationResult LastNightResult { get; private set; }
    public DayIncomeResult LastDayIncome { get; private set; }
    public int LastGoldBeforeIncome { get; private set; }
    public int LastGoldAfterIncome { get; private set; }
    public int LastMaintenanceCost { get; private set; }
    public int LastTaxCost { get; private set; }
    public int RunSeed { get; private set; }

    public event Action<int> OnDayChanged;
    public event Action<int> OnGoldChanged;
    public event Action<GamePhase> OnPhaseChanged;
    public event Action<string> OnMorningReportReady;
    public event Action OnGameOver;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        CurrentDay = startingDay;
        Gold = startingGold;
        CurrentPhase = GamePhase.Day;
        RunSeed = unchecked(Environment.TickCount * 397 ^ Guid.NewGuid().GetHashCode());

        EnsureSubsystems();
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void BootstrapPersistentSystems()
    {
        EnsureInstance();
        SceneManager.sceneLoaded -= OnAnySceneLoaded;
        SceneManager.sceneLoaded += OnAnySceneLoaded;
    }

    private static void OnAnySceneLoaded(Scene scene, LoadSceneMode mode)
    {
        EnsureInstance();
    }

    public static void EnsureInstance()
    {
        if (Instance != null)
            return;

        var managerObject = new GameObject("GameManager");
        managerObject.AddComponent<GameManager>();
    }

    private void EnsureSubsystems()
    {
        if (GetComponent<GuestManager>() == null)
            gameObject.AddComponent<GuestManager>();

        if (GetComponent<MorningReportUI>() == null)
            gameObject.AddComponent<MorningReportUI>();

        if (GetComponent<GameOverUI>() == null)
            gameObject.AddComponent<GameOverUI>();

        if (GetComponent<CaravansaryManagementUI>() == null)
            gameObject.AddComponent<CaravansaryManagementUI>();

        if (GetComponent<WizardRoomManager>() == null)
            gameObject.AddComponent<WizardRoomManager>();

        if (GetComponent<NpcLifeCycleManager>() == null)
            gameObject.AddComponent<NpcLifeCycleManager>();

        if (GetComponent<FactJournal>() == null)
            gameObject.AddComponent<FactJournal>();

        if (GetComponent<ReputationSystem>() == null)
            gameObject.AddComponent<ReputationSystem>();

        if (GetComponent<DialogueUI>() == null)
            gameObject.AddComponent<DialogueUI>();

        if (GetComponent<StaffManager>() == null)
            gameObject.AddComponent<StaffManager>();

        if (GetComponent<InventoryManager>() == null)
            gameObject.AddComponent<InventoryManager>();

        if (GetComponent<MerchantManager>() == null)
            gameObject.AddComponent<MerchantManager>();

        if (GetComponent<MerchantTradeUI>() == null)
            gameObject.AddComponent<MerchantTradeUI>();

        if (GetComponent<WizardGrimoireUI>() == null)
            gameObject.AddComponent<WizardGrimoireUI>();
    }

    private void Update()
    {
        if (IsGameOver || CurrentPhase != GamePhase.Day || Time.timeScale <= 0f)
            return;

        clockAccumulator += Time.deltaTime;
        while (clockAccumulator >= SecondsPerTenMinutes)
        {
            clockAccumulator -= SecondsPerTenMinutes;
            ClockMinute += 10;
            if (ClockMinute < 60)
                continue;

            ClockMinute = 0;
            ClockHour = (ClockHour + 1) % 24;
        }
    }

    public void EndDay()
    {
        if (IsGameOver || CurrentPhase == GamePhase.Night)
            return;

        SetPhase(GamePhase.Night);
        MerchantManager.Instance?.Depart();

        if (GuestManager.Instance != null)
        {
            GuestManager.Instance.ClearNightProtection();
            GuestManager.Instance.BeginNightEconomy();
        }

        LastDayIncome = GuestManager.Instance != null
            ? GuestManager.Instance.CollectNightlyHostingCosts()
            : new DayIncomeResult();

        int goldBeforeNight = Gold;
        Gold -= LastDayIncome.TotalHostingCost;
        OnGoldChanged?.Invoke(Gold);
        LastGoldBeforeIncome = goldBeforeNight;
        LastGoldAfterIncome = Gold;

        LastNightResult = NightSimulator.Simulate(vampireAttackChance);
        int staffWages = StaffManager.Instance != null ? StaffManager.Instance.NightlyWage : 0;
        StaffManager.Instance?.ResolveNight(LastNightResult);

        GuestManager.Instance?.AppendRefundsToReport(LastDayIncome);

        int maintenance = dailyMaintenanceCost;
        int tax = dailyTaxCost;
        int totalGuestCosts = LastDayIncome.TotalHostingCost;
        int totalExpenses = maintenance + tax + totalGuestCosts;
        int goldBeforeFixedExpenses = Gold;
        LastMaintenanceCost = maintenance;
        LastTaxCost = tax;

        Gold -= totalExpenses + staffWages;
        OnGoldChanged?.Invoke(Gold);

        CurrentDay++;
        OnDayChanged?.Invoke(CurrentDay);

        GuestManager.Instance?.SpawnDailyArrivals();

        LastMorningReport = BuildMorningReport(
            goldBeforeNight,
            goldBeforeFixedExpenses,
            maintenance,
            tax,
            totalExpenses,
            staffWages,
            LastDayIncome,
            LastNightResult);

        if (Gold < 0)
        {
            TriggerGameOver("Kasa eksiye dustu. Kervansaray kapandi.");
            return;
        }

        if (LastNightResult.ActiveVampireCount >= vampireGameOverThreshold)
        {
            TriggerGameOver("Kervansaray vampir tehdidi altinda coktu. Isletme kapandi.");
            return;
        }

        MerchantManager.Instance?.Restock();

        SetPhase(GamePhase.Morning);
        OnMorningReportReady?.Invoke(LastMorningReport);
        Debug.Log(LastMorningReport);

        SetPhase(GamePhase.Day);
        ResetClock();
    }

    public void AddGold(int amount)
    {
        if (IsGameOver || amount <= 0)
            return;

        Gold += amount;
        OnGoldChanged?.Invoke(Gold);
    }

    public bool SpendGold(int amount)
    {
        if (IsGameOver || amount <= 0 || Gold < amount)
            return false;

        Gold -= amount;
        OnGoldChanged?.Invoke(Gold);
        return true;
    }

    private string BuildMorningReport(
        int goldBeforeIncome,
        int goldBeforeExpenses,
        int maintenance,
        int tax,
        int totalExpenses,
        int staffWages,
        DayIncomeResult income,
        NightSimulationResult nightResult)
    {
        var report = new StringBuilder();
        report.AppendLine($"=== Sabah Raporu - Gun {CurrentDay} ===");
        report.AppendLine();
        report.AppendLine(NightSimulator.FormatNightSection(nightResult));
        report.AppendLine();

        if (income.TotalRefunds > 0)
        {
            report.AppendLine("--- Iadeler ---");
            report.AppendLine($"Erken olum/ayrilis iadesi: +{income.TotalRefunds} altin");
            report.AppendLine($"Kasa: {goldBeforeIncome} -> {goldBeforeExpenses} altin");
            report.AppendLine();
        }

        report.AppendLine("--- Gece Giderleri ---");
        if (income.TotalHostingCost > 0)
        {
            foreach (string line in income.Lines)
            {
                if (line.StartsWith("Barindirma", StringComparison.Ordinal))
                    report.AppendLine(line);
            }

            report.AppendLine($"Toplam barindirma: -{income.TotalHostingCost} altin");
        }
        else
        {
            report.AppendLine("Konaklayan misafir olmadigi icin barindirma gideri yok.");
        }

        report.AppendLine();
        report.AppendLine("--- Sabit Giderler ---");
        report.AppendLine($"Bakim gideri: -{maintenance} altin");
        report.AppendLine($"Vergi: -{tax} altin");
        if (staffWages > 0)
            report.AppendLine($"Personel maasi: -{staffWages} altin");
        report.AppendLine($"Toplam gider: -{totalExpenses + staffWages} altin");
        report.AppendLine($"Kasa: {goldBeforeExpenses} -> {Gold} altin");

        if (GuestManager.Instance != null)
        {
            report.AppendLine();
            report.AppendLine("--- Misafirler ---");
            foreach (GuestProfile guest in GuestManager.Instance.GetStayingGuests())
                report.AppendLine($"- {guest.DisplayName} ({guest.RoomName})");

            foreach (GuestProfile guest in GuestManager.Instance.AllGuests)
            {
                if (!guest.IsAlive)
                    report.AppendLine($"- {guest.DisplayName}: OLDU");
            }
        }

        return report.ToString();
    }

    private const float SecondsPerTenMinutes = 9f;
    private float clockAccumulator;

    private void ResetClock()
    {
        ClockHour = 8;
        ClockMinute = 0;
        clockAccumulator = 0f;
    }

    private void SetPhase(GamePhase phase)
    {
        CurrentPhase = phase;
        OnPhaseChanged?.Invoke(phase);
    }

    private void TriggerGameOver(string reason)
    {
        IsGameOver = true;
        LastGameOverReason = reason;
        SetPhase(GamePhase.Night);
        Debug.LogWarning($"GAME OVER: {reason}");
        OnGameOver?.Invoke();
    }
}
