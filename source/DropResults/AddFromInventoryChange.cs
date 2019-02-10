using System;
using System.IO;
using BattleTech;
using BattleTech.UI;

namespace CustomComponents
{
    public class AddFromInventoryChange : AddChange
    {
        private static MechLabItemSlotElement search_item(MechLabHelper mechlab,
            Predicate<MechComponentDef> SearchTerms)
        {
            if (mechlab.MechLab.sim != null)
            {
                foreach (var slot in mechlab.DismountWidget.localInventory)
                {
                    if (slot.ComponentRef?.Def == null)
                        continue;

                    if (SearchTerms(slot.ComponentRef.Def))
                    {
                        slot.RemoveFromParent();
                        return slot;
                    }
                }
            }

            foreach (var inventoryItem in mechlab.InventoryWidget.localInventory)
            {
                if(inventoryItem.ComponentRef?.Def == null)
                    continue;

                if (SearchTerms(inventoryItem.ComponentRef.Def))
                {
                    inventoryItem.RemoveFromParent();
                    var slot = mechlab.MechLab.CreateMechComponentItem(inventoryItem.ComponentRef, true,
                        inventoryItem.MountedLocation, inventoryItem.DropParent, inventoryItem);
                    return slot;
                }
            }

            return null;
        }

        public static AddFromInventoryChange FoundInInventory(ChassisLocations location, MechLabHelper mechlab,
            Predicate<MechComponentDef> SearchTerms, Predicate<MechComponentDef> PrioritySeatch)
        {
            var item = search_item(mechlab, PrioritySeatch);
            if (item == null)
                item = search_item(mechlab, SearchTerms);

            return item == null ? null : new AddFromInventoryChange(location, item);
        }

        public static AddFromInventoryChange FoundInInventory(ChassisLocations location, MechLabHelper mechlab, Predicate<MechComponentDef> SearchTerms)
        {
            var item = search_item(mechlab, SearchTerms);
            return item == null ? null : new AddFromInventoryChange(location, item);
        }

        public AddFromInventoryChange(ChassisLocations location, MechLabItemSlotElement item) : base(location, item)
        {

        }

        public override void DoChange(MechLabHelper mechLab, LocationHelper loc)
        {
            Control.LogDebug(DType.ComponentInstall, $"-- AddFromInventoryChange: {item.ComponentRef.ComponentDefID} to {location}");

            var widget = location == loc.widget.loadout.Location ? loc.widget : mechLab.GetLocationWidget(location);
            if (widget == null)
                return;

            widget.OnAddItem(item, false);
            Control.LogDebug(DType.ComponentInstall, "--- added");


            if (mechLab.MechLab.IsSimGame)
            {
                Control.LogDebug(DType.ComponentInstall, "--- not default: create work order");
                WorkOrderEntry_InstallComponent subEntry = mechLab.MechLab.Sim.CreateComponentInstallWorkOrder(
                    mechLab.MechLab.baseWorkOrder.MechID,
                    item.ComponentRef, widget.loadout.Location, item.MountedLocation);
                mechLab.MechLab.baseWorkOrder.AddSubEntry(subEntry);

            }
            item.MountedLocation = location;
        }

        public override void CancelChange(MechLabHelper mechLab, LocationHelper loc)
        {
            mechLab.MechLab.ForceItemDrop(item);
        }
    }
}