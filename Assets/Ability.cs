
using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using UnityEditor;
using UnityEngine;

public abstract class Ability
{
    
    public string Name { get; set; }
    public ETypeIdentity TypeIdentity { get; set; }

    public enum ETypeIdentity : byte
    {
        Attack,
        Defense,
        Utility, 
        Special
    }

    public Ability(string name, ETypeIdentity typeIdentity)
    {
        Name = name;
        TypeIdentity = typeIdentity;
    }

    public class Unsourced : Ability
    {
        public delegate bool TargetCondition(Player user, Unit previousTarget, Unit currentTarget);
        public delegate bool SingleTargetCondition(Player user, Unit target);
        public List<TargetCondition> TargetConditions { get; set; }
        public Action<GameAction.PlayAbility> ActionMethod { get; set; }

        public Unsourced(string name, ETypeIdentity typeIdentity, Action<GameAction.PlayAbility> actionMethod, SingleTargetCondition initialTargetCondition)
            : this(name, typeIdentity, actionMethod, initialTargetCondition, new TargetCondition[0]) { }

        public Unsourced(string name, ETypeIdentity typeIdentity, Action<GameAction.PlayAbility> actionMethod, SingleTargetCondition initialTargetCondition, TargetCondition[] secondaryTargetConditions)
            : base(name, typeIdentity)
        {
            Name = name;
            TypeIdentity = typeIdentity;
            TargetConditions = new()
            {
                (p, _, t) => initialTargetCondition(p, t)
            };
            TargetConditions.AddRange(secondaryTargetConditions);
            ActionMethod = actionMethod;
        }
    }
    

    public class Sourced : Ability
    {
        public delegate bool TargetingCondition(Player user, Unit source, Unit target);
        public delegate bool SourceCondition(Player user, Unit source);
        public HashSet<Vector3Int> HitArea { get; set; }
        public List<ConstructorTemplate<UnitEffect>> TargetEffects { get; set; }
        public List<SourceCondition> SourceConditions { get; set; }
        public List<TargetingCondition> TargetingConditions { get; set; }
        public Action<GameAction.PlayAbility> FollowUpMethod { get; set; }

        public static readonly SourceCondition STANDARD_VALID_SOURCE = (p, s) => p.Team == s.Team;
        public static readonly TargetingCondition STANDARD_ATTACK_TARGET = (p, _, t) => p.Team != t.Team;
        public static readonly TargetingCondition STANDARD_DEFENSE_TARGET = (p, _, t) => p.Team == t.Team;
        public static readonly TargetingCondition STANDARD_COLLISION = (p, s, t) =>
        {
            bool IsCollision(Hex h)
            {
                return h.BlocksTargeting && (h.Occupant is null || h.Occupant.Team == p.Team);
            }
            List<Vector3Int[]> edges;
            foreach(var hex in s.Board.HexesAt(BoardCoords.LineIntersections(s.Position, t.Position, out edges)))
            {
                if (IsCollision(hex)) return false;
            }
            foreach(var edgePair in edges)
            {
                foreach (var edge in edgePair)
                {
                    if (!IsCollision(s.Board.HexAt(edge))) continue;
                    return false;
                }
                    
            }
            return true;
        };

        public Sourced(string name, ETypeIdentity typeIdentity, ConstructorTemplate<UnitEffect>[] targetEffects, HashSet<Vector3Int> hitArea, TargetingCondition[] targetingConditions, SourceCondition[] sourceConditions) :
            base(name, typeIdentity)
        {
            HitArea = new HashSet<Vector3Int>(hitArea);
            TargetEffects = new List<ConstructorTemplate<UnitEffect>>(targetEffects);
            TargetingConditions = new(targetingConditions);
            SourceConditions = new(sourceConditions);
        }
        public Sourced(string name, ETypeIdentity typeIdentity, ConstructorTemplate<UnitEffect>[] targetEffects, HashSet<Vector3Int> hitArea, TargetingCondition[] targetingConditions) :
            this(name, typeIdentity, targetEffects, hitArea, targetingConditions, new SourceCondition[] { STANDARD_VALID_SOURCE })
        { }



    }


}
