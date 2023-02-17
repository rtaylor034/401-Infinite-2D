using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// [ : ] <see cref="MonoBehaviour"/>
/// </summary>
public class ActionPromptWheel : MonoBehaviour
{
    /// <summary>
    /// The <see cref="Player"/> that is being prompted to make an action.
    /// </summary>
    public Player Performer { get; private set; }
    /// <summary>
    /// The <see cref="Selectable"/> that was selected to bring up this action wheel.
    /// </summary>
    public Selectable Root { get; private set; }

    /// <summary>
    /// The <see cref="ActionPromptWheelOption"/> objects that are parented to this wheel.
    /// </summary>
    public HashSet<ActionPromptWheelOption> Options => new(_options);
    private HashSet<ActionPromptWheelOption> _options;

    /// <summary>
    /// <b>[MUST BE CALLED AFTER INSTANTIATION]</b> (<see cref="Object.Instantiate(Object)"/>)
    /// </summary>
    /// <param name="performer"></param>
    /// <param name="root"></param>
    /// <param name="actions"></param>
    /// <param name="optionPrefab"></param>
    /// <returns></returns>
    public ActionPromptWheel Init(Player performer, Selectable root, IEnumerable<ManualAction> actions, ActionPromptWheelOption optionPrefab)
    {
        Performer = performer;
        Root = root;
        _options = new();
        foreach (var action in actions)
        {
            _options.Add(Instantiate(optionPrefab, transform).Init(this, action));
        }
        SpreadOptions(1);
        return this;
    }

    private void SpreadOptions(float radius)
    {
        int i = 0;
        float inc = 360 / _options.Count;
        foreach (var option in _options)
        {
            option.transform.localPosition = ((inc * i) + 90, radius).PolarToCartesian(true);
            i++;
        }
    }

}
