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


        public static void Declare(Player performer, Unit movingUnit, int maxHexes, Selector.SelectionConfirmMethod confirmMethod)
        {
            //TBI (Dummy code)
            
            GameManager.SELECTOR.Prompt(movingUnit.Board.HexDict.Values, Confirm);

            void Confirm(Selector.SelectorArgs args)
            {
                if (args.Selection is not Hex h) throw new System.Exception();
                Move action = new(performer, movingUnit, movingUnit.Position, h.Position);
                GameManager.GAME.PushGameAction(action);

                confirmMethod?.Invoke(args);
            }


        }
    }

}
