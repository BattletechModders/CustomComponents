using System;
using BattleTech;
using Harmony;

namespace CustomComponents
{
    [HarmonyPatch(typeof(Mech), "IsDead")]
    [HarmonyPatch(MethodType.Getter)]
    public static class Mech_IsDead
    {
        [HarmonyPostfix]
        public static void IsDestroyedChecks(Mech __instance, ref bool __result)
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