using System;
using System.Collections;
using UnityEngine;

public class BookInteraction : MonoBehaviour
{
    public event Action OnBookOpened;
    public event Action OnBookClosed;

    public bool IsBookOpen => bookUIPanel != null && bookUIPanel.activeSelf;

    [Header("Mod")]
    [SerializeField] private bool wizardGrimoireMode;
    [SerializeField] private bool requireWizardRoomUnlocked = true;

    [Header("UI Referanslar�")]
    [SerializeField] private GameObject bookUIPanel;
    [SerializeField] private Animator bookAnimator;
    [SerializeField] private CanvasGroup bookCanvasGroup;

    [Header("Sayfa Katmanlar�")]
    [SerializeField] private GameObject flippingPageObject;
    [SerializeField] private Animator flippingPageAnimator;

    [Header("Animasyon & Fade Ayarlar�")]
    [SerializeField] private float closeAnimationDuration = 0.5f;
    [SerializeField] private float fadeDuration = 0.3f;
    [SerializeField] private float flipAnimationDuration = 0.4f;

    [Header("Sayfa Ayarlar�")]
    [SerializeField] private int totalPages = 5;
    private int currentPage = 1;

    private bool isPlayerInRange = false;
    private bool isClosing = false;
    private bool isPageFlipping = false;

    public void SetWizardGrimoireMode(bool enabled)
    {
        wizardGrimoireMode = enabled;
    }

    public void TryWireSceneReferences()
    {
        if (bookUIPanel == null)
            bookUIPanel = FindSceneObject("Book_UI");

        if (bookUIPanel == null)
        {
            Debug.LogWarning("BookInteraction: Book_UI bulunamadi.");
            return;
        }

        Transform animated = bookUIPanel.transform.Find("BookAnimated");
        if (animated != null)
        {
            if (bookAnimator == null)
                bookAnimator = animated.GetComponent<Animator>();

            if (bookCanvasGroup == null)
                bookCanvasGroup = animated.GetComponent<CanvasGroup>();
        }

        Transform flipping = bookUIPanel.transform.Find("FlippingPage");
        if (flipping != null)
        {
            if (flippingPageObject == null)
                flippingPageObject = flipping.gameObject;

            if (flippingPageAnimator == null)
                flippingPageAnimator = flipping.GetComponent<Animator>();
        }

        EnsureBookCanvasVisible();
    }

    private static GameObject FindSceneObject(string objectName)
    {
        foreach (GameObject root in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
        {
            Transform found = FindDeepChild(root.transform, objectName);
            if (found != null)
                return found.gameObject;
        }

        return null;
    }

    private static Transform FindDeepChild(Transform parent, string childName)
    {
        if (parent.name == childName)
            return parent;

        for (int i = 0; i < parent.childCount; i++)
        {
            Transform found = FindDeepChild(parent.GetChild(i), childName);
            if (found != null)
                return found;
        }

        return null;
    }

    private void Update()
    {
        if (isClosing) return;

        // Kitap a��kken input kontrolleri
        if (bookUIPanel != null && bookUIPanel.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                CloseBook();
                return;
            }

            // Sayfa �evrilirken yeni tu� alma
            if (!isPageFlipping)
            {
                // D: Sonraki Sayfa
                if (Input.GetKeyDown(KeyCode.D))
                {
                    if (currentPage < totalPages)
                    {
                        StartCoroutine(FlipPageRoutine(forward: true));
                    }
                }
                // A: �nceki Sayfa
                else if (Input.GetKeyDown(KeyCode.A))
                {
                    if (currentPage > 1)
                    {
                        StartCoroutine(FlipPageRoutine(forward: false));
                    }
                }
            }
        }

        // E ile a�/kapat
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.E))
        {
            ToggleBook();
        }
    }

    public void ToggleBook()
    {
        if (bookUIPanel != null && bookUIPanel.activeSelf)
        {
            CloseBook();
        }
        else
        {
            OpenBook();
        }
    }

    public void OpenBook()
    {
        if (bookUIPanel == null)
        {
            Debug.LogWarning("BookInteraction: bookUIPanel bagli degil.");
            return;
        }

        if (wizardGrimoireMode && requireWizardRoomUnlocked)
        {
            if (WizardRoomManager.Instance == null || !WizardRoomManager.Instance.IsUnlocked)
            {
                Debug.Log("Buyucu odasi kilitli.");
                return;
            }
        }

        EnsureBookCanvasVisible();

        isClosing = false;
        isPageFlipping = false;
        currentPage = 1;

        if (flippingPageObject != null)
        {
            flippingPageObject.SetActive(false);
        }

        if (bookUIPanel != null)
        {
            bookUIPanel.SetActive(true);
        }

        if (bookCanvasGroup != null)
        {
            bookCanvasGroup.alpha = 1f;
        }

        if (bookAnimator != null)
        {
            bookAnimator.Play("Intro_Open", -1, 0f);
        }

        Time.timeScale = 0f;

        if (wizardGrimoireMode)
            WizardGrimoireUI.Instance?.Show();

        OnBookOpened?.Invoke();
    }

    public void CloseBook()
    {
        if (!isClosing && gameObject.activeInHierarchy)
        {
            StartCoroutine(CloseBookRoutine());
        }
    }

    private IEnumerator FlipPageRoutine(bool forward)
    {
        isPageFlipping = true;

        if (flippingPageObject != null)
        {
            flippingPageObject.SetActive(true);
        }

        Animator targetAnim = flippingPageAnimator != null ? flippingPageAnimator : bookAnimator;

        if (targetAnim != null)
        {
            if (forward)
            {
                targetAnim.Play("Action_Flip", -1, 0f);
                currentPage++;
            }
            else
            {
                // Action_Flip_Back state'i speed -1 oldu�u i�in 0f'ten ba�lat�l�r
                targetAnim.Play("Action_Flip_Back", -1, 0f);
                currentPage--;
            }
        }

        yield return new WaitForSecondsRealtime(flipAnimationDuration);

        if (flippingPageObject != null)
        {
            flippingPageObject.SetActive(false);
        }

        isPageFlipping = false;
    }

    private IEnumerator CloseBookRoutine()
    {
        isClosing = true;

        if (flippingPageObject != null)
        {
            flippingPageObject.SetActive(false);
        }

        if (bookAnimator != null)
        {
            bookAnimator.Play("Intro_Close", -1, 0f);
        }

        yield return new WaitForSecondsRealtime(closeAnimationDuration);

        // Fade Out
        if (bookCanvasGroup != null)
        {
            float timer = 0f;
            while (timer < fadeDuration)
            {
                timer += Time.unscaledDeltaTime;
                bookCanvasGroup.alpha = Mathf.Lerp(1f, 0f, timer / fadeDuration);
                yield return null;
            }
            bookCanvasGroup.alpha = 0f;
        }

        if (bookUIPanel != null)
        {
            bookUIPanel.SetActive(false);
        }

        if (wizardGrimoireMode)
            WizardGrimoireUI.Instance?.Hide();

        Time.timeScale = 1f;
        isClosing = false;
        OnBookClosed?.Invoke();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;

            if (bookUIPanel != null && bookUIPanel.activeSelf)
            {
                CloseBook();
            }
        }
    }

    private void OnDisable()
    {
        Time.timeScale = 1f;
    }

    private void EnsureBookCanvasVisible()
    {
        if (bookUIPanel == null)
            return;

        Transform current = bookUIPanel.transform;
        while (current != null)
        {
            if (current.TryGetComponent(out Canvas canvas))
            {
                if (current is RectTransform rect && rect.localScale == Vector3.zero)
                    rect.localScale = Vector3.one;
                break;
            }

            current = current.parent;
        }
    }
}