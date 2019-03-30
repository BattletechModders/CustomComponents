using System;
using BattleTech;
using Harmony;

namespace CustomComponents
{
    [HarmonyPatch(typeof(Turret), "IsDead")]
    [HarmonyPatch(MethodType.Getter)]
    public static class Turret_IsDead
    {
        [HarmonyPostfix]
        public static void IsDestroyedChecks(Turret __instance, ref bool __result)
        {
            if (__instance == null) return;

            try
            {
                IsDestroyed.IsDestroyedChecks(__instance, ref __result);
            }
            catch (Exception e)
            {
                Control.LogError(e);
            }
        }
    }
}