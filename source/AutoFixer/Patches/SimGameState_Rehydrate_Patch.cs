using System;
using System.Collections.Generic;
using System.Linq;
using BattleTech;
using Harmony;

namespace CustomComponents.Patches
{

    [HarmonyPatch(typeof(SimGameState), "Rehydrate")]
    public static class SimGameState_Rehydrate_Patch
    {
        [HarmonyPostfix]
        public static void FixMechInMechbay(SimGameState __instance, StatCollection ___companyStats, Dictionary<int, MechDef> ___ActiveMechs, Dictionary<int, MechDef> ___ReadyingMechs)
        {
            try
            {
                var mechDefs = ___ActiveMechs.Values.Union(___ReadyingMechs.Values).ToList();
                AutoFixer.Shared.FixSavedMech(mechDefs, __instance);

                foreach (var mechDef in mechDefs)
                {
                    string value = $"{mechDef.Description.Id}({mechDef.Description.UIName}) [";
                    if(mechDef.MechTags != null && !mechDef.MechTags.IsEmpty)
                        foreach (var mechDefMechTag in mechDef.MechTags)
                            value += mechDefMechTag + " ";
                    Control.LogError(value);
                }
            }
            catch (Exception e)
            {
                Control.LogError(e);
            }
        }
    }
}