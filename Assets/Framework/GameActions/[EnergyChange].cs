using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract partial class GameAction
{
    /// <summary>
    /// [ : ] <see cref="GameAction"/>
    /// </summary>
    public class EnergyChange : GameAction
    {
        /// <summary>
        /// Occurs when any <see cref="EnergyChange"/> is created.
        /// </summary>
        /// <remarks><inheritdoc cref="__DOC__ExternalResultantEvent"/></remarks>
        public static event GameActionEventHandler<EnergyChange> ExternalResultantEvent;

        /// <summary>
        /// The <see cref="Player"/> that recieved the change in Energy.
        /// </summary>
        public Player Reciever { get; private set; }
        /// <summary>
        /// The function that Reciever.Energy is changed by.
        /// </summary>
        /// <remarks>
        /// <inheritdoc cref="__DOC__ChangeFunction"/>
        /// </remarks>
        public Func<int, int> ChangeFunction { get; private set; }

        private int _ChangedValue => ChangeFunction(Reciever.Energy);
        private readonly Stack<int> _changeStack;

        protected override void InternalPerform()
        {
            _changeStack.Push(_ChangedValue - Reciever.Energy);
            Reciever.UpdateEnergy(_ChangedValue);
        }

        protected override void InternalUndo()
        {
            Reciever.UpdateEnergy(Reciever.Energy - _changeStack.Pop());
        }

        /// <summary>
        /// Changes <paramref name="reciever"/>'s Energy amount by the <paramref name="changeFunction"/>. (by <paramref name="performer"/>)
        /// </summary>
        /// <remarks>
        /// <paramref name="changeFunction"/> : <br></br>
        /// <inheritdoc cref="ChangeFunction"/>
        /// </remarks>
        /// <param name="reciever"></param>
        /// <param name="changeFunction"></param>
        public EnergyChange(Player performer, Player reciever, Func<int, int> changeFunction) : base(performer)
        {
            _changeStack = new();
            ChangeFunction = changeFunction;
            Reciever = reciever;
            ExternalResultantEvent?.Invoke(this);
        }

        public override string ToString()
        {
            var offset = _ChangedValue - Reciever.Energy;
            return $"<ENERGY CHANGE> {Reciever} ({((offset >= 0) ? "+" : "")}{offset} Energy)" + base.ToString();
        }
    }

}
