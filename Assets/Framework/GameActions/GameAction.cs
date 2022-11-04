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
        for (int i = 0; i < _resultantActions.Count; i++) _resultantActions[i].Undo();
        Undo();
    }

    protected abstract void InternalPerform();
    protected abstract void InternalUndo();

    protected GameAction(Player performer)
    {
        Performer = performer;
    }

    protected void MakeResultantOf(GameAction action)
    {
        action._resultantActions.Add(this);
    }
}
