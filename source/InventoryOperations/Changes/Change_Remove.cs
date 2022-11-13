using System.Collections.Generic;
using System.Linq;
using BattleTech;
using BattleTech.UI;

namespace CustomComponents.Changes
{
    public class Change_Remove : IChange_Apply, IChange_Optimize
    {
        private MechComponentRef item;

        public bool Initial { get; set; }
        public bool Applied { get; private set; } = false;
        public string ItemID { get; set; }
        public ChassisLocations Location { get; set; }
        public void AdjustChange(InventoryOperationState state)
        {
            if (item == null) return;
            foreach (var rem_handler in item.GetComponents<IOnRemove>())
            {
                rem_handler.OnRemove(Location, state);
            }
        }

        public void PreviewApply(InventoryOperationState state)
        {
            InvItem i = null;

            if (!Applied)
            {
                i = state.Inventory.FirstOrDefault(i => i.Location == Location && i.Item.ComponentDefID == ItemID);
                item = i?.Item;
            }

            if (i == null)
                return;

            state.Inventory.Remove(i);
        }

        public void ApplyToInventory(MechDef mech, List<MechComponentRef> inventory)
        {
            if (Applied)
                return;
            var item = inventory.FirstOrDefault(i => i.MountedLocation == Location && i.ComponentDefID == ItemID);
            if (item != null)
                inventory.Remove(item);
        }

        public void ApplyToMechlab()
        {
            if (Applied)
                return;
            var lhelper = MechLabHelper.CurrentMechLab.GetLocationHelper(Location);
            if (lhelper == null)
                return;
            var item = lhelper.LocalInventory.FirstOrDefault(i =>
                i.ComponentRef.ComponentDefID == ItemID &&
                !i.ComponentRef.IsModuleFixed(MechLabHelper.CurrentMechLab.ActiveMech));
            if (item != null)
            {
                lhelper.widget.OnRemoveItem(item, true);
                if (item.ComponentRef.IsDefault())
                {
                    item.thisCanvasGroup.blocksRaycasts = true;
                    MechLabHelper.CurrentMechLab.MechLab.dataManager.PoolGameObject(MechLabPanel.MECHCOMPONENT_ITEM_PREFAB, item.gameObject);
                }
                else
                {
                    MechLabHelper.CurrentMechLab.MechLab.ForceItemDrop(item);
                }
            }
        }

        public void DoOptimization(List<IChange_Apply> current)
        {
            for (int i = current.Count - 2; i >= 0; i--)
            {
                var change = current[i];
                if (!change.Initial && change is Change_Add add && !add.Applied && add.Location == Location && add.ItemID == ItemID)
                {
                    Logging.Debug?.LogDebug(DType.InventoryOperaions, "--- OPT {0}, {1}", this, current[i]);
                    current.RemoveAt(i);
                    current.Remove(this);
                    return;
                }
            }
        }

        public Change_Remove()
        {
        }

        public Change_Remove(MechComponentRef item, ChassisLocations location, bool already_applied = false)
        {
            ItemID = item.ComponentDefID;
            Location = location;
            if (already_applied)
            {
                Applied = true;
                this.item = item;
            }
        }

        public Change_Remove(string defid, ChassisLocations location)
        {
            ItemID = defid;
            Location = location;
        }

        public override string ToString()
        {
            return $"Change_Remove {ItemID} =X {Location}";
        }
    }
}