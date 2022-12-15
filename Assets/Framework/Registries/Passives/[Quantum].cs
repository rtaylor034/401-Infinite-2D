using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public partial class Passive
{
    
    /// <summary>
    /// [ : ] <see cref="Passive"/>
    /// </summary>
    public class Quantum : Passive
    {
        public Quantum(string name) : base(name) { }

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

        private void Effect(GameAction.Move.PromptArgs args)
        {
            if (args is not GameAction.Move.PromptArgs.Pathed pathed) return;

            if (pathed.Performer == EmpoweredPlayer && pathed.MovingUnit.Team == EmpoweredPlayer.Team)
            {
                pathed.Distance += 1;
            }

            args.ReturnCode = -1;
        }
    }

}
