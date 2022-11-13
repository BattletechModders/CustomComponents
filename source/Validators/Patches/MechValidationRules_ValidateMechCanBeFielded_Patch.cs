using System;
using BattleTech;
using Harmony;

namespace CustomComponents
{
    [HarmonyPatch(typeof(MechValidationRules), "ValidateMechCanBeFielded")]
    internal static class MechValidationRules_ValidateMechCanBeFielded_Patch
    {
        public static void Postfix(MechDef mechDef, ref bool __result)
        {
            try
            {
                if (mechDef == null)
                {
                    Logging.Debug?.LogDebug(DType.MechValidation, $"Mech validation for NULL return");
                    return;
                }

                Logging.Debug?.LogDebug(DType.MechValidation, $"Mech validation for {mechDef.Name} start from {__result}");

                if(Control.Settings.IgnoreValidationTags != null && Control.Settings.IgnoreValidationTags.Length > 0)
                    foreach (var tag in Control.Settings.IgnoreValidationTags)
                    {
                        if ((mechDef.Chassis.ChassisTags != null && mechDef.Chassis.ChassisTags.Contains(tag)) ||
                            (mechDef.MechTags != null && mechDef.MechTags.Contains(tag)))
                        {
                            Logging.Debug?.LogDebug(DType.MechValidation, $"- Ignored by {tag}");
                            __result = true;
                            return;
                        }
                    }
                
                if (!__result)
                {
                    Logging.Debug?.LogDebug(DType.MechValidation, $"- failed base validation");
                    return;
                }

                Logging.Debug?.LogDebug(DType.MechValidation, $"- fixed validation");
                if (!Validator.ValidateMechCanBeFielded(mechDef))
                {
                    __result = false;
                    Logging.Debug?.LogDebug(DType.MechValidation, $"- failed fixed validation");
                    return;
                }

                Logging.Debug?.LogDebug(DType.MechValidation, $"- component validation");
                foreach (var component in mechDef.Inventory)
                {
                    foreach (var mechValidate in component.GetComponents<IMechValidate>())
                    {
                        Logging.Debug?.LogDebug(DType.MechValidation, $"-- {mechValidate.GetType()}");
                        if (!mechValidate.ValidateMechCanBeFielded(mechDef, component))
                        {
                            __result = false;
                            Logging.Debug?.LogDebug(DType.MechValidation, $"- failed component validation");
                            return;
                        }
                    }


                    if (component.Is<IAllowedLocations>(out var locations)
                        && (component.MountedLocation & locations.GetLocationsFor(mechDef)) <= ChassisLocations.None)
                    {
                        __result = false;
                        Logging.Debug?.LogDebug(DType.MechValidation, $"- failed component location validation");
                        return;
                    }

                }

                Logging.Debug?.LogDebug(DType.MechValidation, $"- validation passed");
            }

            catch (Exception e)
            {
                Logging.Error?.Log(e);
            }
        }
    }
}