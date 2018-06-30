using BattleTech;
using BattleTech.Data;
using Harmony;

namespace CustomComponents
{
    [HarmonyNested(typeof(DataManager), "WeaponDefLoadRequest", "OnLoadedWithJSON")]
    internal static class DataManager_WeaponDef_Patch
    {
        public static bool Prefix(DataManager.ResourceLoadRequest<WeaponDef> __instance,
            string json, ref WeaponDef ___resource)
        {
            return Control.LoaderPatch(
                __instance, json, ref ___resource
                );
        }
    }

    [HarmonyPatch(typeof(WeaponDef), "ToJSON")]
    internal static class WeaponDef_ToJSON_Patch
    {
        public static bool Prefix(WeaponDef __instance, ref string __result)
        {
            return Control.JsonPatch(__instance, ref __result);
        }
    }

}
