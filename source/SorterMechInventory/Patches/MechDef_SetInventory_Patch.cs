using System;
using BattleTech;
using Harmony;

namespace CustomComponents.SorterMechInventory.Patches
{
    [HarmonyPatch(typeof(MechDef), nameof(MechDef.SetInventory))]
    public static class MechDef_SetInventory_Patch
    {
        public static void Prefix(ref MechComponentRef[] newInventory)
        {
            try
            {
                SorterUtils.SortMechDefInventory(newInventory);
            }
            catch (Exception e)
            {
                Log.Main.Error?.Log(e);
            }
        }
    }
}