using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// <b>abstract</b> <br></br>
/// Includes a single property (<see cref="ReturnCode"/>).
/// </summary>
public abstract class CallbackArgs 
{
    /// <summary>
    /// Custom general purpose return code. <br></br>
    /// > Meaning is to be defined by the user on a case-by-case usage (or not at all).
    /// </summary>
    public int ReturnCode { get; set; } = 0;
}
