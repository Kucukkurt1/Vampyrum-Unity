using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class NpcWalker : MonoBehaviour
{
    [Header("Hareket")]
    [SerializeField] private float moveSpeed = 2.5f;
    [SerializeField] private float arrivalThreshold = 0.08f;

    [Header("Pathfinding")]
    [SerializeField] private float cellSize = 0.5f;
    [SerializeField] private int gridHalfWidth = 24;
    [SerializeField] private int gridHalfHeight = 24;
    [SerializeField] private LayerMask obstacleMask;

    private readonly List<Vector2> path = new List<Vector2>();
    private int pathIndex;
    private Rigidbody2D rb;
    private Vector2 targetPosition;
    private bool hasTarget;

    public bool IsMoving => hasTarget && pathIndex < path.Count;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
    }

    public void MoveTo(Vector2 worldPosition)
    {
        path.Clear();
        pathIndex = 0;
        path.AddRange(GridPathfinder.FindPath(
            transform.position,
            worldPosition,
            cellSize,
            gridHalfWidth,
            gridHalfHeight,
            obstacleMask));

        if (path.Count == 0)
            path.Add(worldPosition);

        hasTarget = true;
        targetPosition = worldPosition;
    }

    private void FixedUpdate()
    {
        if (!hasTarget || pathIndex >= path.Count)
        {
            rb.velocity = Vector2.zero;
            return;
        }

        Vector2 nextPoint = path[pathIndex];
        Vector2 current = rb.position;
        Vector2 step = nextPoint - current;
        float distance = step.magnitude;

        if (distance <= arrivalThreshold)
        {
            pathIndex++;
            if (pathIndex >= path.Count)
            {
                rb.velocity = Vector2.zero;
                hasTarget = false;
            }

            return;
        }

        rb.velocity = step.normalized * moveSpeed;
    }

    public void Stop()
    {
        hasTarget = false;
        path.Clear();
        pathIndex = 0;
        rb.velocity = Vector2.zero;
    }
}
