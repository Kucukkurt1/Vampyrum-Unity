using UnityEngine;

public class TableSortTrigger : MonoBehaviour
{
    // Masanýn Sprite Renderer'ýný buraya sürükle
    public SpriteRenderer tableRenderer;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            SpriteRenderer playerSr = collision.GetComponent<SpriteRenderer>();
            if (playerSr != null)
            {
                // Karakter masanýn önündeki alana girince katmaný masadan büyük olsun
                playerSr.sortingOrder = tableRenderer.sortingOrder + 1;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            SpriteRenderer playerSr = collision.GetComponent<SpriteRenderer>();
            if (playerSr != null)
            {
                // Karakter masanýn arkasýna geçince masanýn katmanýndan küçük olsun
                playerSr.sortingOrder = tableRenderer.sortingOrder - 1;
            }
        }
    }
}