using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// <b>abstract</b> [ : ] <see cref="MonoBehaviour"/>
/// </summary>
public abstract class Selectable : MonoBehaviour
{
    public delegate void SelectionCallback(Selectable s);

    private SelectionCallback _callbackMethod;
    private bool _isSelectable = false;


    #region Unity Messages
    private void OnMouseDown()
    {
        if (_isSelectable) SelectionConfirm();
    }

    private void OnMouseEnter()
    {
        if (_isSelectable) OnHover(true);
    }

    private void OnMouseExit()
    {
        if (_isSelectable) OnHover(false);
    }
    #endregion

    public void EnableSelection(SelectionCallback callback)
    {
        _isSelectable = true;
        _callbackMethod = callback;
        OnSelectable(true);
    }

    public void DisableSelection()
    {
        
        _isSelectable = false;
        _callbackMethod = null;
        OnSelectable(false);
    }

    private void SelectionConfirm()
    {
        OnSelected();
        _callbackMethod.Invoke(this);
    }

    //These methods are for unique visual effects when selection state changes.


    /// <summary>
    /// Called whenever this object is hovered over and is selectable.
    /// </summary>
    /// <remarks>
    /// <paramref name="state"/> is true on hover Enter, false on hover Exit.
    /// </remarks>
    /// <param name="state"></param>
    protected virtual void OnHover(bool state)
    {
        var s = GetComponent<SpriteRenderer>();

        if (state) s.color = new Color(s.color.r, s.color.g, s.color.b, 0.4f);
        else OnSelectable(true);
    }
    /// <summary>
    /// Called whenever this object is prompted to be selected.
    /// </summary>
    /// <remarks>
    /// <paramref name="state"/> is true when prompted, false when prompt ends.
    /// </remarks>
    /// <param name="state"></param>
    protected virtual void OnSelectable(bool state)
    {
        var s = GetComponent<SpriteRenderer>();
        if (state) s.color = new Color(s.color.r, s.color.g, s.color.b, 0.7f);
        else s.color = new Color(s.color.r, s.color.g, s.color.b, 1f);
    }
    /// <summary>
    /// Called when this object is selected.
    /// </summary>
    protected virtual void OnSelected()
    {
    }
}
