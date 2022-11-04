using BattleTech.Data;
using Harmony;
using SVGImporter;
using System;

namespace CustomComponents.Icons
{
    [HarmonyPatch(typeof(SVGCache))]
    [HarmonyPatch("GetAsset")]
    public static class SVGCache_GetAsset
    {
        [HarmonyPrefix]
        public static bool GetAsset(string id, ref SVGAsset __result)
        {
            try
            {
                if (string.IsNullOrEmpty(id) || id[0] != '@')
                    return true;

                __result = IconController.Get(id);
                if (__result == null)
                {
                    Control.LogError($"Custom icon {id} not found!");
                }
                return false;
            }
            catch (Exception e)
            {
                Control.LogError(e);
            }
            return true;
        }
    }
}
