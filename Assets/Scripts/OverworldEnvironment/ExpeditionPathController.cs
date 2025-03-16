using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class ExpeditionPathController : MonoBehaviour
{
    Vector2Int origin;
    ExpeditionPath path;
    LineRenderer lineRenderer;

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    void Update()
    {
        if (!Input.GetMouseButton(0)) return;
        var mousePosition = new Vector2Int(Mathf.RoundToInt(Input.mousePosition.x), Mathf.RoundToInt(Input.mousePosition.y));
        if (path == null)
        {
            origin = mousePosition;
            path = new ExpeditionPath(mousePosition);
        }
        if (path.Stretch(mousePosition) != StretchResult.None)
        {
            var positions = new List<Vector3>();
            positions.Add(Camera.main.ScreenToWorldPoint(new Vector3(origin.x, origin.y, 5f)));
            foreach (var segment in path)
                positions.Add(Camera.main.ScreenToWorldPoint(new Vector3(segment.Item2.x, segment.Item2.y, 5f)));
            lineRenderer.positionCount = positions.Count;
            lineRenderer.SetPositions(positions.ToArray());
        }
    }
}
