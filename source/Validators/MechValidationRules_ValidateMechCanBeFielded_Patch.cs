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
                if (mechDef == null)
                {
                    Control.LogDebug(DType.MechValidation, $"Mech validation for NULL return");
                    return;
                }

                Control.LogDebug(DType.MechValidation, $"Mech validation for {mechDef.Name} start from {__result}");

                if (!__result)
                {
                    Control.LogDebug(DType.MechValidation, $"- failed base validation");
                    return;
                }

                Control.LogDebug(DType.MechValidation, $"- fixed validation");
                if (!Validator.ValidateMechCanBeFielded(mechDef))
                {
                    __result = false;
                    Control.LogDebug(DType.MechValidation, $"- failed fixed validation");
                    return;
                }
                Control.LogDebug(DType.MechValidation, $"- component validation");
                foreach (var component in mechDef.Inventory)
                {
                    foreach (var mechValidate in component.GetComponents<IMechValidate>())
                    {

                        Control.LogDebug(DType.MechValidation, $"-- {mechValidate.GetType()}");
                        if (!mechValidate.ValidateMechCanBeFielded(mechDef, component))
                        {
                            __result = false;
                            Control.LogDebug(DType.MechValidation, $"- failed component validation");
                            return;
                        }
                    }
                }

                Control.LogDebug(DType.MechValidation, $"- validation passed");
            }

            catch (Exception e)
            {
                Control.LogError(e);
            }
        }
    }
}