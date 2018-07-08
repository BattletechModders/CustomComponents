using BattleTech;
using BattleTech.UI;

namespace CustomComponents
{
    public class AddChange : SlotChange
    {
        public override void DoChange(MechLabHelper mechLab, LocationHelper loc)
        {
            var widget = location == loc.widget.loadout.Location ? loc.widget : mechLab.GetLocationWidget(location);
            if (widget == null)
                return;

            widget.OnAddItem(item, false);
        }

        public AddChange(ChassisLocations location, MechLabItemSlotElement item)
        {
            this.location = location;
            this.item = item;
        }
    }
}