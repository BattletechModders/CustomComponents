using BattleTech;
using BattleTech.UI;

namespace CustomComponents
{
    public class AddChange : SlotChange
    {
        public override void DoChange(MechLabHelper mechLab, LocationHelper loc)
        {
            Control.Logger.LogDebug($"-- AddChange: {item.ComponentRef.ComponentDefID} to {location}");

            var widget = location == loc.widget.loadout.Location ? loc.widget : mechLab.GetLocationWidget(location);
            if (widget == null)
                return;

            widget.OnAddItem(item, false);
            Control.Logger.LogDebug($"--- added");


            if (mechLab.MechLab.IsSimGame && (!item.ComponentRef.Def.Is<Flags>(out var f) || !f.Default))
            {
                Control.Logger.LogDebug($"--- not default: create work order");
                WorkOrderEntry_InstallComponent subEntry = mechLab.MechLab.Sim.CreateComponentInstallWorkOrder(
                    mechLab.MechLab.baseWorkOrder.MechID,
                    item.ComponentRef, widget.loadout.Location, item.MountedLocation);
                mechLab.MechLab.baseWorkOrder.AddSubEntry(subEntry);

            }
        }

        public AddChange(ChassisLocations location, MechLabItemSlotElement item)
        {
            this.location = location;
            this.item = item;
        }
    }
}