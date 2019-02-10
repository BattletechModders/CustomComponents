using System;
using BattleTech;
using BattleTech.UI;

namespace CustomComponents
{
    public class AddFromInventoryChange : AddChange
    {
        public static AddFromInventoryChange FoundInInventory(ChassisLocations location, Predicate<MechComponentDef> SearchTerms)
        {
            return null;
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