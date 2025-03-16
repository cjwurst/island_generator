using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnTaker : EntityComponent
{
    [SerializeField] ActivityStateChooser activityStateChooser;
    bool IsPC { get { return activityStateChooser == null; } }

    [SerializeField] List<Activity> basicActivities = new List<Activity>();

    Action passTurn = null;
    
    protected override EventPriorityData InitPriorityData()
    {
        return IsPC ? new EventPriorityData(("OnRoundStarted", 1)) : base.InitPriorityData();           // wait for NPC actions each round
    }

    protected override void OnRoundStarted(RoundStartedArgs args, List<IInvertible> commands)
    {
        passTurn = args.turnLock.Lock();
        if (!IsPC)
        {
            while (GetHasAP())
            {
                activityStateChooser.Act(new TurnContext(gameObject, null, grid, pathFinder, director));
                director.RaiseEntityActed(new EntityActedArgs(gameObject, 4));
            }
            passTurn.Invoke();
        }
    }

    protected override void OnDirectionalKeyPressed(DirectionalKeyPressedArgs args, List<IInvertible> commands)
    {
        if (!IsPC || passTurn == null) return;
        if (!GetHasAP()) { passTurn.Invoke(); return; }

        var position = Position;
        if (!pathFinder.IsObstructedAt(position + args.direction))
            director.RaiseEntityPushed(new EntityPushedArgs(gameObject, args.direction)); 
        else
        {
            var attackedArgs = new EntityAttackedArgs(gameObject, new Damage(1, DamageType.bludgeoning), new Vector2Int[] { position + args.direction });
            director.RaiseEntityAttacked(attackedArgs);
        }
        director.RaiseEntityActed(new EntityActedArgs(gameObject, 4));
        if (!GetHasAP()) { passTurn.Invoke(); return; }
    }

    bool GetHasAP()
    {
        var statArgs = new StatsRequestedArgs(StatType.AP, gameObject);
        director.RaiseStatsRequested(statArgs);
        return statArgs.Stat > 0;
    }

    // TEMPORARY
    protected override void OnRangesRequested(RangesRequestedArgs args, List<IInvertible> commands)
    {
        args.ranges[gameObject] = 2f;
    }

    class StateStack
    {
        ActivityStateChooser chooser;
        TurnContext context;

        List<ActivityState> states;
        
        Act NextAct
        {
            get
            {
                ActivityState state;
                if ((state = states.Peek()) == null)
                {
                    states = chooser.ChooseActivityStates(context);
                    state = states.Peek();
                }
                else chooser.CheckStateChanges(context, states);
                return state.NextAct;
            }
        }

        public StateStack(ActivityStateChooser _chooser, TurnContext _context)
        {
            chooser = _chooser;
            context = _context;
            states = chooser.ChooseActivityStates(context);
        }
    }
}

public class EnvironmentContext
{
    public readonly GameObject taker;

    public readonly CellGrid grid;
    public readonly PathFinder pathFinder;
    public readonly CallbackDirector director;

    public EnvironmentContext(GameObject _taker, CellGrid _grid, PathFinder _pathFinder, CallbackDirector _director)
    {
        taker = _taker;

        grid = _grid;
        pathFinder = _pathFinder;
        director = _director;
    }
    public EnvironmentContext(TurnContext turnContext)
    {
        taker = turnContext.taker;

        grid = turnContext.grid;
        pathFinder = turnContext.pathFinder;
        director = turnContext.director;
    }
}

public class TurnContext : EnvironmentContext
{
    public readonly ActivitySet activities;

    public TurnContext (GameObject taker, ActivitySet _activities, CellGrid grid, PathFinder pathFinder, CallbackDirector director) :
        base(taker, grid, pathFinder, director)
    {
        activities = _activities;
    }
}
