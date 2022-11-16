using System;
using System.Collections.Generic;
using System.Linq;
using BattleTech;
using Harmony;
using HBS.Collections;

namespace CustomComponents;

public delegate void AutoFixerDelegate(List<MechDef> mechDefs, SimGameState simgame);


public class AutoFixer
{
    private class mech_record
    {
        public MechDef mech;
        public bool processed;
        public TagSet tags;

        public mech_record(MechDef mech)
        {
            this.mech = mech;
            processed = false;
            tags = new();
            if (mech.MechTags != null)
                tags.UnionWith(mech.MechTags);
            if (mech.Chassis.ChassisTags != null)
                tags.UnionWith(mech.Chassis.ChassisTags);
        }
    }


    public static AutoFixer Shared = new();

    private readonly List<AutoFixerDelegate> fixers = new();
    private readonly List<AutoFixerDelegate> savegamefixers = new();

    private readonly Dictionary<string, List<AutoFixerDelegate>> tagfixers = new();
    private readonly Dictionary<string, List<AutoFixerDelegate>> tagsavefixers = new();

    public void FixMechDef(List<MechDef> mechDefs)
    {


        if (!Control.Settings.RunAutofixer)
        {
            return;
        }

        foreach (var mechDef in mechDefs)
        {
            if (mechDef.ChassisID == null)
                mechDef.Refresh();
        }

        var work_list = new List<MechDef>();
        foreach (var mechDef in mechDefs)
        {
            var ut = mechDef.GetUnitTypes();
            if(ut == null || !ut.Contains(Control.Settings.IgnoreAutofixUnitType))
                work_list.Add(mechDef);
        }

        Log.AutoFixBase.Trace?.Log($"-- default: mechdefs:{work_list.Count} af:{fixers.Count}");
        foreach (var autoFixerDelegate in fixers)
        {
            try
            {
                autoFixerDelegate(work_list, null);
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
                    work_list.Add(mechDef);
            }
            DEBUG_ValidateAll.Validate(work_list);
        }

        if (Control.Settings.DEBUG_ShowAllUnitTypes)
        {
            UnitTypeDatabase.Instance.ShowRegistredTypes();

            foreach (var mechDef in mechDefs)
            {
                var ut = mechDef.GetUnitTypes();
                Log.UnitType.Trace?.Log($"{mechDef.Description.Id}: [{ (ut == null ? "null" : ut.Join(null, ", "))}]");
            }
        }

        Log.AutoFixBase.Trace?.Log($"-- done");

    }

    internal void FixSavedMech(List<MechDef> mechDefs, SimGameState state)
    {
        if (!Control.Settings.RunAutofixer || !Control.Settings.FixSaveGameMech)
        {
            return;
        }

        foreach (var mechDef in mechDefs)
        {
            if (mechDef.ChassisID == null)
                mechDef.Refresh();
        }

        mechDefs = mechDefs.Where(i => i.Chassis != null).Where(i => !i.IsBroken()).ToList();

        var work_list = new List<MechDef>();
        foreach (var mechDef in mechDefs)
        {
            var ut = mechDef.GetUnitTypes();
            if (ut == null || !ut.Contains(Control.Settings.IgnoreAutofixUnitType))
                work_list.Add(mechDef);
        }

        foreach (var autoFixerDelegate in savegamefixers)
        {
            try
            {
                autoFixerDelegate(work_list, null);
            }
            catch (Exception e)
            {
                Log.Main.Error?.Log($"Exception in default Autofixer {autoFixerDelegate.Method.Name}", e);
            }
        }
    }

    public void RegisterMechFixer(AutoFixerDelegate fixer)
    {
        fixers.Add(fixer);
        savegamefixers.Add(fixer);
    }

    public void RegisterSaveMechFixer(AutoFixerDelegate fixer)
    {
        savegamefixers.Add(fixer);
    }


    internal void RemoveEmptyRefs(List<MechDef> mechDefs, SimGameState state)
    {
        foreach (var mechDef in mechDefs)
        {
            if (mechDef.Inventory.All(i => i?.Def != null))
            {
                continue;
            }

            Log.Main.Error?.Log($"Found NULL in {mechDef.Name}({mechDef.Description.Id})");

            foreach (var r in mechDef.Inventory)
            {
                if (r.Def == null)
                    Log.Main.Error?.Log($"--- NULL --- {r.ComponentDefID}");
            }

            mechDef.SetInventory(mechDef.Inventory.Where(i => i.Def != null).ToArray());
        }
    }


    internal void EmptyFixer(List<MechDef> mechDefs, SimGameState state)
    {
        return;
    }

    internal void ReAddFixed(List<MechDef> mechDefs, SimGameState state)
    {
        foreach (var mechDef in mechDefs)
        {
            mechDef.SetInventory(mechDef.Inventory.Where(i => !i.IsModuleFixed(mechDef)).ToArray());
            mechDef.Refresh();
        }
    }
}