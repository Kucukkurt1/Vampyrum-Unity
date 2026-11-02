using UnityEngine;

/// <summary>
/// GothicBook_10 (masa ustu kitap) — sadece Kervan Defteri acar.
/// Varsa BookInteraction kaldirilir, DeskInteraction eklenir.
/// </summary>
[RequireComponent(typeof(Collider2D))]
[DisallowMultipleComponent]
public class GothicBookInteraction : MonoBehaviour
{
    private void Awake()
    {
        EnsureTriggerCollider();
        RemoveBookInteraction();
        EnsureDeskInteraction();
        Destroy(this);
    }

    private void EnsureTriggerCollider()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null && !col.isTrigger)
            col.isTrigger = true;
    }

    private void RemoveBookInteraction()
    {
        BookInteraction[] books = GetComponents<BookInteraction>();
        foreach (BookInteraction book in books)
            Destroy(book);
    }

    private void EnsureDeskInteraction()
    {
        if (GetComponent<DeskInteraction>() == null)
            gameObject.AddComponent<DeskInteraction>();
    }
}
