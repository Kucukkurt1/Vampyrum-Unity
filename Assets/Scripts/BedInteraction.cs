using UnityEngine;

/// <summary>
/// Yatak trigger objesine ekle. Collider Is Trigger olmali.
/// Oyuncu alana girince E ile gunu bitirir.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class BedInteraction : MonoBehaviour
{
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    [Header("Trigger Alani")]
    [SerializeField] private bool autoFitToBedSprite = true;
    [SerializeField] private string bedObjectName = "GothicBed_10";
    [SerializeField] private Vector2 triggerPadding = new Vector2(0.4f, 0.4f);

    private bool isPlayerInRange;

    private void Awake()
    {
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null && !collider.isTrigger)
            collider.isTrigger = true;

        if (autoFitToBedSprite)
            TryFitTriggerToBed();
    }

    private void Update()
    {
        if (!isPlayerInRange || !Input.GetKeyDown(interactKey))
            return;

        if (Time.timeScale == 0f)
            return;

        GameManager.EnsureInstance();

        if (GameManager.Instance.IsGameOver)
            return;

        GameManager.Instance.EndDay();
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

    private void TryFitTriggerToBed()
    {
        BoxCollider2D trigger = GetComponent<BoxCollider2D>();
        if (trigger == null)
            return;

        GameObject bedObject = GameObject.Find(bedObjectName);
        if (bedObject == null)
            return;

        SpriteRenderer bedSprite = bedObject.GetComponent<SpriteRenderer>();
        Collider2D bedCollider = bedObject.GetComponent<Collider2D>();

        Bounds bedBounds;
        if (bedSprite != null)
            bedBounds = bedSprite.bounds;
        else if (bedCollider != null)
            bedBounds = bedCollider.bounds;
        else
            return;

        bedBounds.Expand(new Vector3(triggerPadding.x * 2f, triggerPadding.y * 2f, 0f));

        Vector3 localMin = transform.InverseTransformPoint(bedBounds.min);
        Vector3 localMax = transform.InverseTransformPoint(bedBounds.max);

        Vector2 localCenter = (localMin + localMax) * 0.5f;
        Vector2 localSize = localMax - localMin;

        trigger.offset = localCenter;
        trigger.size = new Vector2(Mathf.Abs(localSize.x), Mathf.Abs(localSize.y));
    }
}
