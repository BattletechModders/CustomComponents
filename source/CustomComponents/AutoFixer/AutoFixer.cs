using System;
using System.Collections.Generic;
using BattleTech;

namespace CustomComponents;

public delegate void AutoFixerDelegate(List<MechDef> mechDefs);

public class AutoFixer: IMechDefProcessor
{
    public static AutoFixer Shared = new();

    private readonly List<AutoFixerDelegate> fixers = new();

    public void ProcessMechDefs(List<MechDef> mechDefs)
    {
        var work_list = new List<MechDef>();
        foreach (var mechDef in mechDefs)
        {
            var ut = mechDef.GetUnitTypes();
            if(ut == null || !ut.Contains(Control.Settings.IgnoreAutofixUnitType))
            {
                work_list.Add(mechDef);
            }
        }

        Log.AutoFixBase.Trace?.Log($"-- default: mechdefs:{work_list.Count} af:{fixers.Count}");
        foreach (var autoFixerDelegate in fixers)
        {
            try
            {
                autoFixerDelegate(work_list);
            }
            catch (Exception e)
            {
                Log.Main.Error?.Log($"Exception in default Autofixer {autoFixerDelegate.Method.Name}", e);
            }
        }

        if (Control.Settings.DEBUG_ValidateMechDefs)
        {
            work_list.Clear();
            foreach (var mechDef in mechDefs)
            {
                var ut = mechDef.GetUnitTypes();
                if (ut == null || !ut.Contains(Control.Settings.IgnoreAutofixUnitType))
                {
                    work_list.Add(mechDef);
                }
            }
            DEBUG_ValidateAll.Validate(work_list);
        }

        if (Control.Settings.DEBUG_ShowAllUnitTypes)
        {
            UnitTypeDatabase.Instance.ShowRegistredTypes();

            foreach (var mechDef in mechDefs)
            {
                var ut = mechDef.GetUnitTypes();
                Log.UnitType.Trace?.Log($"{mechDef.Description.Id}: [{ (ut == null ? "null" : ut.Join())}]");
            }
        }

        Log.AutoFixBase.Trace?.Log("-- done");

    }

    public void RegisterMechFixer(AutoFixerDelegate fixer)
    {
        fixers.Add(fixer);
    }
}