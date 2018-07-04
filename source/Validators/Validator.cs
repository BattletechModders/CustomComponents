using BattleTech;
using BattleTech.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using Harmony;

namespace CustomComponents
{
    /// <summary>
    /// Static class to make validation 
    /// </summary>
    public static class Validator
    {
        // need to be public, so order can be changed if need be
        public static List<ValidateAddDelegate> add_validators = new List<ValidateAddDelegate>();
        public static List<ValidateDropDelegate> drop_validators = new List<ValidateDropDelegate>();
        public static List<ValidateMechDelegate> mech_validators = new List<ValidateMechDelegate>();

        private static List<ValidateMechCanBeFieldedDelegate> field_validators =
            new List<ValidateMechCanBeFieldedDelegate>();

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
        /// register new AddValidator
        /// </summary>
        /// <param name="type"></param>
        /// <param name="validator"></param>
        public static void RegisterDropValidator(ValidateDropDelegate validator)
        {
            drop_validators.Add(validator);
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
        

        public static IEnumerable<ValidateDropDelegate> GetValidateDropDelegates(MechComponentDef componentValidator)
        {
            foreach (var validator in drop_validators)
            {
                yield return validator;
            }

            if (componentValidator is IValidateDrop validateDrop)
            {
                yield return validateDrop.ValidateDrop;
            }

            { // legacy IValidateAdd support, I'd vote to just remove it

                foreach (var validator in add_validators)
                {
                    IValidateDropResult ValidateDrop(MechLabItemSlotElement element, MechLabLocationWidget widget)
                    {
                        string errorMessage = null;
                        var mechlab = widget.GetMechLab();
                        if (!validator(element.ComponentRef.Def, widget, true, ref errorMessage, mechlab))
                        {
                            return new ValidateDropError(errorMessage);
                        }

                        return null;
                    }

                    yield return ValidateDrop;
                }

                if (componentValidator is IValidateAdd add)
                {
                    IValidateDropResult ValidateDrop(MechLabItemSlotElement element, MechLabLocationWidget widget)
                    {
                        string errorMessage = null;
                        var mechlab = widget.GetMechLab();
                        if (!add.ValidateAdd(widget, true, ref errorMessage, mechlab))
                        {
                            return new ValidateDropError(errorMessage);
                        }

                        return null;
                    }

                    yield return ValidateDrop;
                }
            }
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
    }

    public enum ValidateDropStatus
    {
        Continue, Handled
    }

    public interface IValidateDropResult
    {
        ValidateDropStatus Status { get; } // not really necessary, but nice for semantics
    }

    public class ValidateDropReplaceItem : IValidateDropResult
    {
        public ValidateDropStatus Status => ValidateDropStatus.Continue;

        public MechLabItemSlotElement ToReplaceElement { get; }

        public ValidateDropReplaceItem(MechLabItemSlotElement toReplaceElement)
        {
            ToReplaceElement = toReplaceElement;
        }
    }
    
    public class ValidateDropHandled : IValidateDropResult
    {
        public ValidateDropStatus Status => ValidateDropStatus.Handled;
    }

    public class ValidateDropRemoveDragItem : ValidateDropHandled
    {
    }

    public class ValidateDropError : ValidateDropHandled
    {
        public string ErrorMessage { get; }

        public ValidateDropError(string errorMessage)
        {
            ErrorMessage = errorMessage;
        }
    }

    public static class MechLabLocationWidgetExtensions
    {
        public static MechLabPanel GetMechLab(this MechLabLocationWidget @this)
        {
            return @this.parentDropTarget as MechLabPanel;
        }

        public static List<MechLabItemSlotElement> GetInventory(this MechLabLocationWidget @this)
        {
            return Traverse.Create(@this).Field("localInventory").GetValue<List<MechLabItemSlotElement>>();
        }
    }
}
