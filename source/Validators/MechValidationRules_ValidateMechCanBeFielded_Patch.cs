using System;
using System.Linq;
using BattleTech;
using Harmony;

namespace CustomComponents
{
    [HarmonyPatch(typeof(MechValidationRules), "ValidateMechCanBeFielded")]
    internal static class MechValidationRules_ValidateMechCanBeFielded_Patch
    {
        public static void Postfix(MechDef mechDef, ref bool __result)
        {
            try
            {
                Control.Logger.LogDebug($"Mech validation for {mechDef.Name} starter current resutl {__result}");

                if (!__result)
                {
                    Control.Logger.LogDebug($"- ended at base validation");
                    return;
                }

                Control.Logger.LogDebug($"- statrted fixed validation");
                if (!Validator.ValidateMechCanBeFielded(mechDef))
                {
                    __result = false;
                    Control.Logger.LogDebug($"- ended at fixed validation");
                    return;
                }

                Control.Logger.LogDebug($"- statrted component validation");
                if (mechDef.Inventory.Any(component => component.GetComponents<IMechValidate>().Any(validator => !validator.ValidateMechCanBeFielded(mechDef))))
                {
                    __result = false;
                    Control.Logger.LogDebug($"- ended at component validation");
                    return;
                }

                Control.Logger.LogDebug($"- and done");
            }
            catch (Exception e)
            {
                Control.Logger.LogError(e);
            }
        }
    }
}