using BattleTech;
using BattleTech.UI;
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


        private static bool BTValidateAdd(MechComponentDef component, MechLabLocationWidget widget,
            MechLabPanel mechlab, ref string errorMessage)
        {
            var helper = new LocationHelper(widget);
            if (widget.loadout.CurrentInternalStructure <= 0f)
            {
                errorMessage =
                    $"Cannot add {component.Description.Name} to {helper.LocationName}: The location is Destroyed.";
                AddState(new BTValidateState { Error = BTValidateState.ErrorType.LocationDestroyed});
                return false;
            }

            if (helper.UsedSlots + component.InventorySize > helper.MaxSlots)
            {
                errorMessage =
                    $"Cannot add {component.Description.Name} to {helper.LocationName}: Not enough free slots.";
                AddState(new BTValidateState { Error = BTValidateState.ErrorType.Size });
                return false;
            }

            if ((component.AllowedLocations & widget.loadout.Location) <= ChassisLocations.None)
            {
                errorMessage =
                    $"Cannot add {component.Description.Name} to {helper.LocationName}: Component is not permitted in this location.";
                AddState(new BTValidateState { Error = BTValidateState.ErrorType.WrongLocation });
                return false;
            }

            if (component.ComponentType == ComponentType.Weapon)
            {
                int num = 0;
                int num2 = 0;
                WeaponDef weaponDef = component as WeaponDef;
                switch (weaponDef.Category)
                {
                    case WeaponCategory.Ballistic:
                        num = helper.currentBallisticCount;
                        num2 = helper.totalBallisticHardpoints;
                        break;
                    case WeaponCategory.Energy:
                        num = helper.currentEnergyCount;
                        num2 = helper.totalEnergyHardpoints;
                        break;
                    case WeaponCategory.Missile:
                        num = helper.currentMissileCount;
                        num2 = helper.totalMissileHardpoints;
                        break;
                    case WeaponCategory.AntiPersonnel:
                        num = helper.currentSmallCount;
                        num2 = helper.totalSmallHardpoints;
                        break;
                }

                if (num + 1 > num2)
                {
                    errorMessage =
                        $"Cannot add {component.Description.Name} to {helper.LocationName}: There are no available {weaponDef.Category.ToString()} hardpoints.";
                    AddState(new BTValidateState { Error = BTValidateState.ErrorType.Hardpoints });
                    return false;
                }
            }

            if (component.ComponentType == ComponentType.JumpJet)
            {
                int num3 = mechlab.headWidget.currentJumpjetCount + mechlab.centerTorsoWidget.currentJumpjetCount +
                           mechlab.leftTorsoWidget.currentJumpjetCount + mechlab.rightTorsoWidget.currentJumpjetCount +
                           mechlab.leftArmWidget.currentJumpjetCount + mechlab.rightArmWidget.currentJumpjetCount +
                           mechlab.leftLegWidget.currentJumpjetCount + mechlab.rightLegWidget.currentJumpjetCount;
                if (num3 + 1 > mechlab.activeMechDef.Chassis.MaxJumpjets)
                {
                    errorMessage =
                        $"Cannot add {component.Description.Name} to {helper.LocationName}: Max number of jumpjets for 'Mech reached.";
                    return false;
                }
            }
            return true;
        }
        
        internal static bool ValidateAdd(MechComponentDef component, MechLabLocationWidget widget, MechLabPanel mechlab, ref string errorMessage)
        {
            ClearValidatorState();

            // var result = widget.ValidateAdd(component);

            var result = BTValidateAdd(component, widget, mechlab, ref errorMessage);

            foreach (var validator in add_validators)
            {
                result = validator(component, widget, result, ref errorMessage, mechlab);
            }

            if (component is IValidateAdd add)
                result = add.ValidateAdd(widget, result, ref errorMessage, mechlab);


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

        public static void AddState(Object state)
        {
            validator_state.Add(state);
        }

        public static T GetState<T>()
        {
            return validator_state.OfType<T>().FirstOrDefault();
        }
    }
}
