using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Exhaustible : Stat
{
    protected override int BaseValue            // represents the base cap of *current*, e.g. max hp before conditions apply
    {
        get => base.BaseValue;
        set
        {
            base.BaseValue = value;
            Current = Current;
        }
    }

    [SerializeField] int currentDisplay; 
    int m_current;
    public int Current
    {
        get => m_current;
        set
        {
            int updatedValue = Mathf.Min(value, Total);
            if (m_current > 0 && updatedValue <= 0)
                OnExhausted(EventArgs.Empty);
            else if (m_current <= 0 && updatedValue > 0)
                OnRestored(EventArgs.Empty);
            m_current = updatedValue;
            currentDisplay = updatedValue;
        }
    }

    public event EventHandler RaiseExhausted;                   // invoked when *current* goes from positive to nonpositive
    public event EventHandler RaiseRestored;                    // invoked when *current* goes from nonpositive to positive

    public Exhaustible(int _baseCap, int _current) : base(_baseCap, 0, 128)
    {
        Current = _current;
    }

    protected virtual void OnExhausted(EventArgs e) { RaiseExhausted?.Invoke(this, e); }
    protected virtual void OnRestored(EventArgs e) { RaiseRestored?.Invoke(this, e); }

    public static implicit operator int(Exhaustible e) => e.Current;
}
