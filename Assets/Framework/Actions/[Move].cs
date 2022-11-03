using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract partial class Action
{

    public class Move : Action
    {

        public Unit MovedUnit { get; private set; }
        public Vector3Int FromPos { get; private set; }
        public Vector3Int ToPos { get; private set; }


        private Move(Player performer, Unit unit, Vector3Int fromPos, Vector3Int toPos) : base(performer)
        {
            MovedUnit = unit;
            FromPos = fromPos;
            ToPos = toPos;
        }

        public override void Perform()
        {
            throw new System.NotImplementedException();
        }

        public override void Undo()
        {
            throw new System.NotImplementedException();
        }

        public static void Declare(Unit movingUnit, int maxHexes, Selector.SelectionConfirmMethod confirmMethod)
        {
            
        }
    }

}
