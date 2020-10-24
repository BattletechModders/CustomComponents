using System;
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
            try
            {
                if(mechDef == null)
                    return;
                if (Control.Settings.IgnoreValidationTags != null && Control.Settings.IgnoreValidationTags.Length > 0)
                    foreach (var tag in Control.Settings.IgnoreValidationTags)
                    {
                        if ((mechDef.Chassis.ChassisTags != null && mechDef.Chassis.ChassisTags.Contains(tag)) ||
                        (mechDef.MechTags!= null && mechDef.MechTags.Contains(tag)))
                        {
                            Control.LogDebug(DType.MechValidation, $"Validation {mechDef.Description.Id} Ignored by {tag}");
                            foreach (var pair in __result)
                            {
                                pair.Value.Clear();
                            }
                            return;
                        }
                    }


                Validator.ValidateMech(__result, validationLevel, mechDef);
                foreach (var component in mechDef.Inventory)
                {
                    foreach (var validator in component.GetComponents<IMechValidate>())
                    {
                        validator.ValidateMech(__result, validationLevel, mechDef, component);
                    }
                }
            }
            catch (Exception e)
            {
                Control.LogError(e);
            }
        }
    }
}