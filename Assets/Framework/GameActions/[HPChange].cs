using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class GameAction
{

    public class HPChange : GameAction
    {

        public static event GameActionEventHandler<HPChange> ExternalResultantEvent;
        public Unit Reciever { get; private set; }
        public int BeforeAmount { get; private set; }
        public int AfterAmount { get; private set; }

        protected override void InternalPerform()
        {
            Reciever.UpdateHP(AfterAmount);
            
        }

        protected override void InternalUndo()
        {
            Reciever.UpdateHP(BeforeAmount);
        }

        public HPChange(Player performer, Unit reciever, Func<int, int> changeFunction) : base(performer)
        {
            Reciever = reciever;
            BeforeAmount = reciever.HP;
            AfterAmount = changeFunction(reciever.HP);
            ExternalResultantEvent?.Invoke(this);
        }

        public override string ToString()
        {
            return $"<HP CHANGE> {Reciever} {BeforeAmount} -> {AfterAmount}" + base.ToString();
        }
    }


}