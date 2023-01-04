using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public partial class Passive
{
    
    /// <summary>
    /// [ : ] <see cref="Passive"/>
    /// </summary>
    public class Agile : Passive
    {
        public Agile(string name) : base(name) { }

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

        private Task Effect(Player performer, GameAction.Move.Info info)
        {
            var O = Task.CompletedTask;
            if (performer != EmpoweredPlayer) return O;
            if (info is not GameAction.Move.PathedInfo pathed) return O;

            pathed.Distance += 1;
            pathed.MaxDistancePerUnit += 1;
            return O;
        }
    }

}
