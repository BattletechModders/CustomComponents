using System.Collections.Generic;
using System.Linq;
using BattleTech;
using BattleTech.UI;
using UnityEngine;

namespace CustomComponents
{
    public class RemoveChange : SlotChange
    {
        public override void DoChange()
        {
            var mechLab = MechLabHelper.CurrentMechLab;

            Control.LogDebug(DType.ComponentInstall, $"-- RemoveChange: {item.ComponentRef.ComponentDefID} from {location}");
            var widget = mechLab.GetLocationWidget(location);
            if (widget == null)
                return;

            widget.OnRemoveItem(item, true);
            Control.LogDebug(DType.ComponentInstall, $"--- removed");
            if (item.ComponentRef.IsDefault())
            {
                Control.LogDebug(DType.ComponentInstall, $"--- Default: clear");
                item.thisCanvasGroup.blocksRaycasts = true;
                mechLab.MechLab.dataManager.PoolGameObject(MechLabPanel.MECHCOMPONENT_ITEM_PREFAB, item.gameObject);
            }
            else
            {
                Control.LogDebug(DType.ComponentInstall, $"--- Not Default: drop");
                mechLab.MechLab.ForceItemDrop(item);
            }
        }

        public override void PreviewChange(List<SlotInvItem> inventory)
        {
            var to_remove = inventory.FirstOrDefault(i => i.item == item.ComponentRef && i.location == location);
            if (to_remove == null)
            {
                to_remove = inventory.FirstOrDefault(i =>
                    i.item.ComponentDefID == item.ComponentRef.ComponentDefID && i.location == location);
            }

            if (to_remove != null)
                inventory.Remove(to_remove);
            else
                Control.LogError($"Cannot remove preview for {item.ComponentRef.ComponentDefID} at {location}");
        }

        public RemoveChange(ChassisLocations location, MechLabItemSlotElement item)
        {
            this.location = location;
            this.item = item;
        }

        public override bool DoAdjust(Queue<IChange> changes, List<SlotInvItem> inventory)
        {
            bool changed = false;

            foreach (var adjust in item.ComponentRef.GetComponents<IAdjustValidateDrop>())
            {
                changed = changed || adjust.ValidateDropOnRemove(item, location, changes, inventory);
            }

            return changed;
        }
    }
}