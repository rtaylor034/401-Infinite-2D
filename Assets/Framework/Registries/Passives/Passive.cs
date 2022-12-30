using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// <b>abstract</b>
/// </summary>
public abstract partial class Passive
{
    public Player EmpoweredPlayer { get; private set; }
    public string Name { get; set; }
    public object[] State { get; private set; } = new object[0];

    protected Passive(string name)
    {
        Name = name;
    }

    public void SetActive(bool val, Player empoweredPlayer)
    {
        EmpoweredPlayer = empoweredPlayer;
        InternalSetup(val);
    }
    /// <summary>
    /// <b>[abstract]</b> <br></br>
    /// Called when <see cref="SetActive(bool, Player)"/> is called with its <paramref name="val"/>. <br></br>
    /// > Used to subscribe/unsubscribe the appropriate methods for each derivation[ : ] of <see cref="Passive"/>
    /// </summary>
    /// <param name="val"></param>
    protected abstract void InternalSetup(bool val);

    public override string ToString()
    {
        return $"!{Name}!";
    }

    /// <summary>
    /// [ : ] <see cref="GameAction"/>
    /// </summary>
    protected class StateSet : GameAction
    {
        public Passive PassiveObj { get; private set; }
        public object[] AfterState { get; private set; }
        public object[] BeforeState { get; private set; }

        protected override void InternalPerform() => PassiveObj.State = AfterState;
        protected override void InternalUndo() => PassiveObj.State = BeforeState;

        public StateSet(Passive passiveObj, params object[] state) : base(passiveObj.EmpoweredPlayer)
        {
            PassiveObj = passiveObj;
            BeforeState = passiveObj.State;
            AfterState = state;
        }

        public override string ToString()
        {
            return $"<PASSIVE STATE> {PassiveObj}: {string.Join(" ", BeforeState)} -> {string.Join(" ", AfterState)}" + base.ToString();
        }
    }
}
