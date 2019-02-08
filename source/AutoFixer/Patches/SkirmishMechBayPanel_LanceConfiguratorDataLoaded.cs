using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BattleTech.UI;
using Harmony;

namespace CustomComponents.Patches
{
    [HarmonyPatch(typeof(SkirmishMechBayPanel), "LanceConfiguratorDataLoaded")]
    public static class SkirmishMechBayPanel_LanceConfiguratorDataLoaded
    {
        [HarmonyPrefix]
        [HarmonyPriority(Priority.High)]
        public static void FixDefaults(SkirmishMechBayPanel __instance)
        {
            if (!Control.Settings.RunAutofixer)
                return;

            try
            {
                foreach (var pair in __instance.dataManager.MechDefs)
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
