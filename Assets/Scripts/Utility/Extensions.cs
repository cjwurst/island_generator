using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AnimationCurveExtensions
{
    public static float[] Sample(this AnimationCurve curve, int sampleCount)
    {
        var increment = 1f / (sampleCount - 1f);
        var samples = new float[sampleCount];
        var currentInput = 0f;
        samples[0] = curve.Evaluate(0f);
        for (var i = 1; i < sampleCount; i++)
            samples[i] = curve.Evaluate(currentInput += increment);
        return samples;
    }
}

public static class Vector2Extensions
{
    public static Vector2Int RoundToVector2Int(this Vector2 vector2)
    {
        int x = Mathf.RoundToInt(vector2.x);
        int y = Mathf.RoundToInt(vector2.y);
        return new Vector2Int(x, y);
    }

    public static Vector2Int FloorToVector2Int(this Vector2 vector2)
    {
        int x = Mathf.FloorToInt(vector2.x);
        int y = Mathf.FloorToInt(vector2.y);
        return new Vector2Int(x, y);
    }

    public static Vector2Int CeilToVector2Int(this Vector2 vector2)
    {
        int x = Mathf.CeilToInt(vector2.x);
        int y = Mathf.CeilToInt(vector2.y);
        return new Vector2Int(x, y);
    }
}

public static class ArrayExtensions
{
    public static T[] Fill<T>(this T[] array, T element)
    {
        for (int i = 0; i < array.Length; i++)
            array[i] = element;
        return array;
    }
}

public static class IntExtensions
{
    public static int Bound(this int n, int min, int max)
    {
        n = Mathf.Max(n, min);
        n = Mathf.Min(n, max);
        return n;
    }
}

public static class FloatExtensions
{
    public static float Bound(this float n, float min, float max)
    {
        n = Mathf.Max(n, min);
        n = Mathf.Min(n, max);
        return n;
    }
}

public static class GenericListExtensions
{
    public static T Pop<T>(this List<T> list)
    {
        var element = list.PeekWithIndex(out var i);
        if (i >= 0) list.RemoveAt(i);
        return element;
    }
    public static T Peek<T>(this List<T> list)
    {
        return list.PeekWithIndex(out _);
    }
    static T PeekWithIndex<T>(this List<T> list, out int i)
    {
        i = list.Count - 1;
        if (i == -1) return default;
        return list[i];
    }

    public static int NearestAboveBinarySearch<TValue>(this List<TValue> list, float value, Func<TValue, float> evaluate)
    {
        var currentBranch = new IndexTree(list.Count, false);
        IndexTree largerChild;
        while ((largerChild = currentBranch.LargerChild) != null)
        {
            var centerValue = evaluate.Invoke(list[currentBranch.Index]);
            if (centerValue > value)
                currentBranch = currentBranch.SmallerChild;
            else if (centerValue == value)
                return currentBranch.Index;
            else
                currentBranch = largerChild;
        }
        return currentBranch.Index;
    }
    public static int NearestBelowBinarySearch<TValue>(this List<TValue> list, float value, Func<TValue, float> evaluate)
    {
        var currentBranch = new IndexTree(list.Count, true);
        IndexTree largerChild;
        while ((largerChild = currentBranch.LargerChild) != null)
        {
            var centerValue = evaluate.Invoke(list[currentBranch.Index]);
            if (centerValue > value)
                currentBranch = currentBranch.SmallerChild;
            else if (centerValue == value)
                return currentBranch.Index;
            else
                currentBranch = largerChild;
        }
        return currentBranch.Index;
    }

    class IndexTree
    {
        int lowerBound;             // inclusive
        int upperBound;             // ""
        int center;
        public int Index { get => center; }

        bool roundsUp;

        public IndexTree LargerChild
        {
            get
            {
                if (center == upperBound && center == lowerBound) return null;
                return new IndexTree(center, upperBound, roundsUp);
            }
        }
        public IndexTree SmallerChild
        {
            get
            {
                if (center == upperBound && center == lowerBound) return null;
                return new IndexTree(lowerBound, center, roundsUp);
            }
        }

        public IndexTree(int length, bool _roundsUp) => Init(0, length - 1, _roundsUp);
        IndexTree(int _lowerBound, int _upperBound, bool _roundsUp) => Init(_lowerBound, _upperBound, _roundsUp);
        void Init(int _lowerBound, int _upperBound, bool _roundsUp)
        {
            lowerBound = _lowerBound;
            upperBound = _upperBound;

            if (roundsUp) center = Mathf.CeilToInt(upperBound + lowerBound / 2f);
            else center = Mathf.FloorToInt(upperBound + lowerBound / 2f);
        }
    }
}
