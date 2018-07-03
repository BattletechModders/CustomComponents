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

                foreach (var component in mechDef.Inventory.Where(i => i.Def != null).Select(i => i.Def)
                    .OfType<IMechValidate>())
                {
                    __result = component.ValidateMechCanBeFielded(mechDef);
                    if (__result)
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