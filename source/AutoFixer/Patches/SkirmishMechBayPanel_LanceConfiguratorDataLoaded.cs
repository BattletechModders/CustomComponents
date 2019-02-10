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
            try
            {
                var mechDefs = __instance.dataManager.MechDefs.Select(pair => pair.Value).ToList();
                AutoFixer.Shared.FixMechDef(mechDefs);
            }
            catch (Exception e)
            {
                Control.LogError(e);
            }
        }
    }
}
