using BattleTech.UI;

namespace CustomComponents.Patches;

// TODO implement based on Hardpoints
[HarmonyPatch(typeof(MechLabPanel), "MechCanEquipItem")]
internal class MechLabPanel_MechCanEquipItem_Patch
{
    [HarmonyPrefix]
    [HarmonyWrapSafe]
    public static void Prefix(ref bool __runOriginal, ref bool __result)
    {
        if (!__runOriginal)
        {
            return;
        }

        __result = true;
        __runOriginal = false;
    }
}