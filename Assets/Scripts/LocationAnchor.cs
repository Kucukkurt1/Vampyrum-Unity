using System.Collections.Generic;
using UnityEngine;

public class LocationAnchor : MonoBehaviour
{
    [SerializeField] private string locationId = "yemekhane";

    public string LocationId => locationId;
    public Vector2 Position => transform.position;

    private void OnEnable() => LocationAnchorRegistry.Register(this);

    private void OnDisable() => LocationAnchorRegistry.Unregister(this);
}

public static class LocationAnchorRegistry
{
    private static readonly Dictionary<string, LocationAnchor> anchors =
        new Dictionary<string, LocationAnchor>();

    public static void Register(LocationAnchor anchor)
    {
        if (anchor == null || string.IsNullOrEmpty(anchor.LocationId))
            return;

        anchors[anchor.LocationId] = anchor;
    }

    public static void Unregister(LocationAnchor anchor)
    {
        if (anchor == null)
            return;

        if (anchors.TryGetValue(anchor.LocationId, out LocationAnchor current) && current == anchor)
            anchors.Remove(anchor.LocationId);
    }

    public static LocationAnchor Find(string id)
    {
        anchors.TryGetValue(id, out LocationAnchor anchor);
        return anchor;
    }

    public static Vector2? GetPosition(string id)
    {
        LocationAnchor anchor = Find(id);
        return anchor != null ? anchor.Position : (Vector2?)null;
    }
}
