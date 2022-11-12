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

        public static event GameActionEventHandler<EnergyChange> OnPerform;

        public Player Reciever { get; private set; }
        public int BeforeAmount { get; private set; }
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
