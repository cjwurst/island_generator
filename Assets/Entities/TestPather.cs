using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestPather : EntityComponent
{
    [SerializeField] GameObject pathEffect;

    Mover mover;

    void Start()
    {
        mover = GetComponent<Mover>();
    }

    protected override void OnRightMouseClicked(MouseClickedArgs args, List<IInvertible> commands)
    {
        if (mover == null) return;
        if (pathFinder.TryFindPath(out var path, mover.CellPosition, args.cell))
            foreach (var pathCell in path)
                entityBuilder.TryInstantiateEntity(pathEffect, pathCell);
    }
}
