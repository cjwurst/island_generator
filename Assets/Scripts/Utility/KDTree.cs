using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class KDTree<T>
{
    float position;
    KDTree<T> leftChild;
    KDTree<T> rightChild;

    Func<T, float[]> getMagnitudes;
    readonly int k;
    readonly int depth;
    readonly int axis;

    Leaf leaf;

    public KDTree(IEnumerable<T> points, Func<T, float[]> _getMagnitudes, int _k)
    {
        getMagnitudes = _getMagnitudes;
        k = _k;
        depth = 0;
        SplitBranch(points.ToList(), 0);
    }
    KDTree(List<T> points, Func<T, float[]> _getMagnitudes, int _k, int _depth)
    {
        getMagnitudes = _getMagnitudes;
        k = _k;
        depth = _depth;
        axis = depth % k;
        SplitBranch(points, depth);
    }
    void SplitBranch(List<T> points, int depth)
    {
        if (points.Count == 1)
        {
            leaf.Set(points[0]);
            position = getMagnitudes.Invoke(points[0])[axis];
            return;
        }

        points = points.OrderBy((t) => getMagnitudes.Invoke(t)[axis]).ToList();
        var medianIndex = Mathf.FloorToInt((points.Count - 1) / 2f);

        position = getMagnitudes.Invoke(points[medianIndex])[axis];
        leftChild = new KDTree<T>(points.GetRange(0, medianIndex + 1), getMagnitudes, k, depth + 1);
        rightChild = new KDTree<T>(points.GetRange(medianIndex + 1, points.Count - medianIndex - 1), getMagnitudes, k, depth + 1);
    }

    public T GetNearestNeighbor(T element, out float distance)
    {
        if (leaf.TryGet(out var value))
        {
            distance = GetDistance(element, value);
            return value;
        }

        T nearestNeighbor = default;
        var leastDistance = float.PositiveInfinity; 
        if (getMagnitudes.Invoke(element)[axis] > position)
        {
            ConsiderLeaf(rightChild.GetNearestNeighbor(element, out var d), d);
            ConsiderBranch(leftChild);
        }
        else
        {
            ConsiderLeaf(leftChild.GetNearestNeighbor(element, out var d), d);
            ConsiderBranch(rightChild);
        }
        distance = leastDistance;
        return nearestNeighbor;

        void ConsiderBranch(KDTree<T> candidate)
        {
            if (GetDistance(element, this) < leastDistance)
                ConsiderLeaf(candidate.GetNearestNeighbor(element, out var d), d);
        }
        void ConsiderLeaf(T candidate, float candidateDistance)
        {
            if (candidateDistance < leastDistance)
            {
                nearestNeighbor = candidate;
                leastDistance = candidateDistance;
            }
        }
    }

    float GetDistance(T a, T b)
    {
        var magnitudesA = getMagnitudes.Invoke(a);
        var magnitudesB = getMagnitudes.Invoke(b);
        var distanceSquared = 0f;
        for (var i = 0; i < magnitudesA.Length; i++)
        {
            var difference = magnitudesA[i] - magnitudesB[i];
            distanceSquared += difference * difference;
        }
        return Mathf.Sqrt(distanceSquared);
    }
    float GetDistance(T a, KDTree<T> branch)
    {
        var magnitudes = getMagnitudes.Invoke(a);
        var absoluteDifference = Mathf.Abs(magnitudes[branch.axis] - branch.position);
        return absoluteDifference;
    }

    public override string ToString()
    {
        if (leaf.TryGet(out var value)) return value.ToString();
        else return $"{ position } => ({ leftChild }, { rightChild })";
    }

    struct Leaf
    {
        bool hasValue;
        T value;

        public bool TryGet(out T _value)
        {
            _value = value;
            return hasValue;
        }
        public void Set(T _value)
        {
            hasValue = true;
            value = _value;
        }
    }
}
