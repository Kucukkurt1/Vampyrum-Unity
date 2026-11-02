using UnityEngine;

/// <summary>
/// Oyuncu odasindaki masa/defter objesine ekle. Trigger collider gerekir (Is Trigger = true).
/// Oyuncu alana girince E ile Kervan Defteri acilir.
/// </summary>
public class DeskInteraction : MonoBehaviour
{
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    private bool isPlayerInRange;

    private void Update()
    {
        if (!isPlayerInRange || !Input.GetKeyDown(interactKey))
            return;

        if (Time.timeScale == 0f)
        {
            if (CaravansaryManagementUI.Instance != null && CaravansaryManagementUI.Instance.IsVisible)
                return;

            if (MorningReportUI.IsBlockingInput)
                return;

            BookInteraction book = FindObjectOfType<BookInteraction>();
            if (book != null && book.IsBookOpen)
                return;
        }

        BookInteraction openBook = FindObjectOfType<BookInteraction>();
        if (openBook != null && openBook.IsBookOpen)
            return;

        GameManager.EnsureInstance();

        if (GameManager.Instance.IsGameOver || GameOverUI.IsBlockingInteraction)
            return;

        CaravansaryManagementUI.Instance?.Toggle();
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
            CaravansaryManagementUI.Instance?.Hide();
        }
    }
}
