using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Stat
{
    protected readonly int min;
    protected readonly int max;

    int m_baseValue;
    protected virtual int BaseValue
    {
        get => m_baseValue;
        set => m_baseValue = Helper.Bound(value, min, max);
    }
    List<Func<int, int>> modifiers = new List<Func<int, int>>();
    public int Total
    {
        get
        {
            int _total = BaseValue;
            foreach (Func<int, int> modifier in modifiers)
                _total = modifier.Invoke(Total);
            totalDisplay = _total;
            return _total;
        }
    }
    [SerializeField] int totalDisplay;

    public Stat(int _baseValue, int _min = 0, int _max = 10)
    {
        min = Mathf.Max(0, _min);
        max = Mathf.Max(_min, _max);
        BaseValue = _baseValue;
    }

    // returns an action to remove the modifier
    public Action AddModifier(Func<int, int> modifier, bool delay = false)
    {
        if (delay) modifiers.Add(modifier);
        else modifiers.Insert(0, modifier);
        return () => modifiers.Remove(modifier);
    }
}
