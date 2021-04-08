using BattleTech;
using BattleTech.UI;
using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomComponents.Patches
{
    [HarmonyPatch(typeof(MechLabPanel))]
    [HarmonyPatch("LoadMech")]
    public static class MechLabPanel_LoadMech
    {
        [HarmonyPostfix]
        public static void InitMechLabHelper(MechDef newMechDef, MechLabPanel __instance)
        {
            if (newMechDef != null)
                MechLabHelper.EnterMechLab(__instance);
        }
    }
}
