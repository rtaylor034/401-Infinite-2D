using System;
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
        public Func<int, int> ChangeFunction { get; private set; }

        private int _ChangedValue => ChangeFunction(TickingEffect.Duration);
        private readonly Stack<int> _changeStack;

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
            _changeStack = new();
            TickingEffect = effect;
            ChangeFunction = changeFunction;
            ExternalResultantEvent?.Invoke(this);
        }

        protected override void InternalPerform()
        {
            _changeStack.Push(_ChangedValue - TickingEffect.Duration);
            TickingEffect.UpdateDuration(_ChangedValue);

            if (TickingEffect.Duration < 0) TickingEffect.SetActive(false, TickingEffect.AffectedUnit, TickingEffect.Inflicter);
        }

        protected override void InternalUndo()
        {
            TickingEffect.UpdateDuration(TickingEffect.Duration - _changeStack.Pop());

            if (TickingEffect.Duration == 0) TickingEffect.SetActive(true, TickingEffect.AffectedUnit, TickingEffect.Inflicter);
        }

        public override string ToString()
        {
            var offset = _ChangedValue - TickingEffect.Duration;
            return $"<EDURATION CHANGE> {TickingEffect} ({((offset >= 0) ? "+" : "")}{offset} Ticks)" + base.ToString();
        }
    }
}
