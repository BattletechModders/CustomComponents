using System;
using BattleTech;
using Harmony;

namespace CustomComponents
{
    [HarmonyPatch(typeof(SimGameState), "RespondToDefsLoadComplete")]
    public static class SimGameState_RespondToDefsLoadComplete
    {
        [HarmonyPrefix]
        [HarmonyPriority(Priority.High)]
        public static void FixDefaults(SimGameState __instance)
        {
            if (!Control.Settings.RunAutofixer)
                return;

            try
            {
                foreach (var pair in __instance.DataManager.MechDefs)
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