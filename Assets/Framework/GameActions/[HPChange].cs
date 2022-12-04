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
        public Func<int, int> ChangeFunction { get; private set; }
        private int _ChangedHP => ChangeFunction(Reciever.HP);

        //Could be a single int variable, but this supports multiple Perform() calls to the same action. yea this probably will never happen, but why the hell not.
        private readonly Stack<int> _changeStack;

        protected override void InternalPerform()
        {
            _changeStack.Push(_ChangedHP - Reciever.HP);
            Reciever.UpdateHP(_ChangedHP);
            
        }

        protected override void InternalUndo()
        {
            Reciever.UpdateHP(Reciever.HP - _changeStack.Pop());
        }
        
        public HPChange(Player performer, Unit reciever, Func<int, int> changeFunction) : base(performer)
        {
            _changeStack = new();
            _changeStack.Push(0);
            Reciever = reciever;
            ChangeFunction = changeFunction;
            ExternalResultantEvent?.Invoke(this);
        }

        public override string ToString()
        {
            var offset = _ChangedHP - Reciever.HP;
            return $"<HP CHANGE> {Reciever} ({((offset >= 0) ? "+" : "")}{offset} HP)" + base.ToString();
        }
    }


}