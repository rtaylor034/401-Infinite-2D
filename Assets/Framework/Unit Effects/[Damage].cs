using System.Collections;
using System.Collections.Generic;
using System.Net;
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
                GameAction.InflictEffect.ExternalResultantEvent += ShieldHandling;
            } else
            {
                GameAction.InflictEffect.ExternalResultantEvent -= ShieldHandling;
            }
        }

        private void ShieldHandling(GameAction.InflictEffect action)
        {
            if (action.Effect is not UnitEffect.Shield) return;
            action.AddResultant(new GameAction.HPChange(Inflicter, AffectedUnit, hp => hp + 1));

            //once a shield has been used to absorb this damage, stop listening for shields.
            GameAction.InflictEffect.ExternalResultantEvent -= ShieldHandling;
        }

        protected override void WhenInflicted(GameAction.InflictEffect action)
        {
            action.AddResultant(new GameAction.HPChange(action.Performer, action.AffectedUnit, hp => hp - 1));
        }
    }
}
