using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class Selector
{

    /// <summary>
    /// Prompts the player to select an object from the given <paramref name="selectables"/>.<br></br>
    /// Runs <paramref name="confirmMethod"/> when an object is selected or when prompt is cancelled.
    /// </summary>
    /// <param name="selectables"></param>
    /// <param name="confirmMethod"></param>
    /// <returns>
    /// FALSE if a prompt is already active (will do nothing).
    /// </returns>
    public async Task<SelectorArgs> Prompt(IEnumerable<Selectable> selectables)
    {
        CustomTask<SelectorArgs> promptTask = new();

        GameManager.INPUT.Selector.Cancel.performed += __Cancel;

        foreach (var s in selectables) s.EnableSelection(sel => promptTask.Resolve(new SelectorArgs(sel)));

        if (selectables.Count() == 0)
            promptTask.Resolve(new SelectorArgs(null, empty: true));
        
        SelectorArgs o = await promptTask;

        //finalize
        foreach (var s in selectables)
        {
            s.DisableSelection();
        }
        GameManager.INPUT.Selector.Cancel.performed -= __Cancel;
        return o;

        void __Cancel(InputAction.CallbackContext _)
        {
            promptTask.Resolve(new SelectorArgs(null, cancelled: true));
        }
    }

    public class SelectorArgs : CallbackArgs
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
