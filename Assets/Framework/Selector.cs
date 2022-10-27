using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Selector : MonoBehaviour
{
    [SerializeField]
    private LayerMask _selectableLayer;
    [SerializeField]
    private Camera _camera;
    [SerializeField]
    private float _selectRange = 100f;

    private IEnumerable<ISelectable> _currentPrompt;
    private ISelectable _currentSelection;
    private Inputs _input;
    private Player _currentPlayer;

    public delegate void SelectionConfirmMethod(SelectionArgs args);

    private SelectionConfirmMethod _confirmMethod;
    private SelectionConfirmMethod _hoverMethod;

    #region Unity Events
    private void Awake()
    {
        _input = GameManager.GAME.Input;
        _currentSelection = null;
        _currentPlayer = null;
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

    #region Setups
    private void SInputs(bool set)
    {
        if (set)
        {
            _input.Selector.Click.performed += OnClick;
            _input.Selector.Cancel.performed += OnCancel;
        }
        else
        {
            _input.Selector.Click.performed -= OnClick;
            _input.Selector.Cancel.performed -= OnCancel;
        }


    }
    #endregion

    void Update()
    {
        if (_input.Selector.MouseMove.WasPerformedThisFrame())
        {
            HoverSelection();
        }
    }

    private bool HoverSelection()
    {
        if (Physics.Raycast(_camera.ScreenPointToRay(_input.Selector.MousePos.ReadValue<Vector2>()), out RaycastHit hit, _selectRange, _selectableLayer))
        {
            ISelectable tempsel = hit.collider.GetComponent<ISelectable>();
            if (tempsel == _currentSelection) return true;

            foreach (ISelectable comp in _currentPrompt)
            {
                if (comp != tempsel) continue;

                _currentSelection?.ToggleHovered(false);
                tempsel?.ToggleHovered(true);
                _currentSelection = tempsel;

                _hoverMethod?.Invoke(new SelectionArgs(_currentSelection, _currentPlayer));
                return true;
            }

        }

        //fail
        _currentSelection?.ToggleHovered(false);
        _currentSelection = null;
        _hoverMethod?.Invoke(new SelectionArgs(_currentSelection, _currentPlayer, true));
        return false;
    }

    #region Input Events
    private void OnClick(InputAction.CallbackContext context)
    {
        if (!context.action.IsPressed()) return;
        if (HoverSelection())
        {
            SelectionConfirm(_currentSelection);
            return;
        }
        Debug.Log("NOT SELECTABLE");
    }

    private void OnCancel(InputAction.CallbackContext context)
    {
        if (!context.action.IsPressed()) return;
        SelectionCancel();
    }
    #endregion
    /// <summary>
    /// Prompts the player to make a selection out of the objects specified in <paramref name="selectables"/>. <br></br>
    /// When the player makes their selection, confirmMethod is triggered.
    /// </summary>
    /// <remarks>
    /// <i>Returns false and does nothing if there is already a prompt active.</i>
    /// </remarks>
    /// <param name="selectables"></param>
    public bool Prompt(IEnumerable<ISelectable> selectables, Player player, SelectionConfirmMethod confirmMethod, SelectionConfirmMethod hoverMethod = null)
    {
        if (enabled) return false;
        _currentPrompt = selectables;
        _currentSelection = null;
        _currentPlayer = player;
        enabled = true;

        Debug.Log("PROMPTED");

        foreach (var s in _currentPrompt)
        {
            s.ToggleSelectable(true);
        }
        _hoverMethod = hoverMethod;
        _confirmMethod = confirmMethod;

        HoverSelection();

        foreach (ISelectable EXISTS in selectables) { return true; }

        //No selectables failsafe.
        Debug.LogWarning("Selector: No selection available, null returned");
        SelectionConfirm(null);
        return true;
    }

    /// <summary>
    /// Immediatly cancels current selector prompt. Returns false if none is active.
    /// </summary>
    public bool ForceCancel()
    {
        if (!enabled) return false;
        SelectionCancel();
        return true;
    }

    protected virtual void SelectionConfirm(ISelectable selection)
    {
        SelectionArgs args = new SelectionArgs(selection, _currentPlayer);

        foreach (var s in _currentPrompt)
        {
            s.ToggleSelectable(false);
        }

        enabled = false;
        _confirmMethod.Invoke(args);
    }

    protected virtual void SelectionCancel()
    {
        var args = new SelectionArgs(null, _currentPlayer, true);
        
        foreach (var s in _currentPrompt)
        {
            s.ToggleSelectable(false);
        }

        enabled = false;
        _confirmMethod.Invoke(args);
    }

}

public class SelectionArgs : System.EventArgs
{
    public readonly bool wasCancelled;
    public ISelectable Selection { get; }
    public Player Player { get; }
    public SelectionArgs(ISelectable selection, Player player, bool cancelled = false)
    {
        Player = player;
        wasCancelled = cancelled;
        Selection = selection;
    }
    public SelectionArgs(SelectionArgs args)
    {
        Player = args.Player;
        wasCancelled = args.wasCancelled;
        Selection = args.Selection;
    }

}