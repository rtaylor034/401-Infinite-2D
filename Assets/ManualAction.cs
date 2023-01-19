using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class ManualAction
{
    public enum EStandardType
    {
        NonStandard,
        Move,
        Ability,
        Discard
    }
    //make specific delegates
    public EStandardType StandardType { get; set; }
    public Func<Player, IEnumerable<Selectable>> EntryPoints { get; set; }
    public Func<Player, Selectable, Task<GameAction>> Action { get; set; }
    public List<Func<Player, bool>> PlayerConditions { get; set; } = new() { _ => true };
    public List<Func<Player, bool>> PlayerConditionOverrides { get; set; } = new() { _ => false };

    public static readonly Func<Player, bool> ONE_ENERGY_REQUIRED = p => p.Energy >= 1;

    public ManualAction(EStandardType standardType)
    {
        StandardType = standardType;
    }
}
