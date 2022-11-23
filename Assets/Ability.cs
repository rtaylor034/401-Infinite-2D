
using System.Collections.Generic;
using UnityEngine;

public class Ability
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
        public Unsourced(string name, EAbilityType typeIdentity, SingleTargetCondition initialTargetCondition, IList<TargetCondition> followingTargetConditions = null)
            : this(name, typeIdentity, followingTargetConditions)
        {
            TargetConditions.Insert(0, (p, _, u) => initialTargetCondition(p, u));
        }

        private Unsourced(string name, EAbilityType typeIdentity, IList<TargetCondition> targetConditions)
            : base(name, typeIdentity)
        {
            Name = name;
            TypeIdentity = typeIdentity;
            TargetConditions = (targetConditions is null) ?
                new List<TargetCondition>() : new List<TargetCondition>(targetConditions);
        }
    }
    


    public class Sourced : Ability
    {
        public delegate bool TargetingCondition(Player user, Unit source, Unit target);
        public HashSet<Vector3Int> HitArea { get; set; }
        public List<ConstructorTemplate<UnitEffect>> TargetEffects { get; set; }
        public TargetingCondition TargetCondition { get; set; }

        public static readonly TargetingCondition STANDARD_ATTACK = (p, _, t) => p.Team != t.Team;
        public static readonly TargetingCondition STANDARD_DEFENSE = (p, _, t) => p.Team == t.Team;

        public Sourced(string name, EAbilityType typeIdentity, IList<ConstructorTemplate<UnitEffect>> targetEffects, IEnumerable<Vector3Int> hitArea, TargetingCondition targetCondition) :
            base(name, typeIdentity)
        {
            HitArea = new HashSet<Vector3Int>(hitArea);
            TargetEffects = new List<ConstructorTemplate<UnitEffect>>(targetEffects);
            TargetCondition = targetCondition;
        }

    }


}
