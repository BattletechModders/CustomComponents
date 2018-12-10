using System;
using System.Collections.Generic;
using BattleTech;
using BattleTech.Data;
using Harmony;

namespace CustomComponents.Fixes
{
    [HarmonyPatch(typeof(MechDef), "InsertFixedEquipmentIntoInventory")]
    public static class MechDef_InsertFixedEquipmentIntoInventory
    {
        [HarmonyPrefix]
        public static bool FIX(MechDef __instance, ref MechComponentRef[] ___inventory, DataManager ___dataManager)
        {


            if (__instance.Chassis == null)
                return false;
#if CCDEBUG
            if(DEBUGTOOLS.NEEDTOSHOW)
            Control.Logger.LogDebug($"InsertFixedEquipmentIntoInventory for {__instance.Name}");
#endif
            if (__instance.Chassis.FixedEquipment == null || __instance.Chassis.FixedEquipment.Length == 0)
            {
#if CCDEBUG
                if (DEBUGTOOLS.NEEDTOSHOW)
                Control.Logger.LogDebug($"-- NO FIXED, return");
#endif
                return false;
            }
#if CCDEBUG
            if (DEBUGTOOLS.NEEDTOSHOW)
            Control.Logger.LogDebug($"-- start search for fixed");
#endif
            int found = 0;
            for (int i = 0; i < ___inventory.Length; i++)
            {
                if (!string.IsNullOrEmpty(___inventory[i].SimGameUID) && ___inventory[i].SimGameUID.Contains("FixedEquipment"))
                {
#if CCDEBUG
                    if (DEBUGTOOLS.NEEDTOSHOW)
                        Control.Logger.LogDebug($"---- found {___inventory[i].MountedLocation} - {___inventory[i].ComponentDefID} - {___inventory[i].SimGameUID}");
#endif
                    ___inventory[i].SetData(___inventory[i].HardpointSlot, ___inventory[i].DamageLevel, true);
                    found += 1;
                }
            }

            if (found > 0)
                return false;
#if CCDEBUG
            if (DEBUGTOOLS.NEEDTOSHOW)
            Control.Logger.LogDebug($"-- not found. inserting");
#endif
            List<MechComponentRef> list = new List<MechComponentRef>();
            for (int j = 0; j < __instance.Chassis.FixedEquipment.Length; j++)
            {
                MechComponentRef mechComponentRef = new MechComponentRef(__instance.Chassis.FixedEquipment[j], null);
                mechComponentRef.DataManager = ___dataManager;
                mechComponentRef.RefreshComponentDef();
                mechComponentRef.SetSimGameUID($"FixedEquipment-{Guid.NewGuid().ToString()}");
                list.Add(mechComponentRef);
#if CCDEBUG
                if (DEBUGTOOLS.NEEDTOSHOW)
                Control.Logger.LogDebug($"---- add {mechComponentRef.MountedLocation} - {mechComponentRef.ComponentDefID} - {mechComponentRef.SimGameUID}");
#endif
            }
            list.AddRange(___inventory);
            ___inventory = list.ToArray();
            return false;
        }
    }
}