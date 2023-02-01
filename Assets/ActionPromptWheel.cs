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
    public Player Performer { get; private set; }
    public Selectable Root { get; private set; }

    public HashSet<ActionPromptWheelOption> Options => new(_options);
    private HashSet<ActionPromptWheelOption> _options;

    public ActionPromptWheel Init(Player performer, Selectable root, IEnumerable<ManualAction> actions, ActionPromptWheelOption optionPrefab, float radius)
    {
        Performer = performer;
        Root = root;
        _options = new();
        foreach (var action in actions)
        {
            _options.Add(Instantiate(optionPrefab, transform).Init(this, action));
        }
        SpreadOptions(radius);
        return this;
    }

    private void SpreadOptions(float radius)
    {
        int i = 0;
        float inc = 360 / _options.Count;
        foreach (var option in _options)
        {
            option.transform.localPosition = ((inc * i) + 90, radius).PolarToCartesian(true);
        }
    }

}
