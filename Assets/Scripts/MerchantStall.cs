using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MerchantStall : MonoBehaviour
{
    private const string SceneName = "Salon1";
    private static bool hooked;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        if (!hooked)
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            hooked = true;
        }

        OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != SceneName)
            return;

        if (GameObject.Find("MerchantStall") != null)
            return;

        BuildStall();
    }

    private static void BuildStall()
    {
        var root = new GameObject("MerchantStall");
        root.transform.position = new Vector3(2.4f, -4.1f, 0f);

        var table = new GameObject("Table");
        table.transform.SetParent(root.transform, false);
        table.transform.localPosition = Vector3.zero;
        var tableRenderer = table.AddComponent<SpriteRenderer>();
        tableRenderer.sprite = LoadOrMake("merchant_table", MakeTableSprite);
        tableRenderer.sortingOrder = 3;
        var tableBody = table.AddComponent<BoxCollider2D>();
        tableBody.size = new Vector2(3.6f, 1.15f);

        var merchant = new GameObject("Merchant");
        merchant.transform.SetParent(root.transform, false);
        merchant.transform.localPosition = new Vector3(0.15f, 1.15f, 0f);
        var merchantRenderer = merchant.AddComponent<SpriteRenderer>();
        merchantRenderer.sprite = LoadOrMake("merchant_figure", MakeMerchantSprite);
        merchantRenderer.sortingOrder = 2;

        var interact = new GameObject("MerchantInteract");
        interact.transform.SetParent(root.transform, false);
        interact.transform.localPosition = new Vector3(-2.3f, 0.2f, 0f);
        var trigger = interact.AddComponent<BoxCollider2D>();
        trigger.isTrigger = true;
        trigger.size = new Vector2(2.2f, 2.4f);
        interact.AddComponent<MerchantInteraction>();
        CreatePrompt(interact.transform);

        var stall = root.AddComponent<MerchantStall>();
        stall.Bind(merchant, interact);
    }

    private GameObject merchantBody;
    private GameObject interactBody;
    private bool listening;

    private void Bind(GameObject merchant, GameObject interact)
    {
        merchantBody = merchant;
        interactBody = interact;
        TryListen();
        ApplyPresence();
    }

    private void Update()
    {
        if (!listening)
            TryListen();
    }

    private void TryListen()
    {
        if (listening || MerchantManager.Instance == null)
            return;

        MerchantManager.Instance.OnPresenceChanged += ApplyPresence;
        listening = true;
        ApplyPresence();
    }

    private void OnDestroy()
    {
        if (listening && MerchantManager.Instance != null)
            MerchantManager.Instance.OnPresenceChanged -= ApplyPresence;
    }

    private void ApplyPresence()
    {
        bool here = MerchantManager.Instance == null || MerchantManager.Instance.IsPresent;
        if (merchantBody != null)
            merchantBody.SetActive(here);
        if (interactBody != null)
            interactBody.SetActive(here);
        if (!here && MerchantTradeUI.Instance != null && MerchantTradeUI.Instance.IsVisible)
            MerchantTradeUI.Instance.Hide();
    }

    private static Sprite LoadOrMake(string resourceName, System.Func<Sprite> make)
    {
        Sprite loaded = Resources.Load<Sprite>(resourceName);
        return loaded != null ? loaded : make();
    }

    private static void CreatePrompt(Transform parent)
    {
        var promptObject = new GameObject("Prompt");
        promptObject.transform.SetParent(parent, false);
        promptObject.transform.localPosition = new Vector3(0f, 1.35f, 0f);
        var canvas = promptObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.sortingOrder = 30;
        canvas.worldCamera = Camera.main;
        var rect = promptObject.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(120f, 80f);
        rect.localScale = Vector3.one * 0.015f;

        Text text = CaravansaryUITheme.CreateLabel(promptObject.transform, "E", 56, FontStyle.Bold,
            new Color(0.95f, 0.88f, 0.7f), TextAnchor.MiddleCenter);
        var textRect = text.rectTransform;
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        promptObject.SetActive(false);
    }

    private static Sprite MakeTableSprite()
    {
        const int width = 72;
        const int height = 28;
        var pixels = new Color[width * height];
        var clear = new Color(0f, 0f, 0f, 0f);
        var top = new Color(0.55f, 0.36f, 0.18f, 1f);
        var plank = new Color(0.42f, 0.26f, 0.12f, 1f);
        var edge = new Color(0.28f, 0.16f, 0.08f, 1f);
        var leg = new Color(0.2f, 0.11f, 0.06f, 1f);
        var cloth = new Color(0.45f, 0.12f, 0.14f, 1f);
        var clothDark = new Color(0.28f, 0.07f, 0.08f, 1f);

        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = clear;

        for (int y = 6; y < height; y++)
        {
            for (int x = 2; x < width - 2; x++)
            {
                bool seam = y > 8 && (x + y) % 9 == 0;
                pixels[y * width + x] = seam ? plank : top;
            }
        }

        for (int x = 1; x < width - 1; x++)
        {
            pixels[6 * width + x] = edge;
            pixels[7 * width + x] = edge;
            pixels[(height - 1) * width + x] = edge;
        }

        for (int y = 0; y < 7; y++)
        {
            for (int x = 6; x <= 10; x++)
                pixels[y * width + x] = leg;
            for (int x = width - 11; x <= width - 7; x++)
                pixels[y * width + x] = leg;
        }

        for (int y = 14; y < height - 2; y++)
        {
            for (int x = 8; x <= 22; x++)
                pixels[y * width + x] = y == 14 || x == 8 || x == 22 ? clothDark : cloth;
        }

        return MakeSprite(width, height, pixels, 20f);
    }

    private static Sprite MakeMerchantSprite()
    {
        const int width = 24;
        const int height = 40;
        var pixels = new Color[width * height];
        var clear = new Color(0f, 0f, 0f, 0f);
        var cloak = new Color(0.18f, 0.1f, 0.14f, 1f);
        var cloakLight = new Color(0.28f, 0.16f, 0.18f, 1f);
        var hood = new Color(0.1f, 0.06f, 0.08f, 1f);
        var face = new Color(0.76f, 0.6f, 0.46f, 1f);
        var beard = new Color(0.35f, 0.24f, 0.16f, 1f);
        var gold = new Color(0.82f, 0.62f, 0.22f, 1f);

        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = clear;

        for (int y = 0; y < 22; y++)
        {
            int inset = y < 3 ? 6 : (y < 8 ? 3 : 2);
            for (int x = inset; x < width - inset; x++)
                pixels[y * width + x] = (x == inset || x == width - inset - 1) ? cloakLight : cloak;
        }

        for (int y = 18; y < 34; y++)
        {
            int inset = y > 30 ? 6 : 5;
            for (int x = inset; x < width - inset; x++)
                pixels[y * width + x] = hood;
        }

        for (int y = 22; y <= 28; y++)
        {
            for (int x = 9; x <= 14; x++)
                pixels[y * width + x] = face;
        }

        for (int y = 22; y <= 24; y++)
        {
            for (int x = 10; x <= 13; x++)
                pixels[y * width + x] = beard;
        }

        pixels[16 * width + 11] = gold;
        pixels[16 * width + 12] = gold;
        pixels[12 * width + 4] = face;
        pixels[12 * width + 5] = face;
        pixels[12 * width + 18] = face;
        pixels[12 * width + 19] = face;

        return MakeSprite(width, height, pixels, 14f);
    }

    private static readonly System.Collections.Generic.List<Texture2D> heldTextures =
        new System.Collections.Generic.List<Texture2D>();

    private static Sprite MakeSprite(int width, int height, Color[] pixels, float pixelsPerUnit)
    {
        var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.hideFlags = HideFlags.HideAndDontSave;
        texture.SetPixels(pixels);
        texture.Apply();
        heldTextures.Add(texture);
        return Sprite.Create(texture, new Rect(0f, 0f, width, height), new Vector2(0.5f, 0.15f), pixelsPerUnit);
    }
}

public class MerchantInteraction : MonoBehaviour
{
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    private bool isPlayerInRange;

    private void Update()
    {
        Transform prompt = transform.Find("Prompt");
        if (prompt != null && prompt.gameObject.activeSelf != isPlayerInRange)
            prompt.gameObject.SetActive(isPlayerInRange);

        if (!isPlayerInRange || !Input.GetKeyDown(interactKey))
            return;

        if (Time.timeScale == 0f)
            return;

        GameManager.EnsureInstance();

        if (GameManager.Instance.IsGameOver || GameOverUI.IsBlockingInteraction)
            return;

        if (CaravansaryManagementUI.Instance != null && CaravansaryManagementUI.Instance.IsVisible)
            return;

        BookInteraction book = FindObjectOfType<BookInteraction>();
        if (book != null && book.IsBookOpen)
            return;

        MerchantTradeUI.Instance?.Show();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            isPlayerInRange = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            if (MerchantTradeUI.Instance != null && MerchantTradeUI.Instance.IsVisible)
                MerchantTradeUI.Instance.Hide();
        }
    }
}
