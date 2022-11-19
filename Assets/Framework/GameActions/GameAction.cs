using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// <b>abstract</b>
/// </summary>
public abstract partial class GameAction
{
    /// <summary>
    /// [Event Handler Delegate] <br></br>
    /// </summary>
    /// <remarks>
    /// <c>EventSubscriberMethod(<typeparamref name="T"/> <paramref name="action"/>) { }</c>
    /// </remarks>
    /// <typeparam name="T"></typeparam>
    /// <param name="action"></param>
    public delegate void GameActionEventHandler<T>(T action) where T : GameAction;

    /// <summary>
    /// GameActions that were declared as a result of this <see cref="GameAction"/>. <br></br>
    /// > Resultant GameActions will be performed and undone whenever this <see cref="GameAction"/> is.
    /// </summary>
    /// <remarks>
    /// <i>Only GameActions that have a DeclareAsResultant() method can be resultant actions.</i>
    /// </remarks>
    public List<GameAction> ResultantActions => new(_resultantActions);

    /// <summary>
    /// The Player that performed this <see cref="GameAction"/>.
    /// </summary>
    public Player Performer { get; private set; }

    /// <inheritdoc cref="ResultantActions"/>
    private readonly List<GameAction> _resultantActions = new();

    /// <summary>
    /// Performs this <see cref="GameAction"/> and all resultant GameActions.
    /// </summary>
    /// <remarks>
    /// SAFETY: Only be called by <see cref="GameManager"/>.
    /// </remarks>
    public void Perform()
    {
        InternalPerform();
        for (int i = 0; i < _resultantActions.Count; i++) _resultantActions[i].Perform();
    }
    /// <summary>
    /// Undoes this <see cref="GameAction"/> and all resultant GameActions.
    /// </summary>
    /// <remarks>
    /// SAFETY: Only be called by <see cref="GameManager"/>.
    /// </remarks>
    public void Undo()
    {
        for (int i = _resultantActions.Count - 1; i >= 0; i--) _resultantActions[i].Undo();
        InternalUndo();
    }

    /// <summary>
    /// <b>[abstract]</b><br></br>
    /// The internal method that is unique to each derivation[ : ] of <see cref="GameAction"/> that carries out the action. <br></br>
    /// </summary>
    /// <remarks>
    /// This method is called by <see cref="Perform"/>.
    /// </remarks>
    protected abstract void InternalPerform();

    /// <summary>
    /// <b>[abstract]</b><br></br>
    /// The internal method that is unique to each derivation[ : ] of <see cref="GameAction"/> that carries out the inverse action. <br></br>
    /// </summary>
    /// <remarks>
    /// This method is called by <see cref="Perform"/>.
    /// </remarks>
    protected abstract void InternalUndo();

    protected GameAction(Player performer)
    {
        Performer = performer;
    }

    //UPDATEDOC: now public and part of the general workflow
    /// <summary>
    /// Adds <paramref name="action"/> to this GameAction's resultant actions and runs it's <see cref="InternalPerform"/>.
    /// </summary>
    /// <param name="action"></param>
    /// <returns></returns>
    public GameAction AddResultant(GameAction action)
    {
        _resultantActions.Add(action);
        return this;
    }

    //UPDATEDOC: the Declare() method for all gameactions, adds to the main action stack and performs it.
    /// <summary>
    /// MUST be called at the end of all Declare() methods.
    /// </summary>
    /// <param name="action"></param>
    public static void Declare(GameAction action)
    {
        action.Perform();
        GameManager.GAME.PushGameAction(action);
        Debug.Log($"(Action Declare) {action}");
    }

    public override string ToString()
    {
        return $" : {Performer} [{string.Join(" | ", _resultantActions)}]";
    }
}
