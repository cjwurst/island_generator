using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityEnvironment
{
    public readonly CallbackDirector director;
    public readonly TurnTracker turnTracker;
    readonly EntityBuilder entityBuilder;

    public EntityEnvironment(CellGrid grid)
    {
        director = new CallbackDirector();
        var invoker = new InvertibleInvoker(director);
        var pathFinder = new PathFinder(grid.boundedCells, director);
        entityBuilder = new EntityBuilder(director, grid, pathFinder, invoker);
        turnTracker = new TurnTracker(director);
    }

    public void CreateEntity(GameObject entityPrefab, Vector2Int cell) { entityBuilder.TryInstantiateEntity(entityPrefab, cell); }
}
