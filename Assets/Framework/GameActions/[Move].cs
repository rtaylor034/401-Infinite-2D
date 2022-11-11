using Mono.Cecil;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public abstract partial class GameAction
{

    public class Move : GameAction
    {
        public static event GameActionEventHandler<Move> OnPerform;
        public static event Action<PromptArgs> OnPrompt;

        public Unit MovedUnit { get; private set; }
        public Vector3Int FromPos { get; private set; }
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


        public static void Declare(Player performer, Unit movedUnit, Vector3Int fromPos, Vector3Int ToPos)
        {
            FinalizeDeclare(new Move(performer, movedUnit, fromPos, ToPos));
        }

        public static void Prompt(PromptArgs args, Selector.SelectionConfirmMethod confirmMethod)
        {
            OnPrompt?.Invoke(args);
            Unit u = args.MovingUnit;

            if (args is PathArgs p)
            {
                GameManager.SELECTOR.Prompt(u.Board.PathFind(u.Position, (p.MinDistance, p.Distance), GetCombinedPathingCondition(p), h => (p.CustomFinalRestriction(h) && h.IsOccupiable) || p.CustomFinalOverride(h)), OnSelect);
            }
            if (args is PositionalArgs a)
            {

                HashSet<Hex> positions = u.Board.HexesAt(a.PositionalOffsets)
            }
            
            void OnSelect(Selector.SelectorArgs sel)
            {
                if (sel.Selection is Hex s)
                {
                    Declare(args.Performer, u, u.Position, s.Position);
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
            args.CustomPathingRestriction(p, n))
            ||
            args.CustomPathingOverride(p, n);

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
            public Player Performer { get; set; }
            public Unit MovingUnit { get; set; }
            public Board.FinalPathCondition CustomFinalRestriction { get; set; } = _ => true;
            public Board.FinalPathCondition CustomFinalOverride { get; set; } = _ => false;

            protected PromptArgs(Player performer, Unit movingUnit)
            {
                Performer = performer;
                MovingUnit = movingUnit;
            }
        }
        public class PathArgs : PromptArgs
        {
            public int Distance { get; set; }
            public int MinDistance { get; set; } = 0;
            public ECollisionIgnoresF CollisionIgnores { get; set; } = ECollisionIgnoresF.None;
            public (EDirectionalsF, Vector3Int) Directionals { get; set; } = (EDirectionalsF.None, Vector3Int.zero);

            //Will stop pathing if return FALSE
            public Board.ContinuePathCondition CustomPathingRestriction { get; set; } = (_, _) => true;
            //Will always allow pathing if return TRUE (overrides all restrictions)
            public Board.ContinuePathCondition CustomPathingOverride { get; set; } = (_, _) => false;

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
