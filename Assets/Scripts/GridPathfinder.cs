using System.Collections.Generic;
using UnityEngine;

public static class GridPathfinder
{
    private struct GridNode
    {
        public int x;
        public int y;
        public float g;
        public float f;
        public int parentX;
        public int parentY;
        public bool hasParent;
    }

    public static List<Vector2> FindPath(
        Vector2 startWorld,
        Vector2 endWorld,
        float cellSize,
        int gridHalfWidth,
        int gridHalfHeight,
        LayerMask obstacleMask)
    {
        if (cellSize <= 0.01f)
            cellSize = 0.5f;

        Vector2Int startCell = WorldToCell(startWorld, cellSize);
        Vector2Int endCell = WorldToCell(endWorld, cellSize);

        if (startCell == endCell)
            return new List<Vector2> { endWorld };

        int width = gridHalfWidth * 2 + 1;
        int height = gridHalfHeight * 2 + 1;
        int originX = startCell.x - gridHalfWidth;
        int originY = startCell.y - gridHalfHeight;

        if (!IsWalkable(endCell.x, endCell.y, originX, originY, cellSize, obstacleMask))
            endCell = FindNearestWalkable(endCell, originX, originY, width, height, cellSize, obstacleMask);

        var open = new List<Vector2Int>();
        var closed = new HashSet<Vector2Int>();
        var nodes = new Dictionary<Vector2Int, GridNode>();

        Vector2Int startKey = new Vector2Int(startCell.x, startCell.y);
        open.Add(startKey);
        nodes[startKey] = new GridNode
        {
            x = startCell.x,
            y = startCell.y,
            g = 0f,
            f = Heuristic(startCell, endCell),
            hasParent = false
        };

        Vector2Int[] directions =
        {
            new Vector2Int(1, 0),
            new Vector2Int(-1, 0),
            new Vector2Int(0, 1),
            new Vector2Int(0, -1)
        };

        while (open.Count > 0)
        {
            open.Sort((a, b) => nodes[a].f.CompareTo(nodes[b].f));
            Vector2Int current = open[0];
            open.RemoveAt(0);
            closed.Add(current);

            if (current.x == endCell.x && current.y == endCell.y)
                return ReconstructPath(nodes, current, cellSize);

            foreach (Vector2Int direction in directions)
            {
                int nx = current.x + direction.x;
                int ny = current.y + direction.y;
                var neighbor = new Vector2Int(nx, ny);

                if (closed.Contains(neighbor))
                    continue;

                if (!IsInsideGrid(nx, ny, originX, originY, width, height))
                    continue;

                if (!IsWalkable(nx, ny, originX, originY, cellSize, obstacleMask))
                    continue;

                float tentativeG = nodes[current].g + 1f;
                if (nodes.TryGetValue(neighbor, out GridNode existing) && tentativeG >= existing.g)
                    continue;

                nodes[neighbor] = new GridNode
                {
                    x = nx,
                    y = ny,
                    g = tentativeG,
                    f = tentativeG + Heuristic(neighbor, endCell),
                    parentX = current.x,
                    parentY = current.y,
                    hasParent = true
                };

                if (!open.Contains(neighbor))
                    open.Add(neighbor);
            }
        }

        return new List<Vector2> { endWorld };
    }

    private static List<Vector2> ReconstructPath(Dictionary<Vector2Int, GridNode> nodes, Vector2Int end, float cellSize)
    {
        var path = new List<Vector2>();
        Vector2Int current = end;

        while (nodes.TryGetValue(current, out GridNode node))
        {
            path.Add(CellToWorld(current.x, current.y, cellSize));
            if (!node.hasParent)
                break;

            current = new Vector2Int(node.parentX, node.parentY);
        }

        path.Reverse();
        return path;
    }

    private static Vector2Int FindNearestWalkable(
        Vector2Int target,
        int originX,
        int originY,
        int width,
        int height,
        float cellSize,
        LayerMask obstacleMask)
    {
        for (int radius = 1; radius <= 4; radius++)
        {
            for (int dx = -radius; dx <= radius; dx++)
            {
                for (int dy = -radius; dy <= radius; dy++)
                {
                    int x = target.x + dx;
                    int y = target.y + dy;
                    if (IsInsideGrid(x, y, originX, originY, width, height) &&
                        IsWalkable(x, y, originX, originY, cellSize, obstacleMask))
                        return new Vector2Int(x, y);
                }
            }
        }

        return target;
    }

    private static bool IsWalkable(
        int x,
        int y,
        int originX,
        int originY,
        float cellSize,
        LayerMask obstacleMask)
    {
        Vector2 world = CellToWorld(x, y, cellSize);
        float radius = cellSize * 0.35f;
        return Physics2D.OverlapCircle(world, radius, obstacleMask) == null;
    }

    private static bool IsInsideGrid(int x, int y, int originX, int originY, int width, int height)
    {
        int localX = x - originX;
        int localY = y - originY;
        return localX >= 0 && localY >= 0 && localX < width && localY < height;
    }

    private static float Heuristic(Vector2Int a, Vector2Int b) =>
        Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);

    private static Vector2Int WorldToCell(Vector2 world, float cellSize) =>
        new Vector2Int(
            Mathf.RoundToInt(world.x / cellSize),
            Mathf.RoundToInt(world.y / cellSize));

    private static Vector2 CellToWorld(int x, int y, float cellSize) =>
        new Vector2(x * cellSize, y * cellSize);
}
