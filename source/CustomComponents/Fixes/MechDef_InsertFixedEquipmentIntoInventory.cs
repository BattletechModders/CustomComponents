using System;
using System.Collections.Generic;
using BattleTech;
using BattleTech.Data;

namespace CustomComponents.Fixes;

[HarmonyPatch(typeof(MechDef), "InsertFixedEquipmentIntoInventory")]
public static class MechDef_InsertFixedEquipmentIntoInventory
{
    [HarmonyPrefix]
    [HarmonyWrapSafe]
    public static void Prefix(ref bool __runOriginal, MechDef __instance, ref MechComponentRef[] ___inventory, DataManager ___dataManager)
    {
        if (!__runOriginal)
        {
            return;
        }

        if (__instance.Chassis == null)
        {
            __runOriginal = false;
            return;
        }

        if (__instance.Chassis.FixedEquipment == null || __instance.Chassis.FixedEquipment.Length == 0)
        {
            __runOriginal = false;
            return;
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
        {
            __runOriginal = false;
            return;
        }

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
        __runOriginal = false;
    }
}