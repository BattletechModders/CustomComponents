using System;
using BattleTech;
using Harmony;

namespace CustomComponents
{
    [HarmonyPatch(typeof(Vehicle), "IsDead")]
    [HarmonyPatch(MethodType.Getter)]
    public static class Vehicle_IsDead
    {
        [HarmonyPostfix]
        public static void IsDestroyedChecks(Vehicle __instance, ref bool __result)
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