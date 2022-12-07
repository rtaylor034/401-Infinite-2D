
using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using UnityEditor;
using UnityEngine;

/// <summary>
/// <b>abstract</b> <br></br>
/// (See <see cref="Ability.Sourced"/>, <see cref="Ability.Unsourced"/>)
/// </summary>
public abstract class Ability
{
    /// <summary>
    /// [Delegate]
    /// </summary>
    /// <remarks>
    /// <c><see langword="void"/> PlayActionMethod(<see cref="GameAction.PlayAbility"/> <paramref name="action"/>) { }</c> <br></br>
    /// <i><paramref name="action"/> = the <see cref="GameAction.PlayAbility"/> that played this ability.</i>
    /// </remarks>
    /// <param name="action"></param>
    public delegate void PlayAction(GameAction.PlayAbility action);

    /// <summary>
    /// A <see cref="PlayAction"/> that does nothing.<br></br>
    /// <i>If a <see cref="Sourced"/> has no Follow Up, set it to this.</i>
    /// </summary>
    /// <remarks>
    /// <c>_ => { };</c>
    /// </remarks>
    public readonly static PlayAction NO_ACTION = _ => { };
    /// <summary>
    /// The display name of this ability.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The type that this ability identifies as. <br></br>
    /// > This does not affect the internal behavior of this ability.
    /// </summary>
    public ETypeIdentity TypeIdentity { get; set; }

    /// <summary>
    /// Enum for <see cref="TypeIdentity"/>.
    /// </summary>
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

    /// <summary>
    /// [ : ] <see cref="Ability"/> <br></br>
    /// <see cref="Unsourced"/> abilities are played without a Source.
    /// </summary>
    public class Unsourced : Ability
    {
        /// <summary>
        /// [Delegate]
        /// </summary>
        /// <remarks>
        /// <c><see cref="bool"/> TargetConditionMethod(<see cref="Player"/> <paramref name="user"/>, <see cref="Unit"/> <paramref name="previousTarget"/>, <see cref="Unit"/> <paramref name="currentTarget"/>) { }</c> <br></br>
        /// <i><paramref name="user"/> : The <see cref="Player"/> that played the ability. <br></br>
        /// <paramref name="previousTarget"/> : The <see cref="Unit"/> that was selected as a Target before this one. <br></br>
        /// <paramref name="currentTarget"/> : The <see cref="Unit"/> being evaluated as a valid/invalid Target. <br></br>
        /// <see langword="return"/> : Whether or not <paramref name="currentTarget"/> is a valid Target.
        /// </i></remarks>
        /// <param name="user"></param>
        /// <param name="previousTarget"></param>
        /// <param name="currentTarget"></param>
        public delegate bool TargetCondition(Player user, Unit previousTarget, Unit currentTarget);

        /// <summary>
        /// [Delegate] <br></br>
        /// Same as <see cref="TargetCondition"/>, but <paramref name="previousTarget"/> is discarded.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public delegate bool SingleTargetCondition(Player user, Unit target);

        /// <summary>
        /// in order for a Target to be valid, it must pass it's respective <see cref="TargetCondition"/>. <br></br>
        /// > The size of this list determines how many Targets this ability has.
        /// </summary>
        /// <remarks>
        ///<inheritdoc cref="TargetCondition"/> <br></br> <br></br>
        /// TargetConditions[0] is generated from a <see cref="SingleTargetCondition"/> specified in the constructor.
        /// </remarks>
        public List<TargetCondition> TargetConditions { get; set; }
        /// <summary>
        /// The main action method of this ability. <br></br>
        /// > Called when this ability is played.
        /// </summary>
        /// <remarks>
        /// <inheritdoc cref="PlayAction"/>
        /// </remarks>
        public PlayAction ActionMethod { get; set; }

        /// <inheritdoc cref="Unsourced(string, ETypeIdentity, SingleTargetCondition, TargetCondition[], PlayAction)"/>
        /// <remarks>
        /// > This constructor is meant for abilities with only 1 Target. <br></br>
        /// (Only <paramref name="initialTargetCondition"/> needs to be specified)
        /// </remarks>
        public Unsourced(string name, ETypeIdentity typeIdentity, SingleTargetCondition initialTargetCondition, PlayAction actionMethod)
            : this(name, typeIdentity, initialTargetCondition, new TargetCondition[0], actionMethod) { }

        /// <summary>
        /// Creates an <see cref="Unsourced"/> called <paramref name="name"/> of type <paramref name="typeIdentity"/>.<br></br>
        /// - Its primary (first) target must respect <paramref name="initialTargetCondition"/>. <br></br>
        /// - All following targets must respect the <paramref name="secondaryTargetConditions"/> in order. <i>(See <see cref="TargetConditions"/>)</i><br></br>
        /// - When the ability is played, <paramref name="actionMethod"/> will be called with the play <see cref="GameAction"/>. <i>(Chosen targets are <see cref="GameAction.PlayAbility.ParticipatingUnits"/>)</i>
        /// </summary>
        /// <param name="name"></param>
        /// <param name="typeIdentity"></param>
        /// <param name="initialTargetCondition"></param>
        /// <param name="secondaryTargetConditions"></param>
        /// <param name="actionMethod"></param>
        public Unsourced(string name, ETypeIdentity typeIdentity, SingleTargetCondition initialTargetCondition, TargetCondition[] secondaryTargetConditions, PlayAction actionMethod)
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
    
    /// <summary>
    /// [ : ] <see cref="Ability"/> <br></br>
    /// <see cref="Sourced"/> abilities require a Source to be played from.
    /// </summary>
    public class Sourced : Ability
    {
        /// <summary>
        /// [Delegate]
        /// </summary>
        /// <remarks>
        /// <c><see cref="bool"/> TargetingConditionMethod(<see cref="Player"/> <paramref name="user"/>, <see cref="Unit"/> <paramref name="source"/>, <see cref="Unit"/> <paramref name="target"/>) { }</c> <br></br>
        /// <i><paramref name="user"/> : The <see cref="Player"/> that played the ability. <br></br>
        /// <paramref name="source"/> : The <see cref="Unit"/> that was selected as the Source. <br></br>
        /// <paramref name="target"/> : The <see cref="Unit"/> being evaluated as a valid/invalid Target. <br></br>
        /// <see langword="return"/> : Whether or not <paramref name="currentTarget"/> is a valid Target.
        /// </i></remarks>
        /// <param name="user"></param>
        /// <param name="source"></param>
        /// <param name="target"></param>
        public delegate bool TargetingCondition(Player user, Unit source, Unit target);
        /// <summary>
        /// [Delegate]
        /// </summary>
        /// <remarks>
        /// <c><see cref="bool"/> SourceConditionMethod(<see cref="Player"/> <paramref name="user"/>, <see cref="Unit"/> <paramref name="source"/>) { }</c> <br></br>
        /// <i><paramref name="user"/> : The <see cref="Player"/> that played the ability. <br></br>
        /// <paramref name="source"/> : The <see cref="Unit"/> being evaluated as a valid/invalid Source. <br></br>
        /// <see langword="return"/> : Whether or not <paramref name="source"/> is a valid Source.
        /// </i></remarks>
        /// <param name="user"></param>
        /// <param name="source"></param>
        public delegate bool SourceCondition(Player user, Unit source);
        /// <summary>
        /// The positions that Units can occupy to be a valid Target, relative to the Source.
        /// </summary>
        public HashSet<Vector3Int> HitArea { get; set; }
        /// <summary>
        /// The effects that this ability inflicts upon the Target when played.
        /// </summary>
        public List<ConstructorTemplate<UnitEffect>> TargetEffects { get; set; }
        /// <summary>
        /// A <see cref="Unit"/> must pass ALL of these conditions in order to be a valid Source.
        /// </summary>
        /// <remarks>
        /// <inheritdoc cref="SourceCondition"/>
        /// </remarks>
        public List<SourceCondition> SourceConditions { get; set; }
        /// <summary>
        /// A <see cref="Unit"/> must pass ALL of these conditions in order to be a valid Target.
        /// </summary>
        /// <remarks>
        /// <inheritdoc cref="TargetingCondition"/> <br></br> <br></br>
        /// <i>Units will only be evaluated if they are within the Hit Area of the ability.</i>
        /// </remarks>
        public List<TargetingCondition> TargetingConditions { get; set; }
        /// <summary>
        /// The Follow Up method of this ability. <br></br>
        /// > Called when this ability is played. <br></br>
        /// (<i>If this ability has no Follow Up, set to <see cref="NO_ACTION"/>)</i>
        /// </summary>
        /// <remarks>
        /// <inheritdoc cref="PlayAction"/>
        /// </remarks>
        public PlayAction FollowUpMethod { get; set; }

        /// <summary>
        /// The standard <see cref="SourceCondition"/> that all Sourced abilities implicitly have. (Unless explicitly ommitted)
        /// </summary>
        /// <remarks>
        /// <c>(p, s) => p.Team == s.Team;</c>
        /// </remarks>
        public static readonly SourceCondition STANDARD_VALID_SOURCE = (p, s) => p.Team == s.Team;
        /// <summary>
        /// The standard <see cref="TargetingCondition"/> that all Attacks should have.
        /// </summary>
        /// <remarks>
        /// <c>(p, _, t) => p.Team != t.Team;</c>
        /// </remarks>
        public static readonly TargetingCondition STANDARD_ATTACK_TARGET = (p, _, t) => p.Team != t.Team;
        /// <summary>
        /// The standard <see cref="TargetingCondition"/> that all Defenses should have.
        /// </summary>
        /// <remarks>
        /// <c>(p, _, t) => p.Team == t.Team;</c>
        /// </remarks>
        public static readonly TargetingCondition STANDARD_DEFENSE_TARGET = (p, _, t) => p.Team == t.Team;
        /// <summary>
        /// The standard <see cref="TargetingCondition"/> that detects for collision inbetween the Source and Target.
        /// </summary>
        /// <remarks>
        /// <c>(p, s, t) => ...</c> <br></br>
        /// <i>Uses <see cref="BoardCoords.LineIntersections(Vector3Int, Vector3Int, out List{Vector3Int[]})"/> <br></br>
        /// and returns <see langword="false"/> if any Hexes have <see cref="Hex.BlocksTargeting"/> or have opposing-team Occupants.</i>
        /// </remarks>
        public static readonly TargetingCondition STANDARD_COLLISION = (p, s, t) =>
        {
            bool __IsCollision(Hex h)
            {
                return h.BlocksTargeting && (h.Occupant is null || h.Occupant.Team == p.Team);
            }
            List<Vector3Int[]> edges;
            foreach(var hex in s.Board.HexesAt(BoardCoords.LineIntersections(s.Position, t.Position, out edges)))
            {
                if (__IsCollision(hex)) return false;
            }
            foreach(var edgePair in edges)
            {
                foreach (var edge in edgePair)
                {
                    if (!__IsCollision(s.Board.HexAt(edge))) continue;
                    return false;
                }
                    
            }
            return true;
        };

        /// <summary>
        /// Creates a <see cref="Sourced"/> called <paramref name="name"/> of type <paramref name="typeIdentity"/>. <br></br>
        /// - It has a Hit Area of <paramref name="hitArea"/> and inflicts <paramref name="targetEffects"/> onto it's Target. <br></br>
        /// - In addition to inflicting effects, <paramref name="followUpMethod"/> is called when the ability is played. <br></br>
        /// - Targets must pass ALL <paramref name="targetingConditions"/> in addition to being inside of the Hit Area in order to be valid. <br></br>
        /// - Units must also pass ALL <paramref name="sourceConditions"/> in order to be a valid source.
        /// </summary>
        /// <remarks>
        /// <i>See <see cref="Sourced"/>.STANDARD conditions (such as <see cref="STANDARD_ATTACK_TARGET"/>)</i>
        /// </remarks>
        /// <param name="name"></param>
        /// <param name="typeIdentity"></param>
        /// <param name="targetEffects"></param>
        /// <param name="hitArea"></param>
        /// <param name="followUpMethod"></param>
        /// <param name="targetingConditions"></param>
        /// <param name="sourceConditions"></param>
        public Sourced(string name, ETypeIdentity typeIdentity, ConstructorTemplate<UnitEffect>[] targetEffects, HashSet<Vector3Int> hitArea, PlayAction followUpMethod, TargetingCondition[] targetingConditions, SourceCondition[] sourceConditions) :
            base(name, typeIdentity)
        {
            HitArea = new HashSet<Vector3Int>(hitArea);
            TargetEffects = new List<ConstructorTemplate<UnitEffect>>(targetEffects);
            TargetingConditions = new(targetingConditions);
            SourceConditions = new(sourceConditions);
            FollowUpMethod = followUpMethod;
        }
        ///<summary>
        ///<inheritdoc cref="Sourced.Sourced(string, ETypeIdentity, ConstructorTemplate{UnitEffect}[], HashSet{Vector3Int}, PlayAction, TargetingCondition[], SourceCondition[])"/>
        /// </summary>
        /// <remarks>
        /// <inheritdoc cref="Sourced.Sourced(string, ETypeIdentity, ConstructorTemplate{UnitEffect}[], HashSet{Vector3Int}, PlayAction, TargetingCondition[], SourceCondition[])"/> <br></br> <br></br>
        /// >This constructor assumes that <paramref name="sourceConditions"/> only include <see cref="STANDARD_VALID_SOURCE"/>.
        /// </remarks>
        /// <param name="name"></param>
        /// <param name="typeIdentity"></param>
        /// <param name="targetEffects"></param>
        /// <param name="hitArea"></param>
        /// <param name="followUpMethod"></param>
        /// <param name="targetingConditions"></param>
        public Sourced(string name, ETypeIdentity typeIdentity, ConstructorTemplate<UnitEffect>[] targetEffects, HashSet<Vector3Int> hitArea, PlayAction followUpMethod, TargetingCondition[] targetingConditions) :
            this(name, typeIdentity, targetEffects, hitArea, followUpMethod, targetingConditions, new SourceCondition[] { STANDARD_VALID_SOURCE })
        { }



    }


}
