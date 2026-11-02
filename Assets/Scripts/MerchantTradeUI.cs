using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MerchantTradeUI : MonoBehaviour
{
    public static MerchantTradeUI Instance { get; private set; }

    private readonly Dictionary<string, int> offer = new Dictionary<string, int>();
    private readonly Dictionary<string, int> take = new Dictionary<string, int>();

    private GameObject canvasRoot;
    private RectTransform offerContent;
    private RectTransform takeContent;
    private RectTransform gaveList;
    private RectTransform tookList;
    private Text givenGoldText;
    private Text takenGoldText;
    private Text treasuryGoldText;
    private RectTransform givenGoldCoin;
    private RectTransform takenGoldCoin;
    private RectTransform treasuryGoldCoin;
    private InputField offeredGoldInput;
    private int offeredGold;
    private bool writingGold;
    private GameObject tooltipRoot;
    private Text tooltipText;
    private Text tooltipPriceText;
    private RectTransform tooltipCoinRect;
    private Button confirmButton;
    private readonly List<Image> glowImages = new List<Image>();
    private bool applying;
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
        Build();
        Hide();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void Update()
    {
        if (!isVisible)
            return;

        if (Input.GetKeyDown(KeyCode.Escape))
            Hide();

        float wave = 0.5f + Mathf.Sin(Time.unscaledTime * 3.4f) * 0.28f;
        for (int i = 0; i < glowImages.Count; i++)
        {
            Image glow = glowImages[i];
            if (glow == null)
                continue;

            Color color = glow.color;
            color.a = wave;
            glow.color = color;
        }
    }

    public void Show()
    {
        CaravansaryUITheme.EnsureEventSystem();

        if (isVisible)
            return;

        if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
            return;

        if (CaravansaryManagementUI.Instance != null && CaravansaryManagementUI.Instance.IsVisible)
            return;

        if (MorningReportUI.IsBlockingInput || GameOverUI.IsBlockingInteraction)
            return;

        BookInteraction book = FindObjectOfType<BookInteraction>();
        if (book != null && book.IsBookOpen)
            return;

        offer.Clear();
        take.Clear();
        offeredGold = 0;
        canvasRoot.SetActive(true);
        isVisible = true;
        Time.timeScale = 0f;
        Subscribe();
        Refresh();
    }

    public void Hide()
    {
        if (canvasRoot != null)
            canvasRoot.SetActive(false);

        if (!isVisible)
            return;

        isVisible = false;
        Unsubscribe();
        if (Time.timeScale == 0f)
            Time.timeScale = 1f;
    }

    private void Subscribe()
    {
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnInventoryChanged += OnInventoryChanged;
        if (MerchantManager.Instance != null)
            MerchantManager.Instance.OnStockChanged += OnStockChanged;
        if (GameManager.Instance != null)
            GameManager.Instance.OnGoldChanged += OnGoldChanged;
    }

    private void Unsubscribe()
    {
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnInventoryChanged -= OnInventoryChanged;
        if (MerchantManager.Instance != null)
            MerchantManager.Instance.OnStockChanged -= OnStockChanged;
        if (GameManager.Instance != null)
            GameManager.Instance.OnGoldChanged -= OnGoldChanged;
    }

    private void OnInventoryChanged()
    {
        if (!applying)
            Refresh();
    }

    private void OnStockChanged()
    {
        if (!applying)
            Refresh();
    }

    private void OnGoldChanged(int _)
    {
        if (!applying)
            Refresh();
    }

    private void Confirm()
    {
        if (MerchantManager.Instance == null)
            return;

        applying = true;
        bool done = MerchantManager.Instance.TryExecute(offer, take, offeredGold);
        applying = false;

        if (!done)
        {
            Refresh();
            return;
        }

        offer.Clear();
        take.Clear();
        offeredGold = 0;
        Refresh();
    }

    private void Refresh()
    {
        if (!isVisible || offerContent == null)
            return;

        HideTooltip();
        ClampSelection(offer, false);
        ClampSelection(take, true);
        glowImages.Clear();
        RebuildColumn(offerContent, false);
        RebuildColumn(takeContent, true);

        int gold = GameManager.Instance != null ? GameManager.Instance.Gold : 0;
        TradeQuote preview = MerchantManager.Instance != null
            ? MerchantManager.Instance.Quote(offer, take)
            : new TradeQuote();
        int room = gold + preview.OfferValue - preview.TakeValue;
        offeredGold = room > 0 ? Mathf.Clamp(offeredGold, 0, room) : 0;

        TradeQuote quote = MerchantManager.Instance != null
            ? MerchantManager.Instance.Quote(offer, take, offeredGold)
            : new TradeQuote();

        FillTradeList(gaveList, offer, false, offeredGold);
        FillTradeList(tookList, take, true, 0);
        int nextGold = gold + quote.GoldDelta;
        SetGoldStat(givenGoldText, givenGoldCoin, "Verilen altın : " + quote.OfferValue);
        SetGoldStat(takenGoldText, takenGoldCoin, "Alınan altın : " + quote.TakeValue);
        SetGoldStat(treasuryGoldText, treasuryGoldCoin, "Kasa : " + gold + " → " + nextGold);
        if (offeredGoldInput != null && !offeredGoldInput.isFocused)
        {
            writingGold = true;
            offeredGoldInput.text = offeredGold.ToString();
            writingGold = false;
        }

        confirmButton.interactable = quote.CanConfirm;
    }

    private void ClampSelection(Dictionary<string, int> selection, bool fromMerchant)
    {
        var ids = new List<string>(selection.Keys);
        for (int i = 0; i < ids.Count; i++)
        {
            string id = ids[i];
            ItemDefinition item = ItemCatalog.Get(id);
            int owned = 0;
            if (item != null && item.Tradeable)
            {
                owned = fromMerchant
                    ? MerchantManager.Instance != null ? MerchantManager.Instance.GetStock(id) : 0
                    : InventoryManager.Instance != null ? InventoryManager.Instance.GetTradeCount(item) : 0;
            }

            if (owned <= 0)
                selection.Remove(id);
            else if (selection[id] > owned)
                selection[id] = owned;
        }
    }

    private void RebuildColumn(RectTransform content, bool merchantSide)
    {
        for (int i = content.childCount - 1; i >= 0; i--)
            DestroyImmediate(content.GetChild(i).gameObject);

        var slots = new List<TradeSlot>();
        if (merchantSide)
        {
            if (MerchantManager.Instance != null)
            {
                foreach (KeyValuePair<string, int> pair in MerchantManager.Instance.Stock)
                {
                    if (pair.Value <= 0)
                        continue;

                    ItemDefinition item = ItemCatalog.Get(pair.Key);
                    if (item == null || !item.Tradeable)
                        continue;

                    take.TryGetValue(item.Id, out int selected);
                    slots.Add(new TradeSlot(item, pair.Value, selected, ItemCatalog.MerchantPrice(item)));
                }
            }
        }
        else if (InventoryManager.Instance != null)
        {
            IReadOnlyList<ItemDefinition> all = ItemCatalog.All;
            for (int i = 0; i < all.Count; i++)
            {
                ItemDefinition item = all[i];
                if (!item.Tradeable)
                    continue;

                int owned = InventoryManager.Instance.GetTradeCount(item);
                if (owned <= 0)
                    continue;

                offer.TryGetValue(item.Id, out int selected);
                slots.Add(new TradeSlot(item, owned, selected, ItemCatalog.BasePrice(item)));
            }
        }

        slots.Sort(CompareSlots);

        int count = Mathf.Max(SlotColumns * SlotRows, Mathf.CeilToInt(slots.Count / (float)SlotColumns) * SlotColumns);
        for (int i = 0; i < count; i++)
        {
            if (i < slots.Count)
                CreateItemSlot(content, slots[i], merchantSide);
            else
                CreateEmptySlot(content);
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(content);
    }

    private void CreateItemSlot(RectTransform content, TradeSlot slot, bool merchantSide)
    {
        Image plate = CreateSlotPlate(content, slot.Item.Id, slot.Item.Rarity, slot.Selected > 0);
        CreateSphere(plate.transform, slot.Item.Id, slot.Item.Rarity);
        if (slot.Owned > 1)
        {
            CreateSlotLabel(plate.transform, "x" + slot.Owned, 20, FontStyle.Bold, Ink,
                new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f),
                new Vector2(6f, 6f), new Vector2(48f, 24f), TextAnchor.LowerLeft);
        }

        string id = slot.Item.Id;
        TradeSlot shown = slot;
        bool side = merchantSide;
        var trigger = plate.gameObject.AddComponent<EventTrigger>();
        var click = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };
        click.callback.AddListener(data =>
        {
            var pointer = data as PointerEventData;
            int step = pointer != null && pointer.button == PointerEventData.InputButton.Right ? -1 : 1;
            ChangeSelection(id, side, step);
        });
        var enter = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
        enter.callback.AddListener(data => ShowTooltip(shown, side, data as PointerEventData));
        var exit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
        exit.callback.AddListener(_ => HideTooltip());
        trigger.triggers.Add(click);
        trigger.triggers.Add(enter);
        trigger.triggers.Add(exit);
    }

    private static void CreateEmptySlot(RectTransform content)
    {
        var slot = new GameObject("Empty");
        slot.transform.SetParent(content, false);
        var image = slot.AddComponent<Image>();
        ApplyParchment(image, Resources.Load<Sprite>("trade_parchment_slot"), false);
        image.color = new Color(1f, 0.97f, 0.9f, 0.72f);
        image.raycastTarget = false;
    }

    private static Image CreateSlotPlate(RectTransform content, string name, ItemRarity rarity, bool selected)
    {
        var slot = new GameObject(name);
        slot.transform.SetParent(content, false);
        var image = slot.AddComponent<Image>();
        ApplyParchment(image, Resources.Load<Sprite>("trade_parchment_slot"), false);
        Color tint = RarityTint(rarity);
        image.color = selected ? Color.Lerp(tint, Color.black, 0.18f) : tint;
        if (selected)
        {
            var outline = slot.AddComponent<Outline>();
            outline.effectColor = new Color(0.2f, 0.12f, 0.06f, 0.95f);
            outline.effectDistance = new Vector2(2f, -2f);
        }
        return image;
    }

    private static Color RarityTint(ItemRarity rarity)
    {
        switch (rarity)
        {
            case ItemRarity.Siradan:
                return new Color(0.62f, 0.62f, 0.64f, 1f);
            case ItemRarity.Nadir:
                return new Color(0.62f, 0.42f, 0.86f, 1f);
            case ItemRarity.Efsanevi:
                return new Color(0.28f, 0.78f, 0.74f, 1f);
            default:
                return new Color(1f, 0.52f, 0.08f, 1f);
        }
    }

    private static void CreateSphere(Transform parent, string itemId, ItemRarity rarity)
    {
        var glowObject = new GameObject("Glow");
        glowObject.transform.SetParent(parent, false);
        var glowRect = glowObject.AddComponent<RectTransform>();
        glowRect.anchorMin = new Vector2(0.5f, 0.5f);
        glowRect.anchorMax = new Vector2(0.5f, 0.5f);
        glowRect.pivot = new Vector2(0.5f, 0.5f);
        glowRect.sizeDelta = new Vector2(86f, 86f);
        var glow = glowObject.AddComponent<Image>();
        glow.sprite = GlowSprite;
        glow.color = NeonColor(rarity);
        glow.raycastTarget = false;

        var orb = new GameObject("Orb");
        orb.transform.SetParent(parent, false);
        var rect = orb.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(40f, 40f);
        var image = orb.AddComponent<Image>();
        image.sprite = SphereSprite;
        image.color = Color.Lerp(SphereColor(itemId), Color.white, 0.35f);
        image.raycastTarget = false;

        if (parent.GetComponentInParent<MerchantTradeUI>() != null)
            parent.GetComponentInParent<MerchantTradeUI>().glowImages.Add(glow);
    }

    private void ChangeSelection(string id, bool merchantSide, int direction)
    {
        Dictionary<string, int> selection = merchantSide ? take : offer;
        selection.TryGetValue(id, out int selected);
        selected += direction;
        if (selected <= 0)
            selection.Remove(id);
        else
            selection[id] = selected;

        Refresh();
    }

    private void ShowTooltip(TradeSlot slot, bool merchantSide, PointerEventData pointer)
    {
        if (tooltipRoot == null || slot.Item == null)
            return;

        int unit = merchantSide ? ItemCatalog.MerchantPrice(slot.Item) : ItemCatalog.BasePrice(slot.Item);
        tooltipText.supportRichText = true;
        tooltipText.text = "İtem adı : " + slot.Item.DisplayName
            + "\nNadirlik : <color=" + RarityHex(slot.Item.Rarity) + ">" + RarityName(slot.Item.Rarity) + "</color>";
        if (tooltipPriceText != null)
        {
            tooltipPriceText.text = "Birim ücreti : " + unit;
            float priceWidth = tooltipPriceText.preferredWidth;
            if (priceWidth < 8f)
                priceWidth = tooltipPriceText.text.Length * 14f;
            tooltipPriceText.rectTransform.sizeDelta = new Vector2(priceWidth + 4f, 36f);
            if (tooltipCoinRect != null)
                tooltipCoinRect.anchoredPosition = new Vector2(priceWidth + 8f, 0f);
        }
        tooltipRoot.SetActive(true);
        if (pointer != null)
            PlaceTooltip(pointer.position);
    }

    private void HideTooltip()
    {
        if (tooltipRoot != null)
            tooltipRoot.SetActive(false);
    }

    private void PlaceTooltip(Vector2 screenPoint)
    {
        var canvasRect = (RectTransform)canvasRoot.transform;
        var tipRect = (RectTransform)tooltipRoot.transform;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPoint, null, out Vector2 local);
        float halfW = canvasRect.rect.width * 0.5f;
        float halfH = canvasRect.rect.height * 0.5f;
        float width = tipRect.sizeDelta.x;
        float height = tipRect.sizeDelta.y;
        local.x += 18f;
        local.y -= 16f;
        if (local.x + width > halfW - 12f)
            local.x -= width + 36f;
        local.x = Mathf.Clamp(local.x, -halfW + 12f, halfW - width - 12f);
        local.y = Mathf.Clamp(local.y, -halfH + height + 12f, halfH - 12f);
        tipRect.anchoredPosition = local;
    }

    private void BuildTooltip()
    {
        tooltipRoot = new GameObject("ItemTooltip");
        tooltipRoot.transform.SetParent(canvasRoot.transform, false);
        var rect = tooltipRoot.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0f, 1f);
        rect.sizeDelta = new Vector2(340f, 280f);
        var image = tooltipRoot.AddComponent<Image>();
        ApplyParchment(image, Resources.Load<Sprite>("trade_parchment_card"), true);
        image.raycastTarget = false;
        tooltipText = CaravansaryUITheme.CreateParchmentLabel(tooltipRoot.transform, "", 26, FontStyle.Bold, Ink,
            Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f),
            Vector2.zero, Vector2.zero, TextAnchor.UpperLeft);
        tooltipText.fontStyle = FontStyle.Bold;
        var textRect = tooltipText.rectTransform;
        textRect.offsetMin = new Vector2(52f, 90f);
        textRect.offsetMax = new Vector2(-18f, -64f);

        var priceRow = new GameObject("PriceLine");
        priceRow.transform.SetParent(tooltipRoot.transform, false);
        var priceRect = priceRow.AddComponent<RectTransform>();
        priceRect.anchorMin = new Vector2(0f, 1f);
        priceRect.anchorMax = new Vector2(0f, 1f);
        priceRect.pivot = new Vector2(0f, 0.5f);
        priceRect.anchoredPosition = new Vector2(52f, -168f);
        priceRect.sizeDelta = new Vector2(270f, 36f);
        tooltipPriceText = CaravansaryUITheme.CreateParchmentLabel(priceRow.transform, "", 26, FontStyle.Bold, Ink,
            new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(0f, 0.5f),
            Vector2.zero, new Vector2(180f, 0f), TextAnchor.MiddleLeft);
        tooltipPriceText.fontStyle = FontStyle.Bold;
        tooltipPriceText.horizontalOverflow = HorizontalWrapMode.Overflow;

        var coin = new GameObject("Coin");
        coin.transform.SetParent(priceRow.transform, false);
        tooltipCoinRect = coin.AddComponent<RectTransform>();
        tooltipCoinRect.anchorMin = new Vector2(0f, 0.5f);
        tooltipCoinRect.anchorMax = new Vector2(0f, 0.5f);
        tooltipCoinRect.pivot = new Vector2(0f, 0.5f);
        tooltipCoinRect.sizeDelta = new Vector2(22f, 22f);
        var coinImage = coin.AddComponent<Image>();
        coinImage.sprite = CoinSprite;
        coinImage.preserveAspect = true;
        coinImage.raycastTarget = false;

        tooltipRoot.SetActive(false);
    }

    private static string RarityName(ItemRarity rarity)
    {
        switch (rarity)
        {
            case ItemRarity.Siradan:
                return "Sıradan";
            case ItemRarity.Nadir:
                return "Destansı";
            case ItemRarity.Efsanevi:
                return "Efsanevi";
            default:
                return "Ender";
        }
    }

    private static string RarityHex(ItemRarity rarity)
    {
        switch (rarity)
        {
            case ItemRarity.Siradan:
                return "#6A6A70";
            case ItemRarity.Nadir:
                return "#8A52D6";
            case ItemRarity.Efsanevi:
                return "#2EBEB4";
            default:
                return "#E86A00";
        }
    }

    private static readonly Color Ink = new Color(0.27f, 0.15f, 0.08f, 1f);

    private const int SlotColumns = 4;
    private const int SlotRows = 5;

    private static Sprite sphereSprite;

    private static Sprite SphereSprite
    {
        get
        {
            if (sphereSprite != null)
                return sphereSprite;

            const int size = 32;
            var pixels = new Color[size * size];
            var center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
            float radius = size * 0.46f;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - center.x;
                    float dy = y - center.y;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy) / radius;
                    if (dist > 1f)
                    {
                        pixels[y * size + x] = Color.clear;
                        continue;
                    }

                    float shade = Mathf.Lerp(1f, 0.45f, dist);
                    float highlight = Mathf.Clamp01(1f - Vector2.Distance(new Vector2(x, y), center + new Vector2(-6f, 7f)) / 10f);
                    shade = Mathf.Clamp01(shade + highlight * 0.55f);
                    pixels[y * size + x] = new Color(shade, shade, shade, 1f);
                }
            }

            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Point;
            texture.hideFlags = HideFlags.HideAndDontSave;
            texture.SetPixels(pixels);
            texture.Apply();
            sphereSprite = Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), size);
            return sphereSprite;
        }
    }

    private static Color SphereColor(string itemId)
    {
        int hash = 17;
        for (int i = 0; itemId != null && i < itemId.Length; i++)
            hash = hash * 31 + itemId[i];

        float hue = (hash & 255) / 255f;
        return Color.HSVToRGB(hue, 0.72f, 0.95f);
    }

    private static Color NeonColor(ItemRarity rarity)
    {
        switch (rarity)
        {
            case ItemRarity.Siradan:
                return new Color(0.82f, 0.86f, 0.9f, 0.7f);
            case ItemRarity.Nadir:
                return new Color(0.72f, 0.28f, 1f, 0.85f);
            case ItemRarity.Efsanevi:
                return new Color(0.15f, 1f, 0.92f, 0.9f);
            default:
                return new Color(1f, 0.45f, 0.05f, 0.85f);
        }
    }

    private static Sprite coinSprite;

    private static Sprite CoinSprite
    {
        get
        {
            if (coinSprite == null)
                coinSprite = Resources.Load<Sprite>("trade_gold_coin");
            return coinSprite;
        }
    }

    private static Sprite glowSprite;

    private static Sprite GlowSprite
    {
        get
        {
            if (glowSprite != null)
                return glowSprite;

            const int size = 64;
            var pixels = new Color[size * size];
            var center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
            float radius = size * 0.48f;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), center) / radius;
                    float alpha = Mathf.Clamp01(1f - dist);
                    alpha *= alpha;
                    pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
                }
            }

            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Bilinear;
            texture.hideFlags = HideFlags.HideAndDontSave;
            texture.SetPixels(pixels);
            texture.Apply();
            glowSprite = Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), size);
            return glowSprite;
        }
    }

    private static int CompareSlots(TradeSlot a, TradeSlot b)
    {
        int rarity = a.Item.Rarity.CompareTo(b.Item.Rarity);
        if (rarity != 0)
            return rarity;

        return string.CompareOrdinal(a.Item.DisplayName, b.Item.DisplayName);
    }

    private void Build()
    {
        CaravansaryUITheme.EnsureEventSystem();

        canvasRoot = new GameObject("MerchantTradeCanvas");
        canvasRoot.transform.SetParent(transform, false);
        var canvas = canvasRoot.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 180;
        var scaler = canvasRoot.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasRoot.AddComponent<GraphicRaycaster>();

        var overlay = new GameObject("MerchantOverlay");
        overlay.transform.SetParent(canvasRoot.transform, false);
        var overlayRect = overlay.AddComponent<RectTransform>();
        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.offsetMin = Vector2.zero;
        overlayRect.offsetMax = Vector2.zero;
        var overlayImage = overlay.AddComponent<Image>();
        overlayImage.color = new Color(0.02f, 0.03f, 0.05f, 0.72f);
        overlayImage.raycastTarget = true;

        GameObject left = CreateSidePanel(canvasRoot.transform, "İtemlerin", new Vector2(0.012f, 0.08f), new Vector2(0.352f, 0.94f));
        GameObject right = CreateSidePanel(canvasRoot.transform, "Tüccar", new Vector2(0.648f, 0.08f), new Vector2(0.988f, 0.94f));
        offerContent = CreateGrid(left.transform, 40);
        takeContent = CreateGrid(right.transform, 84);

        GameObject center = CreateInfoCard(canvasRoot.transform);
        BuildTradeLedger(center.transform);

        confirmButton = CreateFooterButton(center.transform, "Onayla", new Vector2(-116f, 28f), Confirm);
        CreateFooterButton(center.transform, "Kapat", new Vector2(116f, 28f), Hide);
        BuildTooltip();
    }

    private static GameObject CreateSidePanel(Transform parent, string title, Vector2 anchorMin, Vector2 anchorMax)
    {
        var panel = new GameObject(title);
        panel.transform.SetParent(parent, false);
        var rect = panel.AddComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        var plate = panel.AddComponent<Image>();
        plate.color = new Color(0f, 0f, 0f, 0f);
        plate.raycastTarget = false;

        var header = new GameObject("Header");
        header.transform.SetParent(panel.transform, false);
        var headerRect = header.AddComponent<RectTransform>();
        headerRect.anchorMin = new Vector2(0f, 1f);
        headerRect.anchorMax = new Vector2(1f, 1f);
        headerRect.pivot = new Vector2(0.5f, 1f);
        headerRect.sizeDelta = new Vector2(48f, 112f);
        headerRect.anchoredPosition = new Vector2(0f, 6f);
        var headerImage = header.AddComponent<Image>();
        ApplyParchment(headerImage, Resources.Load<Sprite>("trade_parchment_banner"), true);
        Text titleText = CaravansaryUITheme.CreateParchmentLabel(header.transform, title, 48, FontStyle.Bold, Ink,
            Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f),
            Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter);
        titleText.fontStyle = FontStyle.Bold;
        return panel;
    }

    private static RectTransform CreateGrid(Transform panel, int leftPadding)
    {
        var scrollObject = new GameObject("GridScroll");
        scrollObject.transform.SetParent(panel, false);
        var scrollRectTransform = scrollObject.AddComponent<RectTransform>();
        scrollRectTransform.anchorMin = Vector2.zero;
        scrollRectTransform.anchorMax = Vector2.one;
        scrollRectTransform.offsetMin = new Vector2(12f, 12f);
        scrollRectTransform.offsetMax = new Vector2(-12f, -128f);
        var scrollImage = scrollObject.AddComponent<Image>();
        scrollImage.color = new Color(0f, 0f, 0f, 0f);
        scrollImage.raycastTarget = false;
        var scroll = scrollObject.AddComponent<ScrollRect>();
        scroll.horizontal = false;
        scroll.movementType = ScrollRect.MovementType.Clamped;
        scroll.scrollSensitivity = 28f;

        var viewport = new GameObject("Viewport");
        viewport.transform.SetParent(scrollObject.transform, false);
        var viewportRect = viewport.AddComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.offsetMin = new Vector2(10f, 10f);
        viewportRect.offsetMax = new Vector2(-10f, -10f);
        viewport.AddComponent<Image>().color = new Color(1f, 1f, 1f, 0.01f);
        viewport.AddComponent<Mask>().showMaskGraphic = false;

        var content = new GameObject("Content");
        content.transform.SetParent(viewport.transform, false);
        var contentRect = content.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0f, 1f);
        contentRect.anchorMax = new Vector2(1f, 1f);
        contentRect.pivot = new Vector2(0.5f, 1f);
        contentRect.anchoredPosition = Vector2.zero;
        contentRect.sizeDelta = new Vector2(0f, 0f);

        var grid = content.AddComponent<GridLayoutGroup>();
        grid.padding = new RectOffset(leftPadding, 0, 4, 0);
        grid.cellSize = new Vector2(100f, 100f);
        grid.spacing = new Vector2(8f, 8f);
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = SlotColumns;
        grid.childAlignment = TextAnchor.UpperLeft;
        var fitter = content.AddComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scroll.viewport = viewportRect;
        scroll.content = contentRect;
        return contentRect;
    }

    private static Button CreateFooterButton(Transform parent, string label, Vector2 anchoredPosition, UnityEngine.Events.UnityAction action)
    {
        var buttonObject = new GameObject(label);
        buttonObject.transform.SetParent(parent, false);
        var rect = buttonObject.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0f);
        rect.anchorMax = new Vector2(0.5f, 0f);
        rect.pivot = new Vector2(0.5f, 0f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = new Vector2(210f, 80f);

        var image = buttonObject.AddComponent<Image>();
        ApplyParchment(image, Resources.Load<Sprite>("trade_parchment_banner"), true);
        var button = buttonObject.AddComponent<Button>();
        button.targetGraphic = image;
        var colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1f, 0.96f, 0.88f, 1f);
        colors.pressedColor = new Color(0.82f, 0.74f, 0.6f, 1f);
        colors.disabledColor = new Color(0.7f, 0.66f, 0.58f, 0.55f);
        button.colors = colors;
        button.onClick.AddListener(action);

        Text labelText = CaravansaryUITheme.CreateParchmentLabel(buttonObject.transform, label, 40, FontStyle.Bold, Ink,
            Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f),
            Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter);
        labelText.fontStyle = FontStyle.Bold;
        return button;
    }

    private static Text CreateSlotLabel(Transform parent, string value, int fontSize, FontStyle style, Color color,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 position, Vector2 size, TextAnchor alignment)
    {
        Text text = CaravansaryUITheme.CreateParchmentLabel(parent, value, fontSize, style, color,
            anchorMin, anchorMax, pivot, position, size, alignment);
        text.fontStyle = FontStyle.Bold;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Truncate;
        text.raycastTarget = false;
        return text;
    }

    private static GameObject CreateInfoCard(Transform parent)
    {
        var card = new GameObject("Takas");
        card.transform.SetParent(parent, false);
        var rect = card.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.325f, 0.07f);
        rect.anchorMax = new Vector2(0.615f, 0.85f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        var image = card.AddComponent<Image>();
        ApplyParchment(image, Resources.Load<Sprite>("trade_parchment_card"), true);
        return card;
    }

    private static void ApplyParchment(Image image, Sprite sprite, bool sliced)
    {
        if (sprite == null)
        {
            image.color = new Color(0.9f, 0.82f, 0.67f, 1f);
            return;
        }

        image.sprite = sprite;
        image.type = sliced ? Image.Type.Sliced : Image.Type.Simple;
        image.color = Color.white;
    }

    private void BuildTradeLedger(Transform card)
    {
        CreateLedgerTitle(card, "Verdiklerin", new Vector2(0.04f, 0.9f), new Vector2(0.48f, 0.985f));
        CreateLedgerTitle(card, "Aldıkların", new Vector2(0.52f, 0.9f), new Vector2(0.96f, 0.985f));

        var rule = new GameObject("Rule");
        rule.transform.SetParent(card, false);
        var ruleRect = rule.AddComponent<RectTransform>();
        ruleRect.anchorMin = new Vector2(0.5f, 0.42f);
        ruleRect.anchorMax = new Vector2(0.5f, 0.9f);
        ruleRect.offsetMin = new Vector2(-1f, 0f);
        ruleRect.offsetMax = new Vector2(1f, 0f);
        rule.AddComponent<Image>().color = new Color(0.35f, 0.22f, 0.12f, 0.55f);

        gaveList = CreateLedgerColumn(card, new Vector2(0.07f, 0.42f), new Vector2(0.48f, 0.89f));
        tookList = CreateLedgerColumn(card, new Vector2(0.53f, 0.42f), new Vector2(0.94f, 0.89f));

        var goldBox = new GameObject("GoldBox");
        goldBox.transform.SetParent(card, false);
        var goldRect = goldBox.AddComponent<RectTransform>();
        goldRect.anchorMin = new Vector2(0.04f, 0.135f);
        goldRect.anchorMax = new Vector2(0.96f, 0.4f);
        goldRect.offsetMin = Vector2.zero;
        goldRect.offsetMax = Vector2.zero;
        var goldImage = goldBox.AddComponent<Image>();
        ApplyParchment(goldImage, Resources.Load<Sprite>("trade_parchment_banner"), true);
        CreateGoldStat(goldBox.transform, "Verilen", new Vector2(0.27f, 0.74f), out givenGoldText, out givenGoldCoin);
        CreateGoldStat(goldBox.transform, "Alinan", new Vector2(0.73f, 0.74f), out takenGoldText, out takenGoldCoin);
        CreateGoldStat(goldBox.transform, "Kasa", new Vector2(0.5f, 0.58f), out treasuryGoldText, out treasuryGoldCoin);
        BuildGoldInput(goldBox.transform);
    }

    private void CreateGoldStat(Transform parent, string name, Vector2 anchor, out Text label, out RectTransform coin)
    {
        var row = new GameObject(name);
        row.transform.SetParent(parent, false);
        var rowRect = row.AddComponent<RectTransform>();
        rowRect.anchorMin = anchor;
        rowRect.anchorMax = anchor;
        rowRect.pivot = new Vector2(0.5f, 0.5f);
        rowRect.sizeDelta = new Vector2(240f, 36f);

        label = CaravansaryUITheme.CreateParchmentLabel(row.transform, "", 24, FontStyle.Bold, Ink,
            new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(0f, 0.5f),
            Vector2.zero, new Vector2(180f, 0f), TextAnchor.MiddleLeft);
        label.fontStyle = FontStyle.Bold;
        label.horizontalOverflow = HorizontalWrapMode.Overflow;
        label.verticalOverflow = VerticalWrapMode.Overflow;

        var coinObject = new GameObject("Coin");
        coinObject.transform.SetParent(row.transform, false);
        coin = coinObject.AddComponent<RectTransform>();
        coin.anchorMin = new Vector2(0f, 0.5f);
        coin.anchorMax = new Vector2(0f, 0.5f);
        coin.pivot = new Vector2(0f, 0.5f);
        coin.sizeDelta = new Vector2(22f, 22f);
        var image = coinObject.AddComponent<Image>();
        image.sprite = CoinSprite;
        image.preserveAspect = true;
        image.raycastTarget = false;
    }

    private static void SetGoldStat(Text label, RectTransform coin, string value)
    {
        if (label == null)
            return;

        label.text = value;
        float textWidth = label.preferredWidth;
        if (textWidth < 8f)
            textWidth = value.Length * 13f;
        label.rectTransform.sizeDelta = new Vector2(textWidth + 2f, 32f);
        if (coin != null)
            coin.anchoredPosition = new Vector2(textWidth + 4f, 0f);
        var row = label.rectTransform.parent as RectTransform;
        if (row != null)
            row.sizeDelta = new Vector2(textWidth + 30f, 36f);
    }

    private void BuildGoldInput(Transform goldBox)
    {
        Text caption = CaravansaryUITheme.CreateParchmentLabel(goldBox, "Takasa altın :", 28, FontStyle.Bold, Ink,
            new Vector2(0.03f, 0.12f), new Vector2(0.38f, 0.42f), new Vector2(0f, 0.5f),
            Vector2.zero, Vector2.zero, TextAnchor.MiddleLeft);
        caption.fontStyle = FontStyle.Bold;
        caption.rectTransform.offsetMin = new Vector2(8f, 0f);

        var inputObject = new GameObject("GoldInput");
        inputObject.transform.SetParent(goldBox, false);
        var inputRect = inputObject.AddComponent<RectTransform>();
        inputRect.anchorMin = new Vector2(0.4f, 0.12f);
        inputRect.anchorMax = new Vector2(0.4f, 0.12f);
        inputRect.pivot = new Vector2(0f, 0f);
        inputRect.anchoredPosition = Vector2.zero;
        inputRect.sizeDelta = new Vector2(280f, 76f);
        var inputImage = inputObject.AddComponent<Image>();
        ApplyParchment(inputImage, Resources.Load<Sprite>("trade_parchment_banner"), true);

        var textObject = new GameObject("Text");
        textObject.transform.SetParent(inputObject.transform, false);
        var textRect = textObject.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(118f, 6f);
        textRect.offsetMax = new Vector2(-16f, -6f);
        var inputText = textObject.AddComponent<Text>();
        inputText.font = CaravansaryUITheme.ParchmentFont;
        inputText.fontSize = 34;
        inputText.fontStyle = FontStyle.Bold;
        inputText.color = Ink;
        inputText.alignment = TextAnchor.MiddleLeft;
        inputText.supportRichText = false;
        inputText.horizontalOverflow = HorizontalWrapMode.Overflow;
        inputText.verticalOverflow = VerticalWrapMode.Overflow;

        offeredGoldInput = inputObject.AddComponent<InputField>();
        offeredGoldInput.targetGraphic = inputImage;
        offeredGoldInput.textComponent = inputText;
        offeredGoldInput.contentType = InputField.ContentType.IntegerNumber;
        offeredGoldInput.lineType = InputField.LineType.SingleLine;
        offeredGoldInput.characterValidation = InputField.CharacterValidation.Integer;
        offeredGoldInput.text = "0";
        offeredGoldInput.onEndEdit.AddListener(CommitOfferedGold);
    }

    private void CommitOfferedGold(string value)
    {
        if (writingGold)
            return;

        int amount = 0;
        if (!string.IsNullOrEmpty(value))
            int.TryParse(value, out amount);
        if (amount < 0)
            amount = 0;

        offeredGold = amount;
        Refresh();
    }

    private static void CreateLedgerTitle(Transform parent, string title, Vector2 anchorMin, Vector2 anchorMax)
    {
        Text text = CaravansaryUITheme.CreateParchmentLabel(parent, title, 40, FontStyle.Bold, Ink,
            anchorMin, anchorMax, new Vector2(0.5f, 0.5f),
            Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter);
        text.fontStyle = FontStyle.Bold;
    }

    private static RectTransform CreateLedgerColumn(Transform parent, Vector2 anchorMin, Vector2 anchorMax)
    {
        var scrollObject = new GameObject("LedgerScroll");
        scrollObject.transform.SetParent(parent, false);
        var scrollRectTransform = scrollObject.AddComponent<RectTransform>();
        scrollRectTransform.anchorMin = anchorMin;
        scrollRectTransform.anchorMax = anchorMax;
        scrollRectTransform.offsetMin = Vector2.zero;
        scrollRectTransform.offsetMax = Vector2.zero;
        var scroll = scrollObject.AddComponent<ScrollRect>();
        scroll.horizontal = false;
        scroll.movementType = ScrollRect.MovementType.Clamped;
        scroll.scrollSensitivity = 20f;

        var viewport = new GameObject("Viewport");
        viewport.transform.SetParent(scrollObject.transform, false);
        var viewportRect = viewport.AddComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.offsetMin = Vector2.zero;
        viewportRect.offsetMax = Vector2.zero;
        var viewportImage = viewport.AddComponent<Image>();
        viewportImage.color = new Color(1f, 1f, 1f, 0.01f);
        viewport.AddComponent<RectMask2D>();

        var content = new GameObject("Content");
        content.transform.SetParent(viewport.transform, false);
        var contentRect = content.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0f, 1f);
        contentRect.anchorMax = new Vector2(1f, 1f);
        contentRect.pivot = new Vector2(0f, 1f);
        contentRect.anchoredPosition = Vector2.zero;
        contentRect.sizeDelta = new Vector2(0f, 40f);

        scroll.viewport = viewportRect;
        scroll.content = contentRect;
        return contentRect;
    }

    private void FillTradeList(RectTransform content, Dictionary<string, int> selection, bool merchantSide, int goldLine)
    {
        if (content == null)
            return;

        for (int i = content.childCount - 1; i >= 0; i--)
            DestroyImmediate(content.GetChild(i).gameObject);

        var lines = new List<LedgerLine>();
        if (selection != null)
        {
            foreach (KeyValuePair<string, int> pair in selection)
            {
                if (pair.Value <= 0)
                    continue;

                ItemDefinition item = ItemCatalog.Get(pair.Key);
                if (item == null)
                    continue;

                int unit = merchantSide ? ItemCatalog.MerchantPrice(item) : ItemCatalog.BasePrice(item);
                lines.Add(new LedgerLine(item.DisplayName, pair.Value, unit * pair.Value));
            }
        }

        lines.Sort((a, b) => string.CompareOrdinal(a.Name, b.Name));
        Canvas.ForceUpdateCanvases();
        float width = content.rect.width;
        if (width < 80f)
            width = 200f;

        int index = 0;
        for (int i = 0; i < lines.Count; i++)
            AddLedgerLine(content, lines[i].Count + " " + lines[i].Name + " = " + lines[i].Total, index++, width, true);

        if (goldLine > 0)
            AddLedgerLine(content, goldLine + " altın = " + goldLine, index++, width, true);

        if (index == 0)
            AddLedgerLine(content, "-", index++, width, false);

        content.sizeDelta = new Vector2(0f, index * 48f + 8f);
    }

    private static void AddLedgerLine(RectTransform content, string label, int index, float width, bool withCoin)
    {
        float rowWidth = Mathf.Max(80f, width - 8f);
        var row = new GameObject("Line");
        row.transform.SetParent(content, false);
        var rowRect = row.AddComponent<RectTransform>();
        rowRect.anchorMin = new Vector2(0f, 1f);
        rowRect.anchorMax = new Vector2(0f, 1f);
        rowRect.pivot = new Vector2(0f, 1f);
        rowRect.anchoredPosition = new Vector2(4f, -4f - index * 48f);
        rowRect.sizeDelta = new Vector2(rowWidth, 44f);

        Text line = CreateSlotLabel(row.transform, label, 26, FontStyle.Bold, Ink,
            new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(0f, 0.5f),
            Vector2.zero, new Vector2(rowWidth, 0f), TextAnchor.MiddleLeft);
        var lineRect = line.rectTransform;
        lineRect.anchorMin = new Vector2(0f, 0f);
        lineRect.anchorMax = new Vector2(0f, 1f);
        lineRect.pivot = new Vector2(0f, 0.5f);
        line.horizontalOverflow = HorizontalWrapMode.Overflow;
        line.verticalOverflow = VerticalWrapMode.Overflow;
        line.alignByGeometry = true;
        float textWidth = line.preferredWidth;
        if (textWidth < 8f)
            textWidth = label.Length * 15f;
        lineRect.anchoredPosition = new Vector2(2f, 0f);
        lineRect.sizeDelta = new Vector2(textWidth + 2f, 0f);

        if (!withCoin || CoinSprite == null)
            return;

        var coin = new GameObject("Coin");
        coin.transform.SetParent(row.transform, false);
        var coinRect = coin.AddComponent<RectTransform>();
        coinRect.anchorMin = new Vector2(0f, 0.5f);
        coinRect.anchorMax = new Vector2(0f, 0.5f);
        coinRect.pivot = new Vector2(0f, 0.5f);
        coinRect.anchoredPosition = new Vector2(2f + textWidth + 4f, 0f);
        coinRect.sizeDelta = new Vector2(24f, 24f);
        var image = coin.AddComponent<Image>();
        image.sprite = CoinSprite;
        image.preserveAspect = true;
        image.raycastTarget = false;
    }

    private struct LedgerLine
    {
        public LedgerLine(string name, int count, int total)
        {
            Name = name;
            Count = count;
            Total = total;
        }

        public string Name;
        public int Count;
        public int Total;
    }

    private struct TradeSlot
    {
        public TradeSlot(ItemDefinition item, int owned, int selected, int price)
        {
            Item = item;
            Owned = owned;
            Selected = selected;
            Price = price;
        }

        public ItemDefinition Item;
        public int Owned;
        public int Selected;
        public int Price;
    }
}
