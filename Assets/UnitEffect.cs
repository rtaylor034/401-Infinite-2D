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
    public int Duration { get; private set; }

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
        action.AddResultant(new DurationTick(action.Performer, this));
    }

    public class DurationTick : GameAction
    {
        public UnitEffect TickingEffect { get; private set; }

        /// <summary>
        /// Occurs when any <see cref="DurationTick"/> is created.
        /// </summary>
        /// <remarks><inheritdoc cref="GameAction.__DOC__ExternalResultantEvent"/></remarks>
        public static event GameActionEventHandler<DurationTick> ExternalResultantEvent;
        public DurationTick(Player performer, UnitEffect effect) : base(performer)
        {
            TickingEffect = effect;
        }

        protected override void InternalPerform()
        {
            if (TickingEffect.Duration <= 0) TickingEffect.SetActive(false, TickingEffect.AffectedUnit);
            TickingEffect.Duration--;
        }

        protected override void InternalUndo()
        {
            TickingEffect.Duration++;
            if (TickingEffect.Duration > 0) TickingEffect.SetActive(true, TickingEffect.AffectedUnit);
            ExternalResultantEvent?.Invoke(this);
        }

        public override string ToString()
        {
            return $"<EFFECT TICK> {TickingEffect}--";
        }
    }

    public override string ToString()
    {
        return $"({GetType().Name} {Duration})";
    }
}
