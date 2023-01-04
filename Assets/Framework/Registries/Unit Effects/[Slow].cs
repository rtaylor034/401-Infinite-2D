using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using UnityEngine;

public partial class UnitEffect
{

    public class Slow : UnitEffect
    {
        public Slow(int d) : base(d) { }

        protected override void InternalSetup(bool val)
        {
            if (val)
            {
                GameAction.Move.OnPromptEvent += Effect;
            } else
            {
                GameAction.Move.OnPromptEvent -= Effect;
            }
        }

        private Task Effect(GameAction.Move.Info info)
        {
            Task O = Task.CompletedTask;
            if (info is not GameAction.Move.PathedInfo pathed) return O;

            //doubles the weight at the time of evaluation
            var funcs = pathed.PathingWeightFunctions.InvokeAll(AffectedUnit);
            pathed.PathingWeightFunctions.Add(unit => (p, n) =>
            (unit == AffectedUnit) ? funcs.InvokeAll(p, n).Sum() : 0);

            return O;
        }
    }
}
