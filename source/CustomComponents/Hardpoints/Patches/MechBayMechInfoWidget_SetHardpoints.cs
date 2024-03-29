﻿using BattleTech;
using BattleTech.UI;
using BattleTech.UI.TMProWrapper;

namespace CustomComponents.Patches;

[HarmonyPatch(typeof(MechBayMechInfoWidget))]
[HarmonyPatch("SetHardpoints")]
public static class MechBayMechInfoWidget_SetHardpoints
{
    [HarmonyPrefix]
    [HarmonyWrapSafe]
    public static void Prefix(ref bool __runOriginal, MechBayMechInfoWidget __instance, LocalizableText ___jumpjetHardpointText,
        LocalizableText ___ballisticHardpointText, MechDef ___selectedMech)
    {
        if (!__runOriginal)
        {
            return;
        }

        var hardpoints = __instance.GetComponent<UIModuleHPHandler>();
        if (hardpoints == null)
        {
            hardpoints = __instance.gameObject.AddComponent<UIModuleHPHandler>();
            hardpoints.Init(__instance, ___ballisticHardpointText.gameObject,
                ___jumpjetHardpointText.gameObject, new(-5, -58));
        }

        var usage = ___selectedMech.GetHardpointUsage();
        hardpoints.SetData(usage);
        hardpoints.SetJJ(___selectedMech);

        __runOriginal = false;
    }
}