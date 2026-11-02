using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Hareket Ayarları")]
    public float moveSpeed = 5f;

    private Rigidbody2D rb;
    private Vector2 movement;
    private Animator animator;

    // Sahneler arası hafızada kalacak son basılan yön
    // Oyun ilk açıldığında varsayılan olarak güneye (öne/aşağı) baksın
    public static Vector2 LastMoveDirection = new Vector2(0f, -1f);

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        // 1. Eğer bir kapı/spawn noktası ismi varsa oraya taşı
        if (!string.IsNullOrEmpty(SceneTransition.NextSpawnPointName))
        {
            GameObject spawnPoint = GameObject.Find(SceneTransition.NextSpawnPointName);
            if (spawnPoint != null)
            {
                transform.position = spawnPoint.transform.position;
            }
        }

        // 2. Yeni sahne açıldığı an son basılan yöne (W, S, A, D) bakmasını zorla
        animator.SetFloat("Horizontal", LastMoveDirection.x);
        animator.SetFloat("Vertical", LastMoveDirection.y);
        animator.speed = 0f; // Tuşa basılana kadar yürüyüş animasyonunu durdur

        // Blend Tree'nin ilk karede güncellenmesini garantiye al
        animator.Update(0f);
    }

    void Update()
    {
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        if (movement != Vector2.zero)
        {
            // Oyuncu bir tuşa bastığı an son yönü hafızaya al
            LastMoveDirection = movement;

            animator.SetFloat("Horizontal", movement.x);
            animator.SetFloat("Vertical", movement.y);
            animator.speed = 1f;
        }
        else
        {
            // Tuş bırakıldığında son basılan yönde dursun
            animator.SetFloat("Horizontal", LastMoveDirection.x);
            animator.SetFloat("Vertical", LastMoveDirection.y);
            animator.speed = 0f;
        }

        movement = movement.normalized;
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
    }
}