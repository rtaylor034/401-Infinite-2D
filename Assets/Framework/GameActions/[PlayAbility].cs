using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class GameAction
{

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
                    //realistically should only have 1 target (ParticipatingUnits[1]), but this is multitarget support for no reason.
                    for (int i = 1; i < ParticipatingUnits.Length; i++)
                        AddResultant(new InflictEffect(Performer, effectC.CreateInstance(), ParticipatingUnits[i]));
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
            throw new NotImplementedException();
        }

        public class PromptArgs
        {

        }
    }


}
