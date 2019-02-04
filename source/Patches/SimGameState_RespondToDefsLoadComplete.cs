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
            try
            {
                foreach (var pair in __instance.DataManager.MechDefs)
                {
                    DefaultFixer.FixMech(pair.Value, __instance);
                }
            }
            catch (Exception e)
            {
                Control.Logger.LogError(e);
            }
        }
    }
}