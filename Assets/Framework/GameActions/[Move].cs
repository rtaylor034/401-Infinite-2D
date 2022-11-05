using Mono.Cecil;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public abstract partial class GameAction
{

    public class Move : GameAction
    {
        public static event GameActionEventHandler<Move> OnPerform;

        public Unit MovedUnit { get; private set; }
        public Vector3Int FromPos { get; private set; }
        public Vector3Int ToPos { get; private set; }


        private Move(Player performer, Unit unit, Vector3Int fromPos, Vector3Int toPos) : base(performer)
        {
            MovedUnit = unit;
            FromPos = fromPos;
            ToPos = toPos;
        }

        protected override void InternalPerform()
        {
            MovedUnit.UpdatePosition(ToPos);
            OnPerform?.Invoke(this);
        }

        protected override void InternalUndo()
        {
            MovedUnit.UpdatePosition(FromPos);
        }


        public static void Declare(Player performer, Unit movedUnit, Vector3Int fromPos, Vector3Int ToPos)
        {
            FinalizeDeclare(new Move(performer, movedUnit, fromPos, ToPos));
        }

        public static void Prompt(Player performer, Unit movingUnit, int maxUnits, Selector.SelectorArgs continueMethod)
        {

        }
    }

}
