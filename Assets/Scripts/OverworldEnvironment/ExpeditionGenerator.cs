using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ExpeditionGenerator
{
    const float sampleLength = 0.5f;            // in pixels

    public static List<Vector2Int> PathToPixels(ExpeditionPath path, out float totalLength)
    {
        var pixels = new List<Vector2Int>();
        totalLength = 0f;
        foreach(var segment in path)
        {
            var segmentLength = (segment.Item2 - segment.Item1).magnitude;
            float deltaX = segment.Item2.x - segment.Item1.x;
            float deltaY = segment.Item2.y - segment.Item1.y;
            int sampleCount = Mathf.CeilToInt(segmentLength / sampleLength);
            float xIncrement = deltaX / sampleCount;
            float yIncrement = deltaY / sampleCount;
            float lengthIncrement = Mathf.Sqrt(xIncrement * xIncrement + yIncrement * yIncrement);

            for (var i = 0; i < sampleCount; i++)
                pixels.Add((segment.Item1 + new Vector2(i * xIncrement, i * yIncrement)).RoundToVector2Int());

            totalLength += segmentLength;
        }
        return pixels;
    }

    public static Expedition PixelsToExpedition(List<Vector2Int> pixels)
    {
        return new Expedition(new Vector2Int(7, pixels.Count));
    }
}

public class Expedition
{
    public readonly Vector2Int dimensions;

    public Expedition(Vector2Int _dimensions)
    {
        dimensions = _dimensions;
    }
}
