using System;
using System.Collections.Generic;
using BattleTech;
using Localize;

namespace CustomComponents;

[HarmonyPatch(typeof(MechValidationRules), "ValidateMechDef")]
internal static class MechValidationRulesValidate_ValidateMech_Patch
{
    public static void Postfix(Dictionary<MechValidationType, List<Text>> __result,
        MechValidationLevel validationLevel, MechDef mechDef)
    {
        try
        {
            if(mechDef == null)
            {
                return;
            }

            if (Control.Settings.IgnoreValidationTags != null && Control.Settings.IgnoreValidationTags.Length > 0)
            {
                foreach (var tag in Control.Settings.IgnoreValidationTags)
                {
                    if ((mechDef.Chassis.ChassisTags != null && mechDef.Chassis.ChassisTags.Contains(tag)) ||
                        (mechDef.MechTags!= null && mechDef.MechTags.Contains(tag)))
                    {
                        Log.MechValidation.Trace?.Log($"Validation {mechDef.Description.Id} Ignored by {tag}");
                        foreach (var pair in __result)
                        {
                            pair.Value.Clear();
                        }
                        return;
                    }
                }
            }


            Validator.ValidateMech(__result, validationLevel, mechDef);
            foreach (var component in mechDef.Inventory)
            {
                foreach (var validator in component.GetComponents<IMechValidate>())
                {
                    validator.ValidateMech(__result, validationLevel, mechDef, component);
                }

                if (component.Is<IAllowedLocations>(out var locations)
                    && (component.MountedLocation & locations.GetLocationsFor(mechDef)) <= ChassisLocations.None)
                {
                    __result[MechValidationType.InvalidInventorySlots].Add(
                        new(Control.Settings.Message.Base_LocationDestroyed,
                            component.Def.Description.Name, component.MountedLocation, component.Def.Description.UIName));
                }
            }
        }
        catch (Exception e)
        {
            Log.Main.Error?.Log(e);
        }
    }
}