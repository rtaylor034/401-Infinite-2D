using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public abstract partial class UnitEffect
{
    /// <summary>
    /// The <see cref="Unit"/> that this <see cref="UnitEffect"/> is inflicted upon.
    /// </summary>
    /// <remarks>
    /// <i>Is <see langword="null"/> until <see cref="SetActive(bool, Unit, Player)"/> is called.<br></br>
    /// > If all works correctly, this situation should never come up.</i>
    /// </remarks>
    public Unit AffectedUnit { get; private set; }
    /// <summary>
    /// The <see cref="Player"/> that inflicted this <see cref="UnitEffect"/>.
    /// </summary>
    /// <remarks>
    /// <inheritdoc cref="AffectedUnit"/>
    /// </remarks>
    public Player Inflicter { get; private set; }
    /// <summary>
    /// The amount of Turns this <see cref="UnitEffect"/> has left to last.
    /// </summary>
    /// <remarks>
    /// <i>If set to 1 (default), the effect will last for the Turn it is inflicted, aswell as the Turn after.</i>
    /// </remarks>
    public int Duration { get; private set; }

    protected UnitEffect(int duration)
    {
        Duration = duration;
        GameAction.ExternalEvaluation += CallWhenInflicted;
    }

    /// <summary>
    /// Sets this effect to be active on <paramref name="affectedUnit"/>, inflicted by <paramref name="inflicter"/>. <br></br>
    /// <paramref name="val"/> = TRUE : Activate this effect. <br></br>
    /// <paramref name="val"/> = FALSE : Deactivate this effect.
    /// </summary>
    /// <param name="val"></param>
    /// <param name="affectedUnit"></param>
    /// <param name="inflicter"></param>
    public void SetActive(bool val, Unit affectedUnit, Player inflicter)
    {
        AffectedUnit = affectedUnit;
        Inflicter = inflicter;
        InternalSetup(val);
        if (val)
        {
            GameAction.ExternalEvaluation += TickDown;
        } 
        else
        {
            GameAction.ExternalEvaluation -= TickDown;
        }
        
    }

    /// <summary>
    /// Sets this UnitEffect's Duration to <paramref name="val"/>. (Should only be called from <see cref="GameAction"/>[ : ])
    /// </summary>
    /// <param name="val"></param>
    public void UpdateDuration(int val)
    {
        Duration = val;
    }
    private async IAsyncEnumerable<GameAction> CallWhenInflicted(GameAction action)
    {
        if (action is not GameAction.InflictEffect effect) yield break;
        if (effect.Effect != this) yield break;
        GameAction.ExternalEvaluation -= CallWhenInflicted;

        await foreach (var a in WhenInflicted(effect)) yield return a;
    }

    /// <summary>
    /// <b>[abstract]</b> <br></br>
    /// Called when <see cref="SetActive(bool, Unit, Player)"/> is called with its <paramref name="val"/>. <br></br>
    /// > Used to subscribe/unsubscribe the appropriate methods for each derivation[ : ] of <see cref="UnitEffect"/>
    /// </summary>
    /// <param name="val"></param>
    protected abstract void InternalSetup(bool val);

    /// <summary>
    /// <b>[virtual]</b><br></br>
    /// Called when <see langword="this"/> is inflicted via a <see cref="GameAction.InflictEffect"/>.
    /// </summary>
    /// <remarks>
    /// (<c><see langword="this"/>.AffectedUnit</c> has not been set yet, use <c><paramref name="action"/>.AffectedUnit</c>)
    /// </remarks>
    /// <param name="action"></param>
    protected virtual async IAsyncEnumerable<GameAction> WhenInflicted(GameAction.InflictEffect action)
    {
        await Task.CompletedTask; yield break;
    }

    private async IAsyncEnumerable<GameAction> TickDown(GameAction action)
    {
        if (action is not GameAction.Turn turn) yield break;
        yield return new GameAction.EffectDurationChange(action.Performer, this, d => d - 1);

        await Task.CompletedTask;

    }

    public override string ToString()
    {
        return $"[{GetType().Name}:{Duration}]";
    }
}
