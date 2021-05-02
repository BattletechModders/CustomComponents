using System.Collections.Generic;
using System.Linq;
using BattleTech;
using BattleTech.UI;

namespace CustomComponents.Changes
{
    public class ChangeRemove : IInvChange, IOptimizableChange
    {
        private InvItem item;

        public string ItemID { get; set; }
        public ChassisLocations Location { get; set; }
        public void AdjustChange(InventoryOperationState state)
        {
            if (item == null) return;
            foreach (var rem_handler in item.Item.GetComponents<IOnRemove>())
            {
                rem_handler.OnRemove(Location, state);
            }
        }

        public void PreviewApply(InventoryOperationState state)
        {
            item = state.Inventory.FirstOrDefault(i => i.Location == Location && i.Item.ComponentDefID == ItemID);
            if (item == null)
                return;

            state.Inventory.Remove(item);
        }

        public void ApplyToInventory(MechDef mech, List<MechComponentRef> inventory)
        {
            var item = inventory.FirstOrDefault(i => i.MountedLocation == Location && i.ComponentDefID == ItemID);
            if (item != null)
                inventory.Remove(item);
        }

        public void ApplyToMechlab()
        {
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

        public void DoOptimization(List<IInvChange> current)
        {
            for (int i = current.Count - 2; i >= 0; i--)
            {
                var change = current[i];
                if (change is ChangeAdd add && add.Location == Location && add.ItemID == ItemID)
                {
                    current.RemoveAt(i);
                    current.Remove(this);
                    return;
                }
            }
        }
    }
}