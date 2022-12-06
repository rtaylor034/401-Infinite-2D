using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract partial class UnitEffect
{
    //Will be null until SetActive is called once.
    //If all executes according to plan, will never read as null, as SetActive should be called by appropriate GameActions.
    public Unit AffectedUnit { get; private set; }
    public Player Inflicter { get; private set; }
    
    //EVERY Turn counts as a Duration tick. by default Duration = 1, meaning the effect will only last for the following Turn after it is inflicted (opponents turn), and wears off on your next Turn.
    public int Duration { get; private set; }

    protected UnitEffect(int duration)
    {
        Duration = duration;
        GameAction.InflictEffect.ExternalResultantEvent += CallWhenInflicted;
    }

    public void SetActive(bool val, Unit affectedUnit, Player inflicter)
    {
        AffectedUnit = affectedUnit;
        Inflicter = inflicter;
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

    /// <summary>
    /// Sets this UnitEffect's Duration to <paramref name="val"/>. (Should only be called from <see cref="GameAction"/>[ : ])
    /// </summary>
    /// <param name="val"></param>
    public void UpdateDuration(int val)
    {
        Duration = val;
    }
    private void CallWhenInflicted(GameAction.InflictEffect action)
    {
        if (action.Effect != this) return;
        WhenInflicted(action);
        GameAction.InflictEffect.ExternalResultantEvent -= CallWhenInflicted;
    }

    protected abstract void InternalSetup(bool val);

    /// <summary>
    /// <b>[virtual]</b><br></br>
    /// Called when <see langword="this"/> is inflicted via a <see cref="GameAction.InflictEffect"/>.
    /// </summary>
    /// <remarks>
    /// (<c><see langword="this"/>.AffectedUnit</c> has not been set yet, use <c><paramref name="action"/>.AffectedUnit</c>)
    /// </remarks>
    /// <param name="action"></param>
    protected virtual void WhenInflicted(GameAction.InflictEffect action) { }
    

    private void TickDown(GameAction.Turn action)
    {
        action.AddResultant(new GameAction.EffectDurationChange(action.Performer, this, d => d - 1));
    }


    public override string ToString()
    {
        return $"({GetType().Name} {Duration})";
    }
}
