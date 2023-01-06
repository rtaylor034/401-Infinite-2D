using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

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

        private Task Effect(Player performer, GameAction.Move.Info info)
        {
            var O = Task.CompletedTask;
            if (performer != EmpoweredPlayer) return O;

            foreach (var u in new List<Unit>(info.MovingUnits)) info.MovingUnits.UnionWith(u.Allies);

            return O;
        }

    }

}
