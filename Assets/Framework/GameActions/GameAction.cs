using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Unity.VisualScripting;
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
    /// <c>EventSubscriberMethod(<see cref="GameAction"/> <paramref name="action"/>) { }</c>
    /// </remarks>
    /// <typeparam name="T"></typeparam>
    /// <param name="action"></param>
    public delegate Task EvaluationEventHandler(GameAction action);

    //consider changing from static, kinda lazy
    private readonly static List<EvaluationEventHandler> _onEvaluationEventSubscribers = new();
    public static GuardedCollection<EvaluationEventHandler> OnEvaluationEvent = new(_onEvaluationEventSubscribers);

    /// <summary>
    /// GameActions that occured as a result of this <see cref="GameAction"/>. <br></br>
    /// > Resultant GameActions will be performed and undone whenever this <see cref="GameAction"/> is.
    /// </summary>
    /// <remarks>
    /// <i>See <see cref="AddResultant(GameAction)"/></i>
    /// </remarks>
    public List<GameAction> ResultantActions => new(_resultantActions);
    //consider removing public access from resultant actions, as there is no need, and resultant action reliant behavior is not clean.

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
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// <c><see cref="int"/> ChangeFunctionMethod(<see cref="int"/> <paramref name="originalValue"/>) { }</c> <br></br>
    /// - <paramref name="originalValue"/> : The value before applying the change function. <br></br>
    /// - <see langword="return"/> : The changed value.
    /// </remarks>
    private readonly bool __DOC__ChangeFunction;

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
        Debug.Log("UNDO - " + ToString());
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

    /// <summary>
    /// Makes <paramref name="action"/> a resultant of this <see cref="GameAction"/>. <br></br>
    /// (Adds to <see cref="ResultantActions"/>)
    /// </summary>
    /// <param name="action"></param>
    /// <returns>
    /// <see langword="this"/> <see cref="GameAction"/>
    /// </returns>
    public async Task<GameAction> AddResultant(GameAction action)
    {
        if (action is not null)
        {
            _resultantActions.Add(action);
        }   

        return this;
    }

    /// <summary>
    /// Adds <paramref name="action"/> to the main action stack, finalizing and performing it.
    /// </summary>
    /// <remarks>
    /// Primary method for making GameActions part of the game.
    /// </remarks>
    /// <param name="action"></param>
    public static async Task Declare(GameAction action)
    {
        if (action == null) return;

        await action.Evaluate();
        Debug.Log($"(Action Declare) {action}");
        GameManager.GAME.PushGameAction(action);
        
    }

    private async Task Evaluate()
    {
        await InternalEvaluate();
        InternalPerform();
        //PositionChanges from move must somehow performed first without disrupting flow/perform twice?
        foreach (var externalEvaluation in new List<EvaluationEventHandler>(_onEvaluationEventSubscribers))
        {
            await externalEvaluation(this);
        }
        
        for (int i = 0; i < _resultantActions.Count; i++) await _resultantActions[i].Evaluate();
    }
    protected virtual Task InternalEvaluate() => Task.CompletedTask;

    public override string ToString()
    {
        return $" : {Performer} [{string.Join(" | ", _resultantActions)}]";
    }
}
