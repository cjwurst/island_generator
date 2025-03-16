using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlignmentController : EntityComponent
{
    [SerializeField] AlignmentFlags alignmentFlags;

    protected override void OnEntitiesByAlignmentRequested(EntitiesByAlignmentRequestedArgs args, List<IInvertible> commands)
    {
        var alignment = AlignmentHelper.GetAlignment(args.alignmentFlags, alignmentFlags);
        args.Add(alignment, gameObject);
    }
}
