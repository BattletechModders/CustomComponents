using System;
using BattleTech;
using BattleTech.Data;
using BattleTech.UI;

namespace CustomComponents
{
    public static class CreateHelper
    {
        /// <summary>
        /// create and fill ComponentRef
        /// </summary>
        /// <param name="id"></param>
        /// <param name="type"></param>
        /// <param name="datamanager"></param>
        /// <returns></returns>
        public static MechComponentRef Ref(string id, ComponentType type, DataManager datamanager)
        {
            var component_ref = new MechComponentRef(id, string.Empty, type, ChassisLocations.None);
            component_ref.DataManager = datamanager;
            component_ref.RefreshComponentDef();
            return component_ref;
        }

        /// <summary>
        /// create slot item from component ref
        /// </summary>
        /// <param name="mechLab"></param>
        /// <param name="comp_ref"></param>
        /// <param name="location"></param>
        /// <returns></returns>
        public static MechLabItemSlotElement Slot(MechLabPanel mechLab, MechComponentRef comp_ref, ChassisLocations location)
        {
            return mechLab.CreateMechComponentItem(comp_ref, false, location, mechLab);
        }

    }
}