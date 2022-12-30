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

    /// <summary>
    /// Prompts the player to select an object from the given <paramref name="selectables"/>.
    /// <br></br>
    /// Releases <see langword="await"/> when the player makes a selection and returns it as a <see cref="SelectionArgs"/>. 
    /// </summary>
    /// <param name="selectables"> </param>
    public async Task<SelectionArgs> Prompt(IEnumerable<Selectable> selectables)
    {
        if (!selectables.Any()) return EmptyArgs;

        ControlledTask<SelectionArgs> promptTask = new();

        foreach (var s in selectables) s.EnableSelection(sel => promptTask.Resolve(ArgsOf(sel)));

        GameManager.INPUT.Selector.Cancel.performed += __Cancel;
        void __Cancel(InputAction.CallbackContext _) => promptTask.Resolve(CancelledArgs);

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

    /// <summary>
    /// [ : ] <see cref="CallbackArgs"/>
    /// </summary>
    public class SelectionArgs : CallbackArgs
    {
        /// <summary>
        /// The <see cref="Selectable"/> that was selected. <br></br>
        /// </summary>
        /// <remarks>
        /// > <see langword="null"/> if selection was cancelled/unavailable.
        /// </remarks>
        public Selectable Selection { get; set; }
        /// <summary>
        /// TRUE if selection was manually cancelled.
        /// </summary>
        public bool WasCancelled { get; set; }
        /// <summary>
        /// TRUE if the the selection prompt was empty when given.
        /// </summary>
        public bool WasEmpty { get; set; }

        public SelectionArgs(Selectable selection, bool cancelled = false, bool empty = false)
        {
            WasEmpty = empty;
            Selection = selection;
            WasCancelled = cancelled;
        }
    }

}
