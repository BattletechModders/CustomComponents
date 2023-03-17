using BattleTech;
using BattleTech.UI;
using BattleTech.UI.TMProWrapper;

namespace CustomComponents.Patches;

[HarmonyPatch(typeof(MechBayChassisInfoWidget))]
[HarmonyPatch("SetHardpoints")]
public static class MechBayChassisInfoWidget_SetHardpoints
{
    [HarmonyPrefix]
    [HarmonyWrapSafe]
    public static void Prefix(ref bool __runOriginal, MechBayChassisInfoWidget __instance, LocalizableText ___jumpjetHardpointText,
        LocalizableText ___ballisticHardpointText, ChassisDef ___selectedChassis)
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
                ___jumpjetHardpointText.gameObject, new(320,-25));
        }

        var usage = ___selectedChassis.GetHardpoints();
        hardpoints.SetDataTotal(usage);
        hardpoints.SetJJ(___selectedChassis);

        __runOriginal = false;
    }
}