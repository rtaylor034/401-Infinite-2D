using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class GameAction
{

    public class EffectDurationChange : GameAction
    {
        public UnitEffect TickingEffect { get; private set; }
        public int BeforeAmount { get; private set; }
        public int AfterAmount { get; private set; }

        /// <summary>
        /// Occurs when any <see cref="EffectDurationChange"/> is created.
        /// </summary>
        /// <remarks><inheritdoc cref="GameAction.__DOC__ExternalResultantEvent"/></remarks>
        public static event GameActionEventHandler<EffectDurationChange> ExternalResultantEvent;
        public EffectDurationChange(Player performer, UnitEffect effect, System.Func<int, int> changeFunction) : base(performer)
        {
            TickingEffect = effect;
            BeforeAmount = effect.Duration;
            AfterAmount = changeFunction(effect.Duration);
            ExternalResultantEvent?.Invoke(this);
        }

        protected override void InternalPerform()
        {
            if (TickingEffect.Duration <= 0) TickingEffect.SetActive(false, TickingEffect.AffectedUnit);
            TickingEffect.Duration = AfterAmount;
        }

        protected override void InternalUndo()
        {
            TickingEffect.Duration = BeforeAmount;
            if (TickingEffect.Duration > 0) TickingEffect.SetActive(true, TickingEffect.AffectedUnit);
        }

        public override string ToString()
        {
            return $"<EFFECT DURATION> {TickingEffect} = {BeforeAmount} ->  {AfterAmount}" + base.ToString();
        }
    }
}
