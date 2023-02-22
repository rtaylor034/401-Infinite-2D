
using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
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
    /// <c>(<see langword="async"/>) <see cref="Task"/> PlayActionMethod(<see cref="GameAction.PlayAbility"/> <paramref name="action"/>) { }</c> <br></br>
    /// - <paramref name="action"/> : the <see cref="GameAction.PlayAbility"/> that played this ability.
    /// </remarks>
    /// <param name="action"></param>
    public delegate Task PlayAction(GameAction.PlayAbility action);

    /// <summary>
    /// A <see cref="PlayAction"/> that does nothing.<br></br>
    /// <i>If a <see cref="Sourced"/> has no Follow Up, set it to this.</i>
    /// </summary>
    /// <remarks>
    /// <c>_ => { };</c>
    /// </remarks>
    public readonly static PlayAction NO_ACTION = _ => Task.CompletedTask;
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

    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// - <see cref="Name"/> (Set by constructor)<br></br>
    /// - <see cref="TypeIdentity"/> (Set by constructor)
    /// </remarks>
    /// <param name="name"></param>
    /// <param name="typeIdentity"></param>
    protected Ability(string name, ETypeIdentity typeIdentity)
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
        /// - <paramref name="user"/> : The <see cref="Player"/> that played the ability. <br></br>
        /// - <paramref name="previousTarget"/> : The <see cref="Unit"/> that was selected as a Target before this one. <br></br>
        /// <i>(<see langword="null"/> if there was no previous Target)</i><br></br>
        /// - <paramref name="currentTarget"/> : The <see cref="Unit"/> being evaluated as a valid/invalid Target. <br></br>
        ///  <see langword="return"/> -> Whether or not <paramref name="currentTarget"/> is a valid Target.
        /// </remarks>
        /// <param name="user"></param>
        /// <param name="previousTarget"></param>
        /// <param name="currentTarget"></param>
        public delegate bool TargetCondition(Player user, Unit previousTarget, Unit currentTarget);

        /// <summary>
        /// in order for a Target to be valid, it must pass it's respective <see cref="TargetCondition"/>. <br></br>
        /// > The size of this list determines how many Targets this ability has.
        /// </summary>
        /// <remarks>
        ///<inheritdoc cref="TargetCondition"/> <br></br> <br></br>
        /// TargetConditions[0] will have <see langword="null"/> as previousUnit.
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

        /// <summary>
        /// Creates a <see cref="Unsourced"/> with name <paramref name="name"/> and of type <paramref name="typeIdentity"/>.
        /// </summary>
        /// <remarks>
        /// Required Properties:<br></br>
        /// - <see cref="TargetConditions"/><br></br>
        /// - <see cref="ActionMethod"/><br></br>
        /// <br></br>
        /// Defaulted Properties:<br></br>
        /// <inheritdoc cref="Ability.Ability(string, ETypeIdentity)"/>
        /// </remarks>
        /// <param name="name"></param>
        /// <param name="typeIdentity"></param>
        public Unsourced(string name, ETypeIdentity typeIdentity) : base(name, typeIdentity) { }
        
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
        /// - <paramref name="user"/> : The <see cref="Player"/> that played the ability. <br></br>
        /// - <paramref name="source"/> : The <see cref="Unit"/> that was selected as the Source. <br></br>
        /// - <paramref name="target"/> : The <see cref="Unit"/> being evaluated as a valid/invalid Target. <br></br>
        /// <see langword="return"/> -> Whether or not <paramref name="currentTarget"/> is a valid Target.
        /// </remarks>
        /// <param name="user"></param>
        /// <param name="source"></param>
        /// <param name="target"></param>
        public delegate bool TargetingCondition(Player user, Unit source, Unit target);
        /// <summary>
        /// [Delegate]
        /// </summary>
        /// <remarks>
        /// <c><see cref="bool"/> SourceConditionMethod(<see cref="Player"/> <paramref name="user"/>, <see cref="Unit"/> <paramref name="source"/>) { }</c> <br></br>
        /// - <paramref name="user"/> : The <see cref="Player"/> that played the ability. <br></br>
        /// - <paramref name="source"/> : The <see cref="Unit"/> being evaluated as a valid/invalid Source. <br></br>
        /// <see langword="return"/> -> Whether or not <paramref name="source"/> is a valid Source.
        /// </remarks>
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
        public List<ConstructionTemplate<UnitEffect>> TargetEffects { get; set; }

        /// <summary>
        /// A <see cref="Unit"/> must pass ALL of these conditions in order to be a valid Target.
        /// </summary>
        /// <remarks>
        /// <inheritdoc cref="TargetingCondition"/> <br></br> <br></br>
        /// <i>Units will only be evaluated if they are within the Hit Area of the ability.</i>
        /// </remarks>
        public List<TargetingCondition> TargetingConditions { get; set; }
        /// <summary>
        /// A <see cref="Unit"/> must pass ALL of these conditions in order to be a valid Source.
        /// </summary>
        /// <remarks>
        /// Default: <c>{ <see cref="STANDARD_VALID_SOURCE"/> }</c>
        /// <br></br><br></br>
        /// <inheritdoc cref="SourceCondition"/>
        /// </remarks>
        public List<SourceCondition> SourceConditions { get; set; } = new() { STANDARD_VALID_SOURCE };
        /// <summary>
        /// The Follow Up method of this ability. <br></br>
        /// > Called when this ability is played. (primarily to add implicit resultants)
        /// </summary>
        /// <remarks>
        /// Default: <c><see cref="NO_ACTION"/></c><br></br><br></br>
        /// <inheritdoc cref="PlayAction"/>
        /// </remarks>
        public PlayAction FollowUpMethod { get; set; } = NO_ACTION;

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
            bool __IsCollision(Hex h) => h.BlocksTargeting && (h.Occupant == null || h.Occupant.Team == p.Team);
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
        /// Creates a <see cref="Sourced"/> with name <paramref name="name"/> and of type <paramref name="typeIdentity"/>. <br></br>
        /// </summary>
        /// <remarks>
        /// Required Properties:<br></br>
        /// - <see cref="HitArea"/><br></br>
        /// - <see cref="TargetEffects"/><br></br>
        /// - <see cref="TargetingConditions"/><br></br>
        /// <br></br>
        /// Defaulted Properties:<br></br>
        /// - <see cref="SourceConditions"/><br></br>
        /// - <see cref="FollowUpMethod"/><br></br>
        /// <inheritdoc cref="Ability.Ability(string, ETypeIdentity)"/>
        /// 
        /// </remarks>
        /// <param name="name"></param>
        /// <param name="typeIdentity"></param>
        public Sourced(string name, ETypeIdentity typeIdentity) : base(name, typeIdentity) { }



    }


}
