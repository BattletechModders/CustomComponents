using BattleTech.Data;
using Harmony;

namespace CustomComponents.Icons
{
    [HarmonyPatch(typeof(SVGCache))]
    [HarmonyPatch("Contains")]
    public static class SVGAssets_Contains
    {
        [HarmonyPrefix]
        public static bool Contains(string id, ref bool __result)
        {
            if (string.IsNullOrEmpty(id) || id[0] != '@')
                return true;

            __result = IconController.Contains(id);
            if (!__result)
            {
                Logging.Error?.Log($"Custom icon {id} not exists!");
            }
            return false;
        }
    }
}
