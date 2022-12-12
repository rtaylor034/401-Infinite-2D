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
    private static SelectionArgs ArgsOf(Selectable selection) => new SelectionArgs(selection);
    private static SelectionArgs CancelledArgs => new SelectionArgs(null, cancelled: true);
    private static SelectionArgs EmptyArgs => new SelectionArgs(null, empty: true);

    public async Task<SelectionArgs> Prompt(IEnumerable<Selectable> selectables)
    {
        CustomTask<SelectionArgs> promptTask = new();

        void __Cancel(InputAction.CallbackContext _) =>
            promptTask.Resolve(CancelledArgs);


        GameManager.INPUT.Selector.Cancel.performed += __Cancel;

        foreach (var s in selectables) s.EnableSelection(sel => promptTask.Resolve(ArgsOf(sel)));

        if (selectables.Count() == 0)
            promptTask.Resolve(EmptyArgs);

        SelectionArgs o = await promptTask;

        //finalize
        foreach (var s in selectables)
        {
            s.DisableSelection();
        }
        GameManager.INPUT.Selector.Cancel.performed -= __Cancel;

        return o;
    }

    public SelectionArgs SpoofSelection(Selectable selection)
    {
        return ArgsOf(selection);
    }

    public class SelectionArgs : CallbackArgs
    {
        public Selectable Selection { get; set; }
        public bool WasCancelled { get; set; }
        public bool WasEmpty { get; set; }

        public SelectionArgs(Selectable selection, bool cancelled = false, bool empty = false)
        {
            WasEmpty = empty;
            Selection = selection;
            WasCancelled = cancelled;
        }
    }

}
