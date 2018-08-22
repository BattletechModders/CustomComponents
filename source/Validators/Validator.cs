﻿using BattleTech;
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
        public static List<PreValidateDropDelegate> pre_drop_validators = new List<PreValidateDropDelegate>();
        public static List<ReplaceValidateDropDelegate> rep_drop_validators = new List<ReplaceValidateDropDelegate>();
        public static List<PostValidateDropDelegate> chk_drop_validators = new List<PostValidateDropDelegate>();
        public static List<ValidateMechDelegate> mech_validators = new List<ValidateMechDelegate>();
        private static List<ValidateMechCanBeFieldedDelegate> field_validators =
            new List<ValidateMechCanBeFieldedDelegate>();

        /// <summary>
        /// register new AddValidator
        /// </summary>
        public static void RegisterDropValidator(PreValidateDropDelegate pre = null, ReplaceValidateDropDelegate replace = null,
             PostValidateDropDelegate check = null)
        {
            if (pre != null)
                pre_drop_validators.Add(pre);

            if (replace != null)
                rep_drop_validators.Add(replace);

            if (check != null)
                chk_drop_validators.Add(check);

        }

        public static ReplaceValidateDropDelegate HardpointValidator { get; set; } = null;

        /// <summary>
        /// register new mech validator
        /// </summary>
        public static void RegisterMechValidator(ValidateMechDelegate mechvalidator,
            ValidateMechCanBeFieldedDelegate fieldvalidator)
        {
            if (mechvalidator != null) mech_validators.Add(mechvalidator);
            if (fieldvalidator != null) field_validators.Add(fieldvalidator);
        }

        internal static IEnumerable<PreValidateDropDelegate> GetPre(MechComponentDef component)
        {
            yield return ValidateBase;

            foreach (var validator in component.GetComponents<IPreValidateDrop>())
                yield return validator.PreValidateDrop;

            foreach (var validator in pre_drop_validators)
                yield return validator;
        }


        internal static IEnumerable<ReplaceValidateDropDelegate> GetReplace(MechComponentDef component)
        {
            if (HardpointValidator != null)
                yield return HardpointValidator;
            else
                yield return ValidateHardpoint;

            foreach (var validator in rep_drop_validators)
                yield return validator;

            foreach (var item in component.GetComponents<IReplaceValidateDrop>())
                yield return item.ReplaceValidateDrop;
        }

        internal static IEnumerable<PostValidateDropDelegate> GetPost(MechComponentDef component)
        {
            yield return ValidateSize;
            yield return ValidateJumpJets;

            foreach (var validator in component.GetComponents<IPostValidateDrop>())
                yield return validator.PostValidateDrop;

            foreach (var validator in chk_drop_validators)
                yield return validator;
        }

        private static string ValidateBase(MechLabItemSlotElement item, LocationHelper location, MechLabHelper mechlab)
        {
            var component = item.ComponentRef.Def;

            if (location.widget.loadout.CurrentInternalStructure <= 0f)
            {
                return $"Cannot add {component.Description.Name} to {location.LocationName}: The location is Destroyed.";
            }
            if ((component.AllowedLocations & location.widget.loadout.Location) <= ChassisLocations.None)
            {
                return $"Cannot add {component.Description.Name} to {location.LocationName}: Component is not permitted in this location.";
            }
            return string.Empty;
        }

        private static string ValidateSize(MechLabItemSlotElement drop_item, MechDef mech, List<InvItem> new_inventory, List<IChange> changes)
        {
            var change_by_location = changes
                .OfType<SlotChange>()
                .Select(slot => new { location = slot.location, val = slot.item.ComponentRef.Def.InventorySize * (slot is AddChange ? 1 : -1) })
                .GroupBy(s => s.location)
                .Select(s => new { location = s.Key, val = s.Sum(i => i.val) });

            Control.Logger.LogDebug($"drop_item={drop_item.ComponentRef.Def.Description.Id}");
            foreach(var location in change_by_location)
            {
                Control.Logger.LogDebug($"location={location.location}");
                foreach (var item in mech.Inventory.Where(i => i.MountedLocation == location.location))
                {
                    Control.Logger.LogDebug($" mech.Inventory item={item.Def.Description.Id} size={item.Def.InventorySize}");
                }
                foreach (var item in new_inventory.Where(i => i.location == location.location))
                {
                    Control.Logger.LogDebug($" new_inventory  item={item.item.Def.Description.Id} size={item.item.Def.InventorySize}");
                }
                int used = mech.Inventory.Where(i => i.MountedLocation == location.location).Sum(i => i.Def.InventorySize);
                int max = mech.GetChassisLocationDef(location.location).InventorySlots;
                Control.Logger.LogDebug($" used={used} location.val={location.val} max={max}");

                if (used + location.val > max)
                    return $"Cannot add {drop_item.ComponentRef.Def.Description.Name}: Not enough free slots.";
            }
            return string.Empty;
        }

        private static string ValidateJumpJets(MechLabItemSlotElement drop_item, MechDef mech, List<InvItem> new_inventory, List<IChange> changes)
        {
            var total = new_inventory.Count(i => i.item.ComponentDefType == ComponentType.JumpJet);
            var max = mech.Chassis.MaxJumpjets;
            if (total > max)
                return $"Cannot add {drop_item.ComponentRef.Def.Description.Name}: Max number of jumpjets for 'Mech reached";
            return string.Empty;
        }


        private static string ValidateHardpoint(MechLabItemSlotElement drop_item, LocationHelper location, ref MechLabItemSlotElement current_replace)
        {

            // if dropped item not weapon - skip check
            if (drop_item.ComponentRef.Def.ComponentType != ComponentType.Weapon)
                return string.Empty;

            // if dropped item and replacement both same type weapon - allow replace
            if (current_replace != null
                && current_replace.ComponentRef.Def.ComponentType == ComponentType.Weapon
                && current_replace.weaponDef.Category == drop_item.weaponDef.Category)
                return string.Empty;


            //calculate hardpoint
            int num = 0;
            int num2 = 0;
            WeaponDef weaponDef = drop_item.ComponentRef.Def as WeaponDef;
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

            if (current_replace != null)
            {
                if (num >= num2)
                    return $"Cannot add {weaponDef.Description.Name} to {location.LocationName}: There are no available {weaponDef.Category.ToString()} hardpoints.";
            }
            else if (num == num2)
            {
                var replace = location.LocalInventory.FirstOrDefault(i =>
                    (i?.ComponentRef?.Def is WeaponDef def) && def.Category == weaponDef.Category && def.Description.Id != drop_item.ComponentRef.ComponentDefID);
                if (replace == null)
                    return $"Cannot add {weaponDef.Description.Name} to {location.LocationName}: There are no available {weaponDef.Category.ToString()} hardpoints.";
                else
                    current_replace = replace;
            }
            else if (num > num2)
                return $"Cannot add {weaponDef.Description.Name} to {location.LocationName}: There are no available {weaponDef.Category.ToString()} hardpoints.";

            return string.Empty; 
        }

        internal static void ValidateMech(Dictionary<MechValidationType, List<Localize.Text>> errors,
            MechValidationLevel validationLevel, MechDef mechDef)
        {
            foreach (var validator in mech_validators)
            {
                validator(errors, validationLevel, mechDef);
            }

            var sizes = mechDef.Inventory.Select(cref =>
                    new { location = cref.MountedLocation, size = cref.Def.InventorySize })
                .GroupBy(i => i.location)
                .Select(i => new { location = i.Key, size = i.Sum(a => a.size) }).ToList();

            foreach (var size in sizes)
            {
                if (mechDef.GetChassisLocationDef(size.location).InventorySlots < size.size)
                {
                    errors[MechValidationType.InvalidInventorySlots].Add(new Localize.Text($"{size.location} no space left, remove excess equipment"));
                }
            }
        }

        internal static bool ValidateMechCanBeFielded(MechDef mechDef)
        {
            foreach (var validateMechCanBeFieldedDelegate in field_validators)
            {
                if (!validateMechCanBeFieldedDelegate(mechDef))
                    return false;
            }

            var sizes = mechDef.Inventory.Select(cref =>
                    new {location = cref.MountedLocation, size = cref.Def.InventorySize})
                .GroupBy(i => i.location)
                .Select(i => new {location = i.Key, size = i.Sum(a => a.size)}).ToList();

            foreach (var size in sizes)
            {
                if (mechDef.GetChassisLocationDef(size.location).InventorySlots < size.size)
                    return false;
            }

            return true;
        }
    }
}
