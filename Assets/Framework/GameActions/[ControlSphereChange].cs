using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class GameAction
{
    public class ControlSphereChange : GameAction
    {
        /// <summary>
        /// Occurs when any <see cref="ControlSphereChange"/> is created.
        /// </summary>
        /// <remarks><inheritdoc cref="__DOC__ExternalResultantEvent"/></remarks>
        public static event GameActionEventHandler<ControlSphereChange> ExternalResultantEvent;
        /// <summary>
        /// The <see cref="Player"/> recieving the Control Sphere count change.
        /// </summary>
        public Player Reciever { get; private set; }
        /// <summary>
        /// The function that the Reciever's Control Sphere count changes by.
        /// </summary>
        /// <remarks>
        /// <inheritdoc cref="__DOC__ChangeFunction"/>
        /// </remarks>
        public Func<int, int> ChangeFunction { get; private set; }
        private int _ChangedValue => ChangeFunction(Reciever.ControlSpheres);

        private readonly Stack<int> _changeStack;

        protected override void InternalPerform()
        {
            _changeStack.Push(_ChangedValue - Reciever.ControlSpheres);
            Reciever.UpdateControlSpheres(_ChangedValue);
        }

        protected override void InternalUndo()
        {
            Reciever.UpdateControlSpheres(Reciever.ControlSpheres - _changeStack.Pop());
        }

        /// <summary>
        /// Changes <paramref name="reciever"/>'s Control Sphere count by the <paramref name="changeFunction"/>, by <paramref name="performer"/>.
        /// </summary>
        /// <remarks>
        /// <paramref name="changeFunction"/> : <br></br>
        /// <inheritdoc cref="ChangeFunction"/>
        /// </remarks>
        /// <param name="performer"></param>
        /// <param name="reciever"></param>
        /// <param name="changeFunction"></param>
        public ControlSphereChange(Player performer, Player reciever, Func<int, int> changeFunction) : base(performer)
        {
            _changeStack = new();
            _changeStack.Push(0);
            Reciever = reciever;
            ChangeFunction = changeFunction;
            ExternalResultantEvent?.Invoke(this);
        }

        public override string ToString()
        {
            var offset = _ChangedValue - Reciever.ControlSpheres;
            return $"<CONTROL SPHERE> {Reciever} ({((offset >= 0) ? "+" : "")}{offset} Spheres)" + base.ToString();
        }
    }


}