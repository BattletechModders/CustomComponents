using System;
using System.Collections.Generic;
using System.Linq;
using BattleTech;
using BattleTech.UI;
using Harmony;

namespace CustomComponents.Patches;

[HarmonyPatch(typeof(SkirmishSettings_Beta), "OnLoadComplete")]
public class SkirmishSettings_Beta_OnLoadComplete_Patch
{
    [HarmonyPrefix]
    [HarmonyPriority(Priority.High)]
    public static void Prefix(SkirmishSettings_Beta __instance, UIManager ___uiManager, ref List<MechDef> ___stockMechs)
    {
        try
        {
            var mechDefs = ___uiManager.dataManager.MechDefs.Select(pair => pair.Value).ToList();
            AutoFixer.Shared.FixMechDef(mechDefs);

            ___stockMechs = mechDefs.Where(x => MechValidationRules.MechIsValidForSkirmish(x, false)).ToList();
        }
        catch (Exception e)
        {
            Log.Main.Error?.Log(e);
        }
    }
}