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
    /// 
    /// </summary>
    /// <remarks>
    /// <c> <see langword="async"/> IAsyncEnumerable&lt;<see cref="GameAction"/>&gt; EvaluationResultantAdderMethod(<see cref="GameAction"/> <paramref name="action"/>) { }</c><br></br>
    /// - <paramref name="action"/> : The <see cref="GameAction"/> being evaluated.<br></br>
    /// <see langword="yield"/> -> Each <see cref="GameAction"/> to be evaluated as a resultant of <paramref name="action"/>.<br></br>
    /// (Each <see langword="yield"/> evaluates itself before moving onto the next)
    /// </remarks>
    /// <param name="action"></param>
    /// <returns></returns>
    public delegate IAsyncEnumerable<GameAction> EvaluationResultantAdder(GameAction action);

    //consider changing from static, kinda lazy
    private readonly static List<EvaluationResultantAdder> _externalEvaluationAdders = new();
    private readonly List<EvaluationResultantAdder> _implicitResultantAdders = new();
    /// <summary>
    /// [Custom Return Event]<br></br>
    /// Occurs when any <see cref="GameAction"/> is evaluated.<br></br>
    /// Used to externally add resultants upon evaluation (Passives, game events, etc.)
    /// </summary>
    /// <remarks>
    /// <inheritdoc cref="EvaluationResultantAdder"/>
    /// </remarks>
    public static GuardedCollection<EvaluationResultantAdder> ExternalEvaluation = new(_externalEvaluationAdders);

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

    /*
     * DEV NOTE:
     * Alr, so shit works, thats pretty poggers. still a bug when fully cancelling a split move.
     * and please DO NOT FUCKING FORGET TO UPDATE DOCS, because of the paradigm shift a whole bunch of docs
     * are just obsolete and wrong.
     */
    protected GameAction(Player performer)
    {
        Performer = performer;
    }

    /// <summary>
    /// Makes <paramref name="action"/> an implicit resultant of this <see cref="GameAction"/>. <br></br>
    /// (Adds to <see cref="ResultantActions"/>)
    /// </summary>
    /// <remarks>
    /// > Implicit resultants are not evaluated until the parent <see cref="GameAction"/> is evaluated.
    /// </remarks>
    /// <param name="action"></param>
    /// <returns>
    /// <see langword="this"/> <see cref="GameAction"/>
    /// </returns>
    public GameAction AddImplicitResultant(GameAction action)
    {
        if (action is not null)
        {
            _implicitResultantAdders.Add(_ => action.WrappedAsync());
        }   

        return this;
    }

    /// <summary>
    /// Adds <paramref name="action"/> to the main action stack, evaluating and performing it.
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
        InternalPerform();
        await InternalEvaluate();
        List<EvaluationResultantAdder> resultantAdders = new(_implicitResultantAdders);
        resultantAdders.AddRange(_externalEvaluationAdders);
        foreach(var eval in resultantAdders)
        {
            await foreach(var resultant in eval(this))
            {
                if (resultant == null) continue;
                _resultantActions.Add(resultant);
                await resultant.Evaluate();
            }
        }
    }
    protected virtual Task InternalEvaluate() => Task.CompletedTask;

    public override string ToString()
    {
        return $" : {Performer} [{string.Join(" | ", _resultantActions)}]";
    }
}
