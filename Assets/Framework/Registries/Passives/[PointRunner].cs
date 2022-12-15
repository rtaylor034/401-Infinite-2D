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

        private bool _triggerable;
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
            
            if (!_triggerable) return;
            if (action is not GameAction.Move move || action.Performer != EmpoweredPlayer) return;

            var u = move.MovedUnit;
            if (u.Team != action.Performer.Team) return;

            foreach(Hex hex in u.Board.HexesAt(move.ToPos.GetAdjacent()))
            {
                if (hex.Occupant == null) continue;
                if (hex.Occupant.Team == u.Team && hex.Occupant != u)
                {
                    _triggerable = false;

                    /*
                    var firstSplit = await GameAction.Move.Prompt(
                        new GameAction.Move.PromptArgs.Pathed(EmpoweredPlayer, u, 2));
                    await action.AddResultant(firstSplit);

                    var secondSplit = await GameAction.Move.Prompt(
                        new GameAction.Move.PromptArgs.Pathed(EmpoweredPlayer, hex.Occupant, 2 - firstSplit.ToPos.RadiusBetween(firstSplit.FromPos)));
                    await action.AddResultant(secondSplit);
                    */
                    foreach(var split in await GameAction.Move.PromptSplit(
                        new GameAction.Move.PromptArgs.Pathed(EmpoweredPlayer, u, 2),
                        hex.Occupant.YieldAsEnumerable()))
                    {
                        await move.AddResultant(split);
                    }
                    
                }
                
            }

        }

        private Task Refresh(GameAction action)
        {
            //Need to make a GameAction that changes the value of _triggerable, could be dynamic, could be not
            Task o = Task.CompletedTask;
            if (action is not GameAction.Turn turn) return o;
            if (turn.ToPlayer == EmpoweredPlayer) _triggerable = true;

            return o;
        }
    }

}
