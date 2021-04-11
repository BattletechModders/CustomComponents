using BattleTech;
using BattleTech.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using Harmony;
using HBS.Extensions;

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
        internal static List<ClearInventoryDelegate> clear_inventory = new List<ClearInventoryDelegate>();
        //public static List<ValidateAdjustDelegate> adjust_validators= new List<ValidateAdjustDelegate>();

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

        //public static void RegisterAdjustValidatod(ValidateAdjustDelegate adjust)
        //{
        //    adjust_validators.Add(adjust);
        //}

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

        public static void RegisterClearInventory(ClearInventoryDelegate clear)
        {
            if (clear != null) clear_inventory.Add(clear);
        }

        internal static IEnumerable<PreValidateDropDelegate> GetPre(MechComponentDef component)
        {
            yield return ValidateBase;

            if (Control.Settings.BaseECMValidation)
                yield return ValidateECM;

            foreach (var validator in component.GetComponents<IPreValidateDrop>())
                yield return validator.PreValidateDrop;

            foreach (var validator in pre_drop_validators)
                yield return validator;
        }



        internal static IEnumerable<ReplaceValidateDropDelegate> GetReplace(MechComponentDef component)
        {

            foreach (var validator in rep_drop_validators)
                yield return validator;

            foreach (var item in component.GetComponents<IReplaceValidateDrop>())
                yield return item.ReplaceValidateDrop;

            if (HardpointValidator != null)
                yield return HardpointValidator;
            else
                yield return ValidateHardpoint;

        }

        internal static IEnumerable<PostValidateDropDelegate> GetPost(MechComponentDef component)
        {

            foreach (var validator in component.GetComponents<IPostValidateDrop>())
                yield return validator.PostValidateDrop;

            foreach (var validator in chk_drop_validators)
                yield return validator;
            
            yield return ValidateSize;
            yield return ValidateJumpJets;
        }


        private static string ValidateECM(MechLabItemSlotElement item, ChassisLocations locations)
        {
            var def = item.ComponentRef.Def;

            if (def.ComponentSubType < MechComponentType.Prototype_Generic &&
                def.ComponentSubType != MechComponentType.ElectronicWarfare)
                return string.Empty;

            int count = MechLabHelper.CurrentMechLab.ActiveMech.Inventory.Count(cref => cref.Def.ComponentSubType == def.ComponentSubType);

            if (count > 0)
                if (def.ComponentSubType == MechComponentType.ElectronicWarfare || def.ComponentSubType == MechComponentType.Prototype_ElectronicWarfare)
                    return
                        "ELECTRONIC WARFARE COMPONENT LIMIT: You can only equip one Electronic Warfare component on this 'Mech.";
                else
                    return
                        $"PROTOTYPE COMPONENT LIMIT: You can only equip one {def.ComponentSubType} component on this 'Mech";

            return string.Empty;
        }

        private static string ValidateBase(MechLabItemSlotElement item, ChassisLocations locations)
        {
            var component = item.ComponentRef.Def;
            var lhelper = MechLabHelper.CurrentMechLab.GetLocationHelper(locations);
            var mech = MechLabHelper.CurrentMechLab.ActiveMech;

            if (lhelper.widget.loadout.CurrentInternalStructure <= 0f)
            {
                // 0 - item Name, 1 - Location name, 2 - item.uiname
                return new Localize.Text(Control.Settings.Message.Base_LocationDestroyed, component.Description.Name, lhelper.LocationName, component.Description.UIName).ToString();
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

            Control.LogDebug(DType.ComponentInstall, $"drop_item={drop_item.ComponentRef.Def.Description.Id}");

            foreach (var location in change_by_location)
            {
#if CCDEBUG
                if (Control.Settings.DebugInfo.HasFlag(DType.ComponentInstall))
                    Control.LogDebug(DType.ComponentInstall, $"location={location.location}");
                foreach (var item in mech.Inventory.Where(i => i.MountedLocation == location.location))
                {
                    Control.LogDebug(DType.ComponentInstall, $" mech.Inventory item={item.Def.Description.Id} size={item.Def.InventorySize}");
                }
                foreach (var item in new_inventory.Where(i => i.location == location.location))
                {
                    Control.LogDebug(DType.ComponentInstall, $" new_inventory  item={item.item.Def.Description.Id} size={item.item.Def.InventorySize}");
                }
#endif
                int used = mech.Inventory.Where(i => i.MountedLocation == location.location).Sum(i => i.Def.InventorySize);
                int max = mech.GetChassisLocationDef(location.location).InventorySlots;
                Control.LogDebug(DType.ComponentInstall, $" used={used} location.val={location.val} max={max}");

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


        private static string ValidateHardpoint(MechLabItemSlotElement drop_item, ChassisLocations location, Queue<IChange> changes)
        {
            Control.LogDebug(DType.ComponentInstall, $"-- Hardpoints");
            // if dropped item not weapon - skip check
            if (drop_item.ComponentRef.Def.ComponentType != ComponentType.Weapon)
            {
                Control.LogDebug(DType.ComponentInstall, $"--- Not a weapon");
                return string.Empty;
            }

            if (changes.Count == 1)
            {
                //calculate hardpoint
                int num = 0;
                int num2 = 0;
                WeaponDef weaponDef = drop_item.ComponentRef.Def as WeaponDef;

                var mech = MechLabHelper.CurrentMechLab.ActiveMech;
                var lhelper = MechLabHelper.CurrentMechLab.GetLocationHelper(location);

                var replace_list = lhelper.LocalInventory.Where(i =>
                            (i?.ComponentRef?.Def is WeaponDef def)
                            && def.Description.Id != drop_item.ComponentRef.ComponentDefID
                            && !i.ComponentRef.IsModuleFixed(mech));

                if (weaponDef.WeaponCategoryValue.IsBallistic)
                {
                    num = lhelper.currentBallisticCount;
                    num2 = lhelper.totalBallisticHardpoints;
                    replace_list = replace_list.Where(i => i.weaponDef.WeaponCategoryValue.IsBallistic);
                }
                else if (weaponDef.WeaponCategoryValue.IsEnergy)
                {
                    num = lhelper.currentEnergyCount;
                    num2 = lhelper.totalEnergyHardpoints;
                    replace_list = replace_list.Where(i => i.weaponDef.WeaponCategoryValue.IsEnergy);
                }
                else if (weaponDef.WeaponCategoryValue.IsMissile)
                {
                    num = lhelper.currentMissileCount;
                    num2 = lhelper.totalMissileHardpoints;
                    replace_list = replace_list.Where(i => i.weaponDef.WeaponCategoryValue.IsMissile);
                }
                else if (weaponDef.WeaponCategoryValue.IsSupport)
                {
                    num = lhelper.currentSmallCount;
                    num2 = lhelper.totalSmallHardpoints;
                    replace_list = replace_list.Where(i => i.weaponDef.WeaponCategoryValue.IsSupport);
                }

                if (num2 == 0)
                    return $"Cannot add {weaponDef.Description.Name} to {lhelper.LocationName}: There are no available hardpoints.";
                if (num > num2)
                    return $"Cannot add {weaponDef.Description.Name} to {lhelper.LocationName}: There are no available hardpoints.";

                if (num == num2)
                {
                    var replace = replace_list.FirstOrDefault();
                    if (replace == null)
                        return
                            $"Cannot add {weaponDef.Description.Name} to {lhelper.LocationName}: There are no available hardpoints.";
                    else
                        changes.Add(new RemoveChange(location, replace));
                }
            }
            else
            { 
                //!TODO Check for found replacements

            }
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
                    new { location = cref.MountedLocation, size = cref.Def.InventorySize })
                .GroupBy(i => i.location)
                .Select(i => new { location = i.Key, size = i.Sum(a => a.size) }).ToList();

            foreach (var size in sizes)
            {
                if (mechDef.GetChassisLocationDef(size.location).InventorySlots < size.size)
                    return false;
            }

            return true;
        }
    }
}
