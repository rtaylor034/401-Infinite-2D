
using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using UnityEditor;
using UnityEngine;

public abstract class Ability
{
    
    public string Name { get; set; }
    public EAbilityType TypeIdentity { get; set; }

    public enum EAbilityType
    {
        Attack,
        Defense,
        Utility
    }

    public Ability(string name, EAbilityType typeIdentity)
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

        public Unsourced(string name, EAbilityType typeIdentity, IList<TargetCondition> targetConditions, Action<GameAction.PlayAbility> actionMethod)
            : base(name, typeIdentity)
        {
            Name = name;
            TypeIdentity = typeIdentity;
            TargetConditions = (targetConditions is null) ?
                new List<TargetCondition>() : new List<TargetCondition>(targetConditions);
            ActionMethod = actionMethod;
        }
    }
    

    public class Sourced : Ability
    {
        public delegate bool TargetingCondition(Player user, Unit source, Unit target);
        public HashSet<Vector3Int> HitArea { get; set; }
        public List<ConstructorTemplate<UnitEffect>> TargetEffects { get; set; }
        public TargetingCondition TargetCondition { get; set; }
        public Action<GameAction.PlayAbility> FollowUpMethod { get; set; }

        public static readonly TargetingCondition STANDARD_ATTACK = (p, _, t) => p.Team != t.Team;
        public static readonly TargetingCondition STANDARD_DEFENSE = (p, _, t) => p.Team == t.Team;
        public static readonly TargetingCondition STANDARD_COLLISION = (_, s, t) =>
        {
            bool IsCollision(Hex h)
            {
                return h.BlocksTargeting && (h.Occupant is null || h.Occupant.Team == s.Team);
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

        public Sourced(string name, EAbilityType typeIdentity, IList<ConstructorTemplate<UnitEffect>> targetEffects, IEnumerable<Vector3Int> hitArea, TargetingCondition targetCondition) :
            base(name, typeIdentity)
        {
            HitArea = new HashSet<Vector3Int>(hitArea);
            TargetEffects = new List<ConstructorTemplate<UnitEffect>>(targetEffects);
            TargetCondition = targetCondition;
        }

    }


}
