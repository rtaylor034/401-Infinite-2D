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
        /// <summary>
        /// [Delegate]
        /// </summary>
        /// <param name="performer"></param>
        /// <param name="info"></param>
        /// <remarks>
        /// <c>(<see langword="async"/>) <see cref="Task"/> PromptEventHandlerMethod(<see cref="Player"/> <paramref name="performer"/>, <see cref="Info"/> <paramref name="info"/>) { }</c> <br></br>
        /// - <paramref name="performer"/> : The <see cref="Player"/> that is performing the Move.<br></br>
        /// - <paramref name="info"/> : the <see cref="Info"/> object passed to Prompt(). (Mutable)
        /// </remarks>
        public delegate Task PromptEventHandler(Player performer, Info info);

        private readonly static List<PromptEventHandler> _onPromptEventSubscribers = new();
        /// <summary>
        /// [<see langword="async"/> Event] <br></br> <br></br>
        /// Occurs when <see cref="Prompt(Player, Info, Action{Selector.SelectionArgs})"/> is called. <br></br>
        /// > Allows mutation of the <see cref="Info"/> before it read by Prompt().
        /// </summary>
        /// <remarks>
        /// <inheritdoc cref="PromptEventHandler"/>
        /// </remarks>
        public static GuardedCollection<PromptEventHandler> OnPromptEvent = new(_onPromptEventSubscribers);

        /// <summary>
        /// The information about this <see cref="Move"/>.
        /// </summary>
        public Info MoveInfo { get; private set; }
        /// <summary>
        /// The list of <see cref="PositionChange"/> actions that resulted from this Move.<br></br>
        /// <i>(These are in this Move's ResultantActions)</i>
        /// </summary>
        public List<PositionChange> PositionChanges => new(_positionChanges);
        private readonly List<PositionChange> _positionChanges;

        protected override void InternalPerform() { }
        protected override void InternalUndo() { }
        protected override async Task InternalEvaluate()
        {
            foreach (var change in _positionChanges)
                await AddResultant(change);
        }

        /// <summary>
        /// Creates a <see cref="Move"/> with <paramref name="moveInfo"/>, resulting in the <see cref="PositionChange"/> actions <paramref name="positionChanges"/>, by <paramref name="performer"/>.
        /// <br></br><br></br>
        /// > Use <b><see cref="Prompt(Player, Info, Action{Selector.SelectionArgs})"/></b> unless creating an action that already exists.
        /// </summary>
        /// <param name="performer"></param>
        /// <param name="moveInfo"></param>
        /// <param name="positionChanges"></param>
        public Move(Player performer, Info moveInfo, IEnumerable<PositionChange> positionChanges) : base(performer)
        {
            _positionChanges = new(positionChanges);
            MoveInfo = moveInfo;

        }

        /// <summary>
        /// Prompts <paramref name="performer"/> to make a Move based on <paramref name="info"/> and returns the <see cref="Move"/> action when a selection is made.<br></br>
        /// > If the Move is cancelled, <paramref name="cancelCallback"/> is called with the cancelled <see cref="Selector.SelectionArgs"/> and <see langword="null"/> is returned.
        /// </summary>
        /// <remarks>
        /// <i>(See <see cref="PositionalInfo"/> or <see cref="PathedInfo"/>)</i>
        /// </remarks>
        /// <param name="performer"></param>
        /// <param name="info"></param>
        /// <param name="cancelCallback"></param>
        /// <exception cref="ArgumentException"></exception>
        public static async Task<Move> Prompt(Player performer, Info info, Action<Selector.SelectionArgs> cancelCallback = null)
        {
            await InvokePromptEvent(performer, info);

            //variables used by both Handlers
            Queue<Unit> queue = new(info.MovingUnits);
            Unit movingUnit;
            Board.FinalPathCondition __GetFinalCondition(Unit unit) => (Hex h) =>
                info.FinalConditions
                .InvokeAll(unit)
                .InvokeAll(h)
                .GateAND() ||
                info.FinalOverrides
                .InvokeAll(unit)
                .InvokeAll(h)
                .GateOR();
            bool __CheckCancel(Selector.SelectionArgs selArgs)
            {
                if (selArgs.WasEmpty)
                {
                    if (info.Forced) selArgs.ReturnCode = 1;
                    cancelCallback?.Invoke(selArgs); return true;
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
                    cancelCallback?.Invoke(selArgs); return true;
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
                Board.ContinuePathCondition __GenerateDirectionals(Unit unit)
                {
                    List<Func<Hex, Hex, bool>> blockedConditions = new();
                    var blocks = pathed.DirectionalBlocks.InvokeAll(unit);
                    foreach (var block in blocks)
                    {
                        foreach (var (anchor, rule) in block)
                        {
                            blockedConditions.Add((p, n) =>
                            anchor.RadiusBetween(n.Position) - anchor.RadiusBetween(p.Position) == (sbyte)rule);
                        }
                    }
                    return (p, n) => blockedConditions.Count == 0 || !blockedConditions.InvokeAll(p, n).GateOR();
                }
                Board.ContinuePathCondition __GetPathCondition(Unit unit) => (Hex p, Hex n) =>
                    (pathed.PathingConditions
                    .InvokeAll(unit)
                    .InvokeAll(p, n)
                    .GateAND() &&
                    __GenerateDirectionals(unit)(p, n))
                    ||
                    pathed.PathingOverrides
                    .InvokeAll(unit)
                    .InvokeAll(p, n)
                    .GateOR();
                Board.PathWeightFunction __GetWeightFunction(Unit unit) => (Hex p, Hex n) =>
                    pathed.PathingWeightFunctions
                    .InvokeAll(unit)
                    .InvokeAll(p, n)
                    .Sum();

                int traversed = 0;

                Stack<(PositionChange PosChange, int Dist)> moves = new();
                while (queue.Count > 0)
                {
                    movingUnit = queue.Dequeue();

                    int max = Math.Min(pathed.MaxDistancePerUnit, pathed.Distance - traversed);

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
                    if (available.Count == 0) continue;
                    foreach (Unit u in queue) available.Add(u);
                    if (min == 0) available.Add(movingUnit.Board.HexAt(movingUnit.Position));

                    var selArgs = await GameManager.SELECTOR.Prompt(available);

                    if (moves.Count == 0 && __CheckCancel(selArgs)) continue;
                    if (selArgs.WasEmpty) continue;
                    if (selArgs.WasCancelled)
                    {
                        var (action, travel) = moves.Pop();
                        queue.Enqueue(action.AffectedUnit);
                        queue.Enqueue(movingUnit);
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
                        traversed += pathsFound[hex];
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
                    HashSet<Selectable> available = new(movingUnit.Board.HexesAt(positional.PositionOffsets.Offset(positional.Anchor).Rotate(positional.Anchor, positional.PerspectiveRotation)));
                    foreach (Unit u in queue) available.Add(u);
                    if (!positional.Forced) available.Add(movingUnit.Board.HexAt(movingUnit.Position));

                    var selArgs = await GameManager.SELECTOR.Prompt(available);

                    if (moves.Count == 0 && __CheckCancel(selArgs)) continue;
                    if (selArgs.WasEmpty) continue;
                    if (selArgs.WasCancelled)
                    {
                        var action = moves.Pop();
                        queue.Enqueue(action.AffectedUnit);
                        queue.Enqueue(movingUnit);
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
                        break;
                    }

                }
                return (moves.Count > 0) ? new(performer, info, moves) : null;
            }
        }

        private static async Task InvokePromptEvent(Player performer, Info args)
        {
            foreach (var subscriber in new List<PromptEventHandler>(_onPromptEventSubscribers))
            {
                await subscriber(performer, args);
            }
        }

        public override string ToString()
        {
            return $"<MOVE> ({string.Join(", ",MoveInfo.MovingUnits)})" + base.ToString();
        }
        public abstract record Info
        {
            #region Documentation Helpers
#pragma warning disable IDE0052
#pragma warning disable IDE1006

            /// <summary>
            /// </summary>
            /// <remarks>
            /// <c>Function(<see cref="Unit"/> u) { }</c><br></br>
            /// - u : A given <see cref="Unit"/> out of the moving Units.<br></br>
            /// <see langword="return"/> -> <br></br>
            /// </remarks>
            private static readonly bool __DOC__UnitFunction;

#pragma warning restore IDE0052
#pragma warning restore IDE1006
            #endregion

            /// <summary>
            /// The Units that are being prompted to Move.<br></br>
            /// > <see cref="PathedInfo"/> : The Move is split among these Units.<br></br>
            /// > <see cref="PositionalInfo"/> : A single Unit out of these Units is chosen to Move.
            /// </summary>
            public HashSet<Unit> MovingUnits { get; set; }
            /// <summary>
            /// If TRUE, this Move cannot be cancelled.
            /// </summary>
            /// <remarks>
            /// Default : <c><see langword="false"/></c>
            /// </remarks>
            public virtual bool Forced { get; set; } = false;
            /// <summary>
            /// A <see cref="Hex"/> must pass ALL of these conditions (<see cref="Board.FinalPathCondition"/>) to be considered a valid Move.<br></br>
            /// > Each element is a function of the given <see cref="Unit"/> that would Move, returning a condition that applies to that Unit.
            /// </summary>
            /// <remarks>
            /// Default: <c>{ <see cref="STANDARD_VALID_HEX"/> }</c>
            /// <br></br><br></br>
            /// <inheritdoc cref="__DOC__UnitFunction"/>
            /// ( <inheritdoc cref="Board.FinalPathCondition"/> )
            /// </remarks>
            public List<Func<Unit, Func<Hex, bool>>> FinalConditions { get; set; } = new()
            { STANDARD_VALID_HEX };
            /// <summary>
            /// A <see cref="Hex"/> can pass ANY of these conditions (<see cref="Board.FinalPathCondition"/>) to be considered a valid Move (overriding FinalConditions).<br></br>
            /// > Each element is a function of the given <see cref="Unit"/> that would Move, returning a condition that applies to that Unit.
            /// </summary>
            /// <remarks>
            /// Default: <c>{ _ => (_, _) => <see langword="false"/> }</c>
            /// <br></br><br></br>
            /// <inheritdoc cref="__DOC__UnitFunction"/>
            /// ( <inheritdoc cref="Board.FinalPathCondition"/> )
            /// </remarks>
            public List<Func<Unit, Func<Hex, bool>>> FinalOverrides { get; set; } = new()
            { _ => _ => false };

            /// <summary>
            /// The condition that checks if a <see cref="Hex"/> is occupiable (regardless of <see cref="Unit"/>).<br></br>
            /// <i>(Part of <see cref="STANDARD_VALID_HEX"/>)</i>
            /// </summary>
            /// <remarks>
            /// <c>_ => hex => hex.IsOccupiable;</c>
            /// </remarks>
            public static readonly Func<Unit, Func<Hex, bool>> OCCUPIABLE_CHECK = _ => hex =>
            hex.IsOccupiable;
            /// <summary>
            /// The condition that checks if a <see cref="Hex"/> is a BaseHex and is guarded against the moving <see cref="Unit"/>.<br></br>
            /// <i>(Part of <see cref="STANDARD_VALID_HEX"/>)</i>
            /// </summary>
            /// <remarks>
            /// <c>unit => hex => <br></br>!(hex is BaseHex bhex &amp;&amp; bhex.IsGuarded &amp;&amp; bhex.Team != unit.Team);</c>
            /// </remarks>
            public static readonly Func<Unit, Func<Hex, bool>> GUARDED_BASE_CHECK = unit => hex =>
            !(hex is BaseHex bhex && bhex.IsGuarded && bhex.Team != unit.Team);

            /// <summary>
            /// The standard FinalCondition that all Moves implicitly have. (unless explicitly ommitted)
            /// </summary>
            /// <remarks>
            /// <c>unit => hex => ... </c><br></br>
            /// <i>Combines <see cref="GUARDED_BASE_CHECK"/> and <see cref="OCCUPIABLE_CHECK"/>.</i>
            /// </remarks>
            public static readonly Func<Unit, Func<Hex, bool>> STANDARD_VALID_HEX = unit => hex =>
            new Func<Unit, Func<Hex, bool>>[] { GUARDED_BASE_CHECK, OCCUPIABLE_CHECK }
            .InvokeAll(unit).InvokeAll(hex).GateAND();

            /// <summary>
            /// 
            /// </summary>
            /// <remarks>
            /// - <see cref="Forced"/><br></br>
            /// - <see cref="FinalConditions"/><br></br>
            /// - <see cref="FinalOverrides"/><br></br>
            /// - <see cref="MovingUnits"/> (Set by constructor)
            /// </remarks>
            /// <param name="movingUnits"></param>
            protected Info(IEnumerable<Unit> movingUnits)
            {
                MovingUnits = new(movingUnits);
            }

        }
        /// <summary>
        /// [ : ] <see cref="Info"/><br></br>
        /// </summary>
        public record PositionalInfo : Info
        {
            /// <summary>
            /// <b>Required*</b><br></br>
            /// PositionOffsets are relative to this position.<br></br>
            /// </summary>
            /// <remarks>
            /// (See <see cref="PositionOffsets"/>)
            /// </remarks>
            public Vector3Int Anchor { get; set; }
            /// <summary>
            /// <b>Required*</b><br></br>
            /// The set of positions that this Move can move too, relative to Anchor.
            /// </summary>
            /// <remarks>
            /// (See <see cref="Anchor"/>)<br></br><br></br>
            /// <i>
            /// Standard PositionOffsets: <br></br>
            /// - <see cref="IN_FRONT"/> <br></br>
            /// - <see cref="BEHIND"/> <br></br>
            /// - <see cref="ADJACENT"/>
            /// </i>
            /// </remarks>
            public HashSet<Vector3Int> PositionOffsets { get; set; }
            /// <summary>
            /// <b>Required*</b><br></br>
            /// The team-relative rotation of this positional Move.<br></br>
            /// <i>(Should be relative to the team of the *anchor* Unit of the positional)</i>
            /// </summary>
            /// <remarks>
            /// (See <see cref="Player.PerspectiveRotationOf(Player.ETeam)"/>)
            /// </remarks>
            public int PerspectiveRotation { get; set; }

            /// <summary>
            /// The standard PositionOffsets for "in front".
            /// </summary>
            /// <remarks>
            /// <c>=> <see langword="new"/>() { <see cref="BoardCoords"/>.up };</c>
            /// </remarks>
            public static HashSet<Vector3Int> IN_FRONT => new() { BoardCoords.up };
            /// <summary>
            /// The standard PositionOffsets for "behind".
            /// </summary>
            /// <remarks>
            /// <c>=> <see langword="new"/>() { -<see cref="BoardCoords"/>.up };</c>
            /// </remarks>
            public static HashSet<Vector3Int> BEHIND => new() { -BoardCoords.up };
            /// <summary>
            /// The standard PositionOffsets for "adjacent".
            /// </summary>
            /// <remarks>
            /// <c>=> <see langword="new"/>() { <see cref="Vector3Int"/>.zero.GetAdjacent() };</c>
            /// </remarks>
            public static HashSet<Vector3Int> ADJACENT => new(Vector3Int.zero.GetAdjacent());

            /// <summary>
            /// Creates a <see cref="Info"/> containing information about a positional Move.<br></br>
            /// > Used with <see cref="Prompt(Player, Info, Action{Selector.SelectionArgs})"/>
            /// </summary>
            /// <remarks>
            /// Required Properties:<br></br>
            /// - <see cref="Anchor"/><br></br>
            /// - <see cref="PositionOffsets"/><br></br>
            /// - <see cref="PerspectiveRotation"/><br></br><br></br>
            /// Defaulted Properties:<br></br>
            /// <inheritdoc cref="Info.Info(IEnumerable{Unit})"/>
            /// </remarks>
            /// <param name="movingUnits"></param>
            public PositionalInfo(IEnumerable<Unit> movingUnits) : base(movingUnits) { }
            /// <inheritdoc cref="PositionalInfo.PositionalInfo(IEnumerable{Unit})"/>
            public PositionalInfo(params Unit[] movingUnits) : base(movingUnits) { }
        }
        public record PathedInfo : Info
        {
            /// <summary>
            /// <b>Required*</b><br></br>
            /// The (maximum) amount of steps this Move can take.
            /// </summary>
            public int Distance { get; set; }
            /// <summary>
            /// The minimum amount of steps this Move can take.
            /// </summary>
            /// <remarks>
            /// Default: <c>0</c>
            /// </remarks>
            public int MinDistance { get; set; } = 0;
            /// <summary>
            /// The maximum amount of steps an individual Unit can take in this Move.<br></br>
            /// (For Moves that are split between multiple MovingUnits)
            /// </summary>
            /// <remarks>
            /// Default: <c>1000</c>
            /// </remarks>
            public int MaxDistancePerUnit { get; set; } = 1000;
            /// <summary>
            /// A step must pass ALL of these conditions (<see cref="Board.ContinuePathCondition"/>) to be considered a valid step in this Move.<br></br>
            /// > Each element is a function of the given <see cref="Unit"/> that would Move, returning a condition that applies to that Unit.
            /// </summary>
            /// <remarks>
            /// Default: <c>{ <see cref="STANDARD_COLLISION"/> }</c>
            /// <br></br><br></br>
            /// <inheritdoc cref="Info.__DOC__UnitFunction"/>
            /// ( <inheritdoc cref="Board.ContinuePathCondition"/> )
            /// </remarks>
            public List<Func<Unit, Func<Hex, Hex, bool>>> PathingConditions { get; set; } = new()
            { STANDARD_COLLISION };
            /// <summary>
            /// A step can pass ANY of these conditions (<see cref="Board.ContinuePathCondition"/>) to be considered a valid step in this Move.<br></br>
            /// (Overrides <see cref="PathingConditions"/>)<br></br>
            /// > Each element is a function of the given <see cref="Unit"/> that would Move, returning a condition that applies to that Unit.
            /// </summary>
            /// <remarks>
            /// Default: <c>{ _ => (_, _) => <see langword="false"/> }</c>
            /// <br></br><br></br>
            /// <inheritdoc cref="Info.__DOC__UnitFunction"/>
            /// ( <inheritdoc cref="Board.ContinuePathCondition"/> )
            /// </remarks>
            public List<Func<Unit, Func<Hex, Hex, bool>>> PathingOverrides { get; set; } = new()
            { _ => (_, _) => false };
            /// <summary>
            /// The SUM of these weight functions will be used when pathfinding for this Move.<br></br>
            /// > Each element is a function of the given <see cref="Unit"/> that would Move, returning a weight function that applies to that Unit.
            /// </summary>
            /// <remarks>
            /// Default: <c>{ _ => (_, _) => 1 }</c>
            /// <br></br><br></br>
            /// <inheritdoc cref="Info.__DOC__UnitFunction"/>
            /// ( <inheritdoc cref="Board.PathWeightFunction"/> )
            /// </remarks>
            public List<Func<Unit, Func<Hex, Hex, int>>> PathingWeightFunctions { get; set; } = new()
            { _ => (_, _) => 1 };
            /// <summary>
            /// A step will be invalidated if it follows ANY of these (Anchor, Radius Rule) pairs.<br></br>
            /// <i>(i.e. A set of conditions that return false for going Away/Around/Toward an Anchor)</i><br></br>
            /// > Each element is a function of the given <see cref="Unit"/> that would Move, returning a weight function that applies to that Unit.
            /// </summary>
            /// <remarks>
            /// Default: <c>{ _ => <see cref="DIRECTIONAL_NONE"/> }</c><br></br><br></br>
            /// <inheritdoc cref="Info.__DOC__UnitFunction"/>
            /// A set of <see cref="ERadiusRule"/>(s) and their respective Anchors that this Move must not follow.
            /// <br></br><br></br>
            /// <i><b>DEV:</b> Yea its kinda weird having it be must *not* follow, but trust it makes more sense in terms of expandibility.</i>
            /// </remarks>
            public List<Func<Unit, HashSet<(Vector3Int Anchor, ERadiusRule Rule)>>> DirectionalBlocks { get; set; } = new()
            { _ => DIRECTIONAL_NONE };

            /// <summary>
            /// The standard condition for checking Hex and Enemy Unit collision.<br></br>
            /// (Default element of <see cref="PathingConditions"/>)
            /// </summary>
            /// <remarks>
            /// <c>unit => (_, hex) => <br></br>
            /// !(hex.BlocksPathing || (hex.Occupant != null &amp;&amp; hex.Occupant.Team != unit.Team));</c>
            /// </remarks>
            public static readonly Func<Unit, Func<Hex, Hex, bool>> STANDARD_COLLISION = unit => (_, hex) =>
            !(hex.BlocksPathing || (hex.Occupant != null && hex.Occupant.Team != unit.Team));
            /// <summary>
            /// Represents 'no directional block at all'.
            /// </summary>
            /// <remarks>
            /// <c><see langword="new"/> HashSet&lt;(<see cref="Vector3Int"/> Anchor, <see cref="ERadiusRule"/> Rule)&gt;()</c>
            /// </remarks>
            public static readonly HashSet<(Vector3Int Anchor, ERadiusRule Rule)> DIRECTIONAL_NONE = new();

            /// <summary>
            /// Enum for <see cref="DirectionalBlocks"/>.<br></br>
            /// - <see cref="Toward"/> : Steps decreasing in distance (radius) to Anchor. <br></br>
            /// - <see cref="Away"/> : Steps increasing in distance (radius) to Anchor. <br></br>
            /// - <see cref="Around"/> : Steps staying the same distance (radius) to Anchor.
            /// </summary>
            //it is important that these values are -1, 0, and 1. (they are casted when generating conditions).
            public enum ERadiusRule : sbyte
            {
                Toward = -1,
                Around = 0,
                Away = 1
            }

            /// <summary>
            /// Creates a <see cref="Info"/> containing information about a pathed Move.<br></br>
            /// > Used with <see cref="Prompt(Player, Info, Action{Selector.SelectionArgs})"/>
            /// </summary>
            /// <remarks>
            /// Required Properties:<br></br>
            /// - <see cref="Distance"/><br></br><br></br>
            /// Defaulted Properties:<br></br>
            /// - <see cref="MinDistance"/><br></br>
            /// - <see cref="MaxDistancePerUnit"/><br></br>
            /// - <see cref="PathingConditions"/><br></br>
            /// - <see cref="PathingOverrides"/><br></br>
            /// - <see cref="PathingWeightFunctions"/><br></br>
            /// - <see cref="DirectionalBlocks"/><br></br>
            /// <inheritdoc cref="Info.Info(IEnumerable{Unit})"/>
            /// </remarks>
            /// <param name="movingUnits"></param>
            public PathedInfo(IEnumerable<Unit> movingUnits) : base(movingUnits) { }
            /// <inheritdoc cref="PathedInfo.PathedInfo(IEnumerable{Unit})"/>
            public PathedInfo(params Unit[] movingUnits) : base(movingUnits) { }
        }

    }
}
