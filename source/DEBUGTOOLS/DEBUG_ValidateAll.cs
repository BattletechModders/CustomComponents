using System;
using System.Collections.Generic;
using BattleTech;

namespace CustomComponents
{
    public static class DEBUG_ValidateAll
    {
        internal static void Validate(List<MechDef> mechdefs)
        {
            foreach (var mechDef in mechdefs)
            {
                try
                {
                    var dm = UnityGameInstance.BattleTechGame.DataManager;
                    var work = new WorkOrderEntry_MechLab(WorkOrderType.MechLabGeneric, "test", "test", "test", 0);

                    if (mechDef == null)
                    {
                        Logging.Error?.Log("NullMECHDEF!");
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
                                Logging.Debug?.LogDebug(DType.AutofixValidate, $"{mechDef.Description.Id} Ignored by {tag}");
                                skip = true;
                                break;
                            }
                        }
                        if (skip)
                            continue;
                    }

                    var error = MechValidationRules.ValidateMechDef(MechValidationLevel.Full, dm, 
                        mechDef, work);
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
                                Logging.Debug?.LogDebug(DType.AutofixValidate, $"{mechDef.Description.Id} has errors:");
                            }
                            foreach (var text in pair.Value)
                            {
                                Logging.Debug?.LogDebug(DType.AutofixValidate, $"[{pair.Key}]:{text}");
                            }
                        }
                    }
                    if (!bad_mech && !Control.Settings.DEBUG_ShowOnlyErrors)
                    {
                        Logging.Debug?.LogDebug(DType.AutofixValidate, $"{mechDef.Description.Id} no errors");
                    }
                }
                catch (Exception e)
                {
                    Logging.Error?.Log($"{mechDef.Description.Id} throwed exception on validation", e);
                }
            }
        }
    }
}
