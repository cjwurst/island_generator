using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This code is copypastad from *EntityComponent* because we don't have multiple inheritance.
public abstract class NonComponentSubscriber
{
    protected CellGrid grid;
    protected CallbackDirector director;
    protected PathFinder pathFinder;
    protected EntityBuilder entityBuilder;

    EventPriorityData priorityData;

    public void Init(CellGrid _grid, CallbackDirector _director, PathFinder _pathFinder, EntityBuilder _entityBuilder)
    {
        priorityData = InitPriorityData();

        grid = _grid;
        director = _director;
        pathFinder = _pathFinder;
        entityBuilder = _entityBuilder;

        director.entityActedEvent += CallbackDirector.ResponseToDispatcher<EntityActedArgs>(OnEntityActed, priorityData["OnEntityActed"]);
        director.entityAttackedEvent += CallbackDirector.ResponseToDispatcher<EntityAttackedArgs>(OnEntityAttacked, priorityData["OnEntityAttacked"]);
        director.entityPushedEvent += CallbackDirector.ResponseToDispatcher<EntityPushedArgs>(OnEntityPushed, priorityData["OnEntityPushed"]);

        director.entitiesAtRequestedEvent += CallbackDirector.ResponseToDispatcher<EntitiesAtRequestedArgs>(OnEntitiesRequested, priorityData["OnEntitiesAtRequested"]);
        director.entitiesByAlignmentRequestedEvent += CallbackDirector.ResponseToDispatcher<EntitiesByAlignmentRequestedArgs>(OnEntitiesByAlignmentRequested, priorityData["OnEntitiesByAlignmentRequested"]);
        director.statsRequestedEvent += CallbackDirector.ResponseToDispatcher<StatsRequestedArgs>(OnStatsRequested, priorityData["OnStatsRequested"]);
        director.positionsRequestedEvent += CallbackDirector.ResponseToDispatcher<PositionsRequestedArgs>(OnPositionsRequested, priorityData["OnPositionsRequested"]);
        director.rangesRequestedEvent += CallbackDirector.ResponseToDispatcher<RangesRequestedArgs>(OnRangesRequested, priorityData["OnRangesRequested"]);
        director.activitiesRequestedEvent += CallbackDirector.ResponseToDispatcher<ActivitiesRequestedArgs>(OnActivitiesRequested, priorityData["OnActivitiesRequested"]);

        director.roundStartedEvent += CallbackDirector.ResponseToDispatcher<RoundStartedArgs>(OnRoundStarted, priorityData["OnRoundStarted"]);
        director.roundPassedEvent += CallbackDirector.ResponseToDispatcher(OnRoundPassed, priorityData["OnRoundStarted"]);

        director.leftMouseClickedEvent += CallbackDirector.ResponseToDispatcher<MouseClickedArgs>(OnLeftMouseClicked, priorityData["OnLeftMouseClicked"]);
        director.rightMouseClickedEvent += CallbackDirector.ResponseToDispatcher<MouseClickedArgs>(OnRightMouseClicked, priorityData["OnRightMouseClicked"]);
        director.middleMouseClickedEvent += CallbackDirector.ResponseToDispatcher<MouseClickedArgs>(OnMiddleMouseClicked, priorityData["OnMiddleMouseClicked"]);
        director.directionalKeyPressedEvent += CallbackDirector.ResponseToDispatcher<DirectionalKeyPressedArgs>(OnDirectionalKeyPressed, priorityData["OnDirectionalKeyPressed"]);

        AfterInit();
    }
    protected virtual EventPriorityData InitPriorityData() => new EventPriorityData();
    protected virtual void AfterInit() { }

    protected virtual void OnEntityActed(EntityActedArgs args, List<IInvertible> commands) { }
    protected virtual void OnEntityAttacked(EntityAttackedArgs args, List<IInvertible> commands) { }
    protected virtual void OnEntityPushed(EntityPushedArgs args, List<IInvertible> commands) { }
    protected virtual void OnEntityRealigned(EntityRealignedArgs args, List<IInvertible> commands) { }

    protected virtual void OnEntitiesRequested(EntitiesAtRequestedArgs args, List<IInvertible> commands) { }
    protected virtual void OnEntitiesByAlignmentRequested(EntitiesByAlignmentRequestedArgs args, List<IInvertible> commands) { }
    protected virtual void OnAlignmentRequested(AlignmentRequestedArgs args, List<IInvertible> commands) { }
    protected virtual void OnStatsRequested(StatsRequestedArgs args, List<IInvertible> commands) { }
    protected virtual void OnPositionsRequested(PositionsRequestedArgs args, List<IInvertible> commands) { }
    protected virtual void OnRangesRequested(RangesRequestedArgs args, List<IInvertible> commands) { }
    protected virtual void OnActivitiesRequested(ActivitiesRequestedArgs args, List<IInvertible> commands) { }

    protected virtual void OnRoundStarted(RoundStartedArgs args, List<IInvertible> commands) { }
    protected virtual void OnRoundPassed(List<IInvertible> commands) { }

    protected virtual void OnLeftMouseClicked(MouseClickedArgs args, List<IInvertible> commands) { }
    protected virtual void OnRightMouseClicked(MouseClickedArgs args, List<IInvertible> commands) { }
    protected virtual void OnMiddleMouseClicked(MouseClickedArgs args, List<IInvertible> commands) { }
    protected virtual void OnDirectionalKeyPressed(DirectionalKeyPressedArgs args, List<IInvertible> commands) { }

    protected class EventPriorityData
    {
        readonly Dictionary<string, float> priorities = new Dictionary<string, float>();
        public float this[string s]
        {
            get
            {
                if (priorities.TryGetValue(s, out float priority))
                    return priority;
                return 0f;
            }
        }

        public EventPriorityData(params (string key, float priority)[] pairs)
        {
            foreach (var pair in pairs)
                priorities.Add(pair.key, pair.priority);
        }
    }
}
