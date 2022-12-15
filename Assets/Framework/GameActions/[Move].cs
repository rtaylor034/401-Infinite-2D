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
    public class Move : GameAction
    {

        /// <summary>
        /// Occurs when any <see cref="Move"/> is prompted using <see cref="Prompt(PromptArgs, Action{Move})"/>. <br></br>
        /// </summary>
        /// <remarks>
        /// <i>Modifications to the <see cref="PromptArgs"/> will be applied to the Prompt() call.</i>
        /// </remarks>
        public static event Action<PromptArgs> OnPromptEvent;

        /// <summary>
        /// The <see cref="Unit"/> that is Moved by this action.
        /// </summary>
        public Unit MovedUnit { get; private set; }
        /// <summary>
        /// The position that MovedUnit was at before this action. <br></br>
        /// <i>MovedUnit is Moved to this position when this action is undone.</i>
        /// </summary>
        public Vector3Int FromPos { get; private set; }
        /// <summary>
        /// The position that MovedUnit was Moved to, due to this action. <br></br>
        /// <i>MovedUnit is Moved to this position when this action is performed.</i>
        /// </summary>
        public Vector3Int ToPos { get; private set; }
        /// <summary>
        /// The distance that this Move traversed.
        /// </summary>
        /// <remarks>
        /// <c>=> FromPos.RadiusBetween(ToPos)</c>
        /// </remarks>
        public int MovedDistance => FromPos.RadiusBetween(ToPos);


        /// <summary>
        /// Moves <paramref name="movedUnit"/> from <paramref name="fromPos"/> to <paramref name="toPos"/>, by <paramref name="performer"/>. <br></br> <br></br>
        /// > Unless you are creating a <see cref="Move"/> that has already happened, use
        /// <b><see cref="Prompt(PromptArgs, Action{Move})"/></b>.
        /// </summary>
        /// <remarks>
        /// <i><see cref="Move"/> object is created within Prompt()</i>
        /// </remarks>
        /// <param name="performer"></param>
        /// <param name="movedUnit"></param>
        /// <param name="fromPos"></param>
        /// <param name="toPos"></param>
        public Move(Player performer, Unit movedUnit, Vector3Int fromPos, Vector3Int toPos) : base(performer)
        {
            MovedUnit = movedUnit;
            FromPos = fromPos;
            ToPos = toPos;
        }
        protected override void InternalPerform()
        {
            MovedUnit.UpdatePosition(ToPos);
        }
        protected override void InternalUndo()
        {
            MovedUnit.UpdatePosition(FromPos);
        }

        /// <summary>
        /// Prompts for a split move between (and starting with) the <see cref="Unit"/> specified in <paramref name="args"/> and <paramref name="otherSplitUnits"/>. <br></br>
        /// Max distance per <see cref="Unit"/> is <paramref name="maxPerUnit"/>.
        /// </summary>
        /// <param name="args"></param>
        /// <param name="otherSplitUnits"></param>
        /// <param name="maxPerUnit"></param>
        /// <param name="cancelCallback"></param>
        /// <param name="callPromptEvent"></param>
        /// <returns></returns>
        public static async Task<IEnumerable<Move>> PromptSplit(PromptArgs.Pathed args, IEnumerable<Unit> otherSplitUnits, int maxPerUnit = int.MaxValue, Action<Selector.SelectionArgs> cancelCallback = null, bool callPromptEvent = true)
        {

            if (callPromptEvent) OnPromptEvent?.Invoke(args);
            if (args.ReturnCode == -1) return null;

            Stack<Move> moves = new();
            Queue<Unit> units = new();
            units.Enqueue(args.MovingUnit);
            foreach (Unit u in otherSplitUnits) units.Enqueue(u);

            int required = args.MinDistance;
            int distLeft = args.Distance;

            void __UpdateSplit(Move move, bool undo = false)
            {
                if (undo)
                {
                    distLeft += move.MovedDistance;
                    required += move.MovedDistance;
                }
                else
                {
                    distLeft -= move.MovedDistance;
                    required -= move.MovedDistance;
                }
            }

            while (units.Count > 0)
            {
                var dist = (distLeft <= maxPerUnit) ? distLeft : maxPerUnit;
                args.Distance = dist;
                args.MinDistance = required - ((units.Count - 1) * dist);
                args.MovingUnit = units.Peek();

                HashSet<Selectable> selectables = new(GetPossibleHexes(args));
                foreach (Unit u in units) selectables.Add(u);
                selectables.Remove(args.MovingUnit);

                Selector.SelectionArgs sel = await GetSelection(selectables);

                if (sel.Selection is Unit unit)
                {
                    while (units.Peek() != unit) units.Enqueue(units.Dequeue());
                    continue;
                }
                if (sel.Selection is Hex hex)
                {
                    Move move = new(args.Performer, args.MovingUnit, args.MovingUnit.Position, hex.Position);
                    moves.Push(move);
                    move.InternalPerform();
                    __UpdateSplit(move);

                    units.Dequeue();

                    continue;
                }

                if (moves.Count > 0)
                {
                    Move cancel = moves.Pop();
                    cancel.InternalUndo();
                    __UpdateSplit(cancel, true);

                    //weirdchamp code, should just use a list :P
                    Queue<Unit> oldUnits = new(units);
                    units.Clear();
                    units.Enqueue(cancel.MovedUnit);
                    foreach (Unit old in oldUnits) units.Enqueue(old);
                    continue;
                }
                if (args.Forced)
                {
                    if (!sel.WasEmpty)
                    {
                        Debug.Log("you cannot cancel a forced move");
                        continue;
                    }
                    sel.ReturnCode = 1;
                }

                cancelCallback?.Invoke(sel);
                break;
            }

            //undo because moves were artificially performed
            foreach (Move move in moves) move.InternalUndo();

            return moves.Where(m => m.MovedDistance > 0);
        }

        /// <summary>
        /// Prompts to create a <see cref="Move"/> action based on <paramref name="args"/>. <br></br>
        /// > Calls <paramref name="confirmCallback"/> with the created <see cref="Move"/> once all selections are made. <br></br>
        /// > If any selection is cancelled or invalid, <paramref name="cancelCallback"/> will be called with the invalid <see cref="Selector.SelectorArgs"/> instead.
        /// </summary>
        /// <remarks>
        /// <paramref name="cancelCallback"/>.ReturnCode: <br></br>
        /// 1 - Move was Forced, but no valid Hexes could be found. <br></br>
        /// <br></br>
        /// <i>(See <see cref="PromptArgs.Pathed"/> / <see cref="PromptArgs.Positional"/>)</i>
        /// </remarks>
        /// <param name="args"></param>
        /// <param name="confirmCallback"></param>
        public static async Task<Move> Prompt(PromptArgs args, Action<Selector.SelectionArgs> cancelCallback = null, bool callPromptEvent = true)
        {

            async Task<Move> __Prompt()
            {
                if (callPromptEvent) OnPromptEvent?.Invoke(args);
                if (args.ReturnCode == -1) return null;

                HashSet<Selectable> possibleHexes = new(GetPossibleHexes(args));

                Selector.SelectionArgs sel = await GetSelection(possibleHexes);

                var u = args.MovingUnit;
                if (sel.Selection is not null)
                    return new(args.Performer, u, u.Position, (sel.Selection as Hex).Position);

                if (args.Forced)
                {
                    if (!sel.WasEmpty)
                    {
                        Debug.Log("you cannot cancel a forced move");
                        return await __Prompt();
                    }
                    sel.ReturnCode = 1;
                }

                cancelCallback?.Invoke(sel);
                return null;
            }

            return await __Prompt();
        }

        #region Method Helpers
        private static async Task<Selector.SelectionArgs> GetSelection(IEnumerable<Selectable> selectables)
        {
            return selectables.IsSingleElement(out var single) ?
                    GameManager.SELECTOR.SpoofSelection(single) :
                    await GameManager.SELECTOR.Prompt(selectables);
        }
        private static HashSet<Selectable> GetPossibleHexes(PromptArgs args)
        {
            bool __FinalCondition(Hex h) => GetCombinedFinalConditon(args)(h);
            Unit u = args.MovingUnit;

            HashSet<Selectable> o;
            if (args is PromptArgs.Pathed p)
            {
                o = new(u.Board.PathFind(u.Position, (p.MinDistance - 1, p.Distance), GetCombinedPathingCondition(p), __FinalCondition));
                if (p.MinDistance <= 0) o.Add(p.MovingUnit.Board.HexAt(p.MovingUnit.Position));
            }
            else if (args is PromptArgs.Positional a)
            {
                o = new(u.Board.HexesAt(GetPositionalPositions(a)).Where(__FinalCondition));
                if (!args.Forced) o.Add(a.MovingUnit.Board.HexAt(a.MovingUnit.Position));
            }
            else throw new ArgumentException("PromptArgs not recognized?");

            return o;
        }
        private static Board.ContinuePathCondition GetCombinedPathingCondition(PromptArgs.Pathed args)
        {
            Unit u = args.MovingUnit;
            PromptArgs.Pathed.ECollisionIgnoresF ci = args.CollisionIgnores;
            (PromptArgs.Pathed.EDirectionalsF, Vector3Int) dir = args.Directionals;
            var dpos = dir.Item2;

            return (p, n) =>
            ((ci.HasFlag(PromptArgs.Pathed.ECollisionIgnoresF.Walls) || (HexCollision(p, n) && OpposingUnitCollision(u)(p, n))) &&
            (ci.HasFlag(PromptArgs.Pathed.ECollisionIgnoresF.Bases) || GuardedBaseCollision(u)(p, n))
            &&
            ((dir.Item1 == PromptArgs.Pathed.EDirectionalsF.None) ||
            (dir.Item1.HasFlag(PromptArgs.Pathed.EDirectionalsF.Away) && DirectionalAway(dpos)(p, n)) ||
            (dir.Item1.HasFlag(PromptArgs.Pathed.EDirectionalsF.Toward) && DirectionalToward(dpos)(p, n)) ||
            dir.Item1.HasFlag(PromptArgs.Pathed.EDirectionalsF.Around) && DirectionalAround(dpos)(p, n))
            &&
            __Combined(args.CustomPathingRestrictions)(p, n))
            ||
            __Combined(args.CustomPathingOverrides, true)(p, n);

            Board.ContinuePathCondition __Combined(IEnumerable<Board.ContinuePathCondition> cond, bool invert = false)
            {
                return (invert) ?
                (p, n) =>
                {
                    foreach (var r in cond)
                        if (r.Invoke(p, n)) return true;
                    return false;
                }
                :
                (p, n) =>
                {
                    foreach (var r in cond)
                        if (!r.Invoke(p, n)) return false;
                    return true;
                };
            }
        }
        private static Board.FinalPathCondition GetCombinedFinalConditon(PromptArgs args)
        {
            return (h) => (__Combined(args.CustomFinalRestrictions)(h) && h.IsOccupiable) || __Combined(args.CustomFinalOverrides, true)(h);

            Board.FinalPathCondition __Combined(IEnumerable<Board.FinalPathCondition> cond, bool invert = false)
            {
                return (invert) ?
                (h) =>
                {
                    foreach (var r in cond)
                        if (r.Invoke(h)) return true;
                    return false;
                }
                :
                (h) =>
                {
                    foreach (var r in cond)
                        if (!r.Invoke(h)) return false;
                    return true;
                };
            }

        }
        private static HashSet<Vector3Int> GetPositionalPositions(PromptArgs.Positional args)
        {
            HashSet<Vector3Int> o = new HashSet<Vector3Int>();
            foreach (var pos in args.PositionalOffsets)
            {
                o.Add(pos + args.AnchorPosition);
            }
            BoardCoords.Rotate(o, args.AnchorPosition, Player.PerspectiveRotationOf(args.TeamRelativity));
            return o;
        }
        #endregion
        #region Standard Collision Conditions
        private static Board.ContinuePathCondition HexCollision => (_, next) =>
        !next.BlocksPathing;
        private static Func<Unit, Board.ContinuePathCondition> OpposingUnitCollision => u => (_, next) =>
        !(next.Occupant != null && next.Occupant.Team != u.Team);
        private static Func<Unit, Board.ContinuePathCondition> GuardedBaseCollision => u => (_, next) =>
        !(next is BaseHex bhex && bhex.Team != u.Team && bhex.IsGuarded);

        #endregion
        #region Standard Directional Conditions
        private static Func<Vector3Int, Board.ContinuePathCondition> DirectionalAway => pos => (prev, next) =>
        BoardCoords.RadiusBetween(pos, prev.Position) < BoardCoords.RadiusBetween(pos, next.Position);
        private static Func<Vector3Int, Board.ContinuePathCondition> DirectionalAround => pos => (prev, next) =>
        BoardCoords.RadiusBetween(pos, prev.Position) == BoardCoords.RadiusBetween(pos, next.Position);
        private static Func<Vector3Int, Board.ContinuePathCondition> DirectionalToward => pos => (prev, next) =>
        BoardCoords.RadiusBetween(pos, prev.Position) > BoardCoords.RadiusBetween(pos, next.Position);

        #endregion

        /// <summary>
        /// <b>abstract</b> [ : ] <see cref="CallbackArgs"/>
        /// </summary>
        /// <remarks>
        /// (See <see cref="PromptArgs.Pathed"/>, <see cref="PromptArgs.Positional"/>)<br></br>
        /// <see cref="CallbackArgs.ReturnCode"/>: <br></br>
        /// -1 : Technical Null (will force Prompt() to return <see langword="null"/>).
        /// </remarks>
        public abstract class PromptArgs : CallbackArgs
        {
            /// <summary>
            /// The Player that is performing this <see cref="Move"/>.
            /// </summary>
            public Player Performer { get; set; }
            /// <summary>
            /// The Unit that is Moving.
            /// </summary>
            public Unit MovingUnit { get; set; }
            /// <summary>
            /// Hexes must pass all of these conditions to be included in the selection prompt.
            /// </summary>
            /// <remarks>
            /// Default if empty: <c>(Hex h) => { return true; }</c> <br></br>
            /// </remarks>
            public List<Board.FinalPathCondition> CustomFinalRestrictions { get; set; } = new();

            /// <summary>
            /// Hexes that pass any of these conditions will override and pass the <see cref="Hex.IsOccupiable"/> check and CustomFinalRestrictions.
            /// </summary>
            /// <remarks>
            /// Default if empty: <c>(Hex h) => { return false; }</c> <br></br>
            /// </remarks>
            public List<Board.FinalPathCondition> CustomFinalOverrides { get; set; } = new();

            /// <summary>
            /// If TRUE, This <see cref="Move"/> must be to one of the prompted hexes and cannot be cancelled.
            /// </summary>
            /// <remarks>
            /// Default: <c>false</c>
            /// </remarks>
            public virtual bool Forced { get; set; } = false;

            /// <remarks>
            /// - <see cref="CustomFinalRestrictions"/> <br></br>
            /// - <see cref="CustomFinalOverrides"/> <br></br>
            /// - <see cref="Forced"/>
            /// </remarks>
            /// <param name="performer"></param>
            /// <param name="movingUnit"></param>
            protected PromptArgs(Player performer, Unit movingUnit)
            {
                Performer = performer;
                MovingUnit = movingUnit;
            }

            /// <summary>
            /// [ : ] <see cref="PromptArgs"/>
            /// </summary>
            public class Pathed : PromptArgs
            {
                /// <summary>
                /// The (maximum) amount of Hexes that this Move can traverse.
                /// </summary>
                public int Distance { get; set; }
                /// <summary>
                /// The minimum amount of Hexes this <see cref="Move"/> can traverse.
                /// </summary>
                /// <remarks>
                /// Default: <c>0</c>
                /// </remarks>
                public int MinDistance { get; set; } = 0;

                /// <summary>
                /// This <see cref="Move"/> will ignore these collisions. <br></br>
                /// </summary>
                /// <remarks>
                /// Default: <c><see cref="ECollisionIgnoresF.None"/></c> <br></br>
                /// <i>Walls, Guarded Bases, and enemy Units will have collision (block pathing) by default. <br></br>
                /// (Wall Hexes and enemy Units are grouped as <see cref="ECollisionIgnoresF.Walls"/>) </i>
                /// </remarks>
                public ECollisionIgnoresF CollisionIgnores { get; set; } = ECollisionIgnoresF.None;

                /// <summary>
                /// This <see cref="Move"/> must respect (one of) the <see cref="EDirectionalsF"/> flags, relative to the <see cref="Vector3Int"/> position given. <br></br>
                /// </summary>
                /// <remarks>
                /// Default: <c>(<see cref="EDirectionalsF.None"/>, <see cref="Vector3Int.zero"/>)</c>
                /// </remarks>
                public (EDirectionalsF, Vector3Int) Directionals { get; set; } = (EDirectionalsF.None, Vector3Int.zero);

                /// <summary>
                /// Each step of this <see cref="Move"/> (from <see cref="Hex"/> <b>p</b> to <see cref="Hex"/> <b>n</b>) must pass all of these conditions to be a valid path. <br></br>
                /// <i>Ex: <c>(p, n) => p.Position.x &lt; n.Position.x;</c> <br></br>
                /// Every single step of this Move would need to be to a hex with a greater x coordinate.</i>
                /// </summary>
                /// <remarks>
                /// Default if empty: <c>(<see cref="Hex"/> p, <see cref="Hex"/> n) => { return true; }</c> <br></br>
                /// </remarks>
                public List<Board.ContinuePathCondition> CustomPathingRestrictions { get; set; } = new();

                /// <summary>
                /// Steps (from <see cref="Hex"/> <b>p</b> to <see cref="Hex"/> <b>n</b>) in this <see cref="Move"/> that pass this condition will always be a valid path, overriding all restrictions/collision. <br></br>
                /// <i>Ex: <c>(p, n) => p is <see cref="ControlHex"/>;</c> <br></br>
                /// Any step off of a Control Hex would be a valid path, ignoring all collision/restrictions.</i>
                /// </summary>
                /// <remarks>
                /// Default if empty: <c>(<see cref="Hex"/> p, <see cref="Hex"/> n) => { return false; }</c> <br></br>
                /// </remarks>
                public List<Board.ContinuePathCondition> CustomPathingOverrides { get; set; } = new();

                /// <summary>
                /// Flags for ignoring standard collision (Used in <see cref="CollisionIgnores"/>).
                /// </summary>
                /// <remarks>
                /// <c>None</c> : Does not ignore any collision, standard pathing. <br></br>
                /// <c>Walls</c> : Ignores <see cref="Hex.BlocksPathing"/> (treats as FALSE for all hexes), and ignores enemy Unit collision. <br></br>
                /// <c>Bases</c> : Ignores <see cref="BaseHex.IsGuarded"/> (treats as FALSE for all hexes). <i>(Not recommended for use)</i>
                /// </remarks>
                [Flags]
                public enum ECollisionIgnoresF : byte
                {
                    None = 0,
                    Walls = 1,
                    Bases = 2,
                }
                /// <summary>
                /// Flags for directional Moves (used in <see cref="Directionals"/>). <br></br>
                /// > Accompanied by a respected coordinate (<see cref="Vector3Int"/> <b>pos</b>).
                /// </summary>
                /// <remarks>
                /// <c>None</c> : This <see cref="Move"/> may be in any direction. <br></br>
                /// <c>Toward</c> : All steps in this <see cref="Move"/> must decrease the distance to <b>pos</b>. <br></br>
                /// <c>Away</c> : All steps in this <see cref="Move"/> must increase the distance to <b>pos</b>. <br></br>
                /// <c>Around</c> : All steps in this <see cref="Move"/> must not change the distance to <b>pos</b>.
                /// </remarks>
                [Flags]
                public enum EDirectionalsF : byte
                {
                    None = 0,
                    Toward = 1,
                    Away = 2,
                    Around = 4
                }

                /// <summary>
                /// Prompt <paramref name="performer"/> to Move <paramref name="movingUnit"/> up to <paramref name="distance"/> hexes with a valid path. <br></br>
                /// <i>(i.e. <see cref="PromptArgs"/> for a pathed, Basic or Directional Move)</i>
                /// </summary>
                /// <remarks>
                /// Additional Properties: <br></br>
                /// - <see cref="MinDistance"/> <br></br>
                /// - <see cref="CollisionIgnores"/> <br></br>
                /// - <see cref="Directionals"/> <br></br>
                /// - <see cref="CustomPathingRestrictions"/> <br></br>
                /// - <see cref="CustomPathingOverrides"/> <br></br>
                /// <inheritdoc cref="PromptArgs(Player, Unit)"/>
                /// </remarks>
                /// <param name="performer"></param>
                /// <param name="movingUnit"></param>
                /// <param name="distance"></param>
                public Pathed(Player performer, Unit movingUnit, int distance) : base(performer, movingUnit)
                {
                    Distance = distance;
                }
            }

            /// <summary>
            /// [ : ] <see cref="PromptArgs"/>
            /// </summary>
            public class Positional : PromptArgs
            {
                /// <summary>
                /// The position that PositionalOffsets will offset to get the final prompted positions. <br></br>
                /// <i>Ex: If moving BEHIND a <see cref="Unit"/> (<b>u</b>), <b>u</b>.Position would be the AnchorPosition.</i>
                /// </summary>
                /// <remarks>
                /// (See <see cref="PositionalOffsets"/>)
                /// </remarks>
                public Vector3Int AnchorPosition { get; set; }

                /// <summary>
                /// The set of offsets that are added to AnchorPosition to get the final prompted positions. <br></br>
                /// <i>Ex: If moving BEHIND a position, -<see cref="BoardCoords.up"/> would be the single element of PositionalOffsets.</i>
                /// </summary>
                /// <remarks>
                /// (See <see cref="AnchorPosition"/>)
                /// </remarks>
                public HashSet<Vector3Int> PositionalOffsets { get; set; }

                /// <summary>
                /// PositionalOffsets are rotated to this Team to match their perspective. <br></br>
                /// <i> Ex: If moving BEHIND a Red Unit, set this to <see cref="Player.ETeam.Red"/> so that the offset is rotated to match a Red Unit's view of behind.</i> <br></br>
                /// </summary>
                /// <remarks>
                /// <i>IN_FRONT/BEHIND are perspective dependent, what is one to Blue, is the other to Red.</i>
                /// </remarks>
                public Player.ETeam TeamRelativity { get; set; }

                /// <summary>
                /// <inheritdoc cref="PromptArgs.Forced"/>
                /// </summary>
                /// <remarks>
                /// Default: <c>true</c>
                /// </remarks>
                public override bool Forced { get; set; } = true;

                /// <summary>
                /// <see cref="PositionalOffsets"/> preset that includes all positions adjacent to to the anchor.
                /// </summary>
                public static IEnumerable<Vector3Int> ADJACENT => BoardCoords.GetAdjacent(Vector3Int.zero);
                /// <summary>
                /// <see cref="PositionalOffsets"/> preset that includes the single position that is in front of the anchor.
                /// </summary>
                public static IEnumerable<Vector3Int> IN_FRONT => new[] { BoardCoords.up };
                /// <summary>
                /// <see cref="PositionalOffsets"/> preset that includes the single position that is behind the anchor.
                /// </summary>
                public static IEnumerable<Vector3Int> BEHIND => new[] { -BoardCoords.up };

                /// <summary>
                /// Prompt <paramref name="performer"/> to Move <paramref name="movingUnit"/> to any position in <paramref name="positionalOffsets"/> relative to <paramref name="anchorPosition"/>, from the (rotational) perspective of <paramref name="teamRelativity"/>. <br></br>
                /// <i>(i.e. <see cref="PromptArgs"/> for a Positional Move)</i>
                /// </summary>
                /// <remarks>
                /// Additional Properties: <br></br>
                /// <inheritdoc cref="PromptArgs(Player, Unit)"/>
                /// </remarks>
                /// <param name="performer"></param>
                /// <param name="movingUnit"></param>
                /// <param name="anchorPosition"></param>
                /// <param name="positionalOffsets"></param>
                /// <param name="teamRelativity"></param>
                public Positional(Player performer, Unit movingUnit, Vector3Int anchorPosition, IEnumerable<Vector3Int> positionalOffsets, Player.ETeam teamRelativity) : base(performer, movingUnit)
                {
                    AnchorPosition = anchorPosition;
                    PositionalOffsets = new HashSet<Vector3Int>(positionalOffsets);
                    TeamRelativity = teamRelativity;
                }
            }
        }



        public override string ToString()
        {
            return $"<MOVE> {MovedUnit}: {FromPos} -> {ToPos}" + base.ToString();
        }

    }

}
