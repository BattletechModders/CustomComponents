using System.Collections.Generic;
using BattleTech;
using BattleTech.UI;
using UIWidgetsSamples.Shops;

namespace CustomComponents.Changes
{
    public class ChangeAdd : IInvChange, IOptimizableChange
    {
        private InvItem item;
        private MechLabItemSlotElement slot;

        public string ItemID { get; set; }
        public ComponentType Type { get; set; }
        public ChassisLocations Location { get; set; }

        public void AdjustChange(InventoryOperationState state)
        {
            if (item == null) return;
            foreach (var add_handler in item.Item.GetComponents<IOnAdd>())
            {
                add_handler.OnAdd(Location, state);
            }
        }

        public void PreviewApply(InventoryOperationState state)
        {
            item = new InvItem(DefaultHelper.CreateRef(ItemID, Type, Location), Location);

            state.Inventory.Add(item);
        }

        public void ApplyToInventory(MechDef mech, List<MechComponentRef> inventory)
        {
            var r = DefaultHelper.CreateRef(ItemID, Type, Location);
            if (r.IsDefault())
                inventory.Add(r);
        }

        public void ApplyToMechlab()
        {
            var mechLab = MechLabHelper.CurrentMechLab;
            var lhelper = mechLab.GetLocationHelper(Location);
            if (lhelper == null)
                return;

            if (slot != null)
            {
                if (mechLab.InSimGame)
                {
                    WorkOrderEntry_InstallComponent subEntry = mechLab.MechLab.Sim.CreateComponentInstallWorkOrder(
                        mechLab.MechLab.baseWorkOrder.MechID,
                        slot.ComponentRef, Location, slot.MountedLocation);
                    mechLab.MechLab.baseWorkOrder.AddSubEntry(subEntry);
                }
            }
            else
                slot = DefaultHelper.CreateSlot(ItemID, Type);

            lhelper.widget.OnAddItem(slot, true);
            slot.MountedLocation = Location;

        }

        public void DoOptimization(List<IInvChange> current)
        {
            for (int i = current.Count - 2; i >= 0; i--)
            {
                var change = current[i];
                if (change is ChangeRemove remove && remove.Location == Location && remove.ItemID == ItemID)
                {
                    current.RemoveAt(i);
                    current.Remove(this);
                    return;
                }
            }
        }

        public ChangeAdd()
        {

        }

        public ChangeAdd(string id, ComponentType type, ChassisLocations location)
        {
            ItemID = id;
            Type = type;
            Location = location;
        }

        public ChangeAdd(MechLabItemSlotElement slot, ChassisLocations location)
        {
            this.slot = slot;
            ItemID = slot.ComponentRef.ComponentDefID;
            Type = slot.ComponentRef.ComponentDefType;
            Location = location;
        }
    }
}