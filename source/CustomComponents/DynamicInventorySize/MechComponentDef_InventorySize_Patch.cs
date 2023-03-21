using BattleTech;

namespace CustomComponents.DynamicInventorySize;

[HarmonyPatch(typeof(MechComponentDef), nameof(MechComponentDef.InventorySize), MethodType.Getter)]
internal static class MechComponentDef_InventorySize_Patch
{
    [HarmonyPrepare]
    internal static bool Prepare()
    {
        return false; // disabled for now
    }

    [HarmonyPrefix]
    [HarmonyWrapSafe]
    internal static void Prefix(MechComponentDef __instance, ref bool __runOriginal, ref int __result)
    {
        if (!__instance.Is<IDynamicInventorySize>(out var di))
        {
            return;
        }

        __result = di.InventorySize;
        __runOriginal = false;
    }
}