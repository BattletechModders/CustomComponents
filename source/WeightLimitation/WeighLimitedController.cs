using System;
using System.Collections.Generic;
using System.Linq;
using BattleTech;
using BattleTech.UI;

namespace CustomComponents
{
    internal static class WeighLimitedController
    {
        internal static bool ValidateAdd(MechComponentDef component, MechLabLocationWidget widget,
            bool current_result, ref string errorMessage, MechLabPanel mechlab)
        {
            if (!current_result)
                return false;

            if (component is IWeightLimited)
            {
                var limit = component as IWeightLimited;

                if (mechlab.activeMechDef.Chassis.Tonnage < limit.MinTonnage ||
                    mechlab.activeMechDef.Chassis.Tonnage > limit.MaxTonnage)
                {
                    if (limit.MinTonnage == limit.MaxTonnage)
                        errorMessage = string.Format("{0} designed for {1}t mech", component.Description.Name,
                            limit.MinTonnage);
                    else
                        errorMessage = string.Format("{0} designed for {1}t-{2}t mech", component.Description.Name,
                            limit.MinTonnage, limit.MaxTonnage);
                    return false;
                }
            }

            return true;
        }

        internal static void ValidateMech(Dictionary<MechValidationType, List<string>> errors,
            MechValidationLevel validationLevel, MechDef mechDef)
        {
            foreach (var component in mechDef.Inventory
                .Where(i => i.Def != null)
                .Select(i => i.Def)
                .OfType<IWeightLimited>()
                .Where(i => i.MinTonnage < mechDef.Chassis.Tonnage || i.MaxTonnage > mechDef.Chassis.Tonnage))
            {
                var item = component as MechComponentDef;
                if (component.MinTonnage == component.MaxTonnage)
                    errors[MechValidationType.InvalidInventorySlots].Add(
                        $"{item.Description.Name} designed for {component.MinTonnage}t mech");
                else
                    errors[MechValidationType.InvalidInventorySlots].Add(
                        $"{item.Description.Name.ToUpper()} designed for {component.MinTonnage}t-{component.MaxTonnage}t mech");
            }

            foreach (var component in mechDef.Inventory
                .Where(i => i.Def != null)
                .Select(i => i.Def)
                .OfType<IWeightAllowed>()
                .Where(i => i.AllowedTonnage != mechDef.Chassis.Tonnage))
            {
                var item = component as MechComponentDef;
                errors[MechValidationType.InvalidInventorySlots].Add(
                    $"{item.Description.Name} designed for {component.AllowedTonnage}t mech");
            }
        }

        internal static IValidateDropResult ValidateDrop(MechLabItemSlotElement element, LocationHelper location, IValidateDropResult last_result)
        {
            var component = element.ComponentRef.Def;

            if (component is IWeightLimited wlimit)
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

            if (component is IWeightAllowed alimit)
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
            if (mechDef.Inventory.Where(i => i.Def != null).Select(i => i.Def)
                .OfType<IWeightLimited>().Any(component => component.MinTonnage < mechDef.Chassis.Tonnage || component.MaxTonnage > mechDef.Chassis.Tonnage))
            {
                return false;
            }

            return mechDef.Inventory.Where(i => i.Def != null)
                .Select(i => i.Def)
                .OfType<IWeightAllowed>()
                .All(component => component.AllowedTonnage == mechDef.Chassis.Tonnage);
        }

        internal static bool Filter(MechLabHelper mechlab, MechComponentDef component)
        {
            if (mechlab.MechLab.activeMechDef == null)
                return true;

            var tonnage = mechlab.MechLab.activeMechDef.Chassis.Tonnage;

            if (component is IWeightAllowed wa)
            {
                return wa.AllowedTonnage == tonnage;
            }

            if (component is IWeightLimited wl)
            {
                return wl.MinTonnage >= tonnage && wl.MaxTonnage <= tonnage;
            }

            return true;
        }
    }
}