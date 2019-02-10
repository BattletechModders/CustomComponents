using BattleTech;
using BattleTech.UI;

namespace CustomComponents
{
    public abstract class AddChange : SlotChange
    {
        protected AddChange(ChassisLocations location, MechLabItemSlotElement item)
        {
            this.location = location;
            this.item = item;
        }
    }
}