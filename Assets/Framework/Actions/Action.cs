using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract partial class Action
{
    public Player Performer { get; private set; }
    public abstract void Perform();
    public abstract void Undo();

    protected Action(Player performer)
    {
        Performer = performer;
    }


}
