using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mover : EntityComponent
{
    [SerializeField] bool isObstruction = false;
    public bool IsObstruction { get { return isObstruction; } }

    public Vector2Int CellPosition
    {
        private set => transform.position = grid.CellToWorld(value);
        get => grid.WorldToCell(transform.position);
    }

    protected override EventPriorityData InitPriorityData()
    {
        return new EventPriorityData(("OnEntityPushed", float.PositiveInfinity));
    }

    protected override void OnEntityPushed(EntityPushedArgs args, List<IInvertible> commands)
    {
        if (args.pushee != gameObject) return;
        commands.Add(new MoveCommand(this, args.displacement));
    }

    protected override void OnEntitiesRequested(EntitiesAtRequestedArgs args, List<IInvertible> commands)
    {
        if (!args.cells.Contains(CellPosition)) return;
        args.entities.Add(gameObject);
        if (IsObstruction) args.IncludesObstruction = true;
    }

    protected override void OnPositionsRequested(PositionsRequestedArgs args, List<IInvertible> commands)
    {
        if (!args.positions.Keys.Contains(gameObject)) return;
        args.positions[gameObject] = CellPosition;
    }

    class MoveCommand : IInvertible
    {
        Mover mover;
        Vector2Int displacement;

        public MoveCommand(Mover _mover, Vector2Int _displacement)
        {
            mover = _mover;
            displacement = _displacement;
        }

        public void Do()
        {
            mover.CellPosition += displacement;
        }

        public void Undo()
        {
            mover.CellPosition -= displacement;
        }
    }
}
