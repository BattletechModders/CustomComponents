using System;
using System.Linq;
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
            try
            {
                var mechDefs = ___uiManager.dataManager.MechDefs.Select(pair => pair.Value).ToList();
                AutoFixer.Shared.FixMechDef(mechDefs);
            }
            catch (Exception e)
            {
                Control.Logger.LogError(e);
            }
        }
    }
}