using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellGrid
{
    public readonly float cellSize;
    public readonly Vector2 origin;

    public readonly (Vector2Int lowerBounds, Vector2Int upperBounds) bounds;                // inclusive
    public Vector2Int[] boundedCells { get; private set; }

    public CellGrid (float _cellSize, Vector2 _origin, Vector2Int lowerBounds, Vector2Int upperBounds)
    {
        cellSize = _cellSize;
        origin = _origin;

        bounds.lowerBounds = lowerBounds;
        bounds.upperBounds = upperBounds;
        int width = upperBounds.x - lowerBounds.x + 1;
        int height = upperBounds.y - lowerBounds.y + 1;
        boundedCells = new Vector2Int[width * height];
        for (int i = 0; i < width; i++)
            for (int j = 0; j < height; j++)
                boundedCells[(height * i) + j] = new Vector2Int(lowerBounds.x + i, lowerBounds.y + j);
    }

    public Vector2Int WorldToCell(Vector2 worldVector)
    {
        Vector2 cellVector = worldVector - origin;
        cellVector /= cellSize;
        return cellVector.RoundToVector2Int();
    }

    public Vector2 CellToWorld(Vector2Int cellVector)
    {
        Vector2 worldVector = new Vector2(cellVector.x, cellVector.y) + origin;
        worldVector *= cellSize;
        return worldVector;
    }

    public Vector2Int ScreenToCell(Vector3 screenVector, Camera camera)
    {
        Vector3 worldVector = camera.ScreenToWorldPoint(screenVector);
        return WorldToCell(worldVector);
    }
    public Vector2Int ScreenToCell(Vector3 screenVector) { return ScreenToCell(screenVector, Camera.main); }
}
