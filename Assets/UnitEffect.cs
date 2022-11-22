using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class UnitEffect
{
    public Unit AffectedUnit { get; private set; }
    public int Duration { get; private set; }

    protected UnitEffect(Unit affectedUnit, int duration = 1)
    {
        AffectedUnit = affectedUnit;
        Duration = duration;
    }

    public void SetActive(bool val)
    {
        InternalSetup(val);
        if (val)
        {
            GameAction.Turn.OnPerform += TickDown;
        } 
        else
        {
            GameAction.Turn.OnPerform -= TickDown;
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
        public static event GameActionEventHandler<DurationTick> OnPerform;
        public DurationTick(Player performer, UnitEffect effect) : base(performer)
        {
            TickingEffect = effect;
        }

        protected override void InternalPerform()
        {
            if (TickingEffect.Duration <= 0) TickingEffect.SetActive(false);
            TickingEffect.Duration--;
            OnPerform?.Invoke(this);
        }

        protected override void InternalUndo()
        {
            TickingEffect.Duration++;
            if (TickingEffect.Duration > 0) TickingEffect.SetActive(true);
        }
    }

}
