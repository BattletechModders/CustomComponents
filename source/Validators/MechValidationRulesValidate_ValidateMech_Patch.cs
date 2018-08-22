using System.Collections.Generic;
using System.Linq;
using BattleTech;
using Harmony;
using Localize;

namespace CustomComponents
{
    [HarmonyPatch(typeof(MechValidationRules), "ValidateMechDef")]
    internal static class MechValidationRulesValidate_ValidateMech_Patch
    {
        public static void Postfix(Dictionary<MechValidationType, List<Text>> __result,
            MechValidationLevel validationLevel, MechDef mechDef)
        {
            Validator.ValidateMech(__result, validationLevel, mechDef);
            foreach (var component in mechDef.Inventory)
            {
                foreach (var validator in component.GetComponents<IMechValidate>())
                {
                    validator.ValidateMech(__result, validationLevel, mechDef, component);
                }
            }
        }
    }
}