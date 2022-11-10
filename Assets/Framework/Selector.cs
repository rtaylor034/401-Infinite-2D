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

        if (_currentPrompt.Count() == 0) SelectionEmpty();

        return true;
    }

    private void SelectionConfirm(Selectable selection)
    {
        FinalizeSelection(new SelectorArgs(selection));
    }
    private void SelectionCancel()
    {
        FinalizeSelection(new SelectorArgs(null, cancelled: true));
    }
    private void SelectionEmpty()
    {
        FinalizeSelection(new SelectorArgs(null, empty: true));
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
        public bool WasEmpty { get; set; }

        public SelectorArgs(Selectable selection, bool cancelled = false, bool empty = false)
        {
            WasEmpty = empty;
            Selection = selection;
            WasCancelled = cancelled;
        }
    }

}
