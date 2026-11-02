using UnityEditor;
using UnityEngine;

public static class GothicBookSetup
{
    private const string BookObjectName = "GothicBook_10";

    [MenuItem("Vampyrum/Kitap/GothicBook_10 Kur")]
    private static void SetupGothicBook()
    {
        GameObject book = GameObject.Find(BookObjectName);
        if (book == null)
        {
            EditorUtility.DisplayDialog(
                "GothicBook_10 bulunamadi",
                $"'{BookObjectName}' adli obje acik sahnede yok.\n\nObjeyi secip Inspector'dan GothicBookInteraction ekleyebilirsin.",
                "Tamam");
            return;
        }

        Collider2D collider = book.GetComponent<Collider2D>();
        if (collider == null)
            collider = Undo.AddComponent<BoxCollider2D>(book);

        if (!collider.isTrigger)
        {
            Undo.RecordObject(collider, "Set book collider trigger");
            collider.isTrigger = true;
        }

        BookInteraction[] books = book.GetComponents<BookInteraction>();
        foreach (BookInteraction bookInteraction in books)
            Undo.DestroyObjectImmediate(bookInteraction);

        if (book.GetComponent<DeskInteraction>() == null)
            Undo.AddComponent<DeskInteraction>(book);

        if (book.GetComponent<GothicBookInteraction>() == null)
            Undo.AddComponent<GothicBookInteraction>(book);

        Selection.activeGameObject = book;
        EditorUtility.SetDirty(book);

        Debug.Log("GothicBook_10 hazir: BookInteraction kaldirildi, Kervan Defteri (DeskInteraction) eklendi.");
    }
}
