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
            
            if (action is not GameAction.Move move || action.Performer != EmpoweredPlayer) return;

            if (!Triggerable) return;

            var u = move.MovedUnit;
            if (u.Team != action.Performer.Team) return;

            foreach(Hex hex in u.Board.HexesAt(move.ToPos.GetAdjacent()))
            {
                if (hex.Occupant == null) continue;
                if (hex.Occupant.Team == u.Team && hex.Occupant != u)
                {
                    await move.AddResultant(new StateSet(this, false));

                    foreach(var split in await GameAction.Move.PromptSplit(
                        new GameAction.Move.PromptArgs.Pathed(EmpoweredPlayer, u, 2),
                        hex.Occupant.Wrapped()))
                    {
                        await move.AddResultant(split);
                    }
                    
                }
                
            }

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
