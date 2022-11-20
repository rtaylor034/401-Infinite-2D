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

        /// <summary>
        /// Transfers the turn from <paramref name="fromPlayer"/> to <paramref name="toPlayer"/>.
        /// </summary>
        /// <remarks>
        /// Performer is set to <paramref name="toPlayer"/>.
        /// </remarks>
        /// <param name="fromPlayer"></param>
        /// <param name="toPlayer"></param>
        public Turn(Player fromPlayer, Player toPlayer) : base(toPlayer)
        {
            FromPlayer = fromPlayer;
            ToPlayer = toPlayer;
        }
        /// <summary>
        /// <inheritdoc cref="Turn.Turn(Player, Player)"/> (by <paramref name="performer"/>)
        /// </summary>
        /// <param name="performer"></param>
        /// <param name="fromPlayer"></param>
        /// <param name="toPlayer"></param>
        public Turn(Player performer, Player fromPlayer, Player toPlayer) : base(performer)
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
