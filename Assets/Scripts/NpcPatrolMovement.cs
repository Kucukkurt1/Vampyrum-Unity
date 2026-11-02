using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class NpcPatrolMovement : MonoBehaviour
{
    private const string DefaultControllerResourcePath = "Vampir/Vampir";

    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private RuntimeAnimatorController animatorController;
    [SerializeField] private Transform[] patrolPoints;

    private Rigidbody2D rb;
    private Animator animator;
    private int targetIndex;
    private Vector2 lastDirection = new Vector2(0f, -1f);

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        if (patrolPoints != null && patrolPoints.Length > 0 && patrolPoints[0] != null)
            transform.position = patrolPoints[0].position;

        EnsureAnimatorController();
    }

    private void Start()
    {
        if (GetComponent<NpcTalkInteraction>() == null)
            gameObject.AddComponent<NpcTalkInteraction>();

        SetAnimatorDirection(lastDirection);
        if (animator != null && animator.runtimeAnimatorController != null)
            animator.Update(0f);
    }

    private void EnsureAnimatorController()
    {
        if (animator == null)
            return;

        RuntimeAnimatorController controller = animatorController;
        if (controller == null)
            controller = Resources.Load<RuntimeAnimatorController>(DefaultControllerResourcePath);

        if (controller != null)
            animator.runtimeAnimatorController = controller;
        else
            Debug.LogWarning($"{name}: Vampir AnimatorController bulunamadi.", this);
    }

    private void FixedUpdate()
    {
        if (patrolPoints == null || patrolPoints.Length == 0 ||
            patrolPoints[targetIndex] == null)
        {
            rb.velocity = Vector2.zero;
            SetAnimatorDirection(Vector2.zero);
            return;
        }

        Vector2 target = patrolPoints[targetIndex].position;
        Vector2 toTarget = target - rb.position;

        if (toTarget.magnitude <= 0.08f)
        {
            targetIndex = (targetIndex + 1) % patrolPoints.Length;
            rb.velocity = Vector2.zero;
            SetAnimatorDirection(Vector2.zero);
            return;
        }

        Vector2 direction = toTarget.normalized;
        rb.velocity = direction * moveSpeed;
        SetAnimatorDirection(direction);
    }

    private void SetAnimatorDirection(Vector2 direction)
    {
        if (animator == null || animator.runtimeAnimatorController == null)
            return;

        if (direction != Vector2.zero)
            lastDirection = direction;

        animator.SetFloat("Horizontal", lastDirection.x);
        animator.SetFloat("Vertical", lastDirection.y);
        animator.speed = direction == Vector2.zero ? 0f : 1f;
    }

    private void OnDrawGizmosSelected()
    {
        if (patrolPoints == null || patrolPoints.Length == 0)
            return;

        Gizmos.color = Color.cyan;
        for (int i = 0; i < patrolPoints.Length; i++)
        {
            Transform current = patrolPoints[i];
            Transform next = patrolPoints[(i + 1) % patrolPoints.Length];
            if (current == null)
                continue;

            Gizmos.DrawWireSphere(current.position, 0.2f);
            if (next != null)
                Gizmos.DrawLine(current.position, next.position);
        }
    }
}
