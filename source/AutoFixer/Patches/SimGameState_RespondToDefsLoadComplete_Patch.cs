using System;
using System.Linq;
using BattleTech;
using Harmony;

namespace CustomComponents
{
    [HarmonyPatch(typeof(SimGameState), "RespondToDefsLoadComplete")]
    public static class SimGameState_RespondToDefsLoadComplete_Patch
    {
        [HarmonyPrefix]
        [HarmonyPriority(Priority.High)]
        public static void FixDefaults(SimGameState __instance)
        {
            try
            {
                var mechDefs = __instance.DataManager.MechDefs.Select(pair => pair.Value).ToList();
                AutoFixer.Shared.FixMechDef(mechDefs);
            }
            catch (Exception e)
            {
                Log.Main.Error?.Log(e);
            }
        }
    }
}