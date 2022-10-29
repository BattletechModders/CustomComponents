using BattleTech;
using BattleTech.UI;
using Harmony;

namespace CustomComponents.Patches
{
    [HarmonyPatch(typeof(MechLabPanel), "MechCanUseAmmo")]
    internal class MechLabPanel_MechCanUseAmmo_Patch
    {
        public static bool Prefix(ref bool __result)
        {
            __result = true;
            return false;
        }
    }
}