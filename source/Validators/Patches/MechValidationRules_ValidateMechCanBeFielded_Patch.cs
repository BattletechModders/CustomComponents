using System;
using BattleTech;

namespace CustomComponents;

[HarmonyPatch(typeof(MechValidationRules), "ValidateMechCanBeFielded")]
internal static class MechValidationRules_ValidateMechCanBeFielded_Patch
{
    public static void Postfix(MechDef mechDef, ref bool __result)
    {
        try
        {
            if (mechDef == null)
            {
                Log.MechValidation.Trace?.Log("Mech validation for NULL return");
                return;
            }

            Log.MechValidation.Trace?.Log($"Mech validation for {mechDef.Name} start from {__result}");

            if(Control.Settings.IgnoreValidationTags != null && Control.Settings.IgnoreValidationTags.Length > 0)
            {
                foreach (var tag in Control.Settings.IgnoreValidationTags)
                {
                    if ((mechDef.Chassis.ChassisTags != null && mechDef.Chassis.ChassisTags.Contains(tag)) ||
                        (mechDef.MechTags != null && mechDef.MechTags.Contains(tag)))
                    {
                        Log.MechValidation.Trace?.Log($"- Ignored by {tag}");
                        __result = true;
                        return;
                    }
                }
            }

            if (!__result)
            {
                Log.MechValidation.Trace?.Log("- failed base validation");
                return;
            }

            Log.MechValidation.Trace?.Log("- fixed validation");
            if (!Validator.ValidateMechCanBeFielded(mechDef))
            {
                __result = false;
                Log.MechValidation.Trace?.Log("- failed fixed validation");
                return;
            }

            Log.MechValidation.Trace?.Log("- component validation");
            foreach (var component in mechDef.Inventory)
            {
                foreach (var mechValidate in component.GetComponents<IMechValidate>())
                {
                    Log.MechValidation.Trace?.Log($"-- {mechValidate.GetType()}");
                    if (!mechValidate.ValidateMechCanBeFielded(mechDef, component))
                    {
                        __result = false;
                        Log.MechValidation.Trace?.Log("- failed component validation");
                        return;
                    }
                }


                if (component.Is<IAllowedLocations>(out var locations)
                    && (component.MountedLocation & locations.GetLocationsFor(mechDef)) <= ChassisLocations.None)
                {
                    __result = false;
                    Log.MechValidation.Trace?.Log("- failed component location validation");
                    return;
                }

            }

            Log.MechValidation.Trace?.Log("- validation passed");
        }

        catch (Exception e)
        {
            Log.Main.Error?.Log(e);
        }
    }
}