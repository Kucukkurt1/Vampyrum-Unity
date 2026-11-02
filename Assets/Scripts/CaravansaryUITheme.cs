using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

public static class CaravansaryUITheme
{
    private const float ScrollSensitivity = 30f;

    public static readonly Color Overlay = new Color(0f, 0f, 0f, 0.58f);
    public static readonly Color PanelBg = new Color(0.11f, 0.07f, 0.05f, 0.97f);
    public static readonly Color PanelBorder = new Color(0.72f, 0.54f, 0.28f, 0.9f);
    public static readonly Color CardBg = new Color(0.17f, 0.11f, 0.08f, 0.92f);
    public static readonly Color CardBorder = new Color(0.38f, 0.27f, 0.17f, 0.85f);
    public static readonly Color ScrollBg = new Color(0.06f, 0.04f, 0.03f, 0.55f);
    public static readonly Color TextPrimary = new Color(0.95f, 0.90f, 0.82f);
    public static readonly Color TextSecondary = new Color(0.76f, 0.68f, 0.58f);
    public static readonly Color TextMuted = new Color(0.55f, 0.48f, 0.42f);
    public static readonly Color AccentGold = new Color(0.88f, 0.68f, 0.36f);
    public static readonly Color AccentGreen = new Color(0.58f, 0.80f, 0.52f);
    public static readonly Color AccentRed = new Color(0.88f, 0.46f, 0.40f);
    public static readonly Color TabActive = new Color(0.48f, 0.30f, 0.16f, 1f);
    public static readonly Color TabInactive = new Color(0.22f, 0.14f, 0.10f, 0.95f);
    public static readonly Color ButtonAccept = new Color(0.26f, 0.46f, 0.28f, 1f);
    public static readonly Color ButtonReject = new Color(0.50f, 0.22f, 0.18f, 1f);
    public static readonly Color ButtonPrimary = new Color(0.55f, 0.34f, 0.16f, 1f);
    public static readonly Color ButtonDisabled = new Color(0.25f, 0.22f, 0.20f, 0.7f);
    public static readonly Color StatBoxBg = new Color(0.14f, 0.09f, 0.07f, 0.95f);
    public static readonly Color InkPrimary = Color.black;
    public static readonly Color InkSecondary = Color.black;
    public static readonly Color PortraitPlaceholder = new Color(0.32f, 0.24f, 0.16f, 0.18f);
    public static readonly Color ParchmentSilver = new Color(0.93f, 0.95f, 0.97f, 1f);
    public static readonly Color ParchmentSilverHover = Color.white;
    public static readonly Color ParchmentGold = new Color(192f / 255f, 192f / 255f, 192f / 255f, 1f);
    public static readonly Color ParchmentGoldHover = new Color(216f / 255f, 216f / 255f, 216f / 255f, 1f);
    public static readonly Color ParchmentSilverPressed = new Color(0.74f, 0.78f, 0.82f, 1f);
    public static readonly Color ParchmentSilverDisabled = new Color(0.78f, 0.78f, 0.78f, 0.38f);

    public const string ActiveBookFontResource = "AmaticSC-Regular";

    private static Font parchmentFont;

    public static Font DefaultFont => Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

    public static Font ParchmentFont
    {
        get
        {
            if (parchmentFont != null)
                return parchmentFont;

            Font bundled = Resources.Load<Font>(ActiveBookFontResource);
            if (bundled != null)
            {
                parchmentFont = bundled;
                return parchmentFont;
            }

            string[] handwritingFonts = { "Georgia", "Palatino Linotype", "Times New Roman", "Segoe UI" };
            foreach (string fontName in handwritingFonts)
            {
                try
                {
                    parchmentFont = Font.CreateDynamicFontFromOSFont(fontName, 32);
                    if (parchmentFont != null)
                        return parchmentFont;
                }
                catch (System.Exception)
                {
                }
            }

            parchmentFont = DefaultFont;
            return parchmentFont;
        }
    }

    public static Font ContentsHandFont => ParchmentFont;

    public static GameObject CreateOverlay(Transform parent, string name = "Overlay")
    {
        var overlay = new GameObject(name);
        overlay.transform.SetParent(parent, false);

        var rect = overlay.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        var overlayImage = overlay.AddComponent<Image>();
        overlayImage.color = Overlay;
        overlayImage.raycastTarget = false;
        return overlay;
    }

    public static GameObject CreateFramedPanel(Transform parent, Vector2 size, string name = "Panel")
    {
        var panel = new GameObject(name);
        panel.transform.SetParent(parent, false);

        var rect = panel.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;

        panel.AddComponent<Image>().color = PanelBg;

        var border = new GameObject("Border");
        border.transform.SetParent(panel.transform, false);
        var borderRect = border.AddComponent<RectTransform>();
        borderRect.anchorMin = Vector2.zero;
        borderRect.anchorMax = Vector2.one;
        borderRect.offsetMin = new Vector2(-2f, -2f);
        borderRect.offsetMax = new Vector2(2f, 2f);
        border.transform.SetAsFirstSibling();
        var outline = border.AddComponent<Outline>();
        outline.effectColor = PanelBorder;
        outline.effectDistance = new Vector2(2f, -2f);

        return panel;
    }

    public static GameObject CreateCard(Transform parent, float height, string name = "Card")
    {
        var card = new GameObject(name);
        card.transform.SetParent(parent, false);

        var layout = card.AddComponent<LayoutElement>();
        layout.minHeight = height;
        layout.preferredHeight = height;

        card.AddComponent<Image>().color = CardBg;

        var outline = card.AddComponent<Outline>();
        outline.effectColor = CardBorder;
        outline.effectDistance = new Vector2(1f, -1f);

        return card;
    }

    public static Text CreateLabel(Transform parent, string content, int fontSize, FontStyle style, Color color,
        TextAnchor alignment = TextAnchor.MiddleLeft)
    {
        var textObject = new GameObject("Label");
        textObject.transform.SetParent(parent, false);

        var text = textObject.AddComponent<Text>();
        text.font = DefaultFont;
        text.fontSize = fontSize;
        text.fontStyle = style;
        text.text = content;
        text.color = color;
        text.alignment = alignment;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.raycastTarget = false;
        return text;
    }

    public static Text CreateAnchoredLabel(Transform parent, string content, int fontSize, FontStyle style, Color color,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 position, Vector2 size, TextAnchor alignment)
    {
        var text = CreateLabel(parent, content, fontSize, style, color, alignment);
        var rect = text.rectTransform;
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = pivot;
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        return text;
    }

    public static Text CreateParchmentLabel(
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
        Text text = CreateAnchoredLabel(parent, content, fontSize, style, color,
            anchorMin, anchorMax, pivot, position, size, alignment);
        text.font = ParchmentFont;
        text.fontStyle = FontStyle.Normal;
        text.fontSize = fontSize + 2;
        text.lineSpacing = 1.22f;
        Shadow ink = text.gameObject.AddComponent<Shadow>();
        ink.effectColor = new Color(0.05f, 0.03f, 0.02f, 0.9f);
        ink.effectDistance = new Vector2(0.7f, -0.7f);
        return text;
    }

    public static GameObject CreateSectionHeader(Transform parent, string title, Vector2 position, float width = 620f)
    {
        var header = new GameObject("SectionHeader");
        header.transform.SetParent(parent, false);

        var rect = header.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(width, 28f);

        CreateAnchoredLabel(header.transform, title, 17, FontStyle.Bold, AccentGold,
            Vector2.zero, Vector2.one, new Vector2(0f, 0.5f), new Vector2(8f, 0f), new Vector2(-8f, 24f),
            TextAnchor.MiddleLeft);

        var line = new GameObject("Line");
        line.transform.SetParent(header.transform, false);
        var lineRect = line.AddComponent<RectTransform>();
        lineRect.anchorMin = new Vector2(0f, 0f);
        lineRect.anchorMax = new Vector2(1f, 0f);
        lineRect.pivot = new Vector2(0.5f, 0f);
        lineRect.anchoredPosition = Vector2.zero;
        lineRect.sizeDelta = new Vector2(0f, 2f);
        line.AddComponent<Image>().color = new Color(PanelBorder.r, PanelBorder.g, PanelBorder.b, 0.45f);

        return header;
    }

    public static GameObject CreateStatBadge(Transform parent, string label, string value, Vector2 position, Vector2 size)
    {
        var badge = new GameObject("StatBadge");
        badge.transform.SetParent(parent, false);

        var rect = badge.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        badge.AddComponent<Image>().color = StatBoxBg;

        var outline = badge.AddComponent<Outline>();
        outline.effectColor = CardBorder;
        outline.effectDistance = new Vector2(1f, -1f);

        CreateAnchoredLabel(badge.transform, label, 13, FontStyle.Normal, TextMuted,
            new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -8f), new Vector2(0f, 18f),
            TextAnchor.MiddleCenter);

        CreateAnchoredLabel(badge.transform, value, 20, FontStyle.Bold, AccentGold,
            new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 10f), new Vector2(0f, 28f),
            TextAnchor.MiddleCenter);

        return badge;
    }

    public static (Button button, Image image) CreateButton(Transform parent, string label, Vector2 position, Vector2 size,
        Color bgColor, UnityAction onClick, int fontSize = 15)
    {
        var buttonObject = new GameObject(label + "Button");
        buttonObject.transform.SetParent(parent, false);

        var rect = buttonObject.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        var image = buttonObject.AddComponent<Image>();
        image.color = bgColor;

        var button = buttonObject.AddComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(onClick);

        CreateAnchoredLabel(buttonObject.transform, label, fontSize, FontStyle.Bold, TextPrimary,
            Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter);

        return (button, image);
    }

    public static (Button button, Image image) CreateBottomButton(Transform parent, string label, Vector2 position,
        Vector2 size, Color bgColor, UnityAction onClick, int fontSize = 15)
    {
        var buttonObject = new GameObject(label + "Button");
        buttonObject.transform.SetParent(parent, false);

        var rect = buttonObject.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0f);
        rect.anchorMax = new Vector2(0.5f, 0f);
        rect.pivot = new Vector2(0.5f, 0f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        var image = buttonObject.AddComponent<Image>();
        image.color = bgColor;
        image.raycastTarget = true;

        var button = buttonObject.AddComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(onClick);

        CreateAnchoredLabel(buttonObject.transform, label, fontSize, FontStyle.Bold, TextPrimary,
            Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter);

        return (button, image);
    }

    public static GameObject CreateParchmentSpread(Transform parent, bool fillScreen = false)
    {
        var spread = new GameObject("GuestParchmentSpread");
        spread.transform.SetParent(parent, false);

        var rect = spread.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = fillScreen ? new Vector2(-120f, -70f) : Vector2.zero;
        rect.offsetMax = fillScreen ? new Vector2(120f, 70f) : Vector2.zero;

        var image = spread.AddComponent<Image>();
        image.sprite = Resources.Load<Sprite>("caravansary_guest_parchment");
        image.color = image.sprite != null ? Color.white : new Color(0.86f, 0.76f, 0.58f, 1f);
        image.preserveAspect = !fillScreen;
        image.raycastTarget = false;

        return spread;
    }

    public static GameObject CreatePortraitPlaceholder(
        Transform parent,
        Vector2 anchorMin,
        Vector2 anchorMax,
        out Image portraitImage,
        out Text placeholderLabel)
    {
        var frame = new GameObject("PortraitFrame");
        frame.transform.SetParent(parent, false);

        var rect = frame.AddComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        var art = new GameObject("PortraitArt");
        art.transform.SetParent(frame.transform, false);
        var artRect = art.AddComponent<RectTransform>();
        artRect.anchorMin = Vector2.zero;
        artRect.anchorMax = Vector2.one;
        artRect.offsetMin = Vector2.zero;
        artRect.offsetMax = Vector2.zero;
        portraitImage = art.AddComponent<Image>();
        portraitImage.preserveAspect = true;
        portraitImage.raycastTarget = false;
        portraitImage.enabled = false;

        placeholderLabel = CreateParchmentLabel(frame.transform, "", 20, FontStyle.Italic,
            new Color(InkSecondary.r, InkSecondary.g, InkSecondary.b, 0f),
            new Vector2(0.22f, 0.38f), new Vector2(0.78f, 0.58f), new Vector2(0.5f, 0.5f),
            Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter);
        placeholderLabel.gameObject.SetActive(false);

        return frame;
    }

    public static (Button button, Image image) CreatePageButton(
        Transform parent,
        string label,
        Vector2 anchorMin,
        Vector2 anchorMax,
        UnityAction onClick,
        int fontSize = 17)
    {
        var buttonObject = new GameObject(label + "Button");
        buttonObject.transform.SetParent(parent, false);

        var rect = buttonObject.AddComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        var image = buttonObject.AddComponent<Image>();
        image.color = Color.clear;
        image.raycastTarget = true;

        Text buttonText = CreateParchmentLabel(
            buttonObject.transform, label, fontSize, FontStyle.Bold, Color.white,
            Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero,
            TextAnchor.MiddleCenter);

        Outline stroke = buttonText.gameObject.AddComponent<Outline>();
        stroke.effectColor = new Color(0.10f, 0.08f, 0.06f, 0.9f);
        stroke.effectDistance = new Vector2(1.1f, -1.1f);

        var button = buttonObject.AddComponent<Button>();
        button.targetGraphic = buttonText;
        button.onClick.AddListener(onClick);

        ColorBlock colors = button.colors;
        colors.normalColor = ParchmentSilver;
        colors.highlightedColor = ParchmentSilverHover;
        colors.pressedColor = ParchmentSilverPressed;
        colors.selectedColor = ParchmentSilverHover;
        colors.disabledColor = ParchmentSilverDisabled;
        colors.colorMultiplier = 1f;
        colors.fadeDuration = 0.08f;
        button.colors = colors;

        return (button, image);
    }

    public static Transform CreateScrollArea(Transform parent, Vector2 anchoredPosition, Vector2 size)
    {
        var scrollObject = new GameObject("ScrollArea");
        scrollObject.transform.SetParent(parent, false);

        var scrollRect = scrollObject.AddComponent<RectTransform>();
        scrollRect.anchorMin = new Vector2(0.5f, 1f);
        scrollRect.anchorMax = new Vector2(0.5f, 1f);
        scrollRect.pivot = new Vector2(0.5f, 1f);
        scrollRect.anchoredPosition = anchoredPosition;
        scrollRect.sizeDelta = size;

        scrollObject.AddComponent<Image>().color = ScrollBg;

        var outline = scrollObject.AddComponent<Outline>();
        outline.effectColor = CardBorder;
        outline.effectDistance = new Vector2(1f, -1f);

        var scroll = scrollObject.AddComponent<ScrollRect>();

        var viewport = new GameObject("Viewport");
        viewport.transform.SetParent(scrollObject.transform, false);
        var viewportRect = viewport.AddComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.offsetMin = new Vector2(4f, 4f);
        viewportRect.offsetMax = new Vector2(-4f, -4f);
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
        layout.spacing = 10f;
        layout.padding = new RectOffset(8, 8, 8, 8);
        layout.childControlHeight = true;
        layout.childControlWidth = true;
        layout.childForceExpandHeight = false;
        layout.childForceExpandWidth = true;

        content.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scroll.content = contentRect;
        scroll.viewport = viewportRect;
        scroll.horizontal = false;
        ConfigureScrollRect(scroll);

        return content.transform;
    }

    public static Transform CreateColumnWithScroll(Transform parent, string title, float anchorMinX, float anchorMaxX)
    {
        var column = new GameObject(title + "Column");
        column.transform.SetParent(parent, false);

        var colRect = column.AddComponent<RectTransform>();
        colRect.anchorMin = new Vector2(anchorMinX, 0f);
        colRect.anchorMax = new Vector2(anchorMaxX, 1f);
        colRect.offsetMin = new Vector2(6f, 0f);
        colRect.offsetMax = new Vector2(-6f, 0f);

        var header = new GameObject("Header");
        header.transform.SetParent(column.transform, false);
        var headerRect = header.AddComponent<RectTransform>();
        headerRect.anchorMin = new Vector2(0f, 1f);
        headerRect.anchorMax = new Vector2(1f, 1f);
        headerRect.pivot = new Vector2(0.5f, 1f);
        headerRect.anchoredPosition = Vector2.zero;
        headerRect.sizeDelta = new Vector2(0f, 30f);

        CreateAnchoredLabel(header.transform, title, 17, FontStyle.Bold, AccentGold,
            Vector2.zero, Vector2.one, new Vector2(0f, 0.5f), new Vector2(8f, 0f), new Vector2(-8f, 26f),
            TextAnchor.MiddleLeft);

        var line = new GameObject("Line");
        line.transform.SetParent(header.transform, false);
        var lineRect = line.AddComponent<RectTransform>();
        lineRect.anchorMin = new Vector2(0f, 0f);
        lineRect.anchorMax = new Vector2(1f, 0f);
        lineRect.pivot = new Vector2(0.5f, 0f);
        lineRect.anchoredPosition = Vector2.zero;
        lineRect.sizeDelta = new Vector2(0f, 2f);
        var lineImage = line.AddComponent<Image>();
        lineImage.color = new Color(PanelBorder.r, PanelBorder.g, PanelBorder.b, 0.45f);
        lineImage.raycastTarget = false;

        var scrollObject = new GameObject("ScrollArea");
        scrollObject.transform.SetParent(column.transform, false);

        var scrollRectTransform = scrollObject.AddComponent<RectTransform>();
        scrollRectTransform.anchorMin = Vector2.zero;
        scrollRectTransform.anchorMax = Vector2.one;
        scrollRectTransform.offsetMin = new Vector2(0f, 0f);
        scrollRectTransform.offsetMax = new Vector2(0f, -34f);

        var scrollBg = scrollObject.AddComponent<Image>();
        scrollBg.color = ScrollBg;

        var scroll = scrollObject.AddComponent<ScrollRect>();

        var viewport = new GameObject("Viewport");
        viewport.transform.SetParent(scrollObject.transform, false);
        var viewportRect = viewport.AddComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.offsetMin = new Vector2(4f, 4f);
        viewportRect.offsetMax = new Vector2(-4f, -4f);
        viewport.AddComponent<Mask>().showMaskGraphic = false;
        var viewportImage = viewport.AddComponent<Image>();
        viewportImage.color = new Color(1f, 1f, 1f, 0.02f);
        viewportImage.raycastTarget = true;

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
        ConfigureScrollRect(scroll);

        return content.transform;
    }

    private static void ConfigureScrollRect(ScrollRect scroll)
    {
        scroll.scrollSensitivity = ScrollSensitivity;
        scroll.inertia = true;
        scroll.decelerationRate = 0.12f;
    }

    public static void EnsureEventSystem()
    {
        EventSystem[] found = Object.FindObjectsOfType<EventSystem>();
        EventSystem eventSystem = null;
        for (int i = 0; i < found.Length; i++)
        {
            if (found[i] != null && found[i].gameObject.scene.name == "DontDestroyOnLoad")
            {
                eventSystem = found[i];
                break;
            }
        }

        if (eventSystem == null && found.Length > 0)
            eventSystem = found[0];

        if (eventSystem == null)
        {
            var eventSystemObject = new GameObject("EventSystem");
            Object.DontDestroyOnLoad(eventSystemObject);
            eventSystemObject.AddComponent<EventSystem>();
            UnscaledStandaloneInputModule created = eventSystemObject.AddComponent<UnscaledStandaloneInputModule>();
            created.forceModuleActive = true;
            return;
        }

        if (eventSystem.gameObject.scene.name != "DontDestroyOnLoad")
            Object.DontDestroyOnLoad(eventSystem.gameObject);

        for (int i = 0; i < found.Length; i++)
        {
            if (found[i] != null && found[i] != eventSystem)
                Object.Destroy(found[i].gameObject);
        }

        UnscaledStandaloneInputModule module = eventSystem.GetComponent<UnscaledStandaloneInputModule>();
        if (module == null)
        {
            StandaloneInputModule legacyModule = eventSystem.GetComponent<StandaloneInputModule>();
            if (legacyModule != null)
                Object.Destroy(legacyModule);

            module = eventSystem.gameObject.AddComponent<UnscaledStandaloneInputModule>();
        }

        module.forceModuleActive = true;
    }
}
