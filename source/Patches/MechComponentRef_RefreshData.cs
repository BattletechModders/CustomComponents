using BattleTech;

namespace CustomComponents.Patches;

[HarmonyPatch(typeof(BaseComponentRef), "RefreshComponentDef")]
public static class BaseComponentRef_RefreshComponentDef
{
    [HarmonyPostfix]
    [HarmonyWrapSafe]
    public static void Postfix(BaseComponentRef __instance)
    {
        if (__instance.Def == null)
            return;
        if (__instance.Def.Flags<CCFlags>().NoRemove)
        {
            __instance.IsFixed = true;
        }
    }
}