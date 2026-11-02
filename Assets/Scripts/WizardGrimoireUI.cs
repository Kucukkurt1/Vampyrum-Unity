using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class WizardGrimoireUI : MonoBehaviour
{
    public static WizardGrimoireUI Instance { get; private set; }

    private Canvas canvas;
    private GameObject panelRoot;
    private Transform potionListRoot;
    private Text statusText;
    private bool isVisible;

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

    public void Show()
    {
        panelRoot.SetActive(true);
        isVisible = true;
        SubscribeEvents();
        RefreshUI();
    }

    public void Hide()
    {
        if (panelRoot != null)
            panelRoot.SetActive(false);

        isVisible = false;
        UnsubscribeEvents();
    }

    private void SubscribeEvents()
    {
        if (StaffManager.Instance != null)
            StaffManager.Instance.OnStaffChanged += RefreshUI;

        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnInventoryChanged += RefreshUI;

        if (GameManager.Instance != null)
            GameManager.Instance.OnGoldChanged += OnGoldChanged;
    }

    private void UnsubscribeEvents()
    {
        if (StaffManager.Instance != null)
            StaffManager.Instance.OnStaffChanged -= RefreshUI;

        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnInventoryChanged -= RefreshUI;

        if (GameManager.Instance != null)
            GameManager.Instance.OnGoldChanged -= OnGoldChanged;
    }

    private void OnGoldChanged(int _) => RefreshUI();

    private void RefreshUI()
    {
        RefreshStatus();
        RefreshPotionList();
    }

    private void RefreshStatus()
    {
        if (statusText == null)
            return;

        if (WizardRoomManager.Instance == null || !WizardRoomManager.Instance.IsUnlocked)
        {
            statusText.text = "Buyucu odasi kilitli.";
            return;
        }

        if (StaffManager.Instance != null && !StaffManager.Instance.HasWizardStaff)
        {
            statusText.text =
                $"Buyucu yok. Tarifleri inceleyebilirsiniz.\n" +
                $"Ise al: {StaffManager.Instance.WizardHireCost} altin";
            return;
        }

        int level = StaffManager.Instance != null ? StaffManager.Instance.Level(StaffRole.Wizard) : 0;
        int percent = StaffManager.Instance != null ? StaffManager.Instance.CraftPercent(level) : 0;
        string craft = StaffManager.Instance != null ? StaffManager.Instance.LastCraftMessage : "";
        statusText.text = $"Buyucu seviye {level}. Buyu tutma %{percent}."
            + (string.IsNullOrEmpty(craft) ? "" : "\n" + craft);
    }

    private void RefreshPotionList()
    {
        ClearChildren(potionListRoot);

        foreach (PotionDefinition potion in PotionCatalog.All)
            CreatePotionRow(potion);
    }

    private void CreatePotionRow(PotionDefinition potion)
    {
        var row = CreateRow(potionListRoot, 150f);

        CreateText(row.transform, potion.DisplayName, 18, FontStyle.Bold,
            new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, 1f),
            new Vector2(12f, -8f), new Vector2(-12f, 24f), TextAnchor.UpperLeft);

        CreateText(row.transform, potion.Description, 14, FontStyle.Italic,
            new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, 1f),
            new Vector2(12f, -34f), new Vector2(-12f, 36f), TextAnchor.UpperLeft);

        string materials = InventoryManager.Instance != null
            ? InventoryManager.Instance.FormatMaterialCounts(potion)
            : "Hammadde bilgisi yok.";

        CreateText(row.transform, $"Altin: {potion.GoldCost}\n{materials}", 14, FontStyle.Normal,
            new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, 0f),
            new Vector2(12f, 8f), new Vector2(-140f, 56f), TextAnchor.LowerLeft);

        bool canPurchase = WizardRoomManager.Instance != null &&
                           WizardRoomManager.Instance.CanUseGrimoirePurchase() &&
                           CanAffordPotion(potion);

        CreateActionButton(row.transform, "Satin Al", new Vector2(-70f, 12f), canPurchase,
            () => TryPurchasePotion(potion));

        if (StaffManager.Instance != null && !StaffManager.Instance.HasWizardStaff)
        {
            CreateActionButton(row.transform, "Staff Al", new Vector2(-70f, 50f), CanHireWizard(),
                TryHireWizard);
        }
    }

    private static bool CanHireWizard()
    {
        if (StaffManager.Instance == null || StaffManager.Instance.HasWizardStaff)
            return false;

        return GameManager.Instance != null && GameManager.Instance.Gold >= StaffManager.Instance.WizardHireCost;
    }

    private static void TryHireWizard()
    {
        if (StaffManager.Instance != null && StaffManager.Instance.TryHireWizard())
            Debug.Log("Buyucu staff ise alindi.");
        else
            Debug.Log("Buyucu staff ise alinamadi.");
    }

    private static bool CanAffordPotion(PotionDefinition potion)
    {
        if (GameManager.Instance == null || InventoryManager.Instance == null)
            return false;

        return GameManager.Instance.Gold >= potion.GoldCost &&
               InventoryManager.Instance.HasMaterials(potion);
    }

    private static void TryPurchasePotion(PotionDefinition potion)
    {
        if (WizardRoomManager.Instance == null || !WizardRoomManager.Instance.CanUseGrimoirePurchase())
        {
            Debug.Log("Satin almak icin buyucu staff gerekli.");
            return;
        }

        if (GameManager.Instance == null || InventoryManager.Instance == null)
            return;

        if (GameManager.Instance.Gold < potion.GoldCost)
        {
            Debug.Log("Yeterli altin yok.");
            return;
        }

        if (!InventoryManager.Instance.HasMaterials(potion))
        {
            Debug.Log("Yeterli hammadde yok.");
            return;
        }

        if (!GameManager.Instance.SpendGold(potion.GoldCost))
            return;

        if (!InventoryManager.Instance.TrySpendMaterials(potion))
        {
            GameManager.Instance.AddGold(potion.GoldCost);
            return;
        }

        if (StaffManager.Instance != null && !StaffManager.Instance.RollCraftSuccess())
        {
            Instance?.RefreshUI();
            return;
        }

        InventoryManager.Instance.AddPotion(potion.Id);
        Debug.Log($"{potion.DisplayName} envantere eklendi.");
        Instance?.RefreshUI();
    }

    private void BuildUI()
    {
        EnsureEventSystem();

        var canvasObject = new GameObject("WizardGrimoireCanvas");
        canvasObject.transform.SetParent(transform);
        canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 160;
        var scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasObject.AddComponent<GraphicRaycaster>();

        panelRoot = new GameObject("WizardGrimoirePanel");
        panelRoot.transform.SetParent(canvasObject.transform, false);

        var panelImage = panelRoot.AddComponent<Image>();
        panelImage.color = new Color(0.08f, 0.05f, 0.12f, 0.9f);

        var panelRect = panelRoot.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(520f, 620f);

        CreateText(panelRoot.transform, "Buyu Kitabi", 24, FontStyle.Bold,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0f, -16f), new Vector2(480f, 34f), TextAnchor.MiddleCenter);

        statusText = CreateText(panelRoot.transform, "", 15, FontStyle.Normal,
            new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0f, -52f), new Vector2(480f, 48f), TextAnchor.MiddleCenter);

        CreateText(panelRoot.transform, "Iksirler", 18, FontStyle.Bold,
            new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, 1f),
            new Vector2(16f, -108f), new Vector2(200f, 24f), TextAnchor.MiddleLeft);

        potionListRoot = CreateScrollArea(panelRoot.transform, new Vector2(0f, -140f), new Vector2(480f, 450f));

        panelRoot.SetActive(false);
    }

    private Transform CreateScrollArea(Transform parent, Vector2 anchoredPosition, Vector2 size)
    {
        var scrollObject = new GameObject("ScrollArea");
        scrollObject.transform.SetParent(parent, false);

        var scrollRect = scrollObject.AddComponent<RectTransform>();
        scrollRect.anchorMin = new Vector2(0.5f, 1f);
        scrollRect.anchorMax = new Vector2(0.5f, 1f);
        scrollRect.pivot = new Vector2(0.5f, 1f);
        scrollRect.anchoredPosition = anchoredPosition;
        scrollRect.sizeDelta = size;

        scrollObject.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.25f);
        var scroll = scrollObject.AddComponent<ScrollRect>();

        var viewport = new GameObject("Viewport");
        viewport.transform.SetParent(scrollObject.transform, false);
        var viewportRect = viewport.AddComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.offsetMin = Vector2.zero;
        viewportRect.offsetMax = Vector2.zero;
        viewport.AddComponent<Mask>().showMaskGraphic = false;
        viewport.AddComponent<Image>().color = new Color(1f, 1f, 1f, 0.02f);

        var content = new GameObject("Content");
        content.transform.SetParent(viewport.transform, false);
        var contentRect = content.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0f, 1f);
        contentRect.anchorMax = new Vector2(1f, 1f);
        contentRect.pivot = new Vector2(0.5f, 1f);
        contentRect.anchoredPosition = Vector2.zero;
        contentRect.sizeDelta = new Vector2(0f, 0f);

        var layout = content.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 8f;
        layout.padding = new RectOffset(6, 6, 6, 6);
        layout.childControlHeight = true;
        layout.childControlWidth = true;
        layout.childForceExpandHeight = false;
        layout.childForceExpandWidth = true;

        content.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scroll.content = contentRect;
        scroll.viewport = viewportRect;
        scroll.horizontal = false;

        return content.transform;
    }

    private static GameObject CreateRow(Transform parent, float height)
    {
        var row = new GameObject("Row");
        row.transform.SetParent(parent, false);

        var layout = row.AddComponent<LayoutElement>();
        layout.minHeight = height;
        layout.preferredHeight = height;

        row.AddComponent<Image>().color = new Color(0.18f, 0.1f, 0.22f, 0.7f);
        return row;
    }

    private static void CreateActionButton(Transform parent, string label, Vector2 anchoredPosition, bool enabled,
        UnityEngine.Events.UnityAction onClick)
    {
        var buttonObject = new GameObject(label + "Button");
        buttonObject.transform.SetParent(parent, false);

        var buttonRect = buttonObject.AddComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(1f, 0f);
        buttonRect.anchorMax = new Vector2(1f, 0f);
        buttonRect.pivot = new Vector2(1f, 0f);
        buttonRect.anchoredPosition = anchoredPosition;
        buttonRect.sizeDelta = new Vector2(100f, 30f);

        var image = buttonObject.AddComponent<Image>();
        image.color = enabled ? new Color(0.35f, 0.18f, 0.35f, 1f) : new Color(0.2f, 0.2f, 0.2f, 0.8f);

        var button = buttonObject.AddComponent<Button>();
        button.targetGraphic = image;
        button.interactable = enabled;
        button.onClick.AddListener(onClick);

        CreateText(buttonObject.transform, label, 14, FontStyle.Bold,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            Vector2.zero, new Vector2(100f, 30f), TextAnchor.MiddleCenter);
    }

    private static Text CreateText(Transform parent, string content, int fontSize, FontStyle style,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPosition, Vector2 sizeDelta,
        TextAnchor alignment)
    {
        var textObject = new GameObject("Text");
        textObject.transform.SetParent(parent, false);

        var rect = textObject.AddComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = pivot;
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = sizeDelta;

        var text = textObject.AddComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = fontSize;
        text.fontStyle = style;
        text.text = content;
        text.color = new Color(0.93f, 0.88f, 0.95f);
        text.alignment = alignment;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        return text;
    }

    private static void ClearChildren(Transform parent)
    {
        if (parent == null)
            return;

        for (int i = parent.childCount - 1; i >= 0; i--)
            Destroy(parent.GetChild(i).gameObject);
    }

    private static void EnsureEventSystem()
    {
        if (FindObjectOfType<EventSystem>() != null)
            return;

        var eventSystemObject = new GameObject("EventSystem");
        eventSystemObject.AddComponent<EventSystem>();
        eventSystemObject.AddComponent<StandaloneInputModule>();
    }
}
