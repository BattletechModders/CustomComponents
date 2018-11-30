using BattleTech;
using Harmony;

namespace CustomComponents
{
    [HarmonyPatch(typeof(BaseComponentRef), "RefreshComponentDef")]
    public static class BaseComponentRef_RefreshComponentDef
    {
        [HarmonyPostfix]
        public static void SetFixed(BaseComponentRef __instance)
        {
            if (__instance.Def == null)
                return;
            if (__instance.Def.IsDefault())
            {
                var trav = Traverse.Create(__instance).Property<bool>("IsFixed");
                trav.Value = true;
            }
        }
    }
}