using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public partial class GameAction
{

    /// <summary>
    /// [ : ] <see cref="GameAction"/>
    /// </summary>
    public abstract class Move : GameAction
    {
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

        public abstract record Info
        {
            public HashSet<Unit> MovingUnits { get; private set; }
            public Predicate<Hex> FinalCondition { get; private set; }
            public Predicate<Hex> FinalOverride { get; private set; }

            public Info(IEnumerable<Unit> movingUnits)
            {
                MovingUnits = new(movingUnits);
            }
            public Info(params Unit[] movingUnits)
            {
                MovingUnits = new(movingUnits);
            }

            public record Positional : Info
            {
                public Vector3Int Anchor { get; private set; }
                public HashSet<Vector3Int> PositionOffsets { get; private set; }
            }

            public record Pathed : Info
            {
                public (int Min, int Max) DistanceRange { get; private set; }
                public Board.ContinuePathCondition PathingCondition { get; private set; }
                public Board.ContinuePathCondition PathingOverride { get; private set; }
                public Board.PathWeightFunction PathingWeightFunction { get; private set; }
            }
        }
    }
}
