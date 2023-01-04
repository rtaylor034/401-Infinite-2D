using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
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
    public class Move : GameAction
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
            MoveInfo = moveInfo;

        }

        //for cancelCallback, ReturnCode 1 means move was Forced, but there was no valid move.
        public static async Task<Move> Prompt(Player performer, Info info, Action<Selector.SelectionArgs> cancelCallback)
        {
            await InvokePromptEvent(info);

            //variables used by both Handlers
            Queue<Unit> queue = new(info.MovingUnits);
            Unit movingUnit;
            Board.FinalPathCondition __GetFinalCondition(Unit unit) => (Hex h) =>
                info.FinalConditions
                .InvokeAll(unit).Cast<Func<Hex, bool>>().ToArray()
                .InvokeAll(h)
                .GateAND() ||
                info.FinalOverrides
                .InvokeAll(unit).Cast<Func<Hex, bool>>().ToArray()
                .InvokeAll(h)
                .GateOR();
            bool __CheckCancel(Selector.SelectionArgs selArgs)
            {
                if (selArgs.WasEmpty)
                {
                    if (info.Forced) selArgs.ReturnCode = 1;
                    cancelCallback.Invoke(selArgs); return true;
                }
                if (selArgs.WasCancelled)
                {
                    if (info.Forced)
                    {
                        Debug.Log("You cannot cancel a forced Move.");
                        queue.Enqueue(movingUnit);
                        queue.CycleTo(movingUnit);
                        return true;
                    }
                    cancelCallback.Invoke(selArgs); return true;
                }
                return false;
            }

            //compiler-sugar'd beyond readability.
            return info switch
            {
                PathedInfo pa => await __HandlePathed(pa),
                PositionalInfo po => await __HandlePositional(po),
                _ => throw new ArgumentException("Move Info not recognized?"),
            };

            async Task<Move> __HandlePathed(PathedInfo pathed)
            {
                int traversed = 0;
                Stack<(PositionChange PosChange, int Dist)> moves = new();
                while (queue.Count > 0)
                {
                    movingUnit = queue.Dequeue();
                    Board.ContinuePathCondition __GenerateDirectionals(Unit unit)
                    {
                        List<Func<Hex, Hex, bool>> blockedConditions = new();
                        var blocks = pathed.DirectionalBlocks.InvokeAll(unit);
                        foreach (var block in blocks)
                        {
                            foreach(var (anchor, rule) in block)
                            {
                                blockedConditions.Add((p, n) =>
                                anchor.RadiusBetween(n.Position) - anchor.RadiusBetween(p.Position) == (sbyte)rule);
                            }
                        }
                        return (p, n) => blockedConditions.Count == 0 || !blockedConditions.InvokeAll(p, n).GateOR();
                    }
                    Board.ContinuePathCondition __GetPathCondition(Unit unit) => (Hex p, Hex n) =>
                        (pathed.PathingConditions
                        .InvokeAll(unit).Cast<Func<Hex, Hex, bool>>().ToArray()
                        .InvokeAll(p, n)
                        .GateAND() &&
                        __GenerateDirectionals(unit)(p, n))
                        ||
                        pathed.PathingOverrides
                        .InvokeAll(unit).Cast<Func<Hex, Hex, bool>>().ToArray()
                        .InvokeAll(p, n)
                        .GateOR();
                    Board.PathWeightFunction __GetWeightFunction(Unit unit) => (Hex p, Hex n) =>
                        pathed.PathingWeightFunctions
                        .InvokeAll(unit).Cast<Func<Hex, Hex, int>>().ToArray()
                        .InvokeAll(p, n)
                        .Sum();

                    int max = Math.Max(pathed.MaxDistancePerUnit, pathed.Distance - traversed);

                    int min = pathed.MinDistance - traversed;
                    if (min > 0) foreach(Unit u in queue)
                    {
                        var potentia = u.Board.PathFind(movingUnit.Position, (1, max), __GetPathCondition(u), __GetFinalCondition(u), __GetWeightFunction(u)).Values;
                        if (potentia.Count == 0) continue;
                        min -= potentia.Max();
                    }
                    min = (min < 0) ? 0 : min;

                    Dictionary<Hex, int> pathsFound = movingUnit.Board.PathFind(movingUnit.Position, (min, max), __GetPathCondition(movingUnit), __GetFinalCondition(movingUnit), __GetWeightFunction(movingUnit));

                    HashSet<Selectable> available = new(pathsFound.Keys);
                    foreach (Unit u in queue) available.Add(u);
                    if (min == 0) available.Add(movingUnit.Board.HexAt(movingUnit.Position));

                    var selArgs = await GameManager.SELECTOR.Prompt(available);

                    if (moves.Count == 0 && __CheckCancel(selArgs)) continue;
                    if (selArgs.WasEmpty) continue;
                    if (selArgs.WasCancelled)
                    {
                        var (action, travel) = moves.Pop();
                        queue.Enqueue(action.AffectedUnit);
                        queue.CycleTo(action.AffectedUnit);
                        traversed -= travel;
                        action.InternalUndo();
                        continue;
                    }

                    var selection = selArgs.Selection;
                    if (selection is Unit unit)
                    {
                        queue.Enqueue(movingUnit);
                        queue.CycleTo(unit);
                        continue;
                    }
                    if (selection is Hex hex)
                    {
                        if (hex.Position == movingUnit.Position) continue;
                        PositionChange changeAction = new(performer, movingUnit, movingUnit.Position, hex.Position);
                        moves.Push((changeAction, pathsFound[hex]));
                        changeAction.InternalPerform();
                    }
                }
                List<PositionChange> finalActions = new();
                foreach(var (action, _) in moves)
                {
                    action.InternalUndo();
                    finalActions.Add(action);
                }
                return (finalActions.Count > 0) ? new(performer, info, finalActions) : null;
            }
            async Task<Move> __HandlePositional(PositionalInfo positional)
            {
                Stack<PositionChange> moves = new();

                while (queue.Count > 0)
                {
                    movingUnit = queue.Dequeue();
                    HashSet<Selectable> available = new(movingUnit.Board.HexesAt(positional.PositionOffsets.Offset(positional.Anchor)));
                    foreach (Unit u in queue) available.Add(u);
                    if (!positional.Forced) available.Add(movingUnit.Board.HexAt(movingUnit.Position));

                    var selArgs = await GameManager.SELECTOR.Prompt(available);

                    if (moves.Count == 0 && __CheckCancel(selArgs)) continue;
                    if (selArgs.WasEmpty) continue;
                    if (selArgs.WasCancelled)
                    {
                        var action = moves.Pop();
                        queue.Enqueue(action.AffectedUnit);
                        queue.CycleTo(action.AffectedUnit);
                        action.InternalUndo();
                        continue;
                    }

                    var selection = selArgs.Selection;
                    if (selection is Unit unit)
                    {
                        queue.Enqueue(movingUnit);
                        queue.CycleTo(unit);
                        continue;
                    }
                    if (selection is Hex hex)
                    {
                        if (hex.Position == movingUnit.Position) continue;
                        PositionChange changeAction = new(performer, movingUnit, movingUnit.Position, hex.Position);
                        moves.Push(changeAction);
                        changeAction.InternalPerform();
                    }

                }
                return (moves.Count > 0) ? new(performer, info, moves) : null;
            }

            
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
            public List<Func<Unit, Predicate<Hex>>> FinalConditions { get; private set; } = new()
            { OCCUPIABLE_CHECK, GUARDED_BASE_CHECK };
            public List<Func<Unit, Predicate<Hex>>> FinalOverrides { get; private set; } = new()
            { _ => _ => false };

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
            public int MaxDistancePerUnit { get; private set; } = int.MaxValue;
            public List<Func<Unit, Board.ContinuePathCondition>> PathingConditions { get; private set; } = new()
            { STD_COLLISION };
            public List<Func<Unit, Board.ContinuePathCondition>> PathingOverrides { get; private set; } = new()
            { _ => (_, _) => false };
            public List<Func<Unit, Board.PathWeightFunction>> PathingWeightFunctions { get; private set; } = new()
            { _ => (_, _) => 1 };
            public List<Func<Unit, IEnumerable<(Vector3Int Anchor, ERadiusRule Rule)>>> DirectionalBlocks { get; private set; } = new()
            { _ => DIRECTIONAL_NONE };
            

            public static readonly Func<Unit, Board.ContinuePathCondition> STD_COLLISION = unit => (_, hex) =>
            !(hex.BlocksPathing || (hex.Occupant != null && hex.Occupant.Team != unit.Team));
            public static readonly IEnumerable<(Vector3Int Anchor, ERadiusRule Rule)> DIRECTIONAL_NONE = new(Vector3Int Anchor, ERadiusRule Rule)[0];

            //it is important that these values are -1, 0, and 1. (they are casted when generating conditions).
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
