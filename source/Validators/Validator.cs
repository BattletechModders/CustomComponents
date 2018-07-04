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
        public static List<ValidateDropDelegate> drop_validators = new List<ValidateDropDelegate>();
        public static List<ValidateMechDelegate> mech_validators = new List<ValidateMechDelegate>();
        private static List<ValidateMechCanBeFieldedDelegate> field_validators =
            new List<ValidateMechCanBeFieldedDelegate>();

        /// <summary>
        /// register new AddValidator
        /// </summary>
        /// <param name="type"></param>
        /// <param name="validator"></param>
        public static void RegisterDropValidator(ValidateDropDelegate validator)
        {
            drop_validators.Add(validator);
        }

        public static ValidateDropDelegate HardpointValidator { get; set; } = null;

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


        internal static IEnumerable<ValidateDropDelegate> GetValidateDropDelegates(MechComponentDef componentValidator)
        {
            yield return ValidateBase;

            yield return HardpointValidator ?? ValidateHardpoint;

            foreach (var validator in drop_validators)
            {
                yield return validator;
            }

            if (componentValidator is IValidateDrop validateDrop)
            {
                yield return validateDrop.ValidateDrop;
            }

            yield return ValidateSize;
        }



        private static IValidateDropResult ValidateSize(MechLabItemSlotElement element, LocationHelper location, IValidateDropResult last_result)
        {
            LocationHelper get_helper(MechLabPanel mechlab, ChassisLocations l)
            {
                if (l == location.widget.loadout.Location)
                    return location;

                switch (l)
                {
                    case ChassisLocations.Head:
                        return new LocationHelper(mechlab.headWidget);
                    case ChassisLocations.LeftArm:
                        return new LocationHelper(mechlab.leftArmWidget);
                    case ChassisLocations.LeftTorso:
                        return new LocationHelper(mechlab.leftTorsoWidget);
                    case ChassisLocations.CenterTorso:
                        return new LocationHelper(mechlab.centerTorsoWidget);
                    case ChassisLocations.RightTorso:
                        return new LocationHelper(mechlab.rightTorsoWidget);
                    case ChassisLocations.RightArm:
                        return new LocationHelper(mechlab.rightArmWidget);
                    case ChassisLocations.LeftLeg:
                        return new LocationHelper(mechlab.leftLegWidget);
                    case ChassisLocations.RightLeg:
                        return new LocationHelper(mechlab.rightLegWidget);
                }

                return null;
            }

            bool done = false;


            if (last_result is ValidateDropChange change_result)
            {
                var changes = from change in change_result.Changes.OfType<SlotChange>()
                    group change by change.location
                    into g
                    select new
                    {
                        location = get_helper(location.mechLab, g.Key),
                        change = g.Sum(i =>
                             i is AddChange
                                ? i.item.ComponentRef.Def.InventorySize
                                : -i.item.ComponentRef.Def.InventorySize)
                    };

                foreach (var change in changes)
                {
                    int used = change.location.UsedSlots + change.change;
                    if (change.location.widget.loadout.Location == location.widget.loadout.Location)
                    {
                        used += element.ComponentRef.Def.InventorySize;
                        done = true;
                    }

                    if(used > change.location.MaxSlots)
                        return new ValidateDropError(
                            $"Cannot add {element.ComponentRef.Def.Description.Name} to {location.LocationName}: Not enought free slots.");
                }
            }

            if (done)
                return last_result;
            
            
            int need = location.UsedSlots + element.ComponentRef.Def.InventorySize;
            if (need > location.MaxSlots)
                return new ValidateDropError(
                    $"Cannot add {element.ComponentRef.Def.Description.Name} to {location.LocationName}: Not enought free slots.");


            return last_result;
        }

        private static IValidateDropResult ValidateHardpoint(MechLabItemSlotElement element, LocationHelper location, IValidateDropResult last_result)
        {
            if (element.ComponentRef.Def.ComponentType == ComponentType.Weapon)
            {
                int num = 0;
                int num2 = 0;
                WeaponDef weaponDef = element.ComponentRef.Def as WeaponDef;
                switch (weaponDef.Category)
                {
                    case WeaponCategory.Ballistic:
                        num = location.currentBallisticCount;
                        num2 = location.totalBallisticHardpoints;
                        break;
                    case WeaponCategory.Energy:
                        num = location.currentEnergyCount;
                        num2 = location.totalEnergyHardpoints;
                        break;
                    case WeaponCategory.Missile:
                        num = location.currentMissileCount;
                        num2 = location.totalMissileHardpoints;
                        break;
                    case WeaponCategory.AntiPersonnel:
                        num = location.currentSmallCount;
                        num2 = location.totalSmallHardpoints;
                        break;
                }

                if (num + 1 > num2)
                {
                    var replace = location.LocalInventory.FirstOrDefault(i =>
                        (i?.ComponentRef?.Def is WeaponDef def) && def.Category == weaponDef.Category);
                    if (replace == null)
                        return new ValidateDropError(
                            $"Cannot add {weaponDef.Description.Name} to {location.LocationName}: There are no available {weaponDef.Category.ToString()} hardpoints.");
                    else
                    {
                        return ValidateDropChange.AddOrCreate(last_result,
                            new RemoveChange(location.widget.loadout.Location, replace));
                    }
                }

            }
            return last_result;
        }

        private static IValidateDropResult ValidateBase(MechLabItemSlotElement element, LocationHelper location, IValidateDropResult last_result)
        {
            var component = element.ComponentRef.Def;

            if (location.widget.loadout.CurrentInternalStructure <= 0f)
            {
                return new ValidateDropError(
                    $"Cannot add {component.Description.Name} to {location.LocationName}: The location is Destroyed.");
            }
            if ((component.AllowedLocations & location.widget.loadout.Location) <= ChassisLocations.None)
            {
                return new ValidateDropError(
                    $"Cannot add {component.Description.Name} to {location.LocationName}: Component is not permitted in this location.");
            }

            if (component.ComponentType == ComponentType.JumpJet)
            {
                var mechlab = location.mechLab;
                int num3 = mechlab.headWidget.currentJumpjetCount + mechlab.centerTorsoWidget.currentJumpjetCount +
                           mechlab.leftTorsoWidget.currentJumpjetCount + mechlab.rightTorsoWidget.currentJumpjetCount +
                           mechlab.leftArmWidget.currentJumpjetCount + mechlab.rightArmWidget.currentJumpjetCount +
                           mechlab.leftLegWidget.currentJumpjetCount + mechlab.rightLegWidget.currentJumpjetCount;
                if (num3 + 1 > mechlab.activeMechDef.Chassis.MaxJumpjets)
                    return new ValidateDropError(
                            $"Cannot add {component.Description.Name} to {location.LocationName}: Max number of jumpjets for 'Mech reached.");
            }
            return null;
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
}
