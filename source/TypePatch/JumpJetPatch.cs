using BattleTech;
using BattleTech.Data;
using Harmony;

namespace CustomComponents
{
    [HarmonyNested(typeof(DataManager), "JumpJetDefLoadRequest", "OnLoadedWithJSON")]
    internal static class DataManager_JumpJet_Patch
    {
        public static bool Prefix(DataManager.ResourceLoadRequest<JumpJetDef> __instance,
            string json, ref JumpJetDef ___resource)
        {
            return Control.LoaderPatch(
                __instance, json, ref ___resource
                );
        }
    }

    [HarmonyPatch(typeof(JumpJetDef), "ToJSON")]
    internal static class JumpJetDef_ToJSON_Patch
    {
        public static bool Prefix(JumpJetDef __instance, ref string __result)
        {
            return Control.JsonPatch(__instance, ref __result);
        }
    }

}
