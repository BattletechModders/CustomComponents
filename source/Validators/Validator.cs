using BattleTech;
using BattleTech.UI;
using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CustomComponents
{
    /// <summary>
    /// Static class to make validation 
    /// </summary>
    public static class Validator
    {
        static List<ValidateAddDelegate> add_validators = new List<ValidateAddDelegate>();
        static List<ValidateMechDelegate> mech_validators = new List<ValidateMechDelegate>();

        private static List<ValidateMechCanBeFieldedDelegate> field_validators =
            new List<ValidateMechCanBeFieldedDelegate>();

        private static List<Object> validator_state = new List<object>();

        /// <summary>
        /// register new AddValidator
        /// </summary>
        /// <param name="type"></param>
        /// <param name="validator"></param>
        public static void RegisterAddValidator(ValidateAddDelegate validator)
        {
            add_validators.Add(validator);
        }

        /// <summary>
        /// register new mech validator
        /// </summary>
        /// <param name="validator"></param>
        public static void RegisterMechValidator(ValidateMechDelegate mechvalidator,
            ValidateMechCanBeFieldedDelegate fieldvalidator)
        {
            if (mechvalidator != null) mech_validators.Add(mechvalidator);
            if (fieldvalidator != null) field_validators.Add(fieldvalidator);
        }

        internal static bool ValidateAdd(MechComponentDef component, MechLabLocationWidget widget, bool result,
            ref string errorMessage, MechLabPanel mechlab)
        {
            foreach (var validator in add_validators)
            {
                result = validator(component, widget, result, ref errorMessage, mechlab);
            }

            return result;
        }

        internal static void ValidateMech(Dictionary<MechValidationType, List<string>> errors,
            MechValidationLevel validationLevel, MechDef mechDef)
        {
            foreach (var validator in mech_validators)
            {
                validator(errors, validationLevel, mechDef);
            }
        }

        internal static bool ValidateMechCanBeFielded(MechDef mechDef)
        {
            foreach (var validateMechCanBeFieldedDelegate in field_validators)
            {
                if (!validateMechCanBeFieldedDelegate(mechDef))
                    return false;
            }

            return true;
        }

        internal static void ClearValidatorState()
        {
            validator_state.Clear();
        }

        public static void AddState(CategoryValidatorState state)
        {
            validator_state.Add(state);
        }

        public static T GetState<T>()
        {
            return validator_state.OfType<T>().FirstOrDefault();
        }
    }

    [HarmonyPatch(typeof(MechValidationRules), "ValidateMechDef")]
    internal static class MechValidationRulesValidate_ValidateMech_Patch
    {
        public static void Postfix(Dictionary<MechValidationType, List<string>> __result,
            MechValidationLevel validationLevel, MechDef mechDef)
        {
            Validator.ValidateMech(__result, validationLevel, mechDef);
            foreach (var component in mechDef.Inventory.Where(i => i.Def != null)
                .Select(i => i.Def)
                .GroupBy(i => i.Description.Id)
                .Select(i => i.First())
                .OfType<IMechValidate>())
            {
                component.ValidateMech(__result, validationLevel, mechDef);
            }
        }
    }
}
