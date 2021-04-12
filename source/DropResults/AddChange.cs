using System;
using System.Collections.Generic;
using BattleTech;
using BattleTech.UI;

namespace CustomComponents
{
    public abstract class AddChange : SlotChange
    {
        protected AddChange(ChassisLocations location, MechLabItemSlotElement item)
        {
            this.location = location;
            this.item = item;
        }

        public override void PreviewChange(List<InvItem> inventory)
        {
            inventory.Add(new InvItem() { item = item.ComponentRef, location = location });
        }

        public override bool DoAdjust(Queue<IChange> changes, List<InvItem> inventory)
        {
            bool changed = false;

            foreach (var adjust in item.ComponentRef.GetComponents<IAdjustValidateDrop>())
            {
                changed = changed || adjust.ValidateDropOnAdd(item, location, changes, inventory);
            }

            return changed;
        }
    }
}