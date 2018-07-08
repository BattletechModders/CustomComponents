using BattleTech;
using BattleTech.UI;

namespace CustomComponents
{
    public abstract class SlotChange : IChange
    {
        public ChassisLocations location;
        public MechLabItemSlotElement item;

        public abstract void DoChange(MechLabHelper mechLab, LocationHelper loc);
    }
}