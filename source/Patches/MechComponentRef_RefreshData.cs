using BattleTech;
using Harmony;
using System;

namespace CustomComponents.Patches
{
    [HarmonyPatch(typeof(BaseComponentRef), "RefreshComponentDef")]
    public static class BaseComponentRef_RefreshComponentDef
    {
        [HarmonyPostfix]
        public static void SetFixed(BaseComponentRef __instance)
        {
            try
            {
                if (__instance.Def == null)
                    return;
                if (__instance.Def.IsDefault())
                {
                    var trav = Traverse.Create(__instance).Property<bool>("IsFixed");
                    trav.Value = true;
                }
            }
            catch (Exception e)
            {
                Control.LogError($"Error in {__instance?.ComponentDefID} refreshing, check item", e);
            }
        }
    }
}