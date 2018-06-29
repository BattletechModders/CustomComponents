using BattleTech;
using BattleTech.Data;
using Harmony;

namespace CustomComponents
{
    [HarmonyNested(typeof(DataManager), "UpgradeDefLoadRequest", "OnLoadedWithJSON")]
    public static class DataManager_Upgrade_Patch
    {
        public static bool Prefix(DataManager.ResourceLoadRequest<UpgradeDef> __instance,
            string json, ref UpgradeDef ___resource)
        {
            return Control.LoaderPatch(
                __instance, json, ref ___resource
                );
        }
    }

    [HarmonyPatch(typeof(UpgradeDef), "ToJSON")]
    public static class UpgradeDef_ToJSON_Patch
    {
        public static bool Prefix(UpgradeDef __instance, ref string __result)
        {
            return Control.JsonPatch(__instance, ref __result);
        }
    }

}
