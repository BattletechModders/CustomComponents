using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BattleTech;
using Harmony;

namespace CustomComponents.Fixes
{
    [HarmonyPatch(typeof(SimGameState), "RequestDataManagerResources")]
    public static class SimGameState_RequestDataManagerResources_Patch
    {
        [HarmonyPrefix]
        public static void Preload(SimGameState __instance)
        {
            try
            {
                BTLoadUtils.PreloadComponents(__instance.DataManager);
            }
            catch (Exception e)
            {
                Control.LogError(e);
            }

        }
    }
}
