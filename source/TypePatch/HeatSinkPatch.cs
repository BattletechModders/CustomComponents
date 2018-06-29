using BattleTech;
using BattleTech.Data;
using Harmony;

namespace CustomComponents
{
    [HarmonyNested(typeof(DataManager), "HeatSinkDefLoadRequest", "OnLoadedWithJSON")]
    public static class DataManager_HeatSink_Patch
    {
        public static bool Prefix(DataManager.ResourceLoadRequest<HeatSinkDef> __instance,
            string json, ref HeatSinkDef ___resource)
        {
            return Control.LoaderPatch(
                __instance, json, ref ___resource
                );
        }
    }

    [HarmonyPatch(typeof(HeatSinkDef), "ToJSON")]
    public static class HeatSinkDef_ToJSON_Patch
    {
        public static bool Prefix(HeatSinkDef __instance, ref string __result)
        {
            return Control.JsonPatch(__instance, ref __result);
        }
    }

}
