using System;
using System.Linq;
using BattleTech;
using BattleTech.Data;
using BattleTech.UI;
using Harmony;

namespace CustomComponents
{
    [HarmonyNested(typeof(DataManager), "MechDefLoadRequest", "OnLoadedWithJSON")]
    internal static class DataManager_MechDefLoadRequest
    {
        public static void Postfix(DataManager.ResourceLoadRequest<MechDef> __instance, ref MechDef ___resource)
        {
            if (___resource.Inventory.All(i => i.ComponentDefID != "Gear_Actuator_Default"))
            {

                var inventory = ___resource.Inventory.ToList();

                inventory.Add(new MechComponentRef("Gear_Actuator_Default", null, ComponentType.Upgrade,
                    ChassisLocations.LeftLeg));
                inventory.Add(new MechComponentRef("Gear_Actuator_Default", null, ComponentType.Upgrade,
                    ChassisLocations.RightLeg));

                ___resource.SetInventory(inventory.ToArray());
            }
        }
    }
}