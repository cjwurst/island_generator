using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExpeditionPath : IEnumerable<(Vector2Int, Vector2Int)>
{
    List<Vector2Int> nodes;
    Vector2Int Terminus
    {
        get
        {
            if (nodes.Count == 0) return Vector2Int.zero;
            return nodes[nodes.Count - 1];
        }
        set => nodes.Add(value);
    }

    const float minStretchDistance = 30f;            // in pixels
    const float unstretchThreshold = 120f;           // in degrees

    public ExpeditionPath(Vector2Int _origin)
    {
        nodes = new List<Vector2Int>();
        nodes.Add(_origin);
    }

    // if there is more than one result, returns the most recent
    public StretchResult Stretch (Vector2Int target)
    {
        var result = StretchResult.None;
        while(GetDistanceTo(target, out var normedDifference) > minStretchDistance)
        {
            if (nodes.Count < 2)
            {
                Terminus = (Terminus + minStretchDistance * normedDifference).RoundToVector2Int();
                return result;
            }

            var lastStretch = (Vector2)Terminus - nodes[nodes.Count - 2];
            if (lastStretch == Vector2.zero) lastStretch = Vector2.right;
            if (Mathf.Abs(Vector2.Angle(normedDifference, -lastStretch.normalized)) > unstretchThreshold)
            {
                Terminus = (Terminus + minStretchDistance * normedDifference).RoundToVector2Int();
                result = StretchResult.Stretched;
            }
            else
            {
                nodes.RemoveAt(nodes.Count - 1);
                result = StretchResult.Unstretched;
            }
        }
        return result;

        float GetDistanceTo(Vector2Int vector, out Vector2 normedDifference)
        {
            var difference = (Vector2)vector - Terminus;
            normedDifference = difference.normalized;
            return difference.magnitude;
        }
    }

    public IEnumerator<(Vector2Int, Vector2Int)> GetEnumerator()
    {
        var segments = new (Vector2Int, Vector2Int)[nodes.Count - 1];
        for (var i = 1; i < nodes.Count; i++)
            segments[i - 1] = (nodes[i - 1], nodes[i]);
        return (new List<(Vector2Int, Vector2Int)>(segments)).GetEnumerator();
    }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

public enum StretchResult
{
    Stretched,
    Unstretched,
    None
}
