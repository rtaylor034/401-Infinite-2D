using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionPromptWheelEntry : Selectable
{
    public bool IsActionable =>
        Action.PlayerConditions.InvokeAll(ParentWheel.Performer).GateAND() ||
        Action.PlayerConditionOverrides.InvokeAll(ParentWheel.Performer).GateOR();

    public ActionPromptWheel ParentWheel { get; private set; }
    public ManualAction Action { get; private set; }
    public ActionPromptWheelEntry Init(ActionPromptWheel parentWheel, ManualAction action)
    {
        ParentWheel = parentWheel;
        Action = action;
        return this;
    }
}
