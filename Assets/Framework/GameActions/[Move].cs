using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Scripting.APIUpdating;

public partial class GameAction
{

    /// <summary>
    /// [ : ] <see cref="GameAction"/>
    /// </summary>
    public abstract class Move : GameAction
    {
        public MoveInfo Info { get; private set; }
        public List<PositionChange> PositionChanges => new(_positionChanges);
        private readonly List<PositionChange> _positionChanges;

        protected override void InternalPerform() => throw new System.NotImplementedException();
        protected override void InternalUndo() => throw new System.NotImplementedException();
        protected override async Task InternalEvaluate()
        {
            foreach (var change in _positionChanges)
                await AddResultant(change);
        }

        public Move(Player performer, MoveInfo moveInfo, IEnumerable<PositionChange> positionChanges) : base(performer)
        {
            _positionChanges = new(positionChanges);
            Info = moveInfo;

        }

        public static async Task<Move> Prompt(Player performer, MoveInfo moveInfo, Action<Selector.SelectionArgs> cancelCallback)
        {
            throw new NotImplementedException();
        }

        public abstract record MoveInfo
        {   
            public HashSet<Unit> MovingUnits { get; private set; }
            public Func<Unit, Predicate<Hex>> FinalCondition { get; private set; } =
                OCCUPIABLE_CHECK + GUARDED_BASE_CHECK;
            public Func<Unit, Predicate<Hex>> FinalOverride { get; private set; } = _ => _ => false;

            public static readonly Func<Unit, Predicate<Hex>> OCCUPIABLE_CHECK = _ => hex =>
            hex.IsOccupiable;
            public static readonly Func<Unit, Predicate<Hex>> GUARDED_BASE_CHECK = unit => hex =>
            !(hex is BaseHex bhex && bhex.IsGuarded && bhex.Team != unit.Team);
            protected MoveInfo(IEnumerable<Unit> movingUnits)
            {
                MovingUnits = new(movingUnits);
            }

        }

        public record PositionalInfo : MoveInfo
        {
            public Vector3Int Anchor { get; private set; }
            public HashSet<Vector3Int> PositionOffsets { get; private set; }

            public static HashSet<Vector3Int> IN_FRONT => new() { BoardCoords.up };
            public static HashSet<Vector3Int> BEHIND => new() { -BoardCoords.up };
            public static HashSet<Vector3Int> ADJACENT => new(Vector3Int.zero.GetAdjacent());

            public PositionalInfo(IEnumerable<Unit> movingUnits) : base(movingUnits) { }
            public PositionalInfo(params Unit[] movingUnits) : base(movingUnits) { }
        }

        public record PathedInfo : MoveInfo
        {
            public int Distance { get; private set; }
            public int MinDistance { get; private set; } = 0;
            public Func<Unit, Board.ContinuePathCondition> PathingCondition { get; private set; } = STD_COLLISION;
            public Func<Unit, Board.ContinuePathCondition> PathingOverride { get; private set; } = _ => (_, _) => false;
            public Func<Unit, IEnumerable<(Vector3Int Anchor, ERadiusRule Rule)>> DirectionalBlocks { get; private set; } = _ => new (Vector3Int, ERadiusRule)[0];
            //add all individiual weight functions together to get final weight
            public Func<Unit, Board.PathWeightFunction> PathingWeightFunction { get; private set; } = _ => (_, _) => 1;

            public static readonly Func<Unit, Board.ContinuePathCondition> STD_COLLISION = unit => (_, hex) =>
            !(hex.BlocksPathing || (hex.Occupant != null && hex.Occupant.Team != unit.Team));

            public enum ERadiusRule : sbyte
            {
                Toward = -1,
                Around = 0,
                Away = 1
            }

            public PathedInfo(IEnumerable<Unit> movingUnits) : base(movingUnits) { }
            public PathedInfo(params Unit[] movingUnits) : base(movingUnits) { }
        }
    }
}
