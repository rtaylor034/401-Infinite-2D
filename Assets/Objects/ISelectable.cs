using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISelectable
{

    /// <summary>
    /// Called when this object is prompted to be selectable to the player.
    /// </summary>
    /// <remarks>
    /// (Called by <see cref="Selector"/>)
    /// </remarks>
    /// <param name="toggle"></param>
    public void ToggleSelectable(bool toggle);
    public void ToggleHovered(bool toggle);


}
