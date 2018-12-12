#undef CCDEBUG
using BattleTech;
using Harmony;

namespace CustomComponents.DEBUG
{
#if CCDEBUG


    [HarmonyPatch(typeof(MechDef), "RefreshInventory")]
    public static class MechDef_RefreshInventory
    {
        [HarmonyPrefix]
        [HarmonyPriority(Priority.VeryHigh)]
        public static void BeforeCptMoore(MechDef __instance)
        {
            if (DEBUGTOOLS.NEEDTOSHOW)
            {
                Control.Logger.LogDebug("SHOW INVENTORY BEFORE REFRESH ONE");
                DEBUGTOOLS.ShowInventory(__instance);
            }
        }

        [HarmonyPostfix]
        public static void AfterRefresh(MechDef __instance)
        {
            if (DEBUGTOOLS.NEEDTOSHOW)
            {
                Control.Logger.LogDebug("SHOW INVENTORY after REFRESH");
                DEBUGTOOLS.ShowInventory(__instance);
                DEBUGTOOLS.NEEDTOSHOW = false;
            }
        }
    }

    [HarmonyPatch(typeof(MechDef), "RefreshInventory")]
    public static class MechDef_RefreshInventory2
    {
        [HarmonyPrefix]
        [HarmonyPriority(Priority.VeryLow)]
        public static void AfterCptMoore(MechDef __instance)
        {
            if (DEBUGTOOLS.NEEDTOSHOW)
            {

                Control.Logger.LogDebug("SHOW INVENTORY BEFORE REFRESH Two");
                DEBUGTOOLS.ShowInventory(__instance);
            }
        }
    }
#endif
}