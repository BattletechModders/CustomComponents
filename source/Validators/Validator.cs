using BattleTech;
using BattleTech.UI;
using System.Collections.Generic;
using System.Linq;

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

        private static string ValidateSize(MechLabItemSlotElement drop_item,List<InvItem> new_inventory)
        {
            var items_by_location = new_inventory
                .GroupBy(i => i.location)
                .Select(i => new { location = i.Key, size = i.Sum(a => a.item.Def.InventorySize) });
            var mech = MechLabHelper.CurrentMechLab.ActiveMech;

            foreach(var record in items_by_location)
            {
                if (record.size > mech.Chassis.GetLocationDef(record.location).InventorySlots)
                    return (new Localize.Text(Control.Settings.Message.Base_AddInventorySize,drop_item.ComponentRef.Def.Description.Name, record.location)).ToString();
            }

            return string.Empty;
        }

        private static string ValidateJumpJets(MechLabItemSlotElement drop_item, List<InvItem> new_inventory)
        {
            var total = new_inventory.Count(i => i.item.ComponentDefType == ComponentType.JumpJet);
            var max = MechLabHelper.CurrentMechLab.ActiveMech.Chassis.MaxJumpjets;
            if (total > max)
                return $"Cannot add {drop_item.ComponentRef.Def.Description.Name}: Max number of jumpjets for 'Mech reached";
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
