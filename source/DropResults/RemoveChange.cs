using BattleTech;
using BattleTech.UI;
using UnityEngine;

namespace CustomComponents
{
    public class RemoveChange : SlotChange
    {
        public override void DoChange(MechLabHelper mechLab, LocationHelper loc)
        {
            var widget = location == loc.widget.loadout.Location ? loc.widget : mechLab.GetLocationWidget(location);
            if (widget == null)
                return;

            widget.OnRemoveItem(item, true);
            if (item.ComponentRef.Is<Flags>(out var f) && f.Default)
            {
                item.thisCanvasGroup.blocksRaycasts = true;
                mechLab.MechLab.dataManager.PoolGameObject(MechLabPanel.MECHCOMPONENT_ITEM_PREFAB, item.gameObject);
            }
            else
                mechLab.MechLab.ForceItemDrop(item);
        }

        public RemoveChange(ChassisLocations location, MechLabItemSlotElement item)
        {
            this.location = location;
            this.item = item;
        }
    }
}