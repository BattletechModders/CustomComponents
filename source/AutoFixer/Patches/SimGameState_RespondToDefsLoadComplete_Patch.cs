using System.Linq;
using BattleTech;

namespace CustomComponents;

[HarmonyPatch(typeof(SimGameState), nameof(SimGameState.RespondToDefsLoadComplete))]
public static class SimGameState_RespondToDefsLoadComplete_Patch
{
    [HarmonyPrefix]
    [HarmonyWrapSafe]
    [HarmonyPriority(Priority.High)]
    public static void Prefix(ref bool __runOriginal, SimGameState __instance)
    {
        if (!__runOriginal)
        {
            return;
        }

        var mechDefs = __instance.DataManager.MechDefs.Select(pair => pair.Value).ToList();
        MechDefProcessing.Instance.Process(mechDefs);
    }
}