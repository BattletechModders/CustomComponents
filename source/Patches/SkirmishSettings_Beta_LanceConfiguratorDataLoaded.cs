using System;
using BattleTech.UI;
using Harmony;

namespace CustomComponents.Patches
{
    [HarmonyPatch(typeof(SkirmishSettings_Beta), "OnLoadComplete")]
    public class SkirmishSettings_Beta_LanceConfiguratorDataLoaded
    {
        [HarmonyPrefix]
        public static void Prefix(SkirmishSettings_Beta __instance, UIManager ___uiManager)
        {
            try
            {
                foreach (var pair in ___uiManager.dataManager.MechDefs)
                {
                    DefaultFixer.FixMech(pair.Value, null);
                }
            }
            catch (Exception e)
            {
                Control.Logger.LogError(e);
            }
        }
    }
}