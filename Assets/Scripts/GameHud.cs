using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameHud : MonoBehaviour
{
    private static readonly Color Ink = new Color(0.93f, 0.86f, 0.72f, 1f);
    private static readonly Color PanelInk = new Color(0.27f, 0.15f, 0.08f, 1f);
    private static readonly Color Paper = new Color(0.86f, 0.74f, 0.52f, 0.96f);
    private static readonly Color FullInk = new Color(0.95f, 0.45f, 0.38f, 1f);
    private static readonly Color LowGold = new Color(0.95f, 0.45f, 0.38f, 1f);
    private const float InfoDuration = 0.28f;
    private const float InfoHeight = 368f;

    private GameObject canvasRoot;
    private Text clockValue;
    private Text dayValue;
    private Text goldValue;
    private RectTransform goldCoin;
    private Text capacityValue;
    private Text depotValue;
    private Text reputationValue;
    private RectTransform infoPanel;
    private bool infoOpen;
    private float infoShown;
    private bool gameHooked;
    private bool guestsHooked;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        Ensure();
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        CaravansaryUITheme.EnsureEventSystem();
        Ensure();
    }

    public static void Ensure()
    {
        GameHud existing = FindObjectOfType<GameHud>();
        if (existing != null)
        {
            existing.Show();
            return;
        }

        var host = new GameObject("GameHud");
        DontDestroyOnLoad(host);
        host.AddComponent<GameHud>();
    }

    private void Awake()
    {
        Build();
        Show();
        Refresh();
        ApplyInfo(true);
    }

    private void Start()
    {
        Show();
        Refresh();
    }

    private void OnEnable()
    {
        Show();
    }

    private void Update()
    {
        Show();
        HookSources();
        Refresh();

        float target = infoOpen ? 1f : 0f;
        if (Mathf.Approximately(infoShown, target))
            return;

        infoShown = Mathf.MoveTowards(infoShown, target, Time.unscaledDeltaTime / InfoDuration);
        ApplyInfo(false);
    }

    private void OnDestroy()
    {
        if (gameHooked && GameManager.Instance != null)
        {
            GameManager.Instance.OnDayChanged -= OnDayChanged;
            GameManager.Instance.OnGoldChanged -= OnGoldChanged;
            GameManager.Instance.OnPhaseChanged -= OnPhaseChanged;
        }

        if (guestsHooked && GuestManager.Instance != null)
            GuestManager.Instance.OnGuestsChanged -= OnGuestsChanged;
    }

    private void HookSources()
    {
        if (!gameHooked && GameManager.Instance != null)
        {
            GameManager.Instance.OnDayChanged += OnDayChanged;
            GameManager.Instance.OnGoldChanged += OnGoldChanged;
            GameManager.Instance.OnPhaseChanged += OnPhaseChanged;
            gameHooked = true;
        }

        if (!guestsHooked && GuestManager.Instance != null)
        {
            GuestManager.Instance.OnGuestsChanged += OnGuestsChanged;
            guestsHooked = true;
        }
    }

    private void OnDayChanged(int _) => Refresh();
    private void OnGoldChanged(int _) => Refresh();
    private void OnPhaseChanged(GamePhase _) => Refresh();
    private void OnGuestsChanged() => Refresh();

    private void Show()
    {
        if (!gameObject.activeSelf)
            gameObject.SetActive(true);
        if (canvasRoot != null && !canvasRoot.activeSelf)
            canvasRoot.SetActive(true);
    }

    private void ToggleInfo()
    {
        infoOpen = !infoOpen;
    }

    private void ApplyInfo(bool instant)
    {
        if (infoPanel == null)
            return;

        if (instant)
            infoShown = infoOpen ? 1f : 0f;

        float eased = infoShown * infoShown * (3f - 2f * infoShown);
        infoPanel.sizeDelta = new Vector2(infoPanel.sizeDelta.x, InfoHeight * eased);
        infoPanel.gameObject.SetActive(infoShown > 0.001f);
    }

    private void Refresh()
    {
        GameManager game = GameManager.Instance;
        if (game == null)
            return;

        if (clockValue != null)
        {
            bool running = !game.IsGameOver && game.CurrentPhase == GamePhase.Day && Time.timeScale > 0f;
            float minuteAlpha = 1f;
            if (running)
            {
                const float fade = 0.5f;
                float elapsed = game.ClockStepElapsed;
                float duration = game.ClockStepDuration;
                float untilChange = duration - elapsed;
                float untilMiddle = Mathf.Abs(elapsed - duration * 0.5f);
                float nearest = Mathf.Min(elapsed, Mathf.Min(untilChange, untilMiddle));
                if (nearest < fade)
                    minuteAlpha = nearest / fade;
            }

            string minutes = game.ClockMinute.ToString("00");
            byte alpha = (byte)Mathf.Clamp(Mathf.RoundToInt(minuteAlpha * 255f), 0, 255);
            clockValue.text = game.ClockHour + ":<color=#EED9B8" + alpha.ToString("X2") + ">" + minutes + "</color>";
        }
        if (dayValue != null)
            dayValue.text = "Gün: " + game.CurrentDay;

        if (goldValue != null)
        {
            goldValue.text = "Altın: " + game.Gold;
            goldValue.color = game.Gold < 15 ? LowGold : PanelInk;
            if (goldCoin != null)
                goldCoin.anchoredPosition = new Vector2(goldValue.preferredWidth + 8f, 0f);
        }

        if (capacityValue != null)
        {
            int occupied = GuestManager.Instance != null ? GuestManager.Instance.OccupiedRoomCount : 0;
            int max = GuestManager.Instance != null ? GuestManager.Instance.MaxCapacity : 0;
            capacityValue.text = "Doluluk: " + occupied + "/" + max;
            capacityValue.color = max > 0 && occupied >= max ? FullInk : PanelInk;
        }

        if (depotValue != null && InventoryManager.Instance != null)
        {
            InventoryManager depot = InventoryManager.Instance;
            depotValue.text = "Depo: " + depot.DepotUsed + "/" + depot.DepotCapacity;
            depotValue.color = depot.DepotRoom <= 0 ? FullInk : PanelInk;
        }

        if (reputationValue != null && ReputationSystem.Instance != null)
            reputationValue.text = "İtibar: %" + ReputationSystem.Instance.Reputation;
            reputationValue.color = PanelInk;
    }

    private void Build()
    {
        CaravansaryUITheme.EnsureEventSystem();

        canvasRoot = new GameObject("Canvas");
        canvasRoot.transform.SetParent(transform, false);
        var canvas = canvasRoot.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 40;
        var scaler = canvasRoot.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
        canvasRoot.AddComponent<GraphicRaycaster>();
        canvasRoot.SetActive(true);

        BuildClock(canvasRoot.transform);
        BuildInfo(canvasRoot.transform);
    }

    private void BuildClock(Transform parent)
    {
        var block = new GameObject("Clock");
        block.transform.SetParent(parent, false);
        var rect = block.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = new Vector2(28f, -18f);
        rect.sizeDelta = new Vector2(160f, 92f);

        clockValue = Label(block.transform, "8:00", 42, TextAnchor.UpperLeft,
            new Vector2(0f, 0.38f), new Vector2(1f, 1f));
        clockValue.supportRichText = true;
        dayValue = Label(block.transform, "Gün: 1", 24, TextAnchor.UpperLeft,
            new Vector2(0f, 0f), new Vector2(1f, 0.42f));
    }

    private void BuildInfo(Transform parent)
    {
        var buttonObject = new GameObject("InfoButton");
        buttonObject.transform.SetParent(parent, false);
        var buttonRect = buttonObject.AddComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(1f, 1f);
        buttonRect.anchorMax = new Vector2(1f, 1f);
        buttonRect.pivot = new Vector2(1f, 1f);
        buttonRect.anchoredPosition = new Vector2(-22f, -16f);
        buttonRect.sizeDelta = new Vector2(52f, 52f);
        var buttonImage = buttonObject.AddComponent<Image>();
        buttonImage.sprite = CircleSprite();
        buttonImage.color = Paper;
        buttonImage.raycastTarget = true;
        var button = buttonObject.AddComponent<Button>();
        button.targetGraphic = buttonImage;
        button.onClick.AddListener(ToggleInfo);
        Text infoIcon = Label(buttonObject.transform, "i", 32, TextAnchor.MiddleCenter, Vector2.zero, Vector2.one);
        infoIcon.color = PanelInk;

        infoPanel = new GameObject("InfoPanel").AddComponent<RectTransform>();
        infoPanel.SetParent(parent, false);
        infoPanel.anchorMin = new Vector2(1f, 1f);
        infoPanel.anchorMax = new Vector2(1f, 1f);
        infoPanel.pivot = new Vector2(1f, 1f);
        infoPanel.anchoredPosition = new Vector2(-18f, -76f);
        infoPanel.sizeDelta = new Vector2(340f, 0f);
        var hit = infoPanel.gameObject.AddComponent<Image>();
        hit.color = new Color(1f, 1f, 1f, 0f);
        hit.raycastTarget = true;
        infoPanel.gameObject.AddComponent<RectMask2D>();

        var card = new GameObject("Card");
        card.transform.SetParent(infoPanel, false);
        var cardRect = card.AddComponent<RectTransform>();
        cardRect.anchorMin = new Vector2(0f, 1f);
        cardRect.anchorMax = new Vector2(1f, 1f);
        cardRect.pivot = new Vector2(0.5f, 1f);
        cardRect.anchoredPosition = Vector2.zero;
        cardRect.sizeDelta = new Vector2(0f, InfoHeight);
        var cardImage = card.AddComponent<Image>();
        Sprite parchment = Resources.Load<Sprite>("trade_parchment_card");
        if (parchment != null)
        {
            cardImage.sprite = parchment;
            cardImage.type = Image.Type.Sliced;
            cardImage.color = Color.white;
        }
        else
        {
            cardImage.color = Paper;
        }

        cardImage.raycastTarget = false;

        Text title = Label(card.transform, "Kervan", 30, TextAnchor.MiddleCenter, new Vector2(0f, 1f), new Vector2(1f, 1f));
        title.color = PanelInk;
        title.alignment = TextAnchor.MiddleCenter;
        var titleRect = title.rectTransform;
        titleRect.anchorMin = new Vector2(0f, 1f);
        titleRect.anchorMax = new Vector2(1f, 1f);
        titleRect.pivot = new Vector2(0.5f, 1f);
        titleRect.anchoredPosition = new Vector2(0f, -34f);
        titleRect.sizeDelta = new Vector2(-80f, 40f);
        Rule(card.transform, -78f);

        goldValue = InfoLine(card.transform, "Altın: 0", -88f);
        goldCoin = Coin(goldValue.transform);
        Rule(card.transform, -136f);
        capacityValue = InfoLine(card.transform, "Doluluk: 0/4", -146f);
        Rule(card.transform, -194f);
        depotValue = InfoLine(card.transform, "Depo: 0/50", -204f);
        Rule(card.transform, -252f);
        reputationValue = InfoLine(card.transform, "İtibar: %70", -262f);
        infoPanel.gameObject.SetActive(false);
    }

    private static Text InfoLine(Transform parent, string content, float y)
    {
        Text text = Label(parent, content, 24, TextAnchor.MiddleLeft, new Vector2(0f, 1f), new Vector2(1f, 1f));
        text.color = PanelInk;
        var rect = text.rectTransform;
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = new Vector2(0f, y);
        rect.sizeDelta = new Vector2(-96f, 42f);
        return text;
    }

    private static void Rule(Transform parent, float y)
    {
        var line = new GameObject("Rule");
        line.transform.SetParent(parent, false);
        var rect = line.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(0f, y);
        rect.sizeDelta = new Vector2(-100f, 2f);
        var image = line.AddComponent<Image>();
        image.color = new Color(0.27f, 0.15f, 0.08f, 0.35f);
        image.raycastTarget = false;
    }

    private static Text Label(Transform parent, string content, int size, TextAnchor alignment, Vector2 anchorMin, Vector2 anchorMax)
    {
        Text text = CaravansaryUITheme.CreateParchmentLabel(parent, content, size, FontStyle.Bold, Ink,
            anchorMin, anchorMax, new Vector2(0f, 1f), Vector2.zero, Vector2.zero, alignment);
        text.fontStyle = FontStyle.Bold;
        text.alignment = alignment;
        text.alignByGeometry = true;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.lineSpacing = 1f;
        text.raycastTarget = false;
        text.rectTransform.offsetMin = Vector2.zero;
        text.rectTransform.offsetMax = Vector2.zero;
        return text;
    }

    private static RectTransform Coin(Transform parent)
    {
        var coin = new GameObject("Coin");
        coin.transform.SetParent(parent, false);
        var rect = coin.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 0.5f);
        rect.anchorMax = new Vector2(0f, 0.5f);
        rect.pivot = new Vector2(0f, 0.5f);
        rect.sizeDelta = new Vector2(20f, 20f);
        var image = coin.AddComponent<Image>();
        image.sprite = Resources.Load<Sprite>("trade_gold_coin");
        image.preserveAspect = true;
        image.raycastTarget = false;
        return rect;
    }

    private static Sprite CircleSprite()
    {
        const int size = 32;
        var pixels = new Color[size * size];
        float radius = size * 0.5f - 0.5f;
        var center = new Vector2(radius, radius);
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                pixels[y * size + x] = distance <= radius
                    ? Color.white
                    : new Color(1f, 1f, 1f, 0f);
            }
        }

        var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Bilinear;
        texture.SetPixels(pixels);
        texture.Apply();
        return Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), size);
    }
}
