using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public abstract class ContainerAction : GameAction
{

    private readonly List<GameAction> _containedActions;

    protected override void InternalPerform()
    {
        for (int i = 0; i < _containedActions.Count; i++) _containedActions[i].Perform();
    }
    protected override void InternalUndo()
    {
        for (int i = _containedActions.Count - 1; i >= 0; i--) _containedActions[i].Undo();
    }

    protected ContainerAction(Player performer, IEnumerable<GameAction> contains) : base(performer)
    {
        _containedActions = new(contains);
    }

    public override string ToString()
    {
        return $" {{{string.Join(" | ", _containedActions)}}}" + base.ToString();
    }
}