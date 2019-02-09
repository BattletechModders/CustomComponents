using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BattleTech;
using Harmony;

namespace CustomComponents
{
    [HarmonyPatch(typeof(SimGameState), "InitCompanyStats")]
    public static class SimGameState_InitCompanyStats_Patch
    {
        public static void Postfix(SimGameState __instance)
        {
            try
            {
                AutoFixer.Shared.FixSavedMech(new List<MechDef>(), __instance);
            }
            catch (Exception e)
            {
                Control.Logger.LogError(e);
            }
        }
    }
}
