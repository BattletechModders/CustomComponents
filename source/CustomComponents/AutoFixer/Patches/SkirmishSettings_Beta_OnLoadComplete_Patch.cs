using System.Collections.Generic;
using System.Linq;
using BattleTech;
using BattleTech.UI;

namespace CustomComponents.Patches;

[HarmonyPatch(typeof(SkirmishSettings_Beta), nameof(SkirmishSettings_Beta.OnLoadComplete))]
public class SkirmishSettings_Beta_OnLoadComplete_Patch
{
    [HarmonyPrefix]
    [HarmonyWrapSafe]
    [HarmonyPriority(Priority.High)]
    public static void Prefix(ref bool __runOriginal, SkirmishSettings_Beta __instance, UIManager ___uiManager, ref List<MechDef> ___stockMechs)
    {
        if (!__runOriginal)
        {
            return;
        }

        var mechDefs = ___uiManager.dataManager.MechDefs.Select(pair => pair.Value).ToList();
        MechDefProcessing.Instance.Process(mechDefs);

        ___stockMechs = mechDefs.Where(x => MechValidationRules.MechIsValidForSkirmish(x, false)).ToList();
    }
}