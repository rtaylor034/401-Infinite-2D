using Mono.Cecil;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

public abstract partial class GameAction
{

    public class Move : GameAction
    {
        public static event GameActionEventHandler<Move> OnPerform;
        public static event Action<PromptArgs> OnPrompt;

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

        /// <summary>
        /// Declare a <see cref="Move"/>, Moving <paramref name="movedUnit"/> from <paramref name="fromPos"/> to <paramref name="toPos"/>, by <paramref name="performer"/>. <br></br>
        /// > Unless you are declaring a Move that already happened, use <b><see cref="Prompt(PromptArgs, Selector.SelectionConfirmMethod)"/></b>.
        /// </summary>
        /// <remarks>
        /// <i>Declare() is called within Prompt().</i>
        /// </remarks>
        /// <param name="performer"></param>
        /// <param name="movedUnit"></param>
        /// <param name="fromPos"></param>
        /// <param name="toPos"></param>
        public static void Declare(Player performer, Unit movedUnit, Vector3Int fromPos, Vector3Int toPos)
        {
            FinalizeDeclare(new Move(performer, movedUnit, fromPos, toPos));
        }

        /// <summary>
        /// Prompts a <see cref="Move"/> based on <paramref name="args"/>, and then runs <paramref name="confirmMethod"/> with the <see cref="Selector.SelectorArgs"/> of the selected Move position. <br></br>
        /// </summary>
        /// <param name="args"></param>
        /// <param name="confirmMethod"></param>
        public static void Prompt(PromptArgs args, Selector.SelectionConfirmMethod confirmMethod)
        {
            OnPrompt?.Invoke(args);
            Unit u = args.MovingUnit;
            bool FinalCondition(Hex h) => GetCombinedFinalConditon(args)(h);

            IEnumerable<Selectable> possibleHexes = 
                (args is PathArgs p)        ? u.Board.PathFind(u.Position,(p.MinDistance, p.Distance), GetCombinedPathingCondition(p), FinalCondition):
                (args is PositionalArgs a)  ? u.Board.HexesAt(GetPositionalPositions(a)).Where(FinalCondition):
                throw new ArgumentException("PromptArgs not recognized?");

            if (possibleHexes.IsSingleElement(out var single) && args.Forced) GameManager.SELECTOR.SpoofSelection(single, OnSelect);
            GameManager.SELECTOR.Prompt(possibleHexes, OnSelect);
            
            void OnSelect(Selector.SelectorArgs sel)
            {
                if (sel.Selection is Hex s)
                {
                    Declare(args.Performer, u, u.Position, s.Position);
                }
                if (sel.WasCancelled && args.Forced)
                {
                    if (!sel.WasEmpty)
                    {
                        Debug.Log("you cannot cancel a forced move");
                        Prompt(args, confirmMethod);
                        return;
                    }
                    //TODO FUTURE: Add some sort of Validate or Check function for a PromptArgs to see if that Move would be possible.
                    //Ex: If a card has a forced Move, it should validate the move before it tries to prompt it, so that if validation fails, the card is unplayable. (although it could also be ignored idk.)
                    Debug.LogError("[!!!] Forced Move was prompted, but no Hexes were available.");
                }

                confirmMethod?.Invoke(sel);
            }
        }

        private static Board.ContinuePathCondition GetCombinedPathingCondition(PathArgs args)
        {
            Unit u = args.MovingUnit;
            PathArgs.ECollisionIgnoresF ci = args.CollisionIgnores;
            (PathArgs.EDirectionalsF, Vector3Int) dir = args.Directionals;
            var dpos = dir.Item2;

            return (p, n) =>
            ((ci.HasFlag(PathArgs.ECollisionIgnoresF.Walls) || (HexCollision(p, n) && OpposingUnitCollision(u)(p, n))) &&
            (ci.HasFlag(PathArgs.ECollisionIgnoresF.Bases) || GuardedBaseCollision(u)(p, n))
            &&
            ((dir.Item1 == PathArgs.EDirectionalsF.None) ||
            (dir.Item1.HasFlag(PathArgs.EDirectionalsF.Away) && DirectionalAway(dpos)(p, n)) ||
            (dir.Item1.HasFlag(PathArgs.EDirectionalsF.Toward) && DirectionalToward(dpos)(p, n)) ||
            dir.Item1.HasFlag(PathArgs.EDirectionalsF.Around) && DirectionalAround(dpos)(p, n))
            &&
            Combined(args.CustomPathingRestrictions)(p, n))
            ||
            Combined(args.CustomPathingOverrides, true)(p, n);

            Board.ContinuePathCondition Combined(IEnumerable<Board.ContinuePathCondition> cond, bool invert = false)
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
            return (h) => (Combined(args.CustomFinalRestrictions)(h) && h.IsOccupiable) || Combined(args.CustomFinalOverrides, true)(h);

            Board.FinalPathCondition Combined(IEnumerable<Board.FinalPathCondition> cond, bool invert = false)
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
        private static HashSet<Vector3Int> GetPositionalPositions(PositionalArgs args)
        {
            HashSet<Vector3Int> o = new HashSet<Vector3Int>();
            foreach (var pos in args.PositionalOffsets)
            {
                o.Add(pos + args.AnchorPosition);
            }
            BoardCoords.Rotate(o, args.AnchorPosition, Player.PerspectiveRotationOf(args.TeamRelativity));
            return o;
        }

        #region Standard Collision Conditions
        private static Board.ContinuePathCondition HexCollision => (_, next) =>
        !next.BlocksPathing;
        private static Func<Unit, Board.ContinuePathCondition> OpposingUnitCollision => u => (_, next) =>
        !(next.Occupant != null && next.Occupant.Team != u.Team);
        private static Func<Unit, Board.ContinuePathCondition> GuardedBaseCollision => u => (_, next) =>
        !(next is BaseHex bhex && bhex.Team != u.Team);

        #endregion
        #region Standard Directional Conditions
        private static Func<Vector3Int, Board.ContinuePathCondition> DirectionalAway => pos => (prev, next) =>
        BoardCoords.RadiusBetween(pos, prev.Position) < BoardCoords.RadiusBetween(pos, next.Position);
        private static Func<Vector3Int, Board.ContinuePathCondition> DirectionalAround => pos => (prev, next) =>
        BoardCoords.RadiusBetween(pos, prev.Position) == BoardCoords.RadiusBetween(pos, next.Position);
        private static Func<Vector3Int, Board.ContinuePathCondition> DirectionalToward => pos => (prev, next) =>
        BoardCoords.RadiusBetween(pos, prev.Position) > BoardCoords.RadiusBetween(pos, next.Position);

        #endregion

        public abstract class PromptArgs
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
            public virtual bool Forced { get; set; } = false;

            protected PromptArgs(Player performer, Unit movingUnit)
            {
                Performer = performer;
                MovingUnit = movingUnit;
            }
        }
        public class PathArgs : PromptArgs
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
            /// This <see cref="Move"/> will these ignore collisions.
            /// </summary>
            public ECollisionIgnoresF CollisionIgnores { get; set; } = ECollisionIgnoresF.None;
            public (EDirectionalsF, Vector3Int) Directionals { get; set; } = (EDirectionalsF.None, Vector3Int.zero);

            //Will stop pathing if return FALSE
            public List<Board.ContinuePathCondition> CustomPathingRestrictions { get; set; } = new();
            //Will always allow pathing if return TRUE (overrides all restrictions)
            public List<Board.ContinuePathCondition> CustomPathingOverrides { get; set; } = new();

            [Flags]
            public enum ECollisionIgnoresF : byte
            {
                None = 0,
                Walls = 1,
                Bases = 2,
            }
            [Flags]
            public enum EDirectionalsF : byte
            {
                None = 0,
                Toward = 1,
                Away = 2,
                Around = 4
            }
            public PathArgs(Player performer, Unit movingUnit, int distance) : base(performer, movingUnit)
            {
                Distance = distance;
            }
        }
        public class PositionalArgs : PromptArgs
        {
            public Vector3Int AnchorPosition { get; set; }
            public IEnumerable<Vector3Int> PositionalOffsets { get; set; }
            public Player.ETeam TeamRelativity { get; set; }
            public override bool Forced { get; set; } = true;

            public static IEnumerable<Vector3Int> ADJACENT => BoardCoords.GetAdjacent(Vector3Int.zero);
            public static IEnumerable<Vector3Int> IN_FRONT => new[] { BoardCoords.up };
            public static IEnumerable<Vector3Int> BEHIND => new[] { -BoardCoords.up };

            public PositionalArgs(Player performer, Unit movingUnit, Vector3Int anchorPosition, IEnumerable<Vector3Int> positionalOffset, Player.ETeam teamRelativity) : base(performer, movingUnit)
            {
                AnchorPosition = anchorPosition;
                PositionalOffsets = positionalOffset;
                TeamRelativity = teamRelativity;
            }
        }

        public override string ToString()
        {
            return $"<MOVE>: {MovedUnit}: {FromPos} -> {ToPos}" + base.ToString();
        }

    }

}
