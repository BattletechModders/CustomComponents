using System;
using BattleTech.UI;
using Harmony;

namespace CustomComponents.Patches
{
    [HarmonyPatch(typeof(SkirmishSettings_Beta), "OnLoadComplete")]
    public class SkirmishSettings_Beta_LanceConfiguratorDataLoaded
    {
        [HarmonyPrefix]
        [HarmonyPriority(Priority.High)]
        public static void Prefix(SkirmishSettings_Beta __instance, UIManager ___uiManager)
        {
            if (!Control.Settings.RunAutofixer)
                return;

            try
            {
                foreach (var pair in ___uiManager.dataManager.MechDefs)
                {
                    AutoFixer.Shared.FixMechDef(pair.Value, null);
                }
            }
            catch (Exception e)
            {
                Control.Logger.LogError(e);
            }
        }
    }
}