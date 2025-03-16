using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewActivityStateChooser", menuName = "Entity Toolbox/Activity State Chooser")]
public class ActivityStateChooser : ScriptableObject
{
    [SerializeField, Range(0, 1)] float restlessness = 0;               // tendency to walk when passive
    [SerializeField, Range(0, 1)] float homesickness = 0;               // tendency to stay near a cell when passive

    [SerializeField, Range(0, 1)] float stubbornness = 0;               // resistance to changing activity states
                                                                                                                                                                                                            
    [SerializeField, Range(0, 1)] float aggression = 0;                 // preference for dealing damage to enemies
    [SerializeField, Range(0, 1)] float mischief = 0;                   // preference for debuffing enemies
    [SerializeField, Range(0, 1)] float support = 0;                    // preference for healing allies
    [SerializeField, Range(0, 1)] float leadership = 0;                 // preference for buffing allies

    // returns states which have non-negative priority in ascending order
    public List<ActivityState> ChooseActivityStates(TurnContext turnContext)
    {
        var activities = turnContext.activities;
        var states = new List<ActivityState>();
        foreach(var activity in activities.targetActivities)
        {
            var alignmentRequest = new AlignmentRequestedArgs(turnContext.taker);
            turnContext.director.RaiseAlignmentRequested(alignmentRequest);
            var entitiesRequest = new EntitiesByAlignmentRequestedArgs(alignmentRequest.AlignmentFlags);
            turnContext.director.RaiseEntitiesByAlignmentRequested(entitiesRequest);

            if (Helper.FlagsIncludeAny((byte)activity.ActivityFlags, (byte)(ActivityFlags.Damage | ActivityFlags.Debuff)))
                foreach (var entity in entitiesRequest.Peek(Alignments.Enemy))
                    if (ActivityState.TryBuildFromContext(activity, new TargetContext(turnContext, entity), OnStateCancel, OnStateComplete, out var state))
                        states.Add(state);
            if (Helper.FlagsIncludeAny((byte)activity.ActivityFlags, (byte)(ActivityFlags.Healing | ActivityFlags.Buff)))
                foreach(var entity in entitiesRequest.Peek(Alignments.Ally))
                    if (ActivityState.TryBuildFromContext(activity, new TargetContext(turnContext, entity), OnStateCancel, OnStateComplete, out var state))
                        states.Add(state);
        }

        var priorities = new Dictionary<ActivityState, float>();
        states.OrderBy((s) => GetPriority(s, priorities));
        states.RemoveRange(0, states.NearestBelowBinarySearch(0, (s) => GetPriority(s, priorities)) - 1); 
        
        return states;
    }

    // may rearrange *states* if it is not ordered by priority
    public void CheckStateChanges(TurnContext turnContext, List<ActivityState> states)
    {
        var cachedPriorities = new Dictionary<ActivityState, float>();
        for (var i = 1; i < states.Count; i++)
        {
            var difference = GetPriority(states[i - 1], cachedPriorities) - GetPriority(states[i], cachedPriorities);
            if (difference <= 0) continue;

        }
    }

    float GetPriority(ActivityState state, Dictionary<ActivityState, float> cachedPriorities)
    {
        if (cachedPriorities.TryGetValue(state, out var cachedPriority)) return cachedPriority;
        var priority = GetPriority(state);
        cachedPriorities.Add(state, priority);
        return priority;
    }
    float GetPriority(ActivityState state)
    {
        var profile = state.Profile;
        var priority = (aggression * profile.damage + mischief * profile.debuff + support * profile.healing + leadership * profile.buff) / profile.cost;
        return priority;
    }

    void OnStateCancel(ActivityState state)
    {

    }

    void OnStateComplete(ActivityState state)
    {

    }
    
    public Activity ChooseActivity(TurnContext turnContext, out ActivityContext activityContext) 
    {
        var entitiesAtArgs = new EntitiesAtRequestedArgs(turnContext.grid.boundedCells);
        turnContext.director.RaiseEntitiesAtRequested(entitiesAtArgs);
        GameObject target = entitiesAtArgs.entities.First((x) => x != turnContext.taker);
        var rangeArgs = new RangesRequestedArgs(target);
        turnContext.director.RaiseRangesRequested(rangeArgs);
        var positionArgs = new PositionsRequestedArgs(target, turnContext.taker);
        turnContext.director.RaisePositionsRequested(positionArgs);
        var targetPosition = positionArgs.positions[target];
        var takerPosition = positionArgs.positions[turnContext.taker];

        var cellsWithinRange = turnContext.pathFinder.GetCircle(targetPosition, rangeArgs.ranges[turnContext.taker]);
        if (turnContext.pathFinder.TryFindPath(out var path, takerPosition, cellsWithinRange.ToArray()) && path.Count > 0)
        {
            activityContext = new CellContext(turnContext, path[0] - takerPosition);
            return null;                                      
        }
            turnContext.director.RaiseEntityPushed(new EntityPushedArgs(turnContext.taker, path[0] - takerPosition));

        activityContext = null;
        return null;
    }

    public void Act(TurnContext context)
    {
        var entitiesAtArgs = new EntitiesAtRequestedArgs(context.grid.boundedCells);
        context.director.RaiseEntitiesAtRequested(entitiesAtArgs);
        GameObject target = entitiesAtArgs.entities.First((x) => x != context.taker);
        var rangeArgs = new RangesRequestedArgs(target);
        context.director.RaiseRangesRequested(rangeArgs);
        var positionArgs = new PositionsRequestedArgs(target, context.taker);
        context.director.RaisePositionsRequested(positionArgs);
        var targetPosition = positionArgs.positions[target];
        var takerPosition = positionArgs.positions[context.taker];

        var cellsWithinRange = context.pathFinder.GetCircle(targetPosition, rangeArgs.ranges[context.taker]);
        if (context.pathFinder.TryFindPath(out var path, takerPosition, cellsWithinRange.ToArray()))
            if (path.Count > 0)
                context.director.RaiseEntityPushed(new EntityPushedArgs(context.taker, path[0] - takerPosition));
    }
}
