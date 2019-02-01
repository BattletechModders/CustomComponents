using System;
using BattleTech.UI;
using Harmony;

namespace CustomComponents
{
    [HarmonyPatch(typeof(SkirmishMechBayPanel), "RequestResources")]
    public static class SkirmishMechBayPanel_RequestResources_Patch
    {
        public static void Prefix(SkirmishMechBayPanel __instance)
        {
            try
            {
                BTLoadUtils.PreloadComponents(__instance.dataManager);
            }
            catch (Exception e)
            {
                Control.Logger.LogError(e);
            }
        }
    }
}