using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
    private Canvas canvas;
    private GameObject panelRoot;
    private Text reasonText;
    private bool isVisible;

    public static bool IsBlockingInteraction { get; private set; }

    private void Awake()
    {
        BuildUI();

        if (GameManager.Instance != null)
            GameManager.Instance.OnGameOver += ShowGameOver;
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnGameOver -= ShowGameOver;

        if (IsBlockingInteraction)
            IsBlockingInteraction = false;
    }

    private void ShowGameOver()
    {
        if (GameManager.Instance == null)
            return;

        if (reasonText != null)
            reasonText.text = GameManager.Instance.LastGameOverReason;

        if (panelRoot != null)
            panelRoot.SetActive(true);

        isVisible = true;
        IsBlockingInteraction = true;

        CaravansaryManagementUI.Instance?.Hide();
    }

    private void BuildUI()
    {
        EnsureEventSystem();

        var canvasObject = new GameObject("GameOverCanvas");
        canvasObject.transform.SetParent(transform);
        canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 250;
        canvasObject.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasObject.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1920, 1080);
        canvasObject.AddComponent<GraphicRaycaster>();

        panelRoot = new GameObject("GameOverPanel");
        panelRoot.transform.SetParent(canvasObject.transform, false);

        var panelImage = panelRoot.AddComponent<Image>();
        panelImage.color = new Color(0.05f, 0.02f, 0.02f, 0.88f);

        var panelRect = panelRoot.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        CreateText(panelRoot.transform, "IFLAS", 36, FontStyle.Bold,
            new Vector2(0.5f, 0.62f), new Vector2(600f, 50f), TextAnchor.MiddleCenter);

        var bodyObject = new GameObject("ReasonText");
        bodyObject.transform.SetParent(panelRoot.transform, false);
        var bodyRect = bodyObject.AddComponent<RectTransform>();
        bodyRect.anchorMin = new Vector2(0.5f, 0.5f);
        bodyRect.anchorMax = new Vector2(0.5f, 0.5f);
        bodyRect.pivot = new Vector2(0.5f, 0.5f);
        bodyRect.anchoredPosition = new Vector2(0f, 0f);
        bodyRect.sizeDelta = new Vector2(700f, 120f);

        reasonText = bodyObject.AddComponent<Text>();
        reasonText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        reasonText.fontSize = 22;
        reasonText.color = new Color(0.93f, 0.88f, 0.78f);
        reasonText.alignment = TextAnchor.MiddleCenter;
        reasonText.horizontalOverflow = HorizontalWrapMode.Wrap;
        reasonText.text = "Kervansaray kapandi.";

        CreateText(panelRoot.transform, "Etkilesimler devre disi. Yeni oyun icin Play'i yeniden baslat.", 16, FontStyle.Italic,
            new Vector2(0.5f, 0.34f), new Vector2(700f, 40f), TextAnchor.MiddleCenter);

        panelRoot.SetActive(false);
    }

    private static Text CreateText(Transform parent, string content, int fontSize, FontStyle style,
        Vector2 anchor, Vector2 sizeDelta, TextAnchor alignment)
    {
        var textObject = new GameObject("Text");
        textObject.transform.SetParent(parent, false);

        var rect = textObject.AddComponent<RectTransform>();
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = anchor;
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = sizeDelta;

        var text = textObject.AddComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = fontSize;
        text.fontStyle = style;
        text.text = content;
        text.color = new Color(0.93f, 0.88f, 0.78f);
        text.alignment = alignment;
        return text;
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
