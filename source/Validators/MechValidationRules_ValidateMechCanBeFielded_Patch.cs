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
                if (!__result)
                {
                    return;
                }

                if (!Validator.ValidateMechCanBeFielded(mechDef))
                {
                    __result = false;
                    return;
                }

                if (mechDef.Inventory.Any(component => component.GetComponents<IMechValidate>().Any(validator => validator.ValidateMechCanBeFielded(mechDef))))
                {
                    __result = false;
                    return;
                }
            }
            catch (Exception e)
            {
                Control.Logger.LogError(e);
            }
        }
    }
}