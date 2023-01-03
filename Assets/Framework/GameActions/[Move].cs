using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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
        public delegate Task PromptEventHandler(Info info);

        private readonly static List<PromptEventHandler> _onPromptEventSubscribers = new();
        public static GuardedCollection<PromptEventHandler> OnPromptEvent = new(_onPromptEventSubscribers);

        public Info MoveInfo { get; private set; }
        public List<PositionChange> PositionChanges => new(_positionChanges);
        private readonly List<PositionChange> _positionChanges;

        protected override void InternalPerform() => throw new System.NotImplementedException();
        protected override void InternalUndo() => throw new System.NotImplementedException();
        protected override async Task InternalEvaluate()
        {
            foreach (var change in _positionChanges)
                await AddResultant(change);
        }

        public Move(Player performer, Info moveInfo, IEnumerable<PositionChange> positionChanges) : base(performer)
        {
            _positionChanges = new(positionChanges);
            Info = moveInfo;

        }

        public static async Task<Move> Prompt(Player performer, Info info, Action<Selector.SelectionArgs> cancelCallback)
        {
            await InvokePromptEvent(info);

            Queue<Unit> queue = new(info.MovingUnits);
            while (queue.Count > 0)
            {
                Unit movingUnit = queue.Dequeue();
                HashSet<Hex> availableHexes = new();

                if (info is PathedInfo pathed)
                {

                }
            }
            throw new NotImplementedException();
        }

        private static async Task InvokePromptEvent(Info args)
        {
            foreach (var subscriber in new List<PromptEventHandler>(_onPromptEventSubscribers))
            {
                await subscriber(args);
            }
        }

        public abstract record Info
        {   
            public HashSet<Unit> MovingUnits { get; private set; }
            public virtual bool Forced { get; private set; } = false;
            public Func<Unit, Predicate<Hex>> FinalCondition { get; private set; } =
                OCCUPIABLE_CHECK + GUARDED_BASE_CHECK;
            public Func<Unit, Predicate<Hex>> FinalOverride { get; private set; } = _ => _ => false;

            public static readonly Func<Unit, Predicate<Hex>> OCCUPIABLE_CHECK = _ => hex =>
            hex.IsOccupiable;
            public static readonly Func<Unit, Predicate<Hex>> GUARDED_BASE_CHECK = unit => hex =>
            !(hex is BaseHex bhex && bhex.IsGuarded && bhex.Team != unit.Team);
            protected Info(IEnumerable<Unit> movingUnits)
            {
                MovingUnits = new(movingUnits);
            }

        }
        public record PositionalInfo : Info
        {
            public Vector3Int Anchor { get; private set; }
            public HashSet<Vector3Int> PositionOffsets { get; private set; }
            public new bool Forced { get; private set; } = true;

            public static HashSet<Vector3Int> IN_FRONT => new() { BoardCoords.up };
            public static HashSet<Vector3Int> BEHIND => new() { -BoardCoords.up };
            public static HashSet<Vector3Int> ADJACENT => new(Vector3Int.zero.GetAdjacent());

            public PositionalInfo(IEnumerable<Unit> movingUnits) : base(movingUnits) { }
            public PositionalInfo(params Unit[] movingUnits) : base(movingUnits) { }
        }
        public record PathedInfo : Info
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
