using BattleTech;
using BattleTech.UI;
using System;
using System.Collections.Generic;
using System.Text;

namespace CustomComponents
{

    public abstract class InvItem
    {
        public abstract MechComponentRef item { get; }
        public ChassisLocations location { get; set; }

    }

    public class RefInvItem : InvItem
    {
        private MechComponentRef _item;
        public override MechComponentRef item => _item;

        public RefInvItem(MechComponentRef item, ChassisLocations location)
        {
            this._item = item;
            this.location = location;
        }
    }

    public class SlotInvItem : InvItem
    {
        public MechLabItemSlotElement slot { get; set; }
        public override MechComponentRef item => slot.ComponentRef;

        public SlotInvItem(MechLabItemSlotElement slot, ChassisLocations location)
        {
            this.slot = slot;
            this.location = location;
        }

    }
}
