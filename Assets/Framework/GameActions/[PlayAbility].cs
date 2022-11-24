using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class GameAction
{

    public class PlayAbility : GameAction
    {
        public Ability PlayedAbility { get; private set; }
        public Unit[] ParticipatingUnits { get; private set; }

        public static event GameActionEventHandler<PlayAbility> OnPerform;
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

            OnPerform?.Invoke(this);
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
        }

        public void Prompt()
        {

        }

        public class PromptArgs
        {

        }
    }


}
