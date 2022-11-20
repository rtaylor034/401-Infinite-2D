using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract partial class GameAction
{
    /// <summary>
    /// [ : ] <see cref="GameAction"/>
    /// </summary>
    public class Turn : GameAction
    {
        /// <summary>
        /// Occurs when any <see cref="Turn"/> is performed.
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

        public Turn(Player fromPlayer, Player toPlayer) : base(toPlayer)
        {
            FromPlayer = fromPlayer;
            ToPlayer = toPlayer;
        }

        public override string ToString()
        {
            return $"<TURN>: {FromPlayer} -> {ToPlayer}" + base.ToString();
        }
    }

}
