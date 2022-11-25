using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class GameAction
{

    //READPLEASE: OnPerform needs be changed on OnConstruct (triggers when GameAction is created) if GameActions are to be fully undo/redo safe. As it stands, if a GameAction is performed multiple times, duplicate GameActions will be declared as resultants every time the parent GameAction is performed past the first time.
    public class PlayAbility : GameAction
    {
        public Ability PlayedAbility { get; private set; }
        public Unit[] ParticipatingUnits { get; private set; }

        /// <summary>
        /// Occurs when any <see cref="PlayAbility"/> is created.
        /// </summary>
        /// <remarks><inheritdoc cref="__DOC__ExternalResultantEvent"/></remarks>
        public static event GameActionEventHandler<PlayAbility> ExternalResultantEvent;
        protected override void InternalPerform()
        {
            if (PlayedAbility is Ability.Sourced sourced)
            {
                //DEVNOTE: may create excessive UnitEffect objects, not really sure what to do about that.
                foreach(var effectC in sourced.TargetEffects)
                {
                    AddResultant(new InflictEffect(Performer, effectC.CreateInstance()));
                }

                sourced.FollowUpMethod?.Invoke(this);
            }
            else if (PlayedAbility is Ability.Unsourced unsourced)
            {
                unsourced.ActionMethod?.Invoke(this);
            }
        }

        protected override void InternalUndo()
        {
            throw new System.NotImplementedException();
        }

        public PlayAbility(Player performer, Ability ability, IList<Unit> participants) : base(performer)
        {
            PlayedAbility = ability;
            ParticipatingUnits = new Unit[participants.Count];
            for (int i = 0; i < participants.Count; i++) ParticipatingUnits[i] = participants[i];
            ExternalResultantEvent?.Invoke(this);
        }

        public void Prompt()
        {

        }

        public class PromptArgs
        {

        }
    }


}
