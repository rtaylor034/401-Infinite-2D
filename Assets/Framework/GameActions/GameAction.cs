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
    /// GameActions that occured as a result of this <see cref="GameAction"/>. <br></br>
    /// > Resultant GameActions will be performed and undone whenever this <see cref="GameAction"/> is.
    /// </summary>
    /// <remarks>
    /// <i>See <see cref="AddResultant(GameAction)"/></i>
    /// </remarks>
    public List<GameAction> ResultantActions => new(_resultantActions);

    /// <summary>
    /// The Player that performed this <see cref="GameAction"/>.
    /// </summary>
    public Player Performer { get; private set; }

    /// <inheritdoc cref="ResultantActions"/>
    private readonly List<GameAction> _resultantActions = new();

    #region Quick Documentation Inherits
#pragma warning disable IDE0052
#pragma warning disable IDE1006

    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// Primarily used to add resultants to this <see cref="GameAction"/>. <br></br>
    /// <i>> See <see cref="AddResultant(GameAction)"/></i>
    /// </remarks>
    private readonly bool __DOC__ExternalResultantEvent;

#pragma warning restore IDE1006
#pragma warning restore IDE0052
    #endregion

    /// <summary>
    /// Performs this <see cref="GameAction"/> and all resultant GameActions.
    /// </summary>
    /// <remarks>
    /// <b>SAFETY:</b> Only should be called by <see cref="GameManager"/>.
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
    /// <b>SAFETY:</b> Only should be called by <see cref="GameManager"/>.
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

    //BAD DOC, UPDATE
    /// <summary>
    /// Makes <paramref name="action"/> a resultant of this <see cref="GameAction"/>. <br></br>
    /// <i>(See <see cref="ResultantActions"/>)</i> <br></br> <br></br>
    /// USE <see cref="AddLateResultant(GameAction)"/> IF: <br></br>
    /// Resultant is being added anytime after immediate construction of this <see cref="GameAction"/>.
    /// </summary>
    /// <param name="action"></param>
    /// <remarks>
    /// Returns: <see langword="this"/>.
    /// </remarks>
    public GameAction AddResultant(GameAction action)
    {
        _resultantActions.Add(action);
        return this;
    }

    //NEED DOC
    public GameAction AddLateResultant(GameAction action)
    {
        AddResultant(action);
        action.Perform();
        Debug.Log($"(Action Resultant Late-Added) -> {action}");
        return this;
    }

    /// <summary>
    /// Adds <paramref name="action"/> to the main action stack, finalizing and performing it.
    /// </summary>
    /// <remarks>
    /// Primary method for making GameActions part of the game.
    /// </remarks>
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
