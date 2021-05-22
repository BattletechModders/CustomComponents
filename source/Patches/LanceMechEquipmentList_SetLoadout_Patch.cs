using BattleTech;
using Harmony;
using System;
using System.Collections.Generic;
using BattleTech.UI;
using BattleTech.UI.TMProWrapper;
using UnityEngine;

namespace CustomComponents.Patches
{
    [HarmonyPatch(
        typeof(LanceMechEquipmentList),
        "SetLoadout",
        typeof(LocalizableText),
        typeof(UIColorRefTracker),
        typeof(Transform),
        typeof(ChassisLocations)
        )]
    public static class LanceMechEquipmentList_SetLoadout_Patch
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return instructions.MethodReplacer(
                AccessTools.Property(typeof(MechComponentRef), nameof(MechComponentRef.MountedLocation)).GetGetMethod(),
                AccessTools.Method(typeof(LanceMechEquipmentList_SetLoadout_Patch), nameof(MountedLocation))
            );
        }

        public static ChassisLocations MountedLocation(this MechComponentRef componentRef)
        {
            if (componentRef.Flags<CCFlags>().HideFromEquip)
            {
                return ChassisLocations.None;
            }

            return componentRef.MountedLocation;
        }
    }
}