using BattleTech;

namespace CustomComponents.SorterMechInventory.Patches;

[HarmonyPatch(typeof(MechDef), nameof(MechDef.SetInventory))]
public static class MechDef_SetInventory_Patch
{
    [HarmonyPrefix]
    [HarmonyWrapSafe]
    public static void Prefix(ref bool __runOriginal, ref MechComponentRef[] newInventory)
    {
        if (!__runOriginal)
        {
            return;
        }

        SorterUtils.SortMechDefInventory(newInventory);
    }
}