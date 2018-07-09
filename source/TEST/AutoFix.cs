using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using BattleTech;
using BattleTech.Data;
using BattleTech.UI;
using Harmony;

namespace CustomComponents
{
    //[HarmonyNested(typeof(DataManager), "MechDefLoadRequest", "OnLoadedWithJSON")]
    //internal static class DataManager_MechDefLoadRequest
    //{
    //    public static void Postfix(DataManager.ResourceLoadRequest<MechDef> __instance, ref MechDef ___resource)
    //    {
    //        if (___resource.Inventory.All(i => i.ComponentDefID != "Gear_Actuator_Default"))
    //        {

    //            var inventory = ___resource.Inventory.ToList();
    //            var ref1 = new MechComponentRef("Gear_Actuator_Default", null, ComponentType.Upgrade,
    //                ChassisLocations.LeftLeg);
    //            ref1.DataManager = ___resource.DataManager;
    //            ref1.RefreshComponentDef();
    //            var ref2 = new MechComponentRef("Gear_Actuator_Default", null, ComponentType.Upgrade,
    //                ChassisLocations.RightLeg);
    //            ref2.DataManager = ___resource.DataManager;
    //            ref2.RefreshComponentDef();

    //            inventory.Add(ref1);
    //            inventory.Add(ref2);

    //            ___resource.SetInventory(inventory.ToArray());
    //            Traverse.Create(__instance).Method("TryLoadDependencies").GetValue(___resource);
    //        }
    //    }
    //}

    [HarmonyPatch(typeof(MechDef), "FromJSON")]
    public static class MechDef_FromJSON
    {
        public static void Postfix(MechDef __instance)
        {
            Control.Logger.LogDebug($"Adding Leg Acuators to {__instance.Description.Id}");

            if (__instance.Inventory.Any(i => i.ComponentDefID == "Gear_Actuator_Default"))
                return;

            var inventory = __instance.Inventory.ToList();
            var ref1 = new MechComponentRef("Gear_Actuator_Default", null, ComponentType.Upgrade,
                ChassisLocations.LeftLeg);
            //ref1.DataManager = ___resource.DataManager;
            //ref1.RefreshComponentDef();
            var ref2 = new MechComponentRef("Gear_Actuator_Default", null, ComponentType.Upgrade,
                ChassisLocations.RightLeg);
            //ref2.DataManager = ___resource.DataManager;
            //ref2.RefreshComponentDef();

            inventory.Add(ref1);
            inventory.Add(ref2);

            __instance.SetInventory(inventory.ToArray());

        }
    }
}