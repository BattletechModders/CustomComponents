using System;
using BattleTech;
using Harmony;

namespace CustomComponents.Patches
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
                Control.LogDebug(DType.IsDestroyed, $"Check if dead for vehicle {__instance.VehicleDef.Description.Id}");


                if (__result)
                {
                    Control.LogDebug(DType.IsDestroyed, $"- Vanila destroyed");
                  return ;
                }

                foreach (var item in __instance.VehicleDef.Inventory)
                {
                    if (Control.Settings.CheckCriticalComponent && item.Def.CriticalComponent &&
                        item.DamageLevel == ComponentDamageLevel.Destroyed)
                    {
                        Control.LogDebug(DType.IsDestroyed, $"- Destroyed by CriticalComponent {item.ComponentDefID}");
                        __result = true;
                        return;
                    }

                    if (item.Def.Is<ICheckIsDead>(out var d) && d.IsVechicleDestroyed(item, __instance))
                    {
                        __result = true;
                        Control.LogDebug(DType.IsDestroyed,
                            $"- Destroyed by CheckDestroyed {item.ComponentDefID} of {d.GetType()}");
                        return;
                    }
                }

                Control.LogDebug(DType.IsDestroyed, $"- not destroyed");
            }
            catch (Exception e)
            {
                Control.LogError(e);
            }
        }
    }
}