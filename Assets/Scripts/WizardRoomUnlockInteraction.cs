using UnityEngine;

/// <summary>
/// Büyücü odası kapısına veya tabelaya ekle. E ile odayı parayla açar.
/// </summary>
public class WizardRoomUnlockInteraction : MonoBehaviour
{
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    private bool isPlayerInRange;

    private void Update()
    {
        if (!isPlayerInRange || !Input.GetKeyDown(interactKey))
            return;

        if (WizardRoomManager.Instance == null)
            return;

        if (WizardRoomManager.Instance.IsUnlocked)
        {
            Debug.Log("Buyucu odasi zaten acik.");
            return;
        }

        if (WizardRoomManager.Instance.TryUnlockRoom())
            Debug.Log($"Buyucu odasi acildi. (-{WizardRoomManager.Instance.UnlockCost} altin)");
        else
            Debug.Log($"Yeterli altin yok. Gerekli: {WizardRoomManager.Instance.UnlockCost}");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            isPlayerInRange = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            isPlayerInRange = false;
    }
}
