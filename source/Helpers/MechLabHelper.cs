using System.Collections.Generic;
using BattleTech;
using BattleTech.UI;
using Harmony;

namespace CustomComponents
{
    public class MechLabHelper
    {
        public MechLabPanel MechLab { get; private set; }


        private Traverse main;
        private Traverse drag_item;
        private Traverse<MechLabInventoryWidget> inventory;
        private Traverse<MechLabDismountWidget> bin;


        public IEnumerable<MechLabLocationWidget> GetWidgets()
        {
            yield return MechLab.headWidget;
            yield return MechLab.leftArmWidget;
            yield return MechLab.leftTorsoWidget;
            yield return MechLab.centerTorsoWidget;
            yield return MechLab.rightTorsoWidget;
            yield return MechLab.rightArmWidget;
            yield return MechLab.leftLegWidget;
            yield return MechLab.rightLegWidget;
        }


        public MechLabLocationWidget GetLocationWidget(ChassisLocations location)
        {
            switch (location)
            {
                case ChassisLocations.Head:
                    return MechLab.headWidget;
                case ChassisLocations.LeftArm:
                    return MechLab.leftArmWidget;
                case ChassisLocations.LeftTorso:
                    return MechLab.leftTorsoWidget;
                case ChassisLocations.CenterTorso:
                    return MechLab.centerTorsoWidget;
                case ChassisLocations.RightTorso:
                    return MechLab.rightTorsoWidget;
                case ChassisLocations.RightArm:
                    return MechLab.rightArmWidget;
                case ChassisLocations.LeftLeg:
                    return MechLab.leftLegWidget;
                case ChassisLocations.RightLeg:
                    return MechLab.rightLegWidget;
            }

            return null;
        }

        public MechLabInventoryWidget InventoryWidget
        {
            get
            {
                if (inventory == null)
                    inventory = main.Field<MechLabInventoryWidget>("inventoryWidget");
                return inventory.Value;
            }
        }

        public MechLabDismountWidget DismountWidget
        {
            get
            {
                if (bin == null)
                    bin = main.Field<MechLabDismountWidget>("dismountWidget");
                return bin.Value;
            }
        }

        public MechLabHelper(MechLabPanel mechLab)
        {
            MechLab = mechLab;
            main = Traverse.Create(mechLab);
        }

        public void SetDragItem(MechLabItemSlotElement item)
        {
            if (drag_item == null)
                drag_item = main.Field("dragItem");

            drag_item.SetValue(item);
        }
    }
}