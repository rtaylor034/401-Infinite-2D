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
    /// <summary>
    /// [ : ] <see cref="GameAction"/>
    /// </summary>
    public class Move : GameAction
    {
        /// <summary>
        /// Occurs when any <see cref="Move"/> is performed. <br></br>
        /// </summary>
        public static event GameActionEventHandler<Move> OnPerform;

        /// <summary>
        /// Occurs when any <see cref="Move"/> is prompted using <see cref="Prompt(PromptArgs, Action{Move})"/>. <br></br>
        /// </summary>
        /// <remarks>
        /// <i>Modifications to the <see cref="PromptArgs"/> will be applied to the Prompt() call.</i>
        /// </remarks>
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


        /// <summary>
        /// Moves <paramref name="movedUnit"/> from <paramref name="fromPos"/> to <paramref name="toPos"/>, by <paramref name="performer"/>. <br></br>
        /// > Unless you are creating a Move that already happened, use <b><see cref="Prompt(PromptArgs, Action{Move})"/></b>.
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
            OnPerform?.Invoke(this);
        }
        protected override void InternalUndo()
        {
            MovedUnit.UpdatePosition(FromPos);
        }

        /// <summary>
        /// Prompts to create a <see cref="Move"/> action based on <paramref name="args"/>. <br></br>
        /// > Calls <paramref name="confirmCallback"/> with the created <see cref="Move"/> when a selection is made.
        /// </summary>
        /// <remarks>
        /// <paramref name="confirmCallback"/> will not be called if no <see cref="Move"/> is created. <br></br>
        /// <i>(Selection was cancelled or was invalid)</i>
        /// </remarks>
        /// <param name="args"></param>
        /// <param name="confirmCallback"></param>
        public static void Prompt(PromptArgs args, Action<GameAction.Move> confirmCallback)
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
                    confirmCallback?.Invoke(new(args.Performer, u, u.Position, s.Position));
                }
                if (sel.WasCancelled && args.Forced)
                {
                    if (!sel.WasEmpty)
                    {
                        Debug.Log("you cannot cancel a forced move");
                        Prompt(args, confirmCallback);
                        return;
                    }
                    //TODO FUTURE: Add some sort of Validate or Check function for a PromptArgs to see if that Move would be possible.
                    //Ex: If a card has a forced Move, it should validate the move before it tries to prompt it, so that if validation fails, the card is unplayable. (although it could also be ignored idk.)
                    Debug.LogError("[!!!] Forced Move was prompted, but no Hexes were available.");
                }

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

        /// <summary>
        /// <b>abstract</b>
        /// </summary>
        /// <remarks>
        /// (See <see cref="PathArgs"/>, <see cref="PositionalArgs"/>)
        /// </remarks>
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
        }

        /// <summary>
        /// [ : ] <see cref="PromptArgs"/>
        /// </summary>
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
            public PathArgs(Player performer, Unit movingUnit, int distance) : base(performer, movingUnit)
            {
                Distance = distance;
            }
        }

        /// <summary>
        /// [ : ] <see cref="PromptArgs"/>
        /// </summary>
        public class PositionalArgs : PromptArgs
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
            public PositionalArgs(Player performer, Unit movingUnit, Vector3Int anchorPosition, IEnumerable<Vector3Int> positionalOffsets, Player.ETeam teamRelativity) : base(performer, movingUnit)
            {
                AnchorPosition = anchorPosition;
                PositionalOffsets = new HashSet<Vector3Int>(positionalOffsets);
                TeamRelativity = teamRelativity;
            }
        }

        public override string ToString()
        {
            return $"<MOVE>: {MovedUnit}: {FromPos} -> {ToPos}" + base.ToString();
        }

    }

}
