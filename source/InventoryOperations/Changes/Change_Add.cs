using System.Collections.Generic;
using BattleTech;
using BattleTech.UI;
using UIWidgetsSamples.Shops;

namespace CustomComponents.Changes
{
    public class Change_Add : IChange_Apply, IChange_Optimize
    {
        private MechComponentRef item;
        private MechLabItemSlotElement slot;
        public bool Applied { get; private set; }
        public string ItemID { get; set; }
        public ComponentType Type { get; set; }
        public ChassisLocations Location { get; set; }

        public bool Initial { get; set; }

        public void AdjustChange(InventoryOperationState state)
        {
            if (item == null) return;
            foreach (var add_handler in item.GetComponents<IOnAdd>())
            {
                add_handler.OnAdd(Location, state);
            }
        }

        public void PreviewApply(InventoryOperationState state)
        {
            InvItem i = null;
            if (!Applied)
            {
                i = new InvItem(DefaultHelper.CreateRef(ItemID, Type, Location), Location);
                item = i.Item;
            }

            if(i != null)
                state.Inventory.Add(i);
        }

        public void ApplyToInventory(MechDef mech, List<MechComponentRef> inventory)
        {
            if (Applied)
                return;
            var r = DefaultHelper.CreateRef(ItemID, Type, Location);
            if (r.IsDefault())
                inventory.Add(r);
        }

        public void ApplyToMechlab()
        {
            if (Applied)
                return;
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

        public void DoOptimization(List<IChange_Apply> current)
        {
            for (int i = current.Count - 2; i >= 0; i--)
            {
                var change = current[i];
                if (!change.Initial && change is Change_Remove remove && !remove.Applied && remove.Location == Location && remove.ItemID == ItemID)
                {
                    current.RemoveAt(i);
                    current.Remove(this);
                    return;
                }
            }
        }

        public Change_Add()
        {

        }

        public Change_Add(MechComponentRef item, ChassisLocations location, bool already_applied = false)
        {
            ItemID = item.ComponentDefID;
            Type = item.ComponentDefType;
            Location = location;
            if (already_applied)
            {
                this.item = item;
                Applied = true;
            }
        }

        public Change_Add(string id, ComponentType type, ChassisLocations location)
        {
            ItemID = id;
            Type = type;
            Location = location;
        }

        public Change_Add(MechLabItemSlotElement slot, ChassisLocations location)
        {
            this.slot = slot;
            ItemID = slot.ComponentRef.ComponentDefID;
            Type = slot.ComponentRef.ComponentDefType;
            Location = location;
        }

        public override string ToString()
        {
            return $"Change_Add {ItemID} => {Location}";
        }
    }
}