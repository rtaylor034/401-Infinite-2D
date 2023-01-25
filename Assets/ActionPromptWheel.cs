using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// [ : ] <see cref="MonoBehaviour"/>
/// </summary>
public class ActionPromptWheel : MonoBehaviour
{
    public Player Performer { get; private set; }
    public Selectable Root { get; private set; }

    public HashSet<ActionPromptWheelEntry> Entries => new(_entries);
    private HashSet<ActionPromptWheelEntry> _entries;

    public ActionPromptWheel Init(Player performer, Selectable root, IEnumerable<ManualAction> actions, ActionPromptWheelEntry entryPrefab, float radius)
    {
        Performer = performer;
        Root = root;
        _entries = new();
        foreach (var action in actions)
        {
            _entries.Add(Instantiate(entryPrefab, transform).Init(this, action));
        }
        SpreadEntries(radius);
        return this;
    }

    private void SpreadEntries(float radius)
    {
        int i = 0;
        float inc = 360 / _entries.Count;
        foreach (var entry in _entries)
        {
            entry.transform.localPosition = (inc * i, radius).PolarToCartesian();
        }
    }

}
