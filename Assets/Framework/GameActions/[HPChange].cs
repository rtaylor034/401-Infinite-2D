using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class GameAction
{

    public class HPChange : GameAction
    {

        public static event GameActionEventHandler<HPChange> ExternalResultantEvent;
        public Unit Reciever { get; private set; }
        public int Offset { get; private set; }

        protected override void InternalPerform()
        {
            Reciever.UpdateHP(Reciever.HP + Offset);
            
        }

        protected override void InternalUndo()
        {
            Reciever.UpdateHP(Reciever.HP - Offset);
        }
        
        //DEVNOTE/TODO: Consider making all "Change" GameActions use Offset instead of BeforeAmount and AfterAmount. This supports multiple changes happening in the same GameAction cycle.
        public HPChange(Player performer, Unit reciever, Func<int, int> changeFunction) : base(performer)
        {
            Reciever = reciever;
            Offset = changeFunction(Reciever.HP) - Reciever.HP;
            ExternalResultantEvent?.Invoke(this);
        }

        public override string ToString()
        {
            return $"<HP CHANGE> {Reciever} {Reciever.HP - Offset} -> {Reciever.HP}" + base.ToString();
        }
    }


}