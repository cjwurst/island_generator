using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnTracker
{
    CallbackDirector director;
    Padlock turnLock = new Padlock();

    public TurnTracker(CallbackDirector _director)
    {
        director = _director;
        director.updatedEvent += OnUpdated;
    }

    bool firstUpdate = true;
    void OnUpdated()
    {
        if (!turnLock.IsLocked)
        {
            if (!firstUpdate)
                director.RaiseRoundPassed();
            director.RaiseRoundStarted(new RoundStartedArgs(turnLock));
            firstUpdate = false;
        }
    }
}
