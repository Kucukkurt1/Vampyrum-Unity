using System;
using UnityEngine;
using UnityEngine.UI;

public class DialogueUI : MonoBehaviour
{
    public static DialogueUI Instance { get; private set; }

    private static readonly Color Ink = new Color(0.27f, 0.15f, 0.08f, 1f);

    private GameObject uiCanvasRoot;
    private Text speakerText;
    private Text bodyText;
    private GuestProfile currentGuest;
    private DialogueResolution currentResolution;
    private string[] looseLines;
    private int lineIndex;
    private int openedFrame;
    private Action onClosed;

    public bool IsVisible => uiCanvasRoot != null && uiCanvasRoot.activeSelf;

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

    public void ShowConversation(GuestProfile guest, Action closedCallback = null)
    {
        if (guest == null)
            return;

        currentGuest = guest;
        looseLines = null;
        onClosed = closedCallback;
        currentResolution = DialogueResolver.Resolve(guest);
        lineIndex = 0;
        speakerText.text = guest.DisplayName;
        ShowCurrentLine();
        Show();
    }

    public void ShowLines(string speaker, string[] lines, Action closedCallback = null)
    {
        currentGuest = null;
        currentResolution = null;
        looseLines = lines;
        onClosed = closedCallback;
        lineIndex = 0;
        speakerText.text = string.IsNullOrEmpty(speaker) ? "Yabancı" : speaker;
        ShowCurrentLine();
        Show();
    }

    private void Update()
    {
        if (!IsVisible || Time.frameCount == openedFrame)
            return;

        if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
            AdvanceOrClose();
        else if (Input.GetKeyDown(KeyCode.Escape))
            Close();
    }

    private void ShowCurrentLine()
    {
        string[] lines = CurrentLines();
        if (lines == null || lines.Length == 0)
        {
            bodyText.text = "...";
            return;
        }

        lineIndex = Mathf.Clamp(lineIndex, 0, lines.Length - 1);
        bodyText.text = lines[lineIndex];
    }

    private string[] CurrentLines()
    {
        if (looseLines != null)
            return looseLines;

        return currentResolution?.Lines;
    }

    private void AdvanceOrClose()
    {
        string[] lines = CurrentLines();
        if (lines != null && lineIndex < lines.Length - 1)
        {
            lineIndex++;
            ShowCurrentLine();
            return;
        }

        Close();
    }

    private void Close()
    {
        Hide();
        onClosed?.Invoke();
        onClosed = null;
        currentGuest = null;
        currentResolution = null;
        looseLines = null;
    }

    private void BuildUI()
    {
        CaravansaryUITheme.EnsureEventSystem();

        uiCanvasRoot = new GameObject("DialogueCanvas");
        uiCanvasRoot.transform.SetParent(transform);

        var canvas = uiCanvasRoot.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 200;

        var scaler = uiCanvasRoot.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
        uiCanvasRoot.AddComponent<GraphicRaycaster>();

        var bar = new GameObject("DialogueBar");
        bar.transform.SetParent(uiCanvasRoot.transform, false);
        var barRect = bar.AddComponent<RectTransform>();
        barRect.anchorMin = new Vector2(0.5f, 0f);
        barRect.anchorMax = new Vector2(0.5f, 0f);
        barRect.pivot = new Vector2(0.5f, 0f);
        barRect.anchoredPosition = new Vector2(0f, 28f);
        barRect.sizeDelta = new Vector2(1640f, 188f);

        var paper = bar.AddComponent<Image>();
        Sprite banner = Resources.Load<Sprite>("trade_parchment_banner");
        if (banner != null)
        {
            paper.sprite = banner;
            paper.type = Image.Type.Sliced;
            paper.color = Color.white;
        }
        else
        {
            paper.color = new Color(0.86f, 0.74f, 0.52f, 0.96f);
        }

        paper.raycastTarget = true;
        var button = bar.AddComponent<Button>();
        button.targetGraphic = paper;
        button.onClick.AddListener(AdvanceOrClose);

        var rule = new GameObject("Rule");
        rule.transform.SetParent(bar.transform, false);
        var ruleRect = rule.AddComponent<RectTransform>();
        ruleRect.anchorMin = new Vector2(0f, 0.5f);
        ruleRect.anchorMax = new Vector2(0f, 0.5f);
        ruleRect.pivot = new Vector2(0.5f, 0.5f);
        ruleRect.anchoredPosition = new Vector2(340f, 6f);
        ruleRect.sizeDelta = new Vector2(2f, 96f);
        var ruleImage = rule.AddComponent<Image>();
        ruleImage.color = new Color(Ink.r, Ink.g, Ink.b, 0.35f);
        ruleImage.raycastTarget = false;

        speakerText = CaravansaryUITheme.CreateParchmentLabel(bar.transform, "Yabancı", 32, FontStyle.Bold, Ink,
            new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(0f, 0.5f),
            Vector2.zero, Vector2.zero, TextAnchor.MiddleLeft);
        Pin(speakerText.rectTransform, 48f, 36f, 310f, -36f);
        speakerText.fontStyle = FontStyle.Bold;
        speakerText.alignment = TextAnchor.MiddleLeft;
        speakerText.raycastTarget = false;

        bodyText = CaravansaryUITheme.CreateParchmentLabel(bar.transform, "...", 28, FontStyle.Normal, Ink,
            Vector2.zero, Vector2.one, new Vector2(0f, 0.5f),
            Vector2.zero, Vector2.zero, TextAnchor.MiddleLeft);
        Pin(bodyText.rectTransform, 348f, 40f, -190f, -36f);
        bodyText.alignment = TextAnchor.MiddleLeft;
        bodyText.horizontalOverflow = HorizontalWrapMode.Wrap;
        bodyText.verticalOverflow = VerticalWrapMode.Overflow;
        bodyText.raycastTarget = false;

        Text cue = CaravansaryUITheme.CreateParchmentLabel(bar.transform, "Devam", 22, FontStyle.Bold, Ink,
            new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(1f, 0.5f),
            Vector2.zero, Vector2.zero, TextAnchor.MiddleRight);
        Pin(cue.rectTransform, -170f, 36f, -48f, -36f);
        cue.fontStyle = FontStyle.Bold;
        cue.alignment = TextAnchor.MiddleRight;
        cue.raycastTarget = false;
    }

    private static void Pin(RectTransform rect, float left, float bottom, float right, float top)
    {
        rect.offsetMin = new Vector2(left, bottom);
        rect.offsetMax = new Vector2(right, top);
    }

    private void Show()
    {
        if (uiCanvasRoot != null)
            uiCanvasRoot.SetActive(true);

        openedFrame = Time.frameCount;
        Time.timeScale = 0f;
    }

    private void Hide()
    {
        if (uiCanvasRoot != null)
            uiCanvasRoot.SetActive(false);

        if (Time.timeScale == 0f)
            Time.timeScale = 1f;
    }
}
