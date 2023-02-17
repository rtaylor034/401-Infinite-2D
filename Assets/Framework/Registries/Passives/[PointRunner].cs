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
                GameAction.ExternalEvaluation += Effect;
                GameAction.ExternalEvaluation += Refresh;
            } else
            {
                GameAction.ExternalEvaluation -= Effect;
                GameAction.ExternalEvaluation -= Refresh;
            }
        }

        private async IAsyncEnumerable<GameAction> Effect(GameAction action)
        {
            if (action is not GameAction.Move move) yield break;
            if (action.Performer != EmpoweredPlayer) yield break;
            if (!Triggerable) yield break;

            foreach(var m in move.PositionChanges)
            {
                if (m.AffectedUnit.Team != EmpoweredPlayer.Team) continue;
                foreach (var hex in m.AffectedUnit.Board.HexesAt(m.ToPos.GetAdjacent()))
                {
                    if (hex.Occupant != null && hex.Occupant.Team == EmpoweredPlayer.Team)
                    {
                        yield return new StateSet(this, false);
                        yield return new GameAction.EnergyChange(EmpoweredPlayer, EmpoweredPlayer, e => e + 1);
                        yield return await GameAction.Move.Prompt(EmpoweredPlayer,
                            new GameAction.Move.PathedInfo(m.AffectedUnit, hex.Occupant)
                            {
                                Distance = 2
                            });
                        break;
                    }

                }
            }
            
        }

        private async IAsyncEnumerable<GameAction> Refresh(GameAction action)
        {
            if (action is not GameAction.Turn turn) yield break;
            if (turn.ToPlayer == EmpoweredPlayer) yield return new StateSet(this, true);

            await Task.CompletedTask;
        }
    }

} 
