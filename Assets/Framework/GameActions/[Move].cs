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
            Unit u = args.movingUnit;
            GameManager.SELECTOR.Prompt(u.Board.PathFind(u.Position, (args.minDistance, args.distance), GetCombinedContinueCondition(args), h => (args.customFinalPathRestriction(h) && h.IsOccupiable) || args.customFinalPathOverride(h)), OnSelect);
            
            void OnSelect(Selector.SelectorArgs sel)
            {
                if (sel.Selection is Hex s)
                {
                    Declare(args.performer, u, u.Position, s.Position);
                }

                confirmMethod?.Invoke(sel);
            }
        }

        private static Board.ContinuePathCondition GetCombinedContinueCondition(PromptArgs args)
        {
            Unit u = args.movingUnit;
            PromptArgs.ECollisionIgnoresF ci = args.collisionIgnores;
            (PromptArgs.EDirectionalsF, Vector3Int) dir = args.directionals;

            Board.ContinuePathCondition condition = args.customPathingRestriction;

            //Collision conditions (<Cond> && <col> && <col>...))
            if (!ci.HasFlag(PromptArgs.ECollisionIgnoresF.Walls)) condition = (p, n) => condition(p, n) && HexCollision(p, n) && OpposingUnitCollision(u)(p, n);
            if (!ci.HasFlag(PromptArgs.ECollisionIgnoresF.Bases)) condition = (p, n) => condition(p, n) && GuardedBaseCollision(u)(p, n);

            //Directional conditions (<Cond> && (<dir> || <dir>...))
            if (dir.Item1 != PromptArgs.EDirectionalsF.None)
            {
                Board.ContinuePathCondition direction = (prev, next) => false;
                Vector3Int dpos = dir.Item2;
                if (!dir.Item1.HasFlag(PromptArgs.EDirectionalsF.Toward)) direction = (p, n) => direction(p, n) || DirectionalToward(dpos)(p, n);
                if (!dir.Item1.HasFlag(PromptArgs.EDirectionalsF.Away)) direction = (p, n) => direction(p, n) || DirectionalAway(dpos)(p, n);
                if (!dir.Item1.HasFlag(PromptArgs.EDirectionalsF.Around)) direction = (p, n) => direction(p, n) || DirectionalAround(dpos)(p, n);
                condition = (p, n) => condition(p, n) && direction(p, n);
            }

            //Pathing override
            condition = (p, n) => condition(p, n) || args.customPathingOverride(p, n);
            return condition;
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

        public struct PromptArgs
        {
            public Player performer;
            public Unit movingUnit;
            public int distance;
            public int minDistance;
            public ECollisionIgnoresF collisionIgnores;
            public (EDirectionalsF, Vector3Int) directionals;

            //Will stop pathing if return FALSE
            public Board.ContinuePathCondition customPathingRestriction;
            //Will always allow pathing if return TRUE (overrides all restrictions)
            public Board.ContinuePathCondition customPathingOverride;
            public Board.FinalPathCondition customFinalPathRestriction;
            public Board.FinalPathCondition customFinalPathOverride;

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
            public PromptArgs(Player performer, Unit movingUnit, int distance)
            {
                this.performer = performer;
                this.movingUnit = movingUnit;
                this.distance = distance;
                minDistance = 0;
                collisionIgnores = ECollisionIgnoresF.None;
                directionals = (EDirectionalsF.None, Vector3Int.zero);
                customPathingOverride = (_, _) => false;
                customPathingRestriction = (_, _) => true;
                customFinalPathRestriction = _ => true;
                customFinalPathOverride = _ => true;
            }
            

        }

        

        

    }

}
