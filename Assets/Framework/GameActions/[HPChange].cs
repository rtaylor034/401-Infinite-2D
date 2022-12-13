using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class GameAction
{
    public class HPChange : GameAction
    {

        /// <summary>
        /// The <see cref="Unit"/> recieving the HP change.
        /// </summary>
        public Unit Reciever { get; private set; }
        /// <summary>
        /// The function that the Reciever's HP changes by.
        /// </summary>
        /// <remarks>
        /// <inheritdoc cref="__DOC__ChangeFunction"/>
        /// </remarks>
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

        /// <summary>
        /// Changes <paramref name="reciever"/>'s HP by the <paramref name="changeFunction"/>, by <paramref name="performer"/>.
        /// </summary>
        /// <remarks>
        /// <paramref name="changeFunction"/> : <br></br>
        /// <inheritdoc cref="ChangeFunction"/>
        /// </remarks>
        /// <param name="performer"></param>
        /// <param name="reciever"></param>
        /// <param name="changeFunction"></param>
        public HPChange(Player performer, Unit reciever, Func<int, int> changeFunction) : base(performer)
        {
            _changeStack = new();
            _changeStack.Push(0);
            Reciever = reciever;
            ChangeFunction = changeFunction;
        }

        public override string ToString()
        {
            var offset = _ChangedHP - Reciever.HP;
            return $"<HP CHANGE> {Reciever} ({((offset >= 0) ? "+" : "")}{offset} HP)" + base.ToString();
        }
    }


}