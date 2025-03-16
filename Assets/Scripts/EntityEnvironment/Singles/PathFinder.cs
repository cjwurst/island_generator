using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class PathFinder
{
    readonly Dictionary<Vector2Int, Node> nodes = new Dictionary<Vector2Int, Node>();
    readonly Dictionary<Node, Node[]> nearestNodeArrays = new Dictionary<Node, Node[]>();

    public PathFinder(Vector2Int[] boundedCells, CallbackDirector director)
    {
        foreach (var cell in boundedCells)
            nodes.Add(cell, new Node(cell, director));
        foreach (var node in nodes.Values)
        {
            var nearestNodeArray = new Node[nodes.Values.Count];
            nodes.Values.CopyTo(nearestNodeArray, 0);
            Array.Sort(nearestNodeArray, (a, b) => Distance(a, node).CompareTo(Distance(b, node)));
            nearestNodeArrays.Add(node, nearestNodeArray);
        }
    }

    public bool TryFindPath(out List<Vector2Int> path, Vector2Int start, params Vector2Int[] ends)
    {
        ends = ValidateCells(ends).ToArray();
        Assert.IsTrue(ends.Length > 0);

        path = new List<Vector2Int>();
        if (!nodes.TryGetValue(start, out var startNode)) return false;
        var endNodes = new Node[ends.Length];
        for (var i = 0; i < ends.Length; i++)
        {
            if (!nodes.TryGetValue(ends[i], out var endNode)) return false;
            endNodes[i] = endNode;
        }

        var distance = Distance(startNode, endNodes);
        foreach (var node in nodes.Values) node.Init(distance);
        startNode.gScore = 0;

        var openSet = new List<Node>(new Node[] { startNode });
        while (openSet.Count > 0)
        {
            Node leastNode = openSet[0];
            foreach (Node node in openSet)
                if (node.fScore < leastNode.fScore)
                    leastNode = node;

            if (endNodes.Contains(leastNode))
            {
                Node current = leastNode;
                while (current.parent != null)
                {
                    path.Insert(0, current.position);
                    current = current.parent;
                }
                return true;
            }

            openSet.Remove(leastNode);
            foreach (var neighbor in GetNeighbors(leastNode))
            {
                float gScoreCandidate = leastNode.gScore + Distance(leastNode, neighbor);
                if (gScoreCandidate < neighbor.gScore)
                {
                    neighbor.parent = leastNode;
                    neighbor.gScore = gScoreCandidate;
                    if (!openSet.Contains(neighbor)) openSet.Add(neighbor);
                }
            }
        }
        return false;
    }

    public bool IsObstructedAt(Vector2Int cell) { return nodes[cell].IsObstructed; }

    HashSet<Node> GetNeighbors(Node node)
    {
        var neighbors = new HashSet<Node>();
        for (int i = -1; i <= 1; i++)
            for (int j = -1; j <= 1; j++)
                if ((i != 0 || j != 0) && nodes.TryGetValue(node.position + new Vector2Int(i, j), out var neighbor) && !neighbor.IsObstructed)
                    neighbors.Add(neighbor);
        return neighbors;
    }

    // returns positions within *radius* (inclusive) of *center*
    public HashSet<Vector2Int> GetCircle(Vector2Int center, float radius)
    {
        var circle = new HashSet<Vector2Int>();
        if (!nodes.TryGetValue(center, out var centerNode)) return circle;
        Assert.IsTrue(nearestNodeArrays.TryGetValue(centerNode, out var nearestNodes));

        for (var i = 0; (i < nearestNodes.Length) && (Distance(centerNode, nearestNodes[i]) <= radius); i++)
            circle.Add(nearestNodes[i].position);
        return circle;
    }

    public List<Vector2Int> GetStraightPathBetween(Vector2Int start, Vector2Int end)
    {
        int xDifference = start.x - end.x;
        int yDifference = start.y - end.y;
        int xDistance = Mathf.Abs(xDifference);
        int yDistance = Mathf.Abs(yDifference);
        int diagonalCellCount;
        int straightCellCount;
        Vector2Int straightStep;
        int straightStepCount;
        Vector2Int diagonalStep;
        if (xDistance > yDistance)
        {
            diagonalCellCount = yDistance;
            straightCellCount = xDistance - diagonalCellCount;
            straightStep = Math.Sign(xDifference) * Vector2Int.right;
            straightStepCount = Mathf.CeilToInt(straightCellCount / (diagonalCellCount + 1f));
            diagonalStep = straightStep + Math.Sign(yDifference) * Vector2Int.up;
        }
        else
        {
            diagonalCellCount = xDistance;
            straightCellCount = yDistance - diagonalCellCount;
            straightStep = Math.Sign(yDifference) * Vector2Int.up;
            straightStepCount = Mathf.CeilToInt(straightCellCount / (diagonalCellCount + 1f));
            diagonalStep = straightStep + Math.Sign(xDifference) * Vector2Int.right;
        }

        var steps = new Vector2Int[straightStepCount + 1];
        for (int i = 0; i < straightStepCount; i++)
            steps[i] = straightStep;
        steps[steps.Length - 1] = diagonalStep;

        var turtle = start;
        var cells = new Vector2Int[diagonalCellCount + straightCellCount + 1];
        cells[0] = start;
        for (int i = 1; i < cells.Length; i++)
        {
            turtle += steps[i % steps.Length];
            cells[i] = turtle;
        }
        return ValidateCells(cells);
    }

    float Distance(Node start, Node[] ends)
    {
        Assert.IsTrue(ends.Length > 0);

        Node end;
        if (ends.Length == 1) end = ends[0];
        else end = nearestNodeArrays[start].First(x => ends.Contains(x));

        return Distance(start, end);
    }
    float Distance(Node start, Node end)
    {
        return Distance(start.position, end.position);
    }
    public float Distance(Vector2Int start, Vector2Int end)
    {
        int xDistance = Mathf.Abs(start.x - end.x);
        int yDistance = Mathf.Abs(start.y - end.y);
        (int diagonalCellCount, int straightCellCount) = xDistance < yDistance ? (xDistance, yDistance) : (yDistance, xDistance);
        straightCellCount -= diagonalCellCount;
        return 3f * diagonalCellCount + 2f * straightCellCount;
    }

    // returns all passed cells that have a corresponding node
    List<Vector2Int> ValidateCells(params Vector2Int[] cells)
    {
        var validCells = new List<Vector2Int>();
        foreach (var cell in cells)
            if (nodes.TryGetValue(cell, out _))
                validCells.Add(cell);
        if (validCells.Count == 0)
        {
            ;
        }
        return validCells;
    }

    class Node
    {
        CallbackDirector director;

        public readonly Vector2Int position;

        public float fScore { get { return gScore + hScore; } }
        public float gScore;
        float hScore;
        public bool IsObstructed
        {
            get
            {
                var args = new EntitiesAtRequestedArgs(position);
                director.RaiseEntitiesAtRequested(args);
                return args.IncludesObstruction;
            }
        }

        public Node parent;

        public Node (Vector2Int _position, CallbackDirector _director)
        {
            director = _director;
            position = _position;
        }

        public void Init(float distance)
        {
            gScore = float.PositiveInfinity;
            hScore = distance;

            parent = null;
        }
    }
}
