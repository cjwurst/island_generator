using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestSpawner : EntityComponent
{
    [SerializeField] GameObject creature;
    [SerializeField] GameObject wall;

    protected override void OnLeftMouseClicked(MouseClickedArgs args, List<IInvertible> commands)
    {
        entityBuilder.TryInstantiateEntity(wall, args.cell);
    }

    protected override void OnMiddleMouseClicked(MouseClickedArgs args, List<IInvertible> commands)
    {
        entityBuilder.TryInstantiateEntity(creature, args.cell);
    }
}
