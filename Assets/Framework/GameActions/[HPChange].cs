using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class GameAction
{
    //TODO: Make general "changer" abstract class with all basic implentations for any <x>Chagne GameAction.
    public class HPChange : GameAction
    {
        /// <summary>
        /// Occurs when any <see cref="HPChange"/> is created.
        /// </summary>
        /// <remarks><inheritdoc cref="__DOC__ExternalResultantEvent"/></remarks>
        public static event GameActionEventHandler<HPChange> ExternalResultantEvent;
        /// <summary>
        /// The Unit recieving the HP change.
        /// </summary>
        public Unit Reciever { get; private set; }
        /// <summary>
        /// The function that the Reciever's HP changes by.
        /// </summary>
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

        //TODO: Doc
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