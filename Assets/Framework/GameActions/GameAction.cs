using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract partial class GameAction
{
    public delegate void GameActionEventHandler<T>(T action) where T : GameAction;
    public List<GameAction> ResultantActions => new(_resultantActions);
    public Player Performer { get; private set; }

    private readonly List<GameAction> _resultantActions = new();

    public void Perform()
    {
        InternalPerform();
        for (int i = 0; i < _resultantActions.Count; i++) _resultantActions[i].Perform();
    }
    public void Undo()
    {
        for (int i = _resultantActions.Count - 1; i >= 0; i--) _resultantActions[i].Undo();
        InternalUndo();
    }

    protected abstract void InternalPerform();
    protected abstract void InternalUndo();

    protected GameAction(Player performer)
    {
        Performer = performer;
    }

    protected GameAction AddResultant(GameAction action)
    {
        _resultantActions.Add(action);
        action.InternalPerform();
        return this;
    }

    private static void FinalizeDeclare(GameAction action)
    {
        GameManager.GAME.PushGameAction(action);
        Debug.Log($"(Action Declare) {action}");
    }

    public override string ToString()
    {
        return $" BY {Performer} [{string.Join(" | ", _resultantActions)}]";
    }
}
