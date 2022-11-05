using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class Selector : MonoBehaviour
{
    public delegate void SelectionConfirmMethod(SelectorArgs args);

    private IEnumerable<Selectable> _currentPrompt;
    private SelectionConfirmMethod _confirmMethod;

    #region Setups
    private void SInputs(bool value)
    {
        if (value == true)
        {
            GameManager.INPUT.Selector.Cancel.performed += InputCancel;
        } else
        {
            GameManager.INPUT.Selector.Cancel.performed -= InputCancel;
        }
    }

    #endregion

    #region Unity Messages
    private void Awake()
    {
        enabled = false;
    }

    private void OnEnable()
    {
        SInputs(true);
    }

    private void OnDisable()
    {
        SInputs(false);
    }
    #endregion

    //TODO: add Player argument at some point
    /// <summary>
    /// Prompts the player to select an object from the given <paramref name="selectables"/>.<br></br>
    /// Runs <paramref name="confirmMethod"/> when an object is selected or when prompt is cancelled.
    /// </summary>
    /// <param name="selectables"></param>
    /// <param name="confirmMethod"></param>
    /// <returns>
    /// false if a prompt is already active (will do nothing). | true otherwise.
    /// </returns>
    public bool Prompt(IEnumerable<Selectable> selectables, SelectionConfirmMethod confirmMethod)
    {
        if (enabled) return false;

        enabled = true;
        _currentPrompt = selectables;
        _confirmMethod = confirmMethod;

        foreach(var s in _currentPrompt)
        {
            s.EnableSelection(SelectionConfirm);
        }

        if (_currentPrompt.Count() == 0) throw new ArgumentException("Empty select prompt given.");

        return true;
    }

    private void SelectionConfirm(Selectable selection)
    {
        FinalizeSelection(new SelectorArgs(selection, false));
    }

    private void FinalizeSelection(SelectorArgs args)
    {
        enabled = false;
        foreach (var s in _currentPrompt)
        {
            s.DisableSelection();
        }
        _confirmMethod?.Invoke(args);

        
        
    }

    private void SelectionCancel()
    {
        FinalizeSelection(new SelectorArgs(null, true));
    }

    /// <summary>
    /// Forces the selection prompt to cancel immediatly.
    /// </summary>
    /// <returns>
    /// true if prompt was cancelled. | false if no prompt was active.
    /// </returns>
    public bool ForceCancel()
    {
        if (!enabled) return false;
        SelectionCancel();
        return true;
    }

    #region Input Methods

    public void InputCancel(InputAction.CallbackContext context)
    {
        SelectionCancel();
    }

    #endregion

    public class SelectorArgs : EventArgs
    {
        public Selectable Selection { get; set; }
        public bool WasCancelled { get; set; }

        public SelectorArgs(Selectable selection, bool cancelled)
        {
            Selection = selection;
            WasCancelled = cancelled;
        }
    }

}
