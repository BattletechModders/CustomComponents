using System.Collections.Generic;
using System.Linq;
using BattleTech;
using BattleTech.UI;

namespace CustomComponents
{
    internal static class WeighLimitedController
    {
        internal static void ValidateMech(Dictionary<MechValidationType, List<string>> errors,
            MechValidationLevel validationLevel, MechDef mechDef)
        {
            foreach (var item in mechDef.Inventory
                .Where(i => i.Def != null)
                .Select(i => new { def = i.Def, limit = i.Def.GetComponent<WeightLimited>() })
                .Where(i => i.limit != null && i.limit.MinTonnage < mechDef.Chassis.Tonnage || i.limit.MaxTonnage > mechDef.Chassis.Tonnage))
            {
                if (item.limit.MinTonnage == item.limit.MaxTonnage)
                    errors[MechValidationType.InvalidInventorySlots].Add(
                        $"{item.def.Description.Name} designed for {item.limit.MinTonnage}t mech");
                else
                    errors[MechValidationType.InvalidInventorySlots].Add(
                        $"{item.def.Description.Name.ToUpper()} designed for {item.limit.MinTonnage}t-{item.limit.MaxTonnage}t mech");
            }

            foreach (var item in mechDef.Inventory
                .Where(i => i.Def != null)
                .Select(i => new { def = i.Def, limit = i.Def.GetComponent<WeightAllowed>() })
                .Where(i => i.limit != null && i.limit.AllowedTonnage == mechDef.Chassis.Tonnage))
            {
                errors[MechValidationType.InvalidInventorySlots].Add(
                    $"{item.def.Description.Name} designed for {item.limit.AllowedTonnage}t mech");
            }
        }

        internal static IValidateDropResult ValidateDrop(MechLabItemSlotElement element, LocationHelper location, IValidateDropResult last_result)
        {
            var component = element.ComponentRef.Def;

            if (component.Is<WeightLimited>(out var wlimit))
            {
                var tonnage = location.mechLab.activeMechDef.Chassis.Tonnage;

                if (tonnage < wlimit.MinTonnage ||
                    tonnage > wlimit.MaxTonnage)
                {
                    if (wlimit.MinTonnage == wlimit.MaxTonnage)
                        return new ValidateDropError($"{component.Description.Name} designed for {wlimit.MinTonnage}t mech");
                    else
                        return new ValidateDropError($"{component.Description.Name} designed for {wlimit.MinTonnage}t-{wlimit.MaxTonnage}t mech");
                }
            }

            if (component.Is<WeightAllowed>(out var alimit))
            {
                if (location.mechLab.activeMechDef.Chassis.Tonnage != alimit.AllowedTonnage)
                {
                    return new ValidateDropError($"{component.Description.Name} designed for {alimit.AllowedTonnage}t mech");
                }
            }

            return last_result;
        }

        internal static bool ValidateMechCanBeFielded(MechDef mechDef)
        {
            if (mechDef.Inventory.Where(i => i.Def != null).Select(i => i.Def.GetComponent<WeightLimited>())
                .Where(i => i != null).Any(component => component.MinTonnage < mechDef.Chassis.Tonnage || component.MaxTonnage > mechDef.Chassis.Tonnage))
            {
                return false;
            }

            return mechDef.Inventory
                .Select(i => i.Def.GetComponent<WeightAllowed>())
                .Where(i => i != null)
                .All(component => component.AllowedTonnage == mechDef.Chassis.Tonnage);
        }
    }
}