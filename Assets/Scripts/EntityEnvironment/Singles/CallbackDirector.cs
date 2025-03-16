using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class CallbackDirector
{
    // component request events -- from one component to another, not necessarily within a single entity
    public event EventDispatcher<EntityActedArgs> entityActedEvent;
    public event EventDispatcher<EntityAttackedArgs> entityAttackedEvent;
    public event EventDispatcher<EntityPushedArgs> entityPushedEvent;
    public event EventDispatcher<EntityRealignedArgs> entityRealignedEvent;

    // entity data requests
    public event EventDispatcher<EntitiesAtRequestedArgs> entitiesAtRequestedEvent;
    public event EventDispatcher<EntitiesByAlignmentRequestedArgs> entitiesByAlignmentRequestedEvent;
    public event EventDispatcher<AlignmentRequestedArgs> alignmentRequestedEvent;
    public event EventDispatcher<StatsRequestedArgs> statsRequestedEvent;
    public event EventDispatcher<PositionsRequestedArgs> positionsRequestedEvent;
    public event EventDispatcher<RangesRequestedArgs> rangesRequestedEvent;
    public event EventDispatcher<ActivitiesRequestedArgs> activitiesRequestedEvent;

    // initiative order events
    public event EventDispatcher<RoundStartedArgs> roundStartedEvent;
    public event EventDispatcher roundPassedEvent;

    // input events
    public event EventDispatcher<MouseClickedArgs> leftMouseClickedEvent;
    public event EventDispatcher<MouseClickedArgs> rightMouseClickedEvent;
    public event EventDispatcher<MouseClickedArgs> middleMouseClickedEvent;
    public event EventDispatcher<DirectionalKeyPressedArgs> directionalKeyPressedEvent;

    // events for objects outside of the entity environment
    public event UpdateHandler updatedEvent;
    public event InvokeRequestHandler invokeRequestedEvent;
    public event ReverseRoundRequestHandler reverseRoundRequestedEvent;
    
    public void RaiseEntityPushed(EntityPushedArgs args) { RaiseEvent(entityPushedEvent, args); }
    public void RaiseEntityAttacked(EntityAttackedArgs args) { RaiseEvent(entityAttackedEvent, args); }
    public void RaiseEntityActed(EntityActedArgs args) { RaiseEvent(entityActedEvent, args); }
    public void RaiseEntityRealigned(EntityRealignedArgs args) { RaiseEvent(entityRealignedEvent, args); }

    public void RaiseEntitiesAtRequested(EntitiesAtRequestedArgs args) { RaiseEvent(entitiesAtRequestedEvent, args); }
    public void RaiseEntitiesByAlignmentRequested(EntitiesByAlignmentRequestedArgs args) { RaiseEvent(entitiesByAlignmentRequestedEvent, args); }
    public void RaiseAlignmentRequested(AlignmentRequestedArgs args) { RaiseEvent(alignmentRequestedEvent, args); }
    public void RaiseStatsRequested(StatsRequestedArgs args) { RaiseEvent(statsRequestedEvent, args); }
    public void RaisePositionsRequested(PositionsRequestedArgs args) { RaiseEvent(positionsRequestedEvent, args); }
    public void RaiseRangesRequested(RangesRequestedArgs args) { RaiseEvent(rangesRequestedEvent, args); }
    public void RaiseActivitiesRequested(ActivitiesRequestedArgs args) { RaiseEvent(activitiesRequestedEvent, args); }
    
    public void RaiseRoundStarted(RoundStartedArgs args) { RaiseEvent(roundStartedEvent, args); }
    public void RaiseRoundPassed() { RaiseEvent(roundPassedEvent); }
    public void RaiseReverseRoundRequested(int count) { reverseRoundRequestedEvent?.Invoke(count); }
    
    public void RaiseLeftMouseClicked(MouseClickedArgs args) { RaiseEvent(leftMouseClickedEvent, args); }
    public void RaiseRightMouseClicked(MouseClickedArgs args) { RaiseEvent(rightMouseClickedEvent, args); }
    public void RaiseMiddleMouseClicked(MouseClickedArgs args) { RaiseEvent(middleMouseClickedEvent, args); }
    public void RaiseDirectionalKeyPressed(DirectionalKeyPressedArgs args) { RaiseEvent(directionalKeyPressedEvent, args); }

    public void RaiseUpdated() { updatedEvent?.Invoke(); }

    void RaiseEvent<T>(EventDispatcher<T> dispatch, T args) where T : EventArgs
    {
        if (dispatch == null) return;
        var commands = new List<IInvertible>();
        var delayedActions = new List<DelayedAction>();
        dispatch.Invoke(args, commands, delayedActions);
        ConcludeEvent(commands, delayedActions);
    }
    void RaiseEvent(EventDispatcher dispatch)
    {
        if (dispatch == null) return;
        var commands = new List<IInvertible>();
        var delayedActions = new List<DelayedAction>();
        dispatch.Invoke(commands, delayedActions);
        ConcludeEvent(commands, delayedActions);
    }
    void ConcludeEvent(List<IInvertible> commands, List<DelayedAction> delayedActions)
    {
        delayedActions.OrderBy(delayedAction => delayedAction.priority);
        foreach (var delayedAction in delayedActions)
            delayedAction.Invoke();
        if (commands.Count > 0)
            invokeRequestedEvent.Invoke(Helper.Compose(commands));
    }

    // returns a dispatcher that calls *handler* at a given priority
    public static EventDispatcher<T> ResponseToDispatcher<T>(EventResponse<T> handler, float priority) where T : EventArgs
    {
        return (args, commands, delayedActions) => 
            delayedActions.Add(new DelayedAction(priority, () => handler.Invoke(args, commands)));
    }
    public static EventDispatcher ResponseToDispatcher(EventResponse handler, float priority)
    {
        return (commands, delayedActions) => 
            delayedActions.Add(new DelayedAction(priority, () => handler.Invoke(commands)));
    }
}

public delegate void EventDispatcher<T>(T args, List<IInvertible> commands, List<DelayedAction> delayedActions) where T : EventArgs;
public delegate void EventDispatcher(List<IInvertible> commands, List<DelayedAction> delayedActions);
public delegate void EventResponse<T>(T args, List<IInvertible> commands) where T : EventArgs;
public delegate void EventResponse(List<IInvertible> commands);

public delegate void InvokeRequestHandler(IInvertible command);
public delegate void ReverseRoundRequestHandler(int count);
public delegate void UpdateHandler();

public class DelayedAction
{
    public readonly float priority;
    Action action;

    public DelayedAction(float _priority, Action _action)
    {
        priority = _priority;
        action = _action;
    }

    public void Invoke() => action.Invoke();
}

public class EntityActedArgs : EventArgs
{
    public readonly GameObject entity;
    public int APCost { get; private set; }
    public EntityActedArgs(GameObject _entity, int apCost)
    {
        entity = _entity;
        APCost = apCost;
    }
}
public class EntityAttackedArgs : EventArgs
{
    public GameObject Attacker { get; private set; }
    public Damage Damage { get; private set; }
    public readonly Dictionary<GameObject, int> damageDealt = new Dictionary<GameObject, int>();
    public Vector2Int[] TargetCells { get; private set; }
    public readonly bool isTest;
    public EntityAttackedArgs(GameObject _attacker, Damage _damage, Vector2Int[] _targetCells, bool _isTest = false)
    {
        Attacker = _attacker;
        Damage = _damage;
        TargetCells = _targetCells;
        isTest = _isTest;
    }
}
public class EntityPushedArgs : EventArgs
{
    public readonly GameObject pusher;
    public readonly GameObject pushee;
    public readonly Vector2Int displacement;
    public EntityPushedArgs(GameObject _pusher, GameObject _pushee, Vector2Int _displacement)
    {
        pusher = _pusher;
        pushee = _pushee;
        displacement = _displacement;
    }
    public EntityPushedArgs(GameObject entity, Vector2Int _displacement)
    {
        pusher = entity;
        pushee = entity;
        displacement = _displacement;
    }
}
public class EntityRealignedArgs : EventArgs
{
    public readonly GameObject entity;
    public readonly AlignmentFlags flagsLost;
    public readonly AlignmentFlags flagsGained;
    public EntityRealignedArgs(GameObject _entity, AlignmentFlags _flagsLost, AlignmentFlags _flagsGained)
    {
        entity = _entity;
        flagsLost = _flagsLost;
        flagsGained = _flagsGained;
    }
}

public class EntitiesAtRequestedArgs : EventArgs
{
    public readonly Vector2Int[] cells;
    public readonly List<GameObject> entities = new List<GameObject>();
    public bool IncludesObstruction { get; set; }
    public EntitiesAtRequestedArgs(params Vector2Int[] _cells) { cells = _cells; }
}
public class EntitiesByAlignmentRequestedArgs : EventArgs
{
    public readonly AlignmentFlags alignmentFlags;
    readonly Dictionary<Alignments, List<GameObject>> entities = new Dictionary<Alignments, List<GameObject>>();
    public EntitiesByAlignmentRequestedArgs(AlignmentFlags _alignmentFlags)
    {
        alignmentFlags = _alignmentFlags;
    }
    public void Add(Alignments alignment, GameObject entity)
    {
        var list = GetEntities(alignment);
        list.Add(entity);
    }
    public GameObject[] Peek(Alignments alignment)
    {
        var list = GetEntities(alignment);
        return list.ToArray();
    }
    List<GameObject> GetEntities(Alignments alignment)
    {
        if (!entities.TryGetValue(alignment, out var list))
        {
            list = new List<GameObject>();
            entities.Add(alignment, list);
        }
        return list;
    }
}
public class AlignmentRequestedArgs : EventArgs
{
    public readonly GameObject requester;
    public readonly GameObject entity;
    public AlignmentFlags AlignmentFlags { get; set; }
    public AlignmentRequestedArgs(GameObject _entity)
    {
        requester = _entity;
        entity = _entity;
    }
    public AlignmentRequestedArgs(GameObject _requester, GameObject _entity)
    {
        requester = _requester;
        entity = _entity;
    }
}
public class StatsRequestedArgs : EventArgs
{
    public readonly StatType requestedStatType;
    public readonly Dictionary<GameObject, int> stats = new Dictionary<GameObject, int>();
    public int Stat
    {
        get
        {
            Assert.IsTrue(stats.Keys.Count == 1);
            return stats.Values.ToArray()[0];
        }
    }
    public StatsRequestedArgs(StatType _requestedStatType, params GameObject[] entities)
    {
        requestedStatType = _requestedStatType;
        foreach (var entity in entities)
            stats.Add(entity, 0);
    }
}
public class PositionsRequestedArgs : EventArgs
{
    public readonly Dictionary<GameObject, Vector2Int> positions = new Dictionary<GameObject, Vector2Int>();
    public Vector2Int Position {
        get
        {
            Assert.IsTrue(positions.Keys.Count == 1);
            foreach (var pair in positions) return pair.Value;
            return Vector2Int.zero;
        }
    }
    public PositionsRequestedArgs(params GameObject[] entities)
    {
        foreach (var entity in entities)
            positions.Add(entity, Vector2Int.zero);
    }
}
public class RangesRequestedArgs : EventArgs
{
    public readonly Dictionary<GameObject, float> ranges = new Dictionary<GameObject, float>();
    public float Range
    {
        get
        {
            Assert.IsTrue(ranges.Keys.Count == 1);
            foreach (var pair in ranges) return pair.Value;
            return 0;
        }
    }
    public RangesRequestedArgs(params GameObject[] entities)
    {
        foreach (var entity in entities)
            ranges.Add(entity, 0);
    }
}
public class ActivitiesRequestedArgs : EventArgs
{
    public readonly GameObject entity;
    readonly ActivityFlags flags;
    HashSet<Activity> activities = new HashSet<Activity>();
    public Activity[] Activities { get { return activities.ToArray(); } }
    public ActivitiesRequestedArgs(GameObject _entity, ActivityFlags _flags)
    {
        entity = _entity;
        flags = _flags;
    }
    public void AddActivity(Activity activity) { activities.Add(activity); }
}

public class RoundStartedArgs : EventArgs
{
    public readonly Padlock turnLock;
    public RoundStartedArgs(Padlock _turnLock) { turnLock = _turnLock; }
}

public class MouseClickedArgs : EventArgs
{
    public readonly Vector2Int cell;
    public MouseClickedArgs(Vector2Int _cell) { cell = _cell; }
}
public class DirectionalKeyPressedArgs : EventArgs
{
    public readonly Vector2Int direction;
    public DirectionalKeyPressedArgs(Vector2Int _direction) { direction = _direction; }
}
