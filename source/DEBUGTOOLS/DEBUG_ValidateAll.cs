using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BattleTech;
using Localize;

namespace CustomComponents
{
    public static class DEBUG_ValidateAll
    {
        internal static void Validate(List<MechDef> mechdefs)
        {
            Dictionary<MechValidationType, List<Text>> error = new Dictionary<MechValidationType, List<Text>>();
            var val = Enum.GetValues(typeof(MechValidationType));
            List<MechValidationType> values = new List<MechValidationType>();
            foreach (var item in val)
            {
                var value = (MechValidationType)item;
                values.Add(value);
                error[value] = new List<Text>();
            }

            foreach (var mechDef in mechdefs)
            {
                try
                {
                    foreach (var pair in error)
                    {
                        pair.Value.Clear();
                    }


                    if (mechDef == null)
                    {
                        Control.LogError("NullMECHDEF!");
                        continue;
                    }
                    if (Control.Settings.IgnoreValidationTags != null && Control.Settings.IgnoreValidationTags.Length > 0)
                    {
                        bool skip = false;
                        foreach (var tag in Control.Settings.IgnoreValidationTags)
                        {
                            if ((mechDef.Chassis.ChassisTags != null && mechDef.Chassis.ChassisTags.Contains(tag)) ||
                            (mechDef.MechTags != null && mechDef.MechTags.Contains(tag)))
                            {
                                Control.LogDebug(DType.AutofixValidate, $"{mechDef.Description.Id} Ignored by {tag}");
                                skip = true;
                                break;
                            }
                        }
                        if (skip)
                            continue;
                    }

                    Validator.ValidateMech(error, MechValidationLevel.MechLab, mechDef);
                    foreach (var component in mechDef.Inventory)
                    {
                        foreach (var validator in component.GetComponents<IMechValidate>())
                        {
                            validator.ValidateMech(error, MechValidationLevel.MechLab, mechDef, component);
                        }
                    }
                    bool bad_mech = false;
                    foreach (var pair in error)
                    {
                        if (pair.Value.Count > 0)
                        {
                            if (!bad_mech)
                            {
                                bad_mech = true;
                                Control.LogDebug(DType.AutofixValidate, $"{mechDef.Description.Id} has errors:");
                            }
                            foreach (var text in pair.Value)
                                Control.LogDebug(DType.AutofixValidate, $"[{pair.Key}]:{text}");
                        }
                    }
                    if(!bad_mech && !Control.Settings.DEBUG_ShowOnlyErrors)
                        Control.LogDebug(DType.AutofixValidate, $"{mechDef.Description.Id} no errors");
                }
                catch (Exception e)
                {
                    Control.LogError($"{mechDef.Description.Id} throwed exception on validation", e);
                }
            }

        }
    }
}
