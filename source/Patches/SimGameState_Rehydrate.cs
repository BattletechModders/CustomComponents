using System;
using System.Collections.Generic;
using BattleTech;
using Harmony;

namespace CustomComponents.Patches
{

    [HarmonyPatch(typeof(SimGameState), "Rehydrate")]
    public static class SimGameState_Rehydrate
    {
        [HarmonyPostfix]
        public static void FixMechInMechbay(SimGameState __instance, StatCollection ___companyStats, Dictionary<int, MechDef> ___ActiveMechs, Dictionary<int, MechDef> ___ReadyingMechs)
        {
            if (!Control.Settings.RunAutofixer)
                return;


            try
            {

                if (Control.Settings.FixSaveGameMech)
                {
                    foreach (var pair in ___ActiveMechs)
                        DefaultFixer.Shared.FixSavedMech(pair.Value, __instance);
                    foreach (var pair in ___ReadyingMechs)
                        DefaultFixer.Shared.FixSavedMech(pair.Value, __instance);
                }
                else
                {
                    foreach (var pair in ___ActiveMechs)
                        DefaultFixer.Shared.FixMech(pair.Value, __instance);
                    foreach (var pair in ___ReadyingMechs)
                        DefaultFixer.Shared.FixMech(pair.Value, __instance);
                }

            }
            catch (Exception e)
            {
                Control.Logger.LogError(e);
            }
        }
    }
}