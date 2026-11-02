using UnityEngine;
using UnityEngine.UI;

public class MorningReportUI : MonoBehaviour
{
    [SerializeField] private KeyCode dismissKey = KeyCode.Space;

    private GameObject uiCanvasRoot;
    private Transform reportContentRoot;
    private Text dayHeaderText;
    private bool isVisible;

    public static bool IsBlockingInput { get; private set; }

    private void Awake()
    {
        BuildUI();

        if (GameManager.Instance != null)
            GameManager.Instance.OnMorningReportReady += OnMorningReportReady;
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnMorningReportReady -= OnMorningReportReady;
    }

    private void Update()
    {
        if (!isVisible)
            return;

        if (Input.GetKeyDown(dismissKey) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Escape))
            HideReport();
    }

    private void OnMorningReportReady(string _)
    {
        ShowReport();
    }

    public void ShowReport()
    {
        if (GameManager.Instance != null && dayHeaderText != null)
            dayHeaderText.text = $"Gun {GameManager.Instance.CurrentDay}";

        if (reportContentRoot != null)
            MorningReportViewBuilder.Populate(reportContentRoot, GameManager.Instance);

        if (uiCanvasRoot != null)
            uiCanvasRoot.SetActive(true);

        isVisible = true;
        IsBlockingInput = true;
        Time.timeScale = 0f;
    }

    public void HideReport()
    {
        if (uiCanvasRoot != null)
            uiCanvasRoot.SetActive(false);

        isVisible = false;
        IsBlockingInput = false;

        if (Time.timeScale == 0f)
            Time.timeScale = 1f;

        MerchantManager.Instance?.Return();
    }

    private void BuildUI()
    {
        CaravansaryUITheme.EnsureEventSystem();

        uiCanvasRoot = new GameObject("MorningReportCanvas");
        uiCanvasRoot.transform.SetParent(transform);
        var canvas = uiCanvasRoot.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 200;
        uiCanvasRoot.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        uiCanvasRoot.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1920, 1080);
        uiCanvasRoot.AddComponent<GraphicRaycaster>();

        CaravansaryUITheme.CreateOverlay(uiCanvasRoot.transform, "MorningReportOverlay");

        var panelRoot = CaravansaryUITheme.CreateFramedPanel(uiCanvasRoot.transform, new Vector2(780f, 580f), "MorningReportPanel");

        CaravansaryUITheme.CreateAnchoredLabel(panelRoot.transform, "Sabah Raporu", 30, FontStyle.Bold,
            CaravansaryUITheme.AccentGold,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0f, -20f), new Vector2(700f, 40f), TextAnchor.MiddleCenter);

        dayHeaderText = CaravansaryUITheme.CreateAnchoredLabel(panelRoot.transform, "Gun -", 16, FontStyle.Italic,
            CaravansaryUITheme.TextSecondary,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0f, -52f), new Vector2(300f, 24f), TextAnchor.MiddleCenter);

        reportContentRoot = CaravansaryUITheme.CreateScrollArea(panelRoot.transform,
            new Vector2(0f, -88f), new Vector2(720f, 400f));

        CaravansaryUITheme.CreateBottomButton(panelRoot.transform, "Tamam", new Vector2(0f, 18f),
            new Vector2(200f, 44f), CaravansaryUITheme.ButtonPrimary, HideReport, 18);

        CaravansaryUITheme.CreateAnchoredLabel(panelRoot.transform, "Space / Enter / Esc ile kapat", 14, FontStyle.Italic,
            CaravansaryUITheme.TextMuted,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(0f, 72f), new Vector2(500f, 22f), TextAnchor.MiddleCenter);

        uiCanvasRoot.SetActive(false);
    }
}
