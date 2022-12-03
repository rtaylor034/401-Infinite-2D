using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class GameAction
{

    public class EffectDurationChange : GameAction
    {
        /// <summary>
        /// The <see cref="UnitEffect"/> that had its Duration changed.
        /// </summary>
        public UnitEffect TickingEffect { get; private set; }
        /// <summary>
        /// The <see cref="UnitEffect.Duration"/> before this action was performed.
        /// </summary>
        public int BeforeAmount { get; private set; }
        /// <summary>
        /// The <see cref="UnitEffect.Duration"/> after this action was performed.
        /// </summary>
        public int AfterAmount { get; private set; }

        /// <summary>
        /// Occurs when any <see cref="EffectDurationChange"/> is created.
        /// </summary>
        /// <remarks><inheritdoc cref="GameAction.__DOC__ExternalResultantEvent"/></remarks>
        public static event GameActionEventHandler<EffectDurationChange> ExternalResultantEvent;

        /// <summary>
        /// Changes <paramref name="effect"/>'s Duration by <paramref name="changeFunction"/>, by <paramref name="performer"/>. <br></br>
        /// </summary>
        /// <remarks>
        /// <i>(See <see cref="EnergyChange"/> for an example usage of <paramref name="changeFunction"/>)</i>
        /// </remarks>
        /// <param name="performer"></param>
        /// <param name="effect"></param>
        /// <param name="changeFunction"></param>
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
