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
        /// Occurs when any <see cref="EnergyChange"/> is performed.
        /// </summary>
        public static event GameActionEventHandler<EnergyChange> ExternalResultantEvent;

        /// <summary>
        /// The <see cref="Player"/> that recieved the change in energy on this action.
        /// </summary>
        public Player Reciever { get; private set; }
        /// <summary>
        /// The amount of energy that Reciever had before this action. <br></br>
        /// <i>Reciever's energy is set to this amount when this action is undone.</i>
        /// </summary>
        public int BeforeAmount { get; private set; }
        /// <summary>
        /// The amount of energy that Reciever had after this action. <br></br>
        /// <i>Reciever's energy is set to this amount when this action is performed.</i>
        /// </summary>
        public int AfterAmount { get; private set; }

        protected override void InternalPerform()
        {
            Reciever.Energy = AfterAmount;
        }

        protected override void InternalUndo()
        {
            Reciever.Energy = BeforeAmount;
        }

        /// <summary>
        /// Changes <paramref name="reciever"/>'s Energy amount by the <paramref name="changeFunction"/>. (by <paramref name="performer"/>)
        /// </summary>
        /// <remarks>
        /// <i>Ex: <c><paramref name="changeFunction"/> = (e) => { return e + 1; }</c> <br></br>
        /// This would add 1 to <paramref name="reciever"/>'s energy amount.</i>
        /// </remarks>
        /// <param name="reciever"></param>
        /// <param name="changeFunction"></param>
        public EnergyChange(Player performer, Player reciever, Func<int, int> changeFunction) : base(performer)
        {
            Reciever = reciever;
            BeforeAmount = reciever.Energy;
            AfterAmount = changeFunction(reciever.Energy);
            ExternalResultantEvent?.Invoke(this);
        }

        public override string ToString()
        {
            return $"<ENERGY>: {Reciever} = {BeforeAmount} -> {AfterAmount}" + base.ToString();
        }
    }

}
