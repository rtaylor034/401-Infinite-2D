using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

public partial class Passive
{
    
    /// <summary>
    /// [ : ] <see cref="Passive"/>
    /// </summary>
    public class PointRunner : Passive
    {
        public PointRunner(string name) : base(name) { }

        public bool Triggerable => (bool)State[0];
        protected override void InternalSetup(bool val)
        {
            if (val)
            {
                GameAction.OnEvaluationEvent += Effect;
                GameAction.OnEvaluationEvent += Refresh;
            } else
            {
                GameAction.OnEvaluationEvent -= Effect;
                GameAction.OnEvaluationEvent -= Refresh;
            }
        }

        private async Task Effect(GameAction action)
        {
            throw new System.NotImplementedException();
            if (action is not GameAction.Move move || action.Performer != EmpoweredPlayer) return;

            if (!Triggerable) return;
            
        }

        private async Task Refresh(GameAction action)
        {
            //Need to make a GameAction that changes the value of _triggerable, could be dynamic, could be not
            if (action is not GameAction.Turn turn) return;
            if (turn.ToPlayer == EmpoweredPlayer) await turn.AddResultant(new StateSet(this, true));

            return;
        }
    }

}
