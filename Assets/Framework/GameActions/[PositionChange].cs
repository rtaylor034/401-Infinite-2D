using Mono.Cecil;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using static System.Collections.Specialized.BitVector32;

public abstract partial class GameAction
{
    /// <summary>
    /// [ : ] <see cref="GameAction"/>
    /// </summary>
    public class PositionChange : GameAction
    {
        public Unit AffectedUnit { get; private set; }
        public Vector3Int FromPos { get; private set; }
        public Vector3Int ToPos { get; private set; }

        protected override void InternalPerform() => AffectedUnit.UpdatePosition(ToPos);
        protected override void InternalUndo() => AffectedUnit.UpdatePosition(FromPos);

        public PositionChange(Player performer, Unit unit, Vector3Int fromPos, Vector3Int toPos) : base(performer)
        {
            AffectedUnit = unit;
            FromPos = fromPos;
            ToPos = toPos;
        }

        public override string ToString()
        {
            return $"<POSITION> {AffectedUnit}: {FromPos} -> {ToPos}" + base.ToString();
        }
    }

}
