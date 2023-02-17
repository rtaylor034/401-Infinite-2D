using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class ManualAction
{
    /// <summary>
    /// Enum for <see cref="StandardType"/>.
    /// </summary>
    public enum EStandardType
    {
        NonStandard,
        Move,
        Ability,
        Discard
    }
    /// <summary>
    /// The "standard" type/role identity of this action.<br></br>
    /// > This does not affect internal behavior.
    /// </summary>
    /// <remarks>
    /// <i>This is mainly used to find/replace standard actions externally.<br></br>
    /// (e.g. If a passive were to replace your regular "discard" action with one that does not cost energy)</i>
    /// </remarks>
    public EStandardType StandardType { get; set; }
    /// <summary>
    /// The set of objects that the player can select to be the Root of this action.<br></br>
    /// > Function of the <see cref="Player"/> being prompted to make a manual action.
    /// </summary>
    /// <remarks>
    /// <c><see cref="IEnumerable"/>&lt;<see cref="Selectable"/>&gt;
    /// EntryPointsFunction(<see cref="Player"/> <i>player</i>) { }</c><br></br>
    /// - <i>player</i> : The <see cref="Player"/> that is being prompted to make a manual action.<br></br>
    /// <see langword="return"/> -> The selectables that the player can select as a root of this <see cref="ManualAction"/>.
    /// </remarks>
    public Func<Player, IEnumerable<Selectable>> EntryPoints { get; set; }
    /// <summary>
    /// The <see cref="GameAction"/> object to be declared when this manual action is selected.<br></br>
    /// > Function of the declaring <see cref="Player"/> and their selected Root <see cref="Selectable"/>.
    /// </summary>
    /// <remarks>
    /// <c><see langword="async"/> <see cref="Task"/>&lt;<see cref="GameAction"/>&gt;
    /// ActionFunction(<see cref="Player"/> <i>player</i>, <see cref="Selectable"/> <i>root</i>) { }</c><br></br>
    /// - <i>player</i> : The <see cref="Player"/> declaring the action.<br></br>
    /// - <i>root</i> : The root object that was selected for this action.<br></br>
    /// <see langword="return"/> -> The <see cref="GameAction"/> to be declared.
    /// </remarks>
    public Func<Player, Selectable, Task<GameAction>> Action { get; set; }
    /// <summary>
    /// A Player must pass all of these conditions in order for this manual action to be availabe to them.
    /// </summary>
    /// <remarks>
    /// <c><see cref="bool"/> PlayerConditionFunction(<see cref="Player"/> <i>player</i>) { }</c><br></br>
    /// - <i>player</i> : The <see cref="Player"/> being prompted to make a manual move.<br></br>
    /// <see langword="return"/> -> -> whether or not the condition was passed.
    /// </remarks>
    public List<Func<Player, bool>> PlayerConditions { get; set; } = new() { _ => true };
    /// <summary>
    /// A Player can pass any of these conditions in order for this manual action to be availabe to them.<br></br>
    /// (Overriding <see cref="PlayerConditions"/>)
    /// </summary>
    /// <remarks>
    /// <c><see cref="bool"/> PlayerConditionFunction(<see cref="Player"/> <i>player</i>) { }</c><br></br>
    /// - <i>player</i> : The <see cref="Player"/> being prompted to make a manual move.<br></br>
    /// <see langword="return"/> -> whether or not the condition was passed.
    /// </remarks>
    public List<Func<Player, bool>> PlayerConditionOverrides { get; set; } = new() { _ => false };

    /// <summary>
    /// The standard PlayerCondition that all regular manual actions should have.
    /// </summary>
    /// <remarks>
    /// <c>p => p.Energy >= 1;</c>
    /// </remarks>
    public static readonly Func<Player, bool> ONE_ENERGY_REQUIRED = p => p.Energy >= 1;

    /// <summary>
    /// Creates a <see cref="ManualAction"/> of type <paramref name="standardType"/>.<br></br>
    /// <i><see cref="ManualAction"/> objects are not GameActions themselves, but store information about when/how a <see cref="Player"/> can declare a GameAction when it is their Turn.</i>
    /// </summary>
    /// <remarks>
    /// Required Properties: <br></br>
    /// - <see cref="EntryPoints"/><br></br>
    /// - <see cref="Action"/><br></br>
    /// <br></br>
    /// Defaulted Properties:<br></br>
    /// - <see cref="PlayerConditions"/><br></br>
    /// - <see cref="PlayerConditionOverrides"/><br></br>
    /// - <see cref="StandardType"/> (Set by constructor)<br></br>
    /// </remarks>
    /// <param name="standardType"></param>
    public ManualAction(EStandardType standardType)
    {
        StandardType = standardType;
    }

    public override string ToString()
    {
        return $"<MA:{StandardType}>";
    }
}
