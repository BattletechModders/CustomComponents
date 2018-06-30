using BattleTech;
using BattleTech.Data;
using Harmony;

namespace CustomComponents
{
    [HarmonyNested(typeof(DataManager), "AmmunitionBoxDefLoadRequest", "OnLoadedWithJSON")]
    internal static class DataManager_AmmunitionBoxDef_Patch
    {
        public static bool Prefix(DataManager.ResourceLoadRequest<AmmunitionBoxDef> __instance,
            string json, ref AmmunitionBoxDef ___resource)
        {
            return Control.LoaderPatch(
                __instance, json, ref ___resource
                );
        }
    }

    [HarmonyPatch(typeof(AmmunitionBoxDef), "ToJSON")]
    internal static class AmmunitionBoxDef_ToJSON_Patch
    {
        public static bool Prefix(AmmunitionBoxDef __instance, ref string __result)
        {
            return Control.JsonPatch(__instance, ref __result);
        }
    }

}
