using Mono.Cecil;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public abstract partial class GameAction
{
    /// <summary>
    /// [ : ] <see cref="GameAction"/>
    /// </summary>
    public class PositionChange : GameAction
    {
        /// <summary>
        /// The <see cref="Unit"/> that had its position changed.
        /// </summary>
        public Unit AffectedUnit { get; private set; }
        
        //consider using an offset that gets added to the position instesad of FromPos and ToPos to stay consistent with the rest of the "Change" GameActions.
        /// <summary>
        /// The position that AffectedUnit was at before it was changed.
        /// </summary>
        public Vector3Int FromPos { get; private set; }
        /// <summary>
        /// The position that AffectedUnit's position was changed to.
        /// </summary>
        public Vector3Int ToPos { get; private set; }

        protected override void InternalPerform() => AffectedUnit.UpdatePosition(ToPos);
        protected override void InternalUndo() => AffectedUnit.UpdatePosition(FromPos);

        /// <summary>
        /// Creates a <see cref="PositionChange"/> action, changing <paramref name="unit"/>'s position from <paramref name="fromPos"/> to <paramref name="toPos"/>, by <paramref name="performer"/>.
        /// </summary>
        /// <param name="performer"></param>
        /// <param name="unit"></param>
        /// <param name="fromPos"></param>
        /// <param name="toPos"></param>
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
