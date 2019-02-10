using System;
using BattleTech.UI;
using Harmony;

namespace CustomComponents
{
    [HarmonyPatch(typeof(SkirmishSettings_Beta), "LoadLanceConfiguratorData")]
    public static class SkirmishSettings_Beta_LoadLanceConfiguratorData_Patch
    {
        public static void Prefix(SkirmishSettings_Beta __instance, UIManager ___uiManager)
        {
            try
            {
                BTLoadUtils.PreloadComponents(___uiManager.dataManager);
            }
            catch (Exception e)
            {
                Control.LogError(e);
            }
        }
    }
}