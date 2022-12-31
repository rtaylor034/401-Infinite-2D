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
        public Vector3Int Change { get; private set; }

        protected override void InternalPerform() => AffectedUnit.UpdatePosition(AffectedUnit.Position + Change);
        protected override void InternalUndo() => AffectedUnit.UpdatePosition(AffectedUnit.Position - Change);

        public PositionChange(Player performer, Unit unit, Vector3Int change) : base(performer)
        {
            AffectedUnit = unit;
            Change = change;
        }
    }

}
