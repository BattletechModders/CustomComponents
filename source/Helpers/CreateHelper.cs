using System;
using BattleTech;
using BattleTech.Data;
using BattleTech.UI;

namespace CustomComponents
{
    public static class CreateHelper
    {

        public static MechComponentRef Ref(string id, ComponentType type, DataManager datamanager)
        {
            var component_ref = new MechComponentRef(id, string.Empty, type, ChassisLocations.None);
            component_ref.DataManager = datamanager;
            component_ref.RefreshComponentDef();
            return component_ref;
        }

        public static MechLabItemSlotElement Slot(MechLabPanel mechLab, MechComponentRef comp_ref, ChassisLocations location)
        {
            return mechLab.CreateMechComponentItem(comp_ref, false, location, mechLab);
        }

    }
}