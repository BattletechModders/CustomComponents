using BattleTech;
using BattleTech.UI;
using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CustomComponents
{
    public delegate bool ValidateAddDelegate(MechComponentDef component, MechLabLocationWidget widget, 
        bool current_result, ref string errorMessage, MechLabPanel mechlab);

    public delegate void ValidateMechDelegate(Dictionary<MechValidationType, List<string>> errors,
        MechValidationLevel validationLevel, MechDef mechDef);

    public static class Validator
    {
        static Dictionary<Type, ValidateAddDelegate> add_validators = new Dictionary<Type, ValidateAddDelegate>();
        static List<ValidateMechDelegate> validators = new  List<ValidateMechDelegate>();

        public static void RegisterAddValidator(Type type, ValidateAddDelegate validator)
        {
            add_validators.Add(type, validator);
        }

        public static void RegisterValidator(ValidateMechDelegate validator)
        {
            validators.Add(validator);
        }


        internal static bool ValidateAdd(MechComponentDef component, MechLabLocationWidget widget, bool result, ref string errorMessage, MechLabPanel mechlab)
        {
            foreach (var validator in add_validators)
            {
                if (validator.Key.IsInstanceOfType(component))
                {
                    result = validator.Value(component, widget, result, ref errorMessage, mechlab);
                }
            }

            return result;
        }

        internal static void ValidateMech(Dictionary<MechValidationType, List<string>> errors,
            MechValidationLevel validationLevel, MechDef mechDef)
        {
            foreach (var validator in validators)
            {
                    validator(errors, validationLevel, mechDef);
            }
        }
    }

    [HarmonyPatch(typeof(MechLabLocationWidget), "ValidateAdd", new Type[] { typeof(MechComponentDef) })]
    internal static class MechLabLocationWidget_ValidateAdd_Patch
    {
        internal static void Postfix(MechComponentDef newComponentDef,
            MechLabLocationWidget __instance, ref bool __result, ref string ___dropErrorMessage,
            MechLabPanel ___mechLab
        )
        {
            __result = Validator.ValidateAdd(newComponentDef, __instance, __result, ref ___dropErrorMessage, ___mechLab);
            if (newComponentDef is IValidateAdd)
            {
                __result = (newComponentDef as IValidateAdd).ValidateAdd(__instance, __result, ref ___dropErrorMessage,
                    ___mechLab);
            }
        }
    }

    [HarmonyPatch(typeof(MechValidationRules), "ValidateMechDef")]
    public static class MechValidationRulesValidate_ValidateMech_Patch
    {
        public static void Postfix(Dictionary<MechValidationType, List<string>> __result,
            MechValidationLevel validationLevel, MechDef mechDef)
        {
            Validator.ValidateMech(__result, validationLevel, mechDef);
            foreach (var component in mechDef.Inventory.Where(i => i.Def != null).Select(i => i.Def).OfType<IMechValidate>())
            {
                component.ValidateMech(__result, validationLevel, mechDef);
            }
        }
    }
}
