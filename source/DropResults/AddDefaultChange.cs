using BattleTech;
using BattleTech.UI;

namespace CustomComponents
{
    public class AddDefaultChange : AddChange
    {
        public AddDefaultChange(ChassisLocations location, MechLabItemSlotElement item) : base(location, item)
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
            item.MountedLocation = location;
        }
    }
}