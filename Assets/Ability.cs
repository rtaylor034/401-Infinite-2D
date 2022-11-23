
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
        public delegate bool DTargetCondition(Player user, Unit previousTarget, Unit currentTDarget);
        public delegate bool DSingleTargetCondition(Player user, Unit target);
        public List<DTargetCondition> TargetConditions { get; set; }
        public Unsourced(string name, EAbilityType typeIdentity, DSingleTargetCondition initialTargetCondition, IList<DTargetCondition> followingTargetConditions = null)
            : this(name, typeIdentity, followingTargetConditions)
        {
            TargetConditions.Insert(0, (p, _, u) => initialTargetCondition(p, u));
        }

        private Unsourced(string name, EAbilityType typeIdentity, IList<DTargetCondition> targetConditions)
            : base(name, typeIdentity)
        {
            Name = name;
            TypeIdentity = typeIdentity;
            TargetConditions = (targetConditions is null) ?
                new List<DTargetCondition>() : new List<DTargetCondition>(targetConditions);
        }
    }
    


    public class Sourced : Ability
    {
        public delegate bool DTargetingCondition(Player user, Unit source, Unit target);
        public HashSet<Vector3Int> HitArea { get; set; }
        public List<ConstructorTemplate<UnitEffect>> TargetEffects { get; set; }
        public DTargetingCondition TargetingCondition { get; set; }

        public static readonly DTargetingCondition STANDARD_ATTACK = (p, _, t) => p.Team != t.Team;
        public static readonly DTargetingCondition STANDARD_DEFENSE = (p, _, t) => p.Team == t.Team;

        public Sourced(string name, EAbilityType typeIdentity, IList<ConstructorTemplate<UnitEffect>> targetEffects, IEnumerable<Vector3Int> hitArea, DTargetingCondition targetCondition) :
            base(name, typeIdentity)
        {
            HitArea = new HashSet<Vector3Int>(hitArea);
            TargetEffects = new List<ConstructorTemplate<UnitEffect>>(targetEffects);
            TargetingCondition = targetCondition;
        }

    }


}
