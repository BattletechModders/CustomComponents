using BattleTech;
using BattleTech.UI;

namespace CustomComponents
{
    public class AddDefaultChange : AddChange
    {
        public AddDefaultChange(ChassisLocations location, MechLabItemSlotElement item) : base(location, item)
        {
        }

        public override void DoChange()
        {
            Control.LogDebug(DType.ComponentInstall, $"-- AddFromInventoryChange: {item.ComponentRef.ComponentDefID} to {location}");

            var widget = MechLabHelper.CurrentMechLab.GetLocationWidget(location);
            if (widget == null)
                return;

            widget.OnAddItem(item, false);
            Control.LogDebug(DType.ComponentInstall, "--- added");
            item.MountedLocation = location;
        }

        public override string ToString()
        {
            return $"AddDefaultChange: {item.ComponentRef.ComponentDefID} => {location}";
        }
    }
}