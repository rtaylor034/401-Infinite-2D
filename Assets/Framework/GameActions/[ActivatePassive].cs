using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public partial class GameAction
{

    /// <summary>
    /// [ : ] <see cref="GameAction"/>
    /// </summary>
    public class ActivatePassive : GameAction
    {
        /// <summary>
        /// The <see cref="Passive"/> that was activated (or deactivated).
        /// </summary>
        public Passive PassiveObj { get; private set; }

        /// <summary>
        /// The <see cref="Player"/> that the Passive empowers.
        /// </summary>
        public Player AffectedPlayer { get; private set; }

        protected virtual bool SetVal => true;

        protected override void InternalPerform()
        {
            PassiveObj.SetActive(SetVal, AffectedPlayer);
        }

        protected override void InternalUndo()
        {
            PassiveObj.SetActive(!SetVal, AffectedPlayer);
        }

        /// <summary>
        /// Activates <paramref name="activatedPassive"/> on <paramref name="affectedPlayer"/>, by <paramref name="performer"/>. <br></br>
        /// <i>(<paramref name="activatedPassive"/> will start "empowering" <paramref name="affectedPlayer"/>)</i>
        /// </summary>
        /// <remarks>
        /// It is <b>NECESSARY</b> that all activated Passives are deactivated at some point. <br></br>
        /// (See <see cref="DeactivatePassive"/>)
        /// </remarks>
        /// <param name="performer"></param>
        /// <param name="activatedPassive"></param>
        /// <param name="affectedPlayer"></param>
        public ActivatePassive(Player performer, Passive activatedPassive, Player affectedPlayer) : base(performer)
        {
            PassiveObj = activatedPassive;
            AffectedPlayer = affectedPlayer;
        }
        public override string ToString()
        {
            return $"<ACTIVATE PASSIVE> {PassiveObj} -> {AffectedPlayer}" + base.ToString();
        }
    }

    public class DeactivatePassive : ActivatePassive
    {
        protected override bool SetVal => false;

        /// <summary>
        /// Deactivates <paramref name="activatedPassive"/> from <paramref name="affectedPlayer"/>, by <paramref name="performer"/>. <br></br>
        /// <i>(<paramref name="activatedPassive"/> will stop "empowering" <paramref name="affectedPlayer"/>)</i>
        /// </summary>
        /// <remarks>
        /// It is <b>NECESSARY</b> that all activated Passives are deactivated at some point.
        /// </remarks>
        /// <param name="performer"></param>
        /// <param name="activatedPassive"></param>
        /// <param name="affectedPlayer"></param>
        public DeactivatePassive(Player performer, Passive activatedPassive, Player affectedPlayer) : base(performer, activatedPassive, affectedPlayer) { }

        public override string ToString()
        {
            return $"<DEACTIVATE PASSIVE> {PassiveObj} -> {AffectedPlayer}" + base.ToString();
        }
    }

}