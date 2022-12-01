using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract partial class UnitEffect
{
    //Will be null until SetActive is called once.
    //If all executes according to plan, will never read as null, as SetActive should be called by appropriate GameActions.
    public Unit AffectedUnit { get; private set; }
    
    //EVERY Turn counts as a Duration tick. by default Duration = 1, meaning the effect will only last for the following Turn after it is inflicted (opponents turn), and wears off on your next Turn.
    public int Duration { get; set; }

    protected UnitEffect(int duration)
    {
        Duration = duration;
    }

    public void SetActive(bool val, Unit affectedUnit)
    {
        AffectedUnit = affectedUnit;
        InternalSetup(val);
        if (val)
        {
            GameAction.Turn.ExternalResultantEvent += TickDown;
        } 
        else
        {
            GameAction.Turn.ExternalResultantEvent -= TickDown;
        }
        
    }

    protected abstract void InternalSetup(bool val);

    private void TickDown(GameAction.Turn action)
    {
        action.AddResultant(new GameAction.EffectDurationChange(action.Performer, this, d => d--));
    }


    public override string ToString()
    {
        return $"({GetType().Name} {Duration})";
    }
}
