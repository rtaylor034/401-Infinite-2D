using Mono.Cecil;
using System;
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

        public static void PromptBasic(Player performer, Unit movingUnit, int distance, Selector.SelectionConfirmMethod continueMethod, int minDistance = 0)
        {
            Unit u = movingUnit;
            GameManager.SELECTOR.Prompt(u.Board.PathFind(u.Position, (minDistance, distance), null, null), null);

        }

        #region Standard Movement Conditions
        private static Board.ContinuePathCondition HexCollision => (prev, next) =>
        !next.BlocksPathing;
        private static Func<Unit, Board.ContinuePathCondition> OpposingUnitCollision => u => (prev, next) =>
        !(next.Occupant != null && next.Occupant.Team != u.Team);
        private static Func<Unit, Board.ContinuePathCondition> GuardedBaseCollision => u => (prev, next) =>
        !(next is BaseHex bhex && bhex.Team != u.Team);

        #endregion

    }

}
