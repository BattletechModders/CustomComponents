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
            foreach (var component in mechDef.Inventory.Where(i => i.Def != null).Select(i => i.Def)
                .OfType<IWeightLimited>())
            {
                if (component.MinTonnage < mechDef.Chassis.Tonnage || component.MaxTonnage > mechDef.Chassis.Tonnage)
                {
                    var item = component as MechComponentDef;
                    if (component.MinTonnage == component.MaxTonnage)
                        errors[MechValidationType.InvalidInventorySlots].Add(string.Format("{0} designed for {1}t mech",
                            item.Description.Name, component.MinTonnage));
                    else
                        errors[MechValidationType.InvalidInventorySlots].Add(string.Format(
                            "{0} designed for {1}t-{2}t mech",
                            item.Description.Name.ToUpper(), component.MinTonnage, component.MaxTonnage));
                }
            }
        }

        internal static bool ValidateMechCanBeFielded(MechDef mechDef)
        {
            foreach (var component in mechDef.Inventory.Where(i => i.Def != null).Select(i => i.Def)
                .OfType<IWeightLimited>())
            {
                if (component.MinTonnage < mechDef.Chassis.Tonnage || component.MaxTonnage > mechDef.Chassis.Tonnage)
                    return false;
            }
            return true;
        }
    }
}