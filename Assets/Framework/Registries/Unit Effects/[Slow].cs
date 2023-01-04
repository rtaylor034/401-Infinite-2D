using System.Collections;
using System.Collections.Generic;
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
            throw new System.NotImplementedException();
        }
    }
}
