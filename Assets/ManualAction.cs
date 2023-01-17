using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public record ManualAction
{
    public enum EStandardType
    {
        NonStandard,
        Move,
        Ability,
        Discard
    }
    public EStandardType StandardType { get; set; }
    public Func<Player, IEnumerable<Selectable>> EntryPoints { get; set; }
    public Func<Player, Selectable, Task<GameAction>> ActionFunction { get; set; }
    public List<Func<Player, bool>> PlayerConditions { get; set; } = new() { _ => true };
    public List<Func<Player, bool>> PlayerConditionOverrides { get; set; } = new() { _ => false };

    public static readonly Func<Player, bool> ONE_ENERGY_REQUIRED = p => p.Energy >= 1;

    public ManualAction(EStandardType standardType, Func<Player, IEnumerable<Selectable>> entryPoints, Func<Player, Selectable, Task<GameAction>> actionFunction)
    {
        StandardType = standardType;
        EntryPoints = entryPoints;
        ActionFunction = actionFunction;
    }
}
