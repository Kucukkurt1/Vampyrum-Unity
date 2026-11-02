using UnityEngine;
using UnityEngine.UI;

public class NpcTalkInteraction : MonoBehaviour
{
    private static readonly string[] FallbackLines =
    {
        "Merhaba, ben Nikolai. Bana Niko diyebilirsin.",
        "…'den geliyorum. …'e gitmek istiyorum ama fırtınada dışarıda kalmak istemedim.",
        "Beni içeri aldığın için teşekkürler."
    };

    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private string speakerName = "Nikolai";

    private bool inRange;
    private GameObject prompt;

    private void Awake()
    {
        var trigger = gameObject.AddComponent<CircleCollider2D>();
        trigger.isTrigger = true;
        trigger.radius = 1.6f;
        CreatePrompt();
    }

    private void Update()
    {
        bool talking = DialogueUI.Instance != null && DialogueUI.Instance.IsVisible;
        if (prompt != null && prompt.activeSelf != (inRange && !talking))
            prompt.SetActive(inRange && !talking);

        if (!inRange || talking || !Input.GetKeyDown(interactKey) || Time.timeScale == 0f)
            return;

        GameManager.EnsureInstance();
        DialogueUI.Instance?.ShowLines(speakerName, NikolaiLines());
    }

    private static string[] NikolaiLines()
    {
        NpcCharacterDefinition nikolai = NpcCatalog.GetById(2);
        CharacterDialogueFile file = nikolai != null
            ? NpcDialogueCatalog.GetCharacterDialogue(nikolai, true)
            : null;
        if (file?.beats == null)
            return FallbackLines;

        for (int i = 0; i < file.beats.Length; i++)
        {
            DialogueLine[] lines = file.beats[i]?.lines;
            if (lines == null || lines.Length == 0)
                continue;

            var texts = new string[lines.Length];
            for (int line = 0; line < lines.Length; line++)
                texts[line] = lines[line].text;
            return texts;
        }

        return FallbackLines;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            inRange = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            inRange = false;
    }

    private void CreatePrompt()
    {
        prompt = new GameObject("Prompt");
        prompt.transform.SetParent(transform, false);
        prompt.transform.localPosition = new Vector3(0f, 1.35f, 0f);
        var canvas = prompt.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.sortingOrder = 30;
        var rect = prompt.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(120f, 80f);
        rect.localScale = Vector3.one * 0.015f;

        Text text = CaravansaryUITheme.CreateLabel(prompt.transform, "E", 56, FontStyle.Bold,
            new Color(0.95f, 0.88f, 0.7f), TextAnchor.MiddleCenter);
        var textRect = text.rectTransform;
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        prompt.SetActive(false);
    }
}
