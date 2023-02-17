using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionPromptWheelOption : Selectable
{
    /// <summary>
    /// Is this option available to the player?<br></br>
    /// (Passes the PlayerConditions/PlayerConditionOverrides)
    /// </summary>
    public bool IsActionable =>
        Action.PlayerConditions.InvokeAll(ParentWheel.Performer).GateAND() ||
        Action.PlayerConditionOverrides.InvokeAll(ParentWheel.Performer).GateOR();
    /// <summary>
    /// The <see cref="ActionPromptWheel"/> that this option is a child of.
    /// </summary>
    public ActionPromptWheel ParentWheel { get; private set; }
    /// <summary>
    /// The <see cref="ManualAction"/> that this option represents.
    /// </summary>
    public ManualAction Action { get; private set; }
    /// <summary>
    /// <b>[MUST BE CALLED AFTER INSTANTIATION]</b> (<see cref="Object.Instantiate(Object)"/>)
    /// </summary>
    /// <param name="performer"></param>
    /// <param name="root"></param>
    /// <param name="actions"></param>
    /// <param name="optionPrefab"></param>
    /// <returns></returns>
    public ActionPromptWheelOption Init(ActionPromptWheel parentWheel, ManualAction action)
    {
        ParentWheel = parentWheel;
        Action = action;
        return this;
    }
}
