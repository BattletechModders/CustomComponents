using System;
using System.Collections.Generic;
using BattleTech;
using BattleTech.Data;
using Harmony;

namespace CustomComponents.Fixes;

[HarmonyPatch(typeof(MechDef), "InsertFixedEquipmentIntoInventory")]
public static class MechDef_InsertFixedEquipmentIntoInventory
{
    [HarmonyPrefix]
    public static bool FIX(MechDef __instance, ref MechComponentRef[] ___inventory, DataManager ___dataManager)
    {
        try
        {
            if (__instance.Chassis == null)
                return false;

            if (__instance.Chassis.FixedEquipment == null || __instance.Chassis.FixedEquipment.Length == 0)
            {
                return false;
            }
            var found = 0;
            for (var i = 0; i < ___inventory.Length; i++)
            {
                if (!string.IsNullOrEmpty(___inventory[i].SimGameUID) && ___inventory[i].SimGameUID.Contains("FixedEquipment"))
                {
                    ___inventory[i].SetData(___inventory[i].HardpointSlot, ___inventory[i].DamageLevel, true);
                    found += 1;
                }
            }

            if (found > 0)
                return false;
            var list = new List<MechComponentRef>();
            for (var j = 0; j < __instance.Chassis.FixedEquipment.Length; j++)
            {
                var mechComponentRef = new MechComponentRef(__instance.Chassis.FixedEquipment[j]);
                mechComponentRef.RefreshDef();
                mechComponentRef.SetSimGameUID($"FixedEquipment-{Guid.NewGuid().ToString()}");
                list.Add(mechComponentRef);

            }
            list.AddRange(___inventory);
            ___inventory = list.ToArray();
            return false;

        }
        catch (Exception e)
        {
            Log.Main.Error?.Log(e);
        }
        return true;
    }
}