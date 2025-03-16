using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public abstract partial class ActivityState : NonComponentSubscriber
{
    Queue<Act> acts = new Queue<Act>();
    public Act NextAct
    {
        get
        {
            CheckClean();
            if (acts.Count == 0 || isCancelled)
            {
                onComplete.Invoke();
                return null;
            }
            var act = acts.Peek();
            return act;
        }
    }

    public int RemainingCost
    {
        get
        {
            var sum = 0;
            foreach (Act act in acts) sum += act.Cost;
            return sum;
        }
    }

    public abstract StateProfile Profile { get; }

    Action onComplete;              // called whenever *NextAct* is called and the state is out of acts or has been cancelled
    Action onCancel;                // called when the state is first determined to be impossible to complete (before *onComplete*)
    bool isDirty = true;
    bool isCancelled = false;

    ActivityState(EnvironmentContext context, Action<ActivityState> _onComplete, Action<ActivityState> _onCancel)
    {
        Init(context.grid, context.director, context.pathFinder, null);
        onComplete = () => _onComplete.Invoke(this);
        onCancel = () => { isCancelled = true; _onCancel.Invoke(this); };
        CheckClean();
    }

    void CheckClean()
    {
        if (!isDirty) return;
        if (!TryClean()) onCancel.Invoke();
    }

    protected abstract bool TryClean();

    public Act TryGetNextAct()
    {
        CheckClean();
        if (isCancelled || acts.Count == 0)
        {
            onComplete.Invoke();
            return null;
        }
        return acts.Peek();
    }

    protected void Dirty() => isDirty = true;

    bool TryQueuePathActs
    (
        Vector2Int start, 
        Vector2Int[] ends, 
        EnvironmentContext context, 
        HashSet<Activity<CellContext>> movements, 
        Queue<Act> acts
    )
    {
        // Choose movement activity.
        Activity<CellContext> bestMovement = null;
        var topScore = 0;
        var testCellContext = new CellContext(context, start + Vector2Int.up);
        foreach (var movement in movements)
        {
            var score = movement.GetRange(testCellContext) / movement.GetAPCost(testCellContext);
            if (score > topScore)
            {
                bestMovement = movement;
                topScore = score;
            }
        }

        if (bestMovement == null || !context.pathFinder.TryFindPath(out var path, start, ends.ToArray())) return false;

        foreach (var cell in path)
            acts.Enqueue(new Act<CellContext>(bestMovement, new CellContext(context, cell), () => acts.Dequeue()));

        return true;
    }

    protected HashSet<Activity<CellContext>> GetMovements(GameObject taker)
    {
        var activitiesRequest = new ActivitiesRequestedArgs(taker, ActivityFlags.Movement);
        director.RaiseActivitiesRequested(activitiesRequest);

        var movements = new HashSet<Activity<CellContext>>();
        foreach (var activity in activitiesRequest.Activities)
            movements.Add((Activity<CellContext>)activity);             // assumes that each movement is an *Activity<CellContext>*
        return movements;
    }
}

public struct StateProfile
{
    public readonly int cost;

    public readonly int damage;
    public readonly int healing;
    public readonly int debuff;
    public readonly int buff;

    public StateProfile(int _cost, int _damage = 0, int _healing = 0, int _debuff = 0, int _buff = 0)
    {
        cost = _cost;

        damage = _damage;
        healing = _healing;
        debuff = _debuff;
        buff = _buff;
    }
}
