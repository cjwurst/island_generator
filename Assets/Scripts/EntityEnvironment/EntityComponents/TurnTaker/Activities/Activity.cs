using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public abstract class Activity : ScriptableObject
{
    [SerializeField] protected int baseAPCost;
    [SerializeField] protected int baseRange;

    [SerializeField] ActivityFlags activityFlags;
    public ActivityFlags ActivityFlags { get => activityFlags; }
}

[Flags]
public enum ActivityFlags
{
    None        = 0b_0000_0000,
    Damage      = 0b_0000_0001,
    Healing     = 0b_0000_0010,
    Debuff      = 0b_0000_0100,
    Buff        = 0b_0000_1000,
    Movement    = 0b_0001_0000
}

public abstract class Activity<T> : Activity where T : ActivityContext
{
    public void Activate(T context)
    {
        Activate(context);
        context.director.RaiseEntityActed(new EntityActedArgs(context.taker, GetAPCost(context)));
    }

    public virtual int GetRange(T context)
    {
        return baseRange;
    }

    public virtual int GetAPCost(T context)
    {
        return baseAPCost;
    }

    public virtual int GetExpectedDamage(T context) => 0;
    public virtual int GetExpectedHealing(T context) => 0;
    public virtual int GetExpectedBuff(T context) => 0;
    public virtual int GetExpectedDebuff(T context) => 0;

    protected abstract void OnActivate(T context);
}

public abstract class Act
{
    public virtual int Cost { get => 0; }

    public virtual int Damage { get => 0; }
    public virtual int Healing { get => 0; }
    public virtual int Debuff { get => 0; }
    public virtual int Buff { get => 0; }

    public abstract void Do();
}

public class Act<T> : Act where T : ActivityContext
{
    public readonly Activity<T> activity;
    public readonly T context;

    readonly Action callback;
    bool isValid = true;

    public override int Cost { get => activity.GetAPCost(context); }

    public override int Damage { get => activity.GetExpectedDamage(context); }
    public override int Healing { get => activity.GetExpectedHealing(context); }
    public override int Debuff { get => activity.GetExpectedDebuff(context); }
    public override int Buff { get => activity.GetExpectedBuff(context); }

    public Act(Activity<T> _activity, T _context, Action _callback)
    {
        activity = _activity;
        context = _context;

        callback = _callback;
    }

    public override void Do()
    {
        Assert.IsTrue(isValid);
        callback.Invoke();
        activity.Activate(context);
        isValid = false;
    }
}
