using BattleTech.UI;

namespace CustomComponents.Patches;

// TODO implement based on Hardpoints
[HarmonyPatch(typeof(MechLabPanel), "MechCanUseAmmo")]
internal class MechLabPanel_MechCanUseAmmo_Patch
{
    public static bool Prefix(ref bool __result)
    {
        __result = true;
        return false;
    }
}