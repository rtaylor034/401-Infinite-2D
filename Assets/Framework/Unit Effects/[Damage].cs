using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public partial class UnitEffect
{

    public class Damage : UnitEffect
    {
        public int Amount { get; private set; }
        public Damage(int duration, int amount) : base(duration)
        {
            Amount = amount;
        }
        public Damage(int d) : this(d, 1) { }

        protected override void InternalSetup(bool val)
        {
            if (val)
            {
                GameAction.Turn.ExternalResultantEvent += Effect;
            } else
            {
                GameAction.Turn.ExternalResultantEvent -= Effect;
            }
        }

        private void Effect(GameAction.Turn action)
        {
            if (action.FromPlayer.Team == AffectedUnit.Team)
                action.AddResultant(new GameAction.HPChange(Inflicter, AffectedUnit, hp => hp - Amount));
        }
    }
}
