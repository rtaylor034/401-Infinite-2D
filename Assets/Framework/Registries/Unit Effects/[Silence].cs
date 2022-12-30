using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public partial class UnitEffect
{

    public class Silence : UnitEffect
    {
        public Silence(int d) : base(d) { }

        protected override void InternalSetup(bool val)
        {
            if (val)
            {
                GameAction.PlayAbility.OnPromptEvent += Effect;
            } else
            {
                GameAction.PlayAbility.OnPromptEvent -= Effect;
            }
        }

        private void Effect(GameAction.PlayAbility.PromptArgs args)
        {
            if (args.Ability is not Ability.Sourced sourced) return;

            sourced.SourceConditions.Add((p, s) => !(s == AffectedUnit && s.Team == p.Team));
            sourced.TargetingConditions.Add((p, _, t) => !(t == AffectedUnit && p.Team == t.Team));
        }
    }
}
