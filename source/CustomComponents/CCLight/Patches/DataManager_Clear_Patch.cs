using BattleTech.Data;

namespace CustomComponents.CCLight.Patches;

[HarmonyPatch(typeof(DataManager), nameof(DataManager.Clear))]
public static class DataManager_Clear_Patch
{
    [HarmonyPrefix]
    [HarmonyWrapSafe]
    public static void Prefix(ref bool __runOriginal, bool defs)
    {
        if (!__runOriginal)
        {
            return;
        }

        if (defs)
        {
            Database.Clear();
        }
    }
}