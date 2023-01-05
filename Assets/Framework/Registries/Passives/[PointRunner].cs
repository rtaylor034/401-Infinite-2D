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
            if (action is not GameAction.Move move) return;
            if (action.Performer != EmpoweredPlayer) return;
            if (!Triggerable) return;

            foreach(var m in move.PositionChanges)
            {
                if (m.AffectedUnit.Team != EmpoweredPlayer.Team) continue;
                foreach (var hex in m.AffectedUnit.Board.HexesAt(m.ToPos.GetAdjacent()))
                {
                    if (hex.Occupant != null && hex.Occupant.Team == EmpoweredPlayer.Team)
                    {
                        await action.AddResultant(new StateSet(this, false));
                        await action.AddResultant(new GameAction.EnergyChange
                            (EmpoweredPlayer, EmpoweredPlayer, e => e + 1));
                        await action.AddResultant(await GameAction.Move.Prompt(EmpoweredPlayer,
                            new GameAction.Move.PathedInfo(m.AffectedUnit, hex.Occupant)
                            {
                                Distance = 2,
                                PathingConditions = { GameAction.Move.PathedInfo.STANDARD_COLLISION }
                                
                            }));
                        break;
                    }

                }
            }
            
        }

        private async Task Refresh(GameAction action)
        {
            if (action is not GameAction.Turn turn) return;
            if (turn.ToPlayer == EmpoweredPlayer) await turn.AddResultant(new StateSet(this, true));

            return;
        }
    }

}
