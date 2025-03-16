using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class ActivityState
{
    public static bool TryBuildFromContext
    (
        Activity<TargetContext> activity,
        TargetContext context,
        Action<ActivityState> onComplete,
        Action<ActivityState> onCancel,
        out ActivityState state
    )
    {
        var isValid = true;
        onCancel += (_) => isValid = false;
        state = new TargetState(activity, context, onComplete, onCancel);
        return isValid;
    }

    class TargetState : ActivityState
    {
        HashSet<Activity<CellContext>> movements;

        Act<TargetContext> goal;

        public override StateProfile Profile
        {
            get
            {
                var act = acts.Peek();
                return new StateProfile(RemainingCost, act.Damage, act.Healing, act.Debuff, act.Buff);
            }
        }

        public TargetState
        (
            Activity<TargetContext> goalActivity, 
            TargetContext context, 
            Action<ActivityState> _onComplete,
            Action<ActivityState> _onCancel
        ) : base(context, _onComplete, _onCancel)
        {
            goal = new Act<TargetContext>(goalActivity, context, () => acts.Dequeue());
        }

        protected override bool TryClean()
        {
            var activity = goal.activity;
            var context = goal.context;
            var positionRequest = new PositionsRequestedArgs(context.taker, context.target);
            context.director.RaisePositionsRequested(positionRequest);
            var takerPosition = positionRequest.positions[context.taker];
            var targetPosition = positionRequest.positions[context.target];
            var range = activity.GetRange(context);
            var targetCells = context.pathFinder.GetCircle(targetPosition, range);

            var acts = new Queue<Act>();
            if (context.pathFinder.Distance(takerPosition, targetPosition) > range)
                if (!TryQueuePathActs(takerPosition, targetCells.ToArray(), context, movements, acts))
                    return false;
            acts.Enqueue(goal);  
            return true;
        }
    }
}
