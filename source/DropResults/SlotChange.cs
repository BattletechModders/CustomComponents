using BattleTech;
using BattleTech.UI;

namespace CustomComponents
{
    public abstract class SlotChange : IApplyChange
    {
        public ChassisLocations location;
        public MechLabItemSlotElement item;

        public abstract void DoChange();
    }
}