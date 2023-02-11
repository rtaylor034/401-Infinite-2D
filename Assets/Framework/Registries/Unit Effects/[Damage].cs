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
                GameAction.ExternalEvaluation += ShieldHandling;
            } else
            {
                GameAction.ExternalEvaluation -= ShieldHandling;
            }
        }

        private async IAsyncEnumerable<GameAction> ShieldHandling(GameAction action)
        {
            await Task.CompletedTask;

            if (action is not GameAction.InflictEffect effect) yield break;
            if (effect.Effect is not UnitEffect.Shield) yield break;
            //once a shield has been used to absorb this damage, stop listening for shields.
            GameAction.ExternalEvaluation -= ShieldHandling;

            yield return new GameAction.HPChange(Inflicter, AffectedUnit, hp => hp + 1);
        }

        protected override async IAsyncEnumerable<GameAction> WhenInflicted(GameAction.InflictEffect action)
        {
            await Task.CompletedTask;

            yield return new GameAction.HPChange(action.Performer, action.AffectedUnit, hp => hp - 1);
        }
    }
}
