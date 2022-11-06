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

        public static void Prompt(Player performer, Unit movingUnit, int distance, Selector.SelectionConfirmMethod continueMethod, int minDistance = 0)
        {
            Unit u = movingUnit;
            GameManager.SELECTOR.Prompt(u.Board.PathFind(u.Position, (minDistance, distance), null, null), null);

        }


        private static class MoveConditions
        {
            public delegate Board.PathingCondition UnitPathingCondition(Unit u);
            public delegate Board.FinalCondition UnitFinalCondition(Unit u);


            public static Board.PathingCondition GetPathingCondition(Unit movingUnit, IEnumerable<UnitPathingCondition> unitConditions)
            {
                return (prev, next) =>
                {
                    foreach(var condition in unitConditions)
                    {
                        if (!condition.Invoke(movingUnit).Invoke(prev, next)) return false;
                    }
                    return true;
                };
            }

            public static Board.PathingCondition ExcludeBlockers(Unit u)
            {
                return (prev, next) => !next.BlocksPathing;
            }

        }


    }

}
