using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract partial class GameAction
{

    public class Turn : GameAction
    {
        /// <summary>
        /// Triggered when any <see cref="Turn"/> is performed.
        /// </summary>
        public static event GameActionEventHandler<Turn> OnPerform;

        /// <summary>
        /// The <see cref="Player"/> that ends their turn on this action. <br></br>
        /// <i>Turn is transferred to this Player when this actions is undone.</i>
        /// </summary>
        public Player FromPlayer { get; private set; }
        /// <summary>
        /// The <see cref="Player"/> that starts their turn on this action. <br></br>
        /// <i>Turn is transferred to this Player when this actions is performed.</i>
        /// </summary>
        public Player ToPlayer { get; private set; }

        protected override void InternalPerform()
        {
            //Handled by GameManager
            OnPerform?.Invoke(this);
        }

        protected override void InternalUndo()
        {
            //Handled by GameManager
        }

        private Turn(Player fromPlayer, Player toPlayer) : base(toPlayer)
        {
            FromPlayer = fromPlayer;
            ToPlayer = toPlayer;
        }

        /// <summary>
        /// Declare a <see cref="Turn"/>, transferring turn control from <paramref name="fromPlayer"/> to <paramref name="toPlayer"/>.
        /// </summary>
        /// <param name="fromPlayer"></param>
        /// <param name="toPlayer"></param>
        public static void Declare(Player fromPlayer, Player toPlayer)
        {
            FinalizeDeclare(new Turn(fromPlayer, toPlayer));
        }

        public override string ToString()
        {
            return $"<TURN>: {FromPlayer} -> {ToPlayer}" + base.ToString();
        }
    }

}
