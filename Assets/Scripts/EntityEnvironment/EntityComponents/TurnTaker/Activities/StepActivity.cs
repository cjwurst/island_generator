using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

[CreateAssetMenu(fileName = "Step", menuName = "Entity Toolbox/Activities/Step")]
public class StepActivity : Activity<CellContext>
{
    protected override void OnActivate(CellContext context)
    {
        context.director.RaiseEntityPushed(new EntityPushedArgs(context.taker, context.targetCell));
    }
}
