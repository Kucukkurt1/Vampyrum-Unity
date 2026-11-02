using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum ManagementTab
{
    Contents,
    Guests,
    Staying,
    Treasury,
    Report,
    Storage,
    Statistics,
    Signs,
    Notes,
    Staff
}

public class CaravansaryManagementUI : MonoBehaviour
{
    public static CaravansaryManagementUI Instance { get; private set; }

    [SerializeField] private KeyCode closeKey = KeyCode.Escape;

    // false yapilirsa onceki kucuk kitap paneli tekrar kullanilir.
    private static readonly bool UseFullscreenParchment = true;

    private GameObject uiCanvasRoot;
    private GameObject panelRoot;
    private GameObject guestsTab;
    private GameObject treasuryTab;
    private GameObject reportTab;

    private GameObject guestSpreadRoot;
    private GameObject contentsRoot;
    private GameObject intakeRoot;
    private GameObject guestListRoot;
    private GameObject openingArtRoot;
    private Transform guestNameListRoot;
    private Text guestListEmptyText;
    private bool showingGuestDetail;
    private GameObject stayingRoot;
    private GameObject storageRoot;
    private GameObject statisticsRoot;
    private GameObject signsRoot;
    private GameObject notesRoot;
    private GameObject staffDetailRoot;
    private Transform staffTableRoot;
    private Text staffRoleTitle;
    private Text staffNameText;
    private Text staffStoryText;
    private Text staffPageText;
    private Text staffPortraitLabel;
    private Image staffPortraitImage;
    private Button hireStaffButton;
    private Text hireStaffLabel;
    private int staffRoleIndex;
    private int staffLevel = 1;
    private static readonly StaffRole[] StaffOrder =
    {
        StaffRole.Wizard,
        StaffRole.Watch,
        StaffRole.Cook,
        StaffRole.Runner
    };
    private InputField notesInput;
    private Text stayingText;
    private Text storageText;
    private Text statisticsText;
    private Text treasuryPageText;
    private Text reportPageText;

    private const string ReportPlaceholder =
        "Sabah oldu. Gece sakin geçti.\n\n" +
        "Kapı nöbeti boyunca avluda bagrısma duyulmadı. Mumlar sırayla söndü, odaların kapısı içeriden sürgülü kaldı. Dısarıdan gelen bir sikayet de yazılmadı.\n\n" +
        "Yaralı, kayıp ya da kovulan yolcu yok. Kasa mühürlü bırakıldı, depo kilidi yerinde.\n\n" +
        "Yarın kapıya çıkacaklar henüz deftere geçmedi. Gece olanlar baglandıgında bu sayfa satır satır dolacak.";

    private const string TreasuryPlaceholder =
        "Kasa bu sabah açıldı ve sayıldı.\n\n" +
        "Içerideki altın, gece alınan pesin ücret ve sabah düsülen gider aynı sayfada durur. Barınma, bakım ve vergi ayrı kalem olarak islenir.\n\n" +
        "Mevcut altın henüz yazılmadı.\n" +
        "Son gece gelir henüz yazılmadı.\n" +
        "Son gece gider henüz yazılmadı.\n" +
        "Bakım ve vergi satırı bos.\n\n" +
        "Mühür yerinde. Kalemler baglandıgında bu yazının yerine gerçek döküm gelecek.";

    private const string StayingPlaceholder =
        "Konaklama defteri.\n\n" +
        "Bu sayfada kervansarayda geceleyenler yazar. Her kayıtta ad, tutulan oda ve kalan gece bulunur. Sabah sayımında kalan gün bir eksilir. Süresi biten yolcu ugurlanır, odası bosalır.\n\n" +
        "Su an odalarda kayıtlı kimse yok. Kapıdan kabul edilen ilk yolcunun adı, odası ve kalacagı gece buraya geçecek.";

    private const string StoragePlaceholder =
        "Depo sayımı.\n\n" +
        "Raflar kilitli. Asagıdaki malzemeler kervansarayın elindeki stoktur. Bir kalem gece ya da iksir için kullanılırsa adedi buradan düsülür.\n\n" +
        "Ay otu — sayılmadı\n" +
        "Gümüs tozu — sayılmadı\n" +
        "Kutsal su — sayılmadı\n" +
        "Kurtboganı — sayılmadı\n\n" +
        "Sayım baglandıgında her çizginin karsısına eldeki adet yazılacak.";

    private const string StatisticsPlaceholder =
        "Kervansarayın özeti.\n\n" +
        "Gün ilerledikçe bu sayfa isletmenin halini tutar. Altın, doluluk ve itibar aynı bakısta okunur. Kapıda bekleyen ile içeride kalan ayrı yazılır.\n\n" +
        "Gün — islenmedi\n" +
        "Altın — islenmedi\n" +
        "Doluluk — islenmedi\n" +
        "Kapıdaki misafir — islenmedi\n" +
        "Konaklayan — islenmedi\n" +
        "Itibar — islenmedi";

    private const string SignsBody =
        "Tek başına hiçbiri vampir demek değildir. Aynı yüzde birikince kapısına gidilir.\n\n" +
        "1. Tezcanlı konuşmalar\n" +
        "2. Sarımsaktan korkmak\n" +
        "3. Haçtan korkmak\n" +
        "4. Aynalardan korkmak\n" +
        "5. Geceleri koridorlarda dolaşmak\n" +
        "6. Yemeklerde iştahsızlık\n" +
        "7. Gümüş takı takmamak";

    private const string NotesPrefKey = "Vampyrum.Notebook";
    private GameObject guestPortraitFrame;
    private Image guestPortraitImage;
    private Text guestPortraitLabel;
    private Text guestStoryHeading;
    private Text guestNameText;
    private readonly Text[] ledgerValues = new Text[8];
    private Text guestStoryText;
    private Text guestPageText;
    private Text guestEmptyText;
    private Button acceptGuestButton;
    private Button rejectGuestButton;
    private Button previousGuestButton;
    private Button nextGuestButton;
    private Image acceptGuestButtonImage;
    private Transform reportContentRoot;
    private Transform treasuryContentRoot;

    private Text headerGoldText;
    private Text headerDayText;
    private Text capacityText;
    private Text treasuryGoldText;
    private Text treasuryIncomeText;
    private Text treasuryExpenseText;
    private Button expandDepotButton;
    private Text expandDepotLabel;

    private readonly Dictionary<ManagementTab, Image> tabImages = new Dictionary<ManagementTab, Image>();
    private readonly Dictionary<ManagementTab, Button> contentsButtons = new Dictionary<ManagementTab, Button>();
    private List<GuestProfile> pendingGuests = new List<GuestProfile>();

    private ManagementTab activeTab = ManagementTab.Guests;
    private int pendingGuestIndex;
    private bool isVisible;

    public bool IsVisible => isVisible;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
        BuildUI();
        Hide();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;

        UnsubscribeEvents();
    }

    private void Update()
    {
        if (!isVisible)
            return;

        if (Input.GetKeyDown(closeKey))
            Hide();
    }

    public void Show()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
            return;

        uiCanvasRoot.SetActive(true);
        isVisible = true;
        Time.timeScale = 0f;
        SubscribeEvents();
        if (UseFullscreenParchment)
            SetActiveTab(ManagementTab.Contents);
        RefreshAll();
    }

    public void Hide()
    {
        if (uiCanvasRoot != null)
            uiCanvasRoot.SetActive(false);

        isVisible = false;
        if (notesInput != null)
            PlayerPrefs.Save();

        if (Time.timeScale == 0f)
            Time.timeScale = 1f;

        UnsubscribeEvents();
    }

    public void Toggle()
    {
        if (isVisible)
            Hide();
        else
            Show();
    }

    private void SubscribeEvents()
    {
        if (GuestManager.Instance != null)
            GuestManager.Instance.OnGuestsChanged += RefreshAll;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGoldChanged += OnGoldChanged;
            GameManager.Instance.OnDayChanged += OnDayChanged;
        }

        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnInventoryChanged += OnInventoryChanged;

        if (StaffManager.Instance != null)
            StaffManager.Instance.OnStaffChanged += RefreshStaffPage;
    }

    private void UnsubscribeEvents()
    {
        if (GuestManager.Instance != null)
            GuestManager.Instance.OnGuestsChanged -= RefreshAll;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGoldChanged -= OnGoldChanged;
            GameManager.Instance.OnDayChanged -= OnDayChanged;
        }

        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnInventoryChanged -= OnInventoryChanged;

        if (StaffManager.Instance != null)
            StaffManager.Instance.OnStaffChanged -= RefreshStaffPage;
    }

    private void OnGoldChanged(int _) => RefreshAll();
    private void OnDayChanged(int _) => RefreshAll();
    private void OnInventoryChanged() => RefreshStoragePage();

    private void RefreshAll()
    {
        RefreshHeader();
        RefreshGuestsTab();
        RefreshStayingPage();
        RefreshStoragePage();
        RefreshStatisticsPage();
        RefreshTreasuryTab();
        RefreshReportTab();
        RefreshStaffPage();
    }

    private void RefreshHeader()
    {
        if (GameManager.Instance == null || headerGoldText == null || headerDayText == null)
            return;

        headerGoldText.text = $"{GameManager.Instance.Gold}";
        headerDayText.text = $"{GameManager.Instance.CurrentDay}";

        if (GuestManager.Instance != null && capacityText != null)
        {
            capacityText.text =
                $"{GuestManager.Instance.OccupiedRoomCount} / {GuestManager.Instance.MaxCapacity}";
        }
    }

    private void RefreshGuestsTab()
    {
        if (GuestManager.Instance == null)
        {
            pendingGuests = new List<GuestProfile>();
            if (UseFullscreenParchment)
            {
                showingGuestDetail = false;
                RebuildGuestNameList("Misafir bilgileri hazır degil.");
                ApplyBookVisibility();
            }
            else
            {
                BindEmptyGuestSpread("Misafir bilgileri hazir degil.");
            }
            return;
        }

        pendingGuests = GuestManager.Instance.GetPendingGuests();
        if (pendingGuests.Count == 0)
        {
            pendingGuestIndex = 0;
            showingGuestDetail = false;
            if (UseFullscreenParchment)
            {
                RebuildGuestNameList("Kapıda bekleyen misafir yok.");
                ApplyBookVisibility();
            }
            else
            {
                BindEmptyGuestSpread("Kapida bekleyen misafir yok.");
            }
            return;
        }

        pendingGuestIndex = Mathf.Clamp(pendingGuestIndex, 0, pendingGuests.Count - 1);
        if (UseFullscreenParchment)
        {
            RebuildGuestNameList(null);
            if (showingGuestDetail)
                BindGuestSpread(pendingGuests[pendingGuestIndex]);
            ApplyBookVisibility();
            return;
        }

        BindGuestSpread(pendingGuests[pendingGuestIndex]);
    }

    private void RefreshTreasuryTab()
    {
        if (UseFullscreenParchment)
        {
            if (treasuryPageText != null)
                treasuryPageText.text = TreasuryPlaceholder;
            return;
        }

        if (GameManager.Instance == null)
        {
            treasuryGoldText.text = "-";
            treasuryIncomeText.text = "-";
            treasuryExpenseText.text = "-";
            return;
        }

        treasuryGoldText.text = $"{GameManager.Instance.Gold}";
        treasuryIncomeText.text = GameManager.Instance.LastDayIncome != null
            ? $"+{GameManager.Instance.LastDayIncome.TotalRefunds}"
            : "+0";
        int guestCosts = GameManager.Instance.LastDayIncome?.TotalHostingCost ?? 0;
        treasuryExpenseText.text = $"-{guestCosts + GameManager.Instance.LastMaintenanceCost + GameManager.Instance.LastTaxCost}";

        ClearChildren(treasuryContentRoot);

        AddTreasuryLine(treasuryContentRoot, "Son gece iade",
            GameManager.Instance.LastDayIncome?.TotalRefunds ?? 0, true);
        AddTreasuryLine(treasuryContentRoot, "Son gece barindirma",
            GameManager.Instance.LastDayIncome?.TotalHostingCost ?? 0, false);
        AddTreasuryLine(treasuryContentRoot, "Gunluk bakim",
            GameManager.Instance.LastMaintenanceCost, false);
        AddTreasuryLine(treasuryContentRoot, "Gunluk vergi",
            GameManager.Instance.LastTaxCost, false);

        AddTreasuryNote(treasuryContentRoot,
            $"Son gece barindirma sonrasi kasa: {GameManager.Instance.LastGoldBeforeIncome} → {GameManager.Instance.LastGoldAfterIncome} altin");
    }

    private void RefreshReportTab()
    {
        if (UseFullscreenParchment)
        {
            if (reportPageText != null)
                reportPageText.text = ReportPlaceholder;
            return;
        }

        if (GameManager.Instance == null || GameManager.Instance.LastNightResult == null)
            MorningReportViewBuilder.PopulateEmpty(reportContentRoot);
        else
            MorningReportViewBuilder.Populate(reportContentRoot, GameManager.Instance);
    }

    private void SetActiveTab(ManagementTab tab)
    {
        activeTab = tab;
        showingGuestDetail = false;
        if (UseFullscreenParchment)
        {
            guestsTab.SetActive(true);
            ApplyBookVisibility();
        }
        else
        {
            guestsTab.SetActive(tab == ManagementTab.Guests);
        }

        if (stayingRoot != null)
            stayingRoot.SetActive(tab == ManagementTab.Staying);
        if (storageRoot != null)
            storageRoot.SetActive(tab == ManagementTab.Storage);
        if (statisticsRoot != null)
            statisticsRoot.SetActive(tab == ManagementTab.Statistics);
        if (signsRoot != null)
            signsRoot.SetActive(tab == ManagementTab.Signs);
        if (notesRoot != null)
            notesRoot.SetActive(tab == ManagementTab.Notes);
        treasuryTab.SetActive(tab == ManagementTab.Treasury);
        reportTab.SetActive(tab == ManagementTab.Report);

        foreach (KeyValuePair<ManagementTab, Image> pair in tabImages)
        {
            pair.Value.color = pair.Key == tab
                ? CaravansaryUITheme.TabActive
                : CaravansaryUITheme.TabInactive;
        }
    }

    private void BindGuestSpread(GuestProfile guest)
    {
        NpcCharacterDefinition definition = NpcCatalog.GetById(guest.CatalogId);

        guestNameText.text = guest.DisplayName;
        guestStoryText.text = string.IsNullOrWhiteSpace(guest.Description)
            ? "Bu yolcu hikayesini anlatmak istemiyor."
            : guest.Description;

        string age = definition != null && definition.yas > 0 ? definition.yas.ToString() : "-";
        string gender = definition != null && !string.IsNullOrWhiteSpace(definition.cinsiyet)
            ? definition.cinsiyet
            : "-";
        string origin = definition != null && !string.IsNullOrWhiteSpace(definition.koken)
            ? definition.koken
            : "-";
        string occupation = definition != null && !string.IsNullOrWhiteSpace(definition.meslek)
            ? definition.meslek
            : "-";

        ledgerValues[0].text = age;
        ledgerValues[1].text = gender;
        ledgerValues[2].text = origin;
        ledgerValues[3].text = occupation;
        int dailyPay = ReputationSystem.ScaleGuestPay(guest.UcretGunluk);
        ledgerValues[4].text = guest.PlanlananGun + " gun";
        ledgerValues[5].text = dailyPay + " altin";
        ledgerValues[6].text = (dailyPay * guest.PlanlananGun) + " altin";
        ledgerValues[7].text = guest.MaliyetGunluk + "/gece";

        guestPageText.text = $"{pendingGuestIndex + 1} / {pendingGuests.Count}";
        guestEmptyText.gameObject.SetActive(false);
        ApplyGuestPortrait(definition);
        SetGuestControlsVisible(true);

        bool canAccept = GuestManager.Instance != null && GuestManager.Instance.HasFreeRoom();
        acceptGuestButton.interactable = canAccept;
        acceptGuestButtonImage.color = Color.clear;

        bool hasMultipleGuests = pendingGuests.Count > 1;
        previousGuestButton.gameObject.SetActive(hasMultipleGuests);
        nextGuestButton.gameObject.SetActive(hasMultipleGuests);
    }

    private void BindEmptyGuestSpread(string message)
    {
        guestNameText.text = "";
        for (int i = 0; i < ledgerValues.Length; i++)
        {
            if (ledgerValues[i] != null)
                ledgerValues[i].text = "";
        }
        guestStoryText.text = "";
        guestPageText.text = "";
        guestEmptyText.text = message;
        guestEmptyText.gameObject.SetActive(true);
        ApplyGuestPortrait(null);
        SetGuestControlsVisible(false);
    }

    private void ApplyGuestPortrait(NpcCharacterDefinition definition)
    {
        Sprite sprite = definition == null
            ? null
            : Resources.Load<Sprite>("portrait_" + definition.FileSlug);

        if (guestPortraitImage != null)
        {
            guestPortraitImage.sprite = sprite;
            guestPortraitImage.enabled = sprite != null;
            guestPortraitImage.color = Color.white;
        }

        if (guestPortraitLabel != null)
            guestPortraitLabel.gameObject.SetActive(false);
    }

    private static void StyleDecisionButton(Button button)
    {
        ColorBlock colors = button.colors;
        colors.normalColor = new Color(0.12f, 0.08f, 0.06f, 1f);
        colors.highlightedColor = new Color(0.45f, 0.28f, 0.14f, 1f);
        colors.pressedColor = new Color(0.06f, 0.04f, 0.03f, 1f);
        colors.selectedColor = colors.normalColor;
        colors.disabledColor = new Color(0.28f, 0.24f, 0.20f, 0.45f);
        colors.colorMultiplier = 1f;
        button.colors = colors;

        Outline stroke = button.GetComponentInChildren<Outline>();
        if (stroke != null)
            UnityEngine.Object.Destroy(stroke);
    }

    private void SetGuestControlsVisible(bool visible)
    {
        acceptGuestButton.gameObject.SetActive(visible);
        rejectGuestButton.gameObject.SetActive(visible);
        previousGuestButton.gameObject.SetActive(visible);
        nextGuestButton.gameObject.SetActive(visible);
        guestPageText.gameObject.SetActive(visible);
        if (guestPortraitFrame != null)
            guestPortraitFrame.SetActive(visible);
        if (guestStoryHeading != null)
            guestStoryHeading.gameObject.SetActive(visible);
    }

    private void ShowPreviousGuest()
    {
        if (pendingGuests.Count < 2)
            return;

        pendingGuestIndex = (pendingGuestIndex - 1 + pendingGuests.Count) % pendingGuests.Count;
        BindGuestSpread(pendingGuests[pendingGuestIndex]);
    }

    private void ShowNextGuest()
    {
        if (pendingGuests.Count < 2)
            return;

        pendingGuestIndex = (pendingGuestIndex + 1) % pendingGuests.Count;
        BindGuestSpread(pendingGuests[pendingGuestIndex]);
    }

    private void AcceptCurrentGuest()
    {
        if (pendingGuests.Count == 0)
            return;

        AcceptGuest(pendingGuests[pendingGuestIndex].Id);
    }

    private void RejectCurrentGuest()
    {
        if (pendingGuests.Count == 0)
            return;

        RejectGuest(pendingGuests[pendingGuestIndex].Id);
    }

    private void AcceptGuest(string guestId)
    {
        if (GuestManager.Instance == null)
            return;

        if (!GuestManager.Instance.AcceptGuest(guestId))
            Debug.Log("Misafir kabul edilemedi. Oda dolu olabilir.");
    }

    private void RejectGuest(string guestId)
    {
        GuestManager.Instance?.RejectGuest(guestId);
    }

    private void BuildUI()
    {
        if (UseFullscreenParchment)
            BuildFullscreenLedger();
        else
            BuildCompactLedger();
    }

    private void BuildFullscreenLedger()
    {
        CaravansaryUITheme.EnsureEventSystem();

        uiCanvasRoot = new GameObject("ManagementCanvas");
        uiCanvasRoot.transform.SetParent(transform);
        var canvas = uiCanvasRoot.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 150;
        var scaler = uiCanvasRoot.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        uiCanvasRoot.AddComponent<GraphicRaycaster>();

        CaravansaryUITheme.CreateOverlay(uiCanvasRoot.transform);

        guestsTab = CreateFullscreenContent(uiCanvasRoot.transform, "GuestsTab");
        BuildGuestSpread(guestsTab.transform, true);
        contentsRoot = BuildContents(guestSpreadRoot.transform);
        openingArtRoot = BuildOpeningArt(guestSpreadRoot.transform);
        guestListRoot = BuildGuestNameList(guestSpreadRoot.transform);
        stayingRoot = BuildBookPage(guestSpreadRoot.transform, "Konaklayanlar", StayingPlaceholder, out stayingText);
        storageRoot = BuildBookPage(guestSpreadRoot.transform, "Depo", StoragePlaceholder, out storageText);
        BuildDepotExpandButton(storageRoot.transform);
        statisticsRoot = BuildBookPage(guestSpreadRoot.transform, "Istatistikler", StatisticsPlaceholder, out statisticsText);
        signsRoot = BuildBookPage(guestSpreadRoot.transform, "Vampir İşaretleri", SignsBody, out _);
        notesRoot = BuildNotesPage(guestSpreadRoot.transform);
        staffDetailRoot = BuildStaffDetail(guestSpreadRoot.transform);
        treasuryTab = BuildBookPage(guestSpreadRoot.transform, "Kasa", TreasuryPlaceholder, out treasuryPageText);
        reportTab = BuildBookPage(guestSpreadRoot.transform, "Günlük Rapor", ReportPlaceholder, out reportPageText);

        SetActiveTab(ManagementTab.Contents);
        uiCanvasRoot.SetActive(false);
    }

    private void BuildCompactLedger()
    {
        CaravansaryUITheme.EnsureEventSystem();

        uiCanvasRoot = new GameObject("ManagementCanvas");
        uiCanvasRoot.transform.SetParent(transform);
        var canvas = uiCanvasRoot.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 150;
        var scaler = uiCanvasRoot.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        uiCanvasRoot.AddComponent<GraphicRaycaster>();

        CaravansaryUITheme.CreateOverlay(uiCanvasRoot.transform);
        panelRoot = CaravansaryUITheme.CreateFramedPanel(uiCanvasRoot.transform, new Vector2(1180f, 760f));

        CaravansaryUITheme.CreateAnchoredLabel(panelRoot.transform, "Kervan Defteri", 28, FontStyle.Bold,
            CaravansaryUITheme.AccentGold,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0f, -16f), new Vector2(700f, 36f), TextAnchor.MiddleCenter);

        var dayBadge = CaravansaryUITheme.CreateStatBadge(panelRoot.transform, "GUN", "-",
            new Vector2(-360f, -58f), new Vector2(140f, 56f));
        headerDayText = dayBadge.GetComponentsInChildren<Text>()[1];

        var goldBadge = CaravansaryUITheme.CreateStatBadge(panelRoot.transform, "ALTIN", "-",
            new Vector2(0f, -58f), new Vector2(140f, 56f));
        headerGoldText = goldBadge.GetComponentsInChildren<Text>()[1];

        var capacityBadge = CaravansaryUITheme.CreateStatBadge(panelRoot.transform, "DOLULUK", "-",
            new Vector2(360f, -58f), new Vector2(140f, 56f));
        capacityText = capacityBadge.GetComponentsInChildren<Text>()[1];

        RegisterTab(ManagementTab.Guests, "Misafirler", new Vector2(-220f, -128f));
        RegisterTab(ManagementTab.Treasury, "Kasa", new Vector2(-80f, -128f));
        RegisterTab(ManagementTab.Report, "Rapor", new Vector2(60f, -128f));
        CaravansaryUITheme.CreateButton(panelRoot.transform, "Kapat (Esc)", new Vector2(400f, -128f),
            new Vector2(130f, 34f), CaravansaryUITheme.ButtonReject, Hide, 14);

        guestsTab = CreateTabContentStretch(panelRoot.transform, "GuestsTab", 162f, 16f);
        BuildGuestSpread(guestsTab.transform, false);

        treasuryTab = CreateTabContentStretch(panelRoot.transform, "TreasuryTab", 162f, 16f);
        treasuryTab.SetActive(false);
        BuildTreasuryTab(new Vector2(700f, 300f));

        reportTab = CreateTabContentStretch(panelRoot.transform, "ReportTab", 162f, 16f);
        reportTab.SetActive(false);
        BuildReportTab(new Vector2(700f, 360f));

        SetActiveTab(ManagementTab.Guests);
        uiCanvasRoot.SetActive(false);
    }

    private void BuildTreasuryTab(Vector2 scrollSize)
    {
        var treasuryGoldBadge = CaravansaryUITheme.CreateStatBadge(treasuryTab.transform, "MEVCUT", "-",
            new Vector2(-220f, -8f), new Vector2(180f, 58f));
        treasuryGoldText = treasuryGoldBadge.GetComponentsInChildren<Text>()[1];

        var treasuryIncomeBadge = CaravansaryUITheme.CreateStatBadge(treasuryTab.transform, "SON GELIR", "-",
            new Vector2(0f, -8f), new Vector2(180f, 58f));
        treasuryIncomeText = treasuryIncomeBadge.GetComponentsInChildren<Text>()[1];

        var treasuryExpenseBadge = CaravansaryUITheme.CreateStatBadge(treasuryTab.transform, "SON GIDER", "-",
            new Vector2(220f, -8f), new Vector2(180f, 58f));
        treasuryExpenseText = treasuryExpenseBadge.GetComponentsInChildren<Text>()[1];

        CaravansaryUITheme.CreateSectionHeader(treasuryTab.transform, "Detay", new Vector2(0f, -78f), scrollSize.x - 40f);
        treasuryContentRoot = CaravansaryUITheme.CreateScrollArea(treasuryTab.transform,
            new Vector2(0f, -110f), scrollSize);
    }

    private void BuildReportTab(Vector2 scrollSize)
    {
        CaravansaryUITheme.CreateSectionHeader(reportTab.transform, "Son Sabah Raporu", new Vector2(0f, -4f), scrollSize.x - 40f);
        reportContentRoot = CaravansaryUITheme.CreateScrollArea(reportTab.transform,
            new Vector2(0f, -36f), scrollSize);
    }

    private void BuildGuestSpread(Transform parent, bool fullscreen)
    {
        guestSpreadRoot = CaravansaryUITheme.CreateParchmentSpread(parent, fullscreen);
        Transform spread = guestSpreadRoot.transform;
        if (fullscreen)
        {
            intakeRoot = CreateStretch("Intake", spread);
            spread = intakeRoot.transform;
        }
        int bodySize = fullscreen ? 26 : 22;
        int storySize = fullscreen ? 28 : 22;

        guestPortraitFrame = CaravansaryUITheme.CreatePortraitPlaceholder(
            spread,
            new Vector2(0.205f, 0.40f),
            new Vector2(0.374f, 0.70f),
            out guestPortraitImage,
            out guestPortraitLabel);

        CreateInkRule(spread, new Vector2(0.22f, 0.385f), new Vector2(0.36f, 0.385f), 1.6f, false);

        guestNameText = BookLabel(
            spread, "", 38, FontStyle.Bold, CaravansaryUITheme.InkPrimary,
            new Vector2(0.16f, 0.302f), new Vector2(0.42f, 0.375f), new Vector2(0.5f, 0.5f),
            Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter);

        CreateInkRule(spread, new Vector2(0.24f, 0.292f), new Vector2(0.34f, 0.292f), 1.4f, false);

        BookLabel(
            spread, "MISAFIR KAYDI", fullscreen ? 32 : 32, FontStyle.Bold, CaravansaryUITheme.InkPrimary,
            new Vector2(0.56f, 0.755f), new Vector2(0.88f, 0.825f), new Vector2(0.5f, 0.5f),
            Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter);

        CreateLedgerTable(spread, bodySize);

        guestStoryHeading = BookLabel(
            spread, "HIKAYESI", bodySize, FontStyle.Bold, CaravansaryUITheme.InkPrimary,
            new Vector2(0.56f, 0.40f), new Vector2(0.88f, 0.455f), new Vector2(0f, 0.5f),
            Vector2.zero, Vector2.zero, TextAnchor.MiddleLeft);

        guestStoryText = BookLabel(
            spread, "", storySize, FontStyle.Normal, CaravansaryUITheme.InkPrimary,
            new Vector2(0.56f, 0.15f), new Vector2(0.88f, 0.39f), new Vector2(0f, 1f),
            Vector2.zero, Vector2.zero, TextAnchor.UpperLeft);

        guestEmptyText = BookLabel(
            spread, "", 26, FontStyle.Normal, CaravansaryUITheme.InkPrimary,
            new Vector2(0.56f, 0.22f), new Vector2(0.88f, 0.42f), new Vector2(0.5f, 0.5f),
            Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter);

        (acceptGuestButton, acceptGuestButtonImage) = CaravansaryUITheme.CreatePageButton(
            spread, "Kabul Et",
            new Vector2(0.16f, 0.11f), new Vector2(0.27f, 0.21f),
            AcceptCurrentGuest, 34);
        StyleDecisionButton(acceptGuestButton);

        (rejectGuestButton, _) = CaravansaryUITheme.CreatePageButton(
            spread, "Reddet",
            new Vector2(0.31f, 0.11f), new Vector2(0.42f, 0.21f),
            RejectCurrentGuest, 34);
        StyleDecisionButton(rejectGuestButton);

        if (fullscreen)
        {
            CaravansaryUITheme.CreatePageButton(
                spread, "← Içindekiler",
                new Vector2(0.075f, 0.785f), new Vector2(0.26f, 0.845f),
                CloseGuestDetail, 22);
        }

        (previousGuestButton, _) = CaravansaryUITheme.CreatePageButton(
            spread, "←",
            new Vector2(0.055f, 0.45f), new Vector2(0.105f, 0.55f),
            ShowPreviousGuest, 30);

        (nextGuestButton, _) = CaravansaryUITheme.CreatePageButton(
            spread, "→",
            new Vector2(0.895f, 0.45f), new Vector2(0.945f, 0.55f),
            ShowNextGuest, 30);

        guestPageText = BookLabel(
            spread, "", 20, FontStyle.Bold, CaravansaryUITheme.InkPrimary,
            new Vector2(0.44f, 0.07f), new Vector2(0.56f, 0.14f), new Vector2(0.5f, 0.5f),
            Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter);
    }

    private GameObject BuildGuestNameList(Transform parent)
    {
        GameObject list = CreateStretch("GuestNameList", parent);
        BookLabel(
            list.transform, "Kapıda Bekleyenler", 38, FontStyle.Bold, CaravansaryUITheme.InkPrimary,
            new Vector2(0.55f, 0.78f), new Vector2(0.90f, 0.88f), new Vector2(0.5f, 0.5f),
            Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter);

        CreateLedgerLabel(list.transform, "No", 0.55f, 0.625f, 0.695f, 0.755f, TextAnchor.MiddleCenter);
        CreateLedgerLabel(list.transform, "Isim", 0.635f, 0.785f, 0.695f, 0.755f, TextAnchor.MiddleLeft);
        CreateLedgerLabel(list.transform, "Kalıs", 0.795f, 0.90f, 0.695f, 0.755f, TextAnchor.MiddleCenter);
        CreateInkRule(list.transform, new Vector2(0.55f, 0.692f), new Vector2(0.90f, 0.692f), 3f, false);
        CreateInkRule(list.transform, new Vector2(0.55f, 0.18f), new Vector2(0.90f, 0.18f), 3f, false);
        CreateInkRule(list.transform, new Vector2(0.628f, 0.18f), new Vector2(0.628f, 0.755f), 2.5f, true);
        CreateInkRule(list.transform, new Vector2(0.788f, 0.18f), new Vector2(0.788f, 0.755f), 2.5f, true);

        GameObject names = CreateStretch("Names", list.transform);
        guestNameListRoot = names.transform;
        guestListEmptyText = BookLabel(
            list.transform, "", 26, FontStyle.Italic, CaravansaryUITheme.InkPrimary,
            new Vector2(0.56f, 0.34f), new Vector2(0.89f, 0.58f), new Vector2(0.5f, 0.5f),
            Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter);
        guestListEmptyText.gameObject.SetActive(false);
        list.SetActive(false);
        return list;
    }

    private void RebuildGuestNameList(string emptyMessage)
    {
        if (guestNameListRoot == null)
            return;

        ClearChildren(guestNameListRoot);
        bool empty = !string.IsNullOrEmpty(emptyMessage);
        if (guestListEmptyText != null)
        {
            guestListEmptyText.gameObject.SetActive(empty);
            guestListEmptyText.text = emptyMessage ?? "";
        }

        int slots = Mathf.Max(pendingGuests.Count, 6);
        const float tableTop = 0.68f;
        const float tableBottom = 0.18f;
        float row = (tableTop - tableBottom) / slots;
        int fontSize = slots > 8 ? 22 : 24;

        for (int i = 1; i < slots; i++)
        {
            float line = tableTop - (i * row);
            CreateInkRule(guestNameListRoot, new Vector2(0.55f, line), new Vector2(0.90f, line), 2f, false);
        }

        if (empty)
            return;

        for (int i = 0; i < pendingGuests.Count; i++)
            CreateWaitingRow(i, tableTop - (i * row), row, fontSize);
    }

    private void CreateWaitingRow(int index, float top, float height, int fontSize)
    {
        GuestProfile guest = pendingGuests[index];
        var row = new GameObject("WaitingRow");
        row.transform.SetParent(guestNameListRoot, false);
        var rect = row.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.55f, top - height);
        rect.anchorMax = new Vector2(0.90f, top);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        var wash = row.AddComponent<Image>();
        wash.color = Color.white;
        wash.raycastTarget = true;

        var button = row.AddComponent<Button>();
        button.targetGraphic = wash;
        int captured = index;
        button.onClick.AddListener(() => OpenGuestDetail(captured));

        Color gold = CaravansaryUITheme.ParchmentGold;
        ColorBlock colors = button.colors;
        colors.normalColor = new Color(gold.r, gold.g, gold.b, 0f);
        colors.highlightedColor = new Color(gold.r, gold.g, gold.b, 0.28f);
        colors.pressedColor = new Color(gold.r, gold.g, gold.b, 0.42f);
        colors.selectedColor = colors.highlightedColor;
        colors.disabledColor = new Color(1f, 1f, 1f, 0f);
        colors.colorMultiplier = 1f;
        colors.fadeDuration = 0.08f;
        button.colors = colors;

        BookLabel(
            row.transform, (index + 1).ToString(), fontSize, FontStyle.Normal, CaravansaryUITheme.InkPrimary,
            new Vector2(0f, 0f), new Vector2(0.22f, 1f), new Vector2(0.5f, 0.5f),
            Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter);

        BookLabel(
            row.transform, guest.DisplayName, fontSize, FontStyle.Normal, CaravansaryUITheme.InkPrimary,
            new Vector2(0.26f, 0f), new Vector2(0.66f, 1f), new Vector2(0f, 0.5f),
            Vector2.zero, Vector2.zero, TextAnchor.MiddleLeft);

        BookLabel(
            row.transform, $"{guest.PlanlananGun} gece", fontSize, FontStyle.Normal, CaravansaryUITheme.InkPrimary,
            new Vector2(0.70f, 0f), new Vector2(1f, 1f), new Vector2(0.5f, 0.5f),
            Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter);
    }

    private static void CreateLedgerLabel(Transform parent, string content, float xMin, float xMax, float yMin, float yMax, TextAnchor alignment)
    {
        BookLabel(
            parent, content, 22, FontStyle.Bold, CaravansaryUITheme.InkPrimary,
            new Vector2(xMin, yMin), new Vector2(xMax, yMax), new Vector2(0.5f, 0.5f),
            Vector2.zero, Vector2.zero, alignment);
    }

    private void CreateLedgerTable(Transform parent, int valueSize)
    {
        string[] labels =
        {
            "Yas", "Cinsiyet", "Memleket", "Meslek",
            "Kalis", "Ucret", "Pesin", "Maliyet"
        };

        float left = 0.56f;
        float right = 0.89f;
        float split = 0.70f;
        float top = 0.735f;
        float rowHeight = 0.032f;

        for (int row = 0; row < labels.Length; row++)
        {
            float yTop = top - row * rowHeight;
            float yBottom = yTop - rowHeight;
            CreateInkRule(parent, new Vector2(left, yTop), new Vector2(right, yTop), 1.4f, false);
            CreateLedgerLabel(parent, labels[row], left + 0.012f, split - 0.01f, yBottom, yTop, TextAnchor.MiddleLeft);
            ledgerValues[row] = BookLabel(
                parent, "", valueSize, FontStyle.Normal, CaravansaryUITheme.InkPrimary,
                new Vector2(split + 0.012f, yBottom), new Vector2(right - 0.01f, yTop),
                new Vector2(0f, 0.5f), Vector2.zero, Vector2.zero, TextAnchor.MiddleLeft);
        }

        float bottom = top - labels.Length * rowHeight;
        CreateInkRule(parent, new Vector2(left, bottom), new Vector2(right, bottom), 1.4f, false);
        CreateInkRule(parent, new Vector2(left, bottom), new Vector2(left, top), 1.4f, true);
        CreateInkRule(parent, new Vector2(split, bottom), new Vector2(split, top), 1.4f, true);
        CreateInkRule(parent, new Vector2(right, bottom), new Vector2(right, top), 1.4f, true);
    }

    private static void CreateInkRule(Transform parent, Vector2 anchorMin, Vector2 anchorMax, float thickness, bool vertical)
    {
        var rule = new GameObject(vertical ? "VRule" : "HRule");
        rule.transform.SetParent(parent, false);
        var rect = rule.AddComponent<RectTransform>();
        rect.pivot = new Vector2(0.5f, 0.5f);
        if (vertical)
        {
            rect.anchorMin = new Vector2(anchorMin.x, anchorMin.y);
            rect.anchorMax = new Vector2(anchorMin.x, anchorMax.y);
            rect.offsetMin = new Vector2(-thickness * 0.5f, 0f);
            rect.offsetMax = new Vector2(thickness * 0.5f, 0f);
        }
        else
        {
            rect.anchorMin = new Vector2(anchorMin.x, anchorMin.y);
            rect.anchorMax = new Vector2(anchorMax.x, anchorMin.y);
            rect.offsetMin = new Vector2(0f, -thickness * 0.5f);
            rect.offsetMax = new Vector2(0f, thickness * 0.5f);
        }

        var image = rule.AddComponent<Image>();
        image.color = new Color(0.12f, 0.08f, 0.05f, 0.85f);
        image.raycastTarget = false;
    }

    private void OpenGuestDetail(int index)
    {
        if (index < 0 || index >= pendingGuests.Count)
            return;

        pendingGuestIndex = index;
        showingGuestDetail = true;
        activeTab = ManagementTab.Guests;
        BindGuestSpread(pendingGuests[index]);
        ApplyBookVisibility();
    }

    private void CloseGuestDetail()
    {
        showingGuestDetail = false;
        ApplyBookVisibility();
    }

    private void ApplyBookVisibility()
    {
        if (!UseFullscreenParchment)
            return;

        bool detail = activeTab == ManagementTab.Guests && showingGuestDetail;
        bool list = activeTab == ManagementTab.Guests && !showingGuestDetail;
        bool staffOpen = activeTab == ManagementTab.Staff;
        if (contentsRoot != null)
            contentsRoot.SetActive(!detail && !staffOpen);
        if (intakeRoot != null)
            intakeRoot.SetActive(detail);
        if (guestListRoot != null)
            guestListRoot.SetActive(list);
        if (openingArtRoot != null)
            openingArtRoot.SetActive(activeTab == ManagementTab.Contents);
        if (staffDetailRoot != null)
            staffDetailRoot.SetActive(staffOpen);
        RefreshContentsSelection();
    }

    private GameObject BuildContents(Transform parent)
    {
        GameObject contents = CreateStretch("Contents", parent);
        Text title = BookLabel(
            contents.transform, "Içindekiler", 44, FontStyle.Normal, CaravansaryUITheme.InkPrimary,
            new Vector2(0.12f, 0.78f), new Vector2(0.44f, 0.89f), new Vector2(0.5f, 0.5f),
            Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter);
        title.font = CaravansaryUITheme.ContentsHandFont;
        title.lineSpacing = 1.22f;

        CreateInkRule(contents.transform, new Vector2(0.13f, 0.765f), new Vector2(0.43f, 0.765f), 2.4f, false);

        AddContentsEntry(contents.transform, ManagementTab.Report, "1", "Günlük Rapor", 0);
        AddContentsEntry(contents.transform, ManagementTab.Treasury, "2", "Kasa", 1);
        AddContentsEntry(contents.transform, ManagementTab.Guests, "3", "Kervana Alım", 2);
        AddContentsEntry(contents.transform, ManagementTab.Staying, "4", "Konaklayanlar", 3);
        AddContentsEntry(contents.transform, ManagementTab.Storage, "5", "Depo", 4);
        AddContentsEntry(contents.transform, ManagementTab.Statistics, "6", "Istatistikler", 5);
        AddContentsEntry(contents.transform, ManagementTab.Signs, "7", "Vampir İşaretleri", 6);
        AddContentsEntry(contents.transform, ManagementTab.Notes, "8", "Not Defteri", 7);
        AddContentsEntry(contents.transform, ManagementTab.Staff, "9", "Personel", 8);
        return contents;
    }

    private GameObject BuildOpeningArt(Transform parent)
    {
        var art = new GameObject("OpeningBattle");
        art.transform.SetParent(parent, false);
        var rect = art.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.56f, 0.16f);
        rect.anchorMax = new Vector2(0.90f, 0.88f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        var image = art.AddComponent<Image>();
        image.sprite = Resources.Load<Sprite>("caravansary_opening_sketch_ink");
        image.preserveAspect = true;
        image.raycastTarget = false;
        image.color = Color.white;
        art.SetActive(false);
        return art;
    }

    private void AddContentsEntry(Transform parent, ManagementTab tab, string number, string label, int index)
    {
        const float top = 0.755f;
        const float rowHeight = 0.062f;
        float rowTop = top - (index * rowHeight);

        var row = new GameObject(label + "Row");
        row.transform.SetParent(parent, false);
        var rect = row.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.12f, rowTop - rowHeight);
        rect.anchorMax = new Vector2(0.44f, rowTop);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        var wash = row.AddComponent<Image>();
        wash.color = Color.white;
        wash.raycastTarget = true;

        var button = row.AddComponent<Button>();
        button.targetGraphic = wash;
        button.transition = Selectable.Transition.None;
        button.onClick.AddListener(() => SetActiveTab(tab));
        contentsButtons[tab] = button;

        ContentsEntryFeedback feedback = row.AddComponent<ContentsEntryFeedback>();
        feedback.Bind(wash);

        CreateHandLabel(row.transform, number, 28,
            new Vector2(0.02f, 0.08f), new Vector2(0.18f, 0.92f), TextAnchor.MiddleCenter);
        CreateHandLabel(row.transform, label, 32,
            new Vector2(0.20f, 0.08f), new Vector2(0.96f, 0.92f), TextAnchor.MiddleLeft);

        CreateInkRule(row.transform, new Vector2(0.04f, 0f), new Vector2(0.96f, 0f), 1.6f, false);
    }

    private static Text BookLabel(
        Transform parent,
        string content,
        int fontSize,
        FontStyle style,
        Color color,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 pivot,
        Vector2 position,
        Vector2 size,
        TextAnchor alignment)
    {
        return CaravansaryUITheme.CreateParchmentLabel(
            parent, content, fontSize, style, color,
            anchorMin, anchorMax, pivot, position, size, alignment);
    }

    private static void CreateHandLabel(Transform parent, string content, int fontSize, Vector2 anchorMin, Vector2 anchorMax, TextAnchor alignment)
    {
        Text text = BookLabel(
            parent, content, fontSize, FontStyle.Normal, CaravansaryUITheme.InkPrimary,
            anchorMin, anchorMax, new Vector2(0.5f, 0.5f),
            Vector2.zero, Vector2.zero, alignment);
        text.font = CaravansaryUITheme.ContentsHandFont;
        text.lineSpacing = 1.22f;
    }

    private void RefreshContentsSelection()
    {
        foreach (KeyValuePair<ManagementTab, Button> pair in contentsButtons)
        {
            ContentsEntryFeedback feedback = pair.Value.GetComponent<ContentsEntryFeedback>();
            if (feedback != null)
                feedback.SetSelected(pair.Key == activeTab);
        }
    }

    private GameObject BuildBookPage(Transform parent, string title, string body, out Text bodyText)
    {
        GameObject section = CreateStretch(title + "Page", parent);
        BookLabel(
            section.transform, title, 38, FontStyle.Bold, CaravansaryUITheme.InkPrimary,
            new Vector2(0.55f, 0.78f), new Vector2(0.90f, 0.88f), new Vector2(0.5f, 0.5f),
            Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter);
        bodyText = BookLabel(
            section.transform, body, 26, FontStyle.Normal, CaravansaryUITheme.InkPrimary,
            new Vector2(0.55f, 0.20f), new Vector2(0.90f, 0.74f), new Vector2(0f, 1f),
            Vector2.zero, Vector2.zero, TextAnchor.UpperLeft);
        section.SetActive(false);
        return section;
    }

    private GameObject BuildStaffDetail(Transform parent)
    {
        GameObject detail = CreateStretch("StaffDetail", parent);
        Transform spread = detail.transform;

        CaravansaryUITheme.CreatePortraitPlaceholder(
            spread,
            new Vector2(0.205f, 0.40f),
            new Vector2(0.374f, 0.70f),
            out staffPortraitImage,
            out staffPortraitLabel);

        CreateInkRule(spread, new Vector2(0.22f, 0.385f), new Vector2(0.36f, 0.385f), 1.6f, false);
        staffNameText = BookLabel(
            spread, "", 34, FontStyle.Bold, CaravansaryUITheme.InkPrimary,
            new Vector2(0.16f, 0.302f), new Vector2(0.42f, 0.375f), new Vector2(0.5f, 0.5f),
            Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter);
        CreateInkRule(spread, new Vector2(0.24f, 0.292f), new Vector2(0.34f, 0.292f), 1.4f, false);

        staffRoleTitle = BookLabel(
            spread, "Büyücü", 34, FontStyle.Bold, CaravansaryUITheme.InkPrimary,
            new Vector2(0.56f, 0.78f), new Vector2(0.88f, 0.86f), new Vector2(0.5f, 0.5f),
            Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter);

        BookLabel(spread, "Seviye", 26, FontStyle.Bold, CaravansaryUITheme.InkPrimary,
            new Vector2(0.56f, 0.70f), new Vector2(0.66f, 0.76f), new Vector2(0.5f, 0.5f),
            Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter);
        BookLabel(spread, "Ücret", 26, FontStyle.Bold, CaravansaryUITheme.InkPrimary,
            new Vector2(0.66f, 0.70f), new Vector2(0.78f, 0.76f), new Vector2(0.5f, 0.5f),
            Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter);
        BookLabel(spread, "Durum", 26, FontStyle.Bold, CaravansaryUITheme.InkPrimary,
            new Vector2(0.78f, 0.70f), new Vector2(0.90f, 0.76f), new Vector2(0.5f, 0.5f),
            Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter);
        CreateInkRule(spread, new Vector2(0.56f, 0.70f), new Vector2(0.90f, 0.70f), 2.4f, false);
        CreateInkRule(spread, new Vector2(0.56f, 0.52f), new Vector2(0.90f, 0.52f), 2.4f, false);
        CreateInkRule(spread, new Vector2(0.66f, 0.52f), new Vector2(0.66f, 0.76f), 2f, true);
        CreateInkRule(spread, new Vector2(0.78f, 0.52f), new Vector2(0.78f, 0.76f), 2f, true);

        GameObject table = CreateStretch("StaffTable", spread);
        staffTableRoot = table.transform;

        staffStoryText = BookLabel(
            spread, "", 26, FontStyle.Normal, CaravansaryUITheme.InkPrimary,
            new Vector2(0.56f, 0.12f), new Vector2(0.90f, 0.50f), new Vector2(0f, 1f),
            Vector2.zero, Vector2.zero, TextAnchor.UpperLeft);

        (hireStaffButton, _) = CaravansaryUITheme.CreatePageButton(
            spread, "İşe al",
            new Vector2(0.16f, 0.11f), new Vector2(0.42f, 0.21f),
            HireCurrentStaff, 34);
        StyleDecisionButton(hireStaffButton);
        hireStaffLabel = hireStaffButton.GetComponentInChildren<Text>();

        CaravansaryUITheme.CreatePageButton(
            spread, "← İçindekiler",
            new Vector2(0.075f, 0.785f), new Vector2(0.30f, 0.845f),
            () => SetActiveTab(ManagementTab.Contents), 22);

        CaravansaryUITheme.CreatePageButton(
            spread, "←",
            new Vector2(0.055f, 0.45f), new Vector2(0.105f, 0.55f),
            ShowPreviousStaff, 30);
        CaravansaryUITheme.CreatePageButton(
            spread, "→",
            new Vector2(0.895f, 0.45f), new Vector2(0.945f, 0.55f),
            ShowNextStaff, 30);

        staffPageText = BookLabel(
            spread, "", 20, FontStyle.Bold, CaravansaryUITheme.InkPrimary,
            new Vector2(0.44f, 0.07f), new Vector2(0.56f, 0.14f), new Vector2(0.5f, 0.5f),
            Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter);

        detail.SetActive(false);
        return detail;
    }

    private static string StaffTitle(StaffRole role)
    {
        switch (role)
        {
            case StaffRole.Wizard: return "Büyücü";
            case StaffRole.Watch: return "Nöbetçi";
            case StaffRole.Cook: return "Aşçı";
            default: return "Seferci";
        }
    }

    private void SelectStaffLevel(int level)
    {
        staffLevel = Mathf.Clamp(level, 1, 3);
        BindStaffPage();
    }

    private void ShowPreviousStaff()
    {
        staffRoleIndex = (staffRoleIndex + StaffOrder.Length - 1) % StaffOrder.Length;
        staffLevel = 1;
        BindStaffPage();
    }

    private void ShowNextStaff()
    {
        staffRoleIndex = (staffRoleIndex + 1) % StaffOrder.Length;
        staffLevel = 1;
        BindStaffPage();
    }

    private void HireCurrentStaff()
    {
        if (StaffManager.Instance == null)
            return;

        StaffRole role = StaffOrder[staffRoleIndex];
        if (!StaffManager.Instance.TryHire(role, staffLevel))
            return;

        BindStaffPage();
        RefreshAll();
    }

    private void BindStaffPage()
    {
        if (staffNameText == null || staffTableRoot == null || StaffManager.Instance == null)
            return;

        StaffRole role = StaffOrder[staffRoleIndex];
        int employed = StaffManager.Instance.Level(role);
        int price = StaffManager.Instance.HirePrice(role, staffLevel);

        if (staffRoleTitle != null)
            staffRoleTitle.text = StaffTitle(role);
        staffNameText.text = "Seviye " + staffLevel;
        staffStoryText.text = StaffManager.Instance.Describe(role, staffLevel);
        staffPageText.text = (staffRoleIndex + 1) + " / " + StaffOrder.Length;

        bool already = employed == staffLevel;
        bool canPay = !already && GameManager.Instance != null && GameManager.Instance.Gold >= price;
        hireStaffButton.interactable = canPay;
        if (hireStaffLabel != null)
            hireStaffLabel.text = already ? "Çalışıyor" : "İşe al";

        if (staffPortraitImage != null)
        {
            staffPortraitImage.sprite = null;
            staffPortraitImage.enabled = false;
        }

        if (staffPortraitLabel != null)
            staffPortraitLabel.gameObject.SetActive(false);

        RebuildStaffTable(role, employed);
    }

    private void RebuildStaffTable(StaffRole role, int employed)
    {
        ClearChildren(staffTableRoot);
        const float tableTop = 0.70f;
        const float rowHeight = 0.06f;

        for (int level = 1; level <= 3; level++)
        {
            float top = tableTop - ((level - 1) * rowHeight);
            CreateStaffLevelRow(role, level, employed, top, rowHeight);
            if (level < 3)
            {
                float line = top - rowHeight;
                CreateInkRule(staffTableRoot, new Vector2(0.56f, line), new Vector2(0.90f, line), 1.6f, false);
            }
        }
    }

    private void CreateStaffLevelRow(StaffRole role, int level, int employed, float top, float height)
    {
        int price = StaffManager.Instance != null ? StaffManager.Instance.HirePrice(role, level) : 0;
        bool selected = level == staffLevel;

        var row = new GameObject("Level" + level);
        row.transform.SetParent(staffTableRoot, false);
        var rect = row.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.56f, top - height);
        rect.anchorMax = new Vector2(0.90f, top);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        var wash = row.AddComponent<Image>();
        wash.color = Color.white;
        wash.raycastTarget = true;
        var button = row.AddComponent<Button>();
        button.targetGraphic = wash;
        int captured = level;
        button.onClick.AddListener(() => SelectStaffLevel(captured));

        Color gold = CaravansaryUITheme.ParchmentGold;
        ColorBlock colors = button.colors;
        colors.normalColor = selected
            ? new Color(gold.r, gold.g, gold.b, 0.35f)
            : new Color(gold.r, gold.g, gold.b, 0f);
        colors.highlightedColor = new Color(gold.r, gold.g, gold.b, 0.28f);
        colors.pressedColor = new Color(gold.r, gold.g, gold.b, 0.42f);
        colors.selectedColor = colors.highlightedColor;
        colors.disabledColor = new Color(1f, 1f, 1f, 0f);
        colors.colorMultiplier = 1f;
        colors.fadeDuration = 0.08f;
        button.colors = colors;

        BookLabel(row.transform, level.ToString(), 26, FontStyle.Normal, CaravansaryUITheme.InkPrimary,
            new Vector2(0f, 0f), new Vector2(0.28f, 1f), new Vector2(0.5f, 0.5f),
            Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter);
        BookLabel(row.transform, price + " altın", 26, FontStyle.Normal, CaravansaryUITheme.InkPrimary,
            new Vector2(0.28f, 0f), new Vector2(0.64f, 1f), new Vector2(0.5f, 0.5f),
            Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter);
        BookLabel(row.transform, employed == level ? "İşte" : "—", 26, FontStyle.Normal, CaravansaryUITheme.InkPrimary,
            new Vector2(0.64f, 0f), new Vector2(1f, 1f), new Vector2(0.5f, 0.5f),
            Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter);
    }

    private void RefreshStaffPage()
    {
        if (staffDetailRoot == null)
            return;

        BindStaffPage();
    }

    private GameObject BuildNotesPage(Transform parent)
    {
        GameObject section = CreateStretch("NotDefteriPage", parent);
        BookLabel(
            section.transform, "Not Defteri", 38, FontStyle.Bold, CaravansaryUITheme.InkPrimary,
            new Vector2(0.55f, 0.78f), new Vector2(0.90f, 0.88f), new Vector2(0.5f, 0.5f),
            Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter);

        var fieldObject = new GameObject("NotesField");
        fieldObject.transform.SetParent(section.transform, false);
        var fieldRect = fieldObject.AddComponent<RectTransform>();
        fieldRect.anchorMin = new Vector2(0.55f, 0.16f);
        fieldRect.anchorMax = new Vector2(0.90f, 0.74f);
        fieldRect.offsetMin = Vector2.zero;
        fieldRect.offsetMax = Vector2.zero;
        var fieldImage = fieldObject.AddComponent<Image>();
        fieldImage.color = new Color(0.2f, 0.12f, 0.06f, 0.04f);
        fieldImage.raycastTarget = true;

        var textObject = new GameObject("Text");
        textObject.transform.SetParent(fieldObject.transform, false);
        var textRect = textObject.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(8f, 8f);
        textRect.offsetMax = new Vector2(-8f, -8f);
        var inputText = textObject.AddComponent<Text>();
        inputText.font = CaravansaryUITheme.ParchmentFont;
        inputText.fontSize = 28;
        inputText.color = CaravansaryUITheme.InkPrimary;
        inputText.alignment = TextAnchor.UpperLeft;
        inputText.horizontalOverflow = HorizontalWrapMode.Wrap;
        inputText.verticalOverflow = VerticalWrapMode.Overflow;
        inputText.supportRichText = false;
        inputText.raycastTarget = false;
        inputText.lineSpacing = 1.22f;
        var ink = inputText.gameObject.AddComponent<Shadow>();
        ink.effectColor = new Color(0.05f, 0.03f, 0.02f, 0.9f);
        ink.effectDistance = new Vector2(0.7f, -0.7f);

        var placeholderObject = new GameObject("Placeholder");
        placeholderObject.transform.SetParent(fieldObject.transform, false);
        var placeholderRect = placeholderObject.AddComponent<RectTransform>();
        placeholderRect.anchorMin = Vector2.zero;
        placeholderRect.anchorMax = Vector2.one;
        placeholderRect.offsetMin = new Vector2(8f, 8f);
        placeholderRect.offsetMax = new Vector2(-8f, -8f);
        var placeholder = placeholderObject.AddComponent<Text>();
        placeholder.font = CaravansaryUITheme.ParchmentFont;
        placeholder.fontSize = 28;
        placeholder.fontStyle = FontStyle.Italic;
        placeholder.color = new Color(0.27f, 0.15f, 0.08f, 0.45f);
        placeholder.alignment = TextAnchor.UpperLeft;
        placeholder.horizontalOverflow = HorizontalWrapMode.Wrap;
        placeholder.verticalOverflow = VerticalWrapMode.Overflow;
        placeholder.text = "Buraya aklında kalanı yaz.";
        placeholder.raycastTarget = false;

        notesInput = fieldObject.AddComponent<InputField>();
        notesInput.targetGraphic = fieldImage;
        notesInput.textComponent = inputText;
        notesInput.placeholder = placeholder;
        notesInput.lineType = InputField.LineType.MultiLineNewline;
        notesInput.characterLimit = 2000;
        notesInput.text = PlayerPrefs.GetString(NotesPrefKey, string.Empty);
        notesInput.caretColor = CaravansaryUITheme.InkPrimary;
        notesInput.customCaretColor = true;
        notesInput.selectionColor = new Color(0.45f, 0.28f, 0.14f, 0.35f);
        notesInput.onValueChanged.AddListener(value => PlayerPrefs.SetString(NotesPrefKey, value ?? string.Empty));

        section.SetActive(false);
        return section;
    }

    private void RefreshStayingPage()
    {
        if (stayingText != null)
            stayingText.text = StayingPlaceholder;
    }

    private void RefreshStoragePage()
    {
        if (storageText == null)
            return;

        storageText.horizontalOverflow = HorizontalWrapMode.Wrap;
        storageText.verticalOverflow = VerticalWrapMode.Overflow;
        storageText.text = InventoryManager.Instance != null
            ? InventoryManager.Instance.FormatDepotListing()
            : StoragePlaceholder;
        RefreshDepotExpandButton();
    }

    private void BuildDepotExpandButton(Transform page)
    {
        var buttonObject = new GameObject("ExpandDepot");
        buttonObject.transform.SetParent(page, false);
        var rect = buttonObject.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.58f, 0.08f);
        rect.anchorMax = new Vector2(0.90f, 0.16f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        var image = buttonObject.AddComponent<Image>();
        image.color = new Color(0.33f, 0.2f, 0.1f, 1f);
        image.raycastTarget = true;
        expandDepotButton = buttonObject.AddComponent<Button>();
        expandDepotButton.targetGraphic = image;
        expandDepotButton.onClick.AddListener(ExpandDepot);
        expandDepotLabel = BookLabel(buttonObject.transform, "Genişlet", 22, FontStyle.Bold,
            new Color(0.93f, 0.86f, 0.72f, 1f),
            Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter);
        expandDepotLabel.fontStyle = FontStyle.Bold;
        expandDepotLabel.raycastTarget = false;
    }

    private void ExpandDepot()
    {
        if (InventoryManager.Instance == null || !InventoryManager.Instance.TryExpandDepot())
            return;

        RefreshStoragePage();
        RefreshAll();
    }

    private void RefreshDepotExpandButton()
    {
        if (expandDepotLabel == null || expandDepotButton == null || InventoryManager.Instance == null)
            return;

        int cost = InventoryManager.Instance.DepotExpandCost;
        int step = InventoryManager.Instance.DepotExpandStep;
        expandDepotLabel.text = "Genişlet +" + step + "  —  " + cost + " altın";
        bool canPay = GameManager.Instance != null && GameManager.Instance.Gold >= cost;
        expandDepotButton.interactable = canPay;
    }

    private void RefreshStatisticsPage()
    {
        if (statisticsText != null)
            statisticsText.text = StatisticsPlaceholder;
    }

    private static GameObject CreateStretch(string name, Transform parent)
    {
        var panel = new GameObject(name);
        panel.transform.SetParent(parent, false);
        var rect = panel.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        return panel;
    }

    private void RegisterTab(ManagementTab tab, string label, Vector2 position)
    {
        var (button, image) = CaravansaryUITheme.CreateButton(panelRoot.transform, label, position,
            new Vector2(120f, 34f), CaravansaryUITheme.TabInactive, () => SetActiveTab(tab));
        tabImages[tab] = image;
    }

    private static GameObject CreateFullscreenContent(Transform parent, string name)
    {
        var content = new GameObject(name);
        content.transform.SetParent(parent, false);

        var rect = content.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        return content;
    }

    private static GameObject CreateTabContentStretch(Transform parent, string name, float topInset, float bottomInset)
    {
        var tab = new GameObject(name);
        tab.transform.SetParent(parent, false);

        var rect = tab.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(20f, bottomInset);
        rect.offsetMax = new Vector2(-20f, -topInset);
        return tab;
    }

    private static void AddTreasuryLine(Transform parent, string label, int amount, bool isIncome)
    {
        var row = CaravansaryUITheme.CreateCard(parent, 36f);
        row.GetComponent<Image>().color = CaravansaryUITheme.StatBoxBg;

        CaravansaryUITheme.CreateAnchoredLabel(row.transform, label, 15, FontStyle.Normal,
            CaravansaryUITheme.TextSecondary,
            new Vector2(0f, 0f), new Vector2(0.6f, 1f), new Vector2(0f, 0.5f),
            new Vector2(10f, 0f), Vector2.zero, TextAnchor.MiddleLeft);

        string prefix = isIncome ? "+" : "-";
        Color valueColor = isIncome ? CaravansaryUITheme.AccentGreen : CaravansaryUITheme.AccentRed;
        CaravansaryUITheme.CreateAnchoredLabel(row.transform, $"{prefix}{amount} altin", 15, FontStyle.Bold, valueColor,
            new Vector2(0.6f, 0f), new Vector2(1f, 1f), new Vector2(1f, 0.5f),
            new Vector2(-10f, 0f), Vector2.zero, TextAnchor.MiddleRight);
    }

    private static void AddTreasuryNote(Transform parent, string note)
    {
        var row = CaravansaryUITheme.CreateCard(parent, 36f);
        CaravansaryUITheme.CreateAnchoredLabel(row.transform, note, 14, FontStyle.Italic,
            CaravansaryUITheme.TextMuted,
            Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero,
            TextAnchor.MiddleCenter);
    }

    private static void ClearChildren(Transform parent)
    {
        if (parent == null)
            return;

        for (int i = parent.childCount - 1; i >= 0; i--)
            Destroy(parent.GetChild(i).gameObject);
    }
}

public class ContentsEntryFeedback : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private static readonly Color SelectedInk = new Color(62f / 255f, 24f / 255f, 20f / 255f, 1f);
    private static readonly Color HoverInk = new Color(74f / 255f, 48f / 255f, 22f / 255f, 1f);

    private Image wash;
    private bool selected;
    private bool hovering;

    public void Bind(Image rowWash)
    {
        wash = rowWash;
        Apply();
    }

    public void SetSelected(bool value)
    {
        selected = value;
        Apply();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        hovering = true;
        Apply();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        hovering = false;
        Apply();
    }

    private void Apply()
    {
        Color ink = hovering ? HoverInk : (selected ? SelectedInk : CaravansaryUITheme.InkPrimary);
        foreach (Text label in GetComponentsInChildren<Text>())
            label.color = ink;

        if (wash == null)
            return;

            if (hovering)
                wash.color = new Color(HoverInk.r, HoverInk.g, HoverInk.b, 0.14f);
            else if (selected)
                wash.color = new Color(SelectedInk.r, SelectedInk.g, SelectedInk.b, 0.12f);
        else
            wash.color = new Color(1f, 1f, 1f, 0f);
    }
}
