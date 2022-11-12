using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract partial class GameAction
{
    /// <summary>
    /// <b>[ : ] <see cref="GameAction"/></b>
    /// </summary>
    public class EnergyChange : GameAction
    {
        /// <summary>
        /// Occurs when any <see cref="EnergyChange"/> is performed.
        /// </summary>
        public static event GameActionEventHandler<EnergyChange> OnPerform;

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
            OnPerform?.Invoke(this);
        }

        protected override void InternalUndo()
        {
            Reciever.Energy = BeforeAmount;
        }

        private EnergyChange(Player performer, Player reciever, int before, int after) : base(performer)
        {
            Reciever = reciever;
            BeforeAmount = before;
            AfterAmount = after;
        }

        /// <summary>
        /// Change <paramref name="reciever"/>'s energy amount by the <paramref name="changeFunction"/>. <br></br>
        /// > This <see cref="EnergyChange"/> will be a resultant of <paramref name="resultOf"/>.
        /// </summary>
        /// <remarks>
        /// <i>Ex: <c><paramref name="changeFunction"/> = (e) => { return e + 1; }</c> <br></br>
        /// This would add 1 to <paramref name="reciever"/>'s energy amount.</i>
        /// </remarks>
        /// <param name="resultOf"></param>
        /// <param name="reciever"></param>
        /// <param name="changeFunction"></param>
        public static void DeclareAsResultant(GameAction resultOf, Player reciever, Func<int, int> changeFunction)
        {
            resultOf.AddResultant(new EnergyChange(resultOf.Performer, reciever, reciever.Energy, changeFunction(reciever.Energy)));
        }

        public override string ToString()
        {
            return $"<ENERGY>: {Reciever} = {BeforeAmount} -> {AfterAmount}" + base.ToString();
        }
    }

}
