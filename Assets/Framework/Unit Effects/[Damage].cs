using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using UnityEngine;

public partial class UnitEffect
{

    public class Damage : UnitEffect
    {
        public Damage(int duration) : base(duration) { }
        protected override void InternalSetup(bool val)
        {
            if (val)
            {
                GameAction.OnEvaluationEvent += ShieldHandling;
            } else
            {
                GameAction.OnEvaluationEvent -= ShieldHandling;
            }
        }

        private async Task ShieldHandling(GameAction action)
        {
            if (action is not GameAction.InflictEffect effect) return;
            if (effect.Effect is not UnitEffect.Shield) return;
            //once a shield has been used to absorb this damage, stop listening for shields.
            GameAction.OnEvaluationEvent -= ShieldHandling;

            await action.AddResultant(new GameAction.HPChange(Inflicter, AffectedUnit, hp => hp + 1));
        }

        protected override async Task WhenInflicted(GameAction.InflictEffect action)
        {
            await action.AddResultant(new GameAction.HPChange(action.Performer, action.AffectedUnit, hp => hp - 1));
        }
    }
}
