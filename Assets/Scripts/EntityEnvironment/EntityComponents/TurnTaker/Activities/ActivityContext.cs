using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ActivityContext : EnvironmentContext
{
    public ActivityContext(GameObject taker, CellGrid grid, PathFinder pathFinder, CallbackDirector director)
        : base(taker, grid, pathFinder, director) { }
    public ActivityContext(EnvironmentContext environContext)
        : base(environContext.taker, environContext.grid, environContext.pathFinder, environContext.director) { }
}

public class TargetContext : ActivityContext
{
    public readonly GameObject target;

    public TargetContext(GameObject taker, CellGrid grid, PathFinder pathFinder, CallbackDirector director, GameObject _target)
        : base(taker, grid, pathFinder, director)
    {
        target = _target;
    }
    public TargetContext(EnvironmentContext environContext, GameObject _target) : base(environContext)
    {
        target = _target;
    }
}

public class MultiTargetContext : ActivityContext
{
    public readonly GameObject[] targets;

    public MultiTargetContext(GameObject taker, CellGrid grid, PathFinder pathFinder, CallbackDirector director, params GameObject[] _targets)
        : base(taker, grid, pathFinder, director)
    {
        targets = _targets;
    }
    public MultiTargetContext(EnvironmentContext environContext, params GameObject[] _targets) : base(environContext)
    {
        targets = _targets;
    }
}

public class CellContext : ActivityContext
{
    public readonly Vector2Int targetCell;

    public CellContext(GameObject taker, CellGrid grid, PathFinder pathFinder, CallbackDirector director, Vector2Int _targetCell)
        : base(taker, grid, pathFinder, director)
    {
        targetCell = _targetCell;
    }
    public CellContext(EnvironmentContext environContext, Vector2Int _targetCell) : base(environContext)
    {
        targetCell = _targetCell;
    }
}

public class MultiCellContext : ActivityContext
{
    public readonly Vector2Int[] targetCells;

    public MultiCellContext(GameObject taker, CellGrid grid, PathFinder pathFinder, CallbackDirector director, params Vector2Int[] _targetCells)
        : base(taker, grid, pathFinder, director)
    {
        targetCells = _targetCells;
    }
    public MultiCellContext(EnvironmentContext environContext, params Vector2Int[] _targetCells) : base(environContext)
    {
        targetCells = _targetCells;
    }
}

public class ActivitySet
{
    public readonly HashSet<Activity<TargetContext>> targetActivities = new HashSet<Activity<TargetContext>>();
    public readonly HashSet<Activity<MultiTargetContext>> multiTargetActivities = new HashSet<Activity<MultiTargetContext>>();
    public readonly HashSet<Activity<CellContext>> cellActivities = new HashSet<Activity<CellContext>>();
    public readonly HashSet<Activity<MultiCellContext>> multiCellActivities = new HashSet<Activity<MultiCellContext>>();

    public readonly HashSet<Activity<CellContext>> moveActivities = new HashSet<Activity<CellContext>>();                       // subset of *cellActivities*

    public ActivitySet(params Activity[] activities)
    {
        foreach (var activity in activities)
        {
            if (activity is Activity<TargetContext>) targetActivities.Add((Activity<TargetContext>)activity);
            else if (activity is Activity<MultiTargetContext>) multiTargetActivities.Add((Activity<MultiTargetContext>)activity);
            else if (activity is Activity<CellContext>)
            {
                cellActivities.Add((Activity<CellContext>)activity);
                if (Helper.FlagsIncludeAny((byte)activity.ActivityFlags, (byte)ActivityFlags.Movement))
                    moveActivities.Add((Activity<CellContext>)activity);
            }
            else if (activity is Activity<MultiCellContext>) multiCellActivities.Add((Activity<MultiCellContext>)activity);
        }
    }
}