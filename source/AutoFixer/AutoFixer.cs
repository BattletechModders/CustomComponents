using System;
using System.Collections.Generic;
using System.Linq;
using BattleTech;
using HBS.Collections;

namespace CustomComponents
{
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
                tags = new TagSet();
                if (mech.MechTags != null)
                    tags.UnionWith(mech.MechTags);
                if (mech.Chassis.ChassisTags != null)
                    tags.UnionWith(mech.Chassis.ChassisTags);
            }
        }


        public static AutoFixer Shared = new AutoFixer();

        private readonly List<AutoFixerDelegate> fixers = new List<AutoFixerDelegate>();
        private readonly List<AutoFixerDelegate> savegamefixers = new List<AutoFixerDelegate>();

        private readonly Dictionary<string, List<AutoFixerDelegate>> tagfixers = new Dictionary<string, List<AutoFixerDelegate>>();
        private readonly Dictionary<string, List<AutoFixerDelegate>> tagsavefixers = new Dictionary<string, List<AutoFixerDelegate>>();

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

            var work_list = mechDefs.Where(i => i != null && i.Chassis != null).Select(i => new mech_record(i))
                .ToList();
            var temp_work_list = new List<MechDef>();

            Control.LogDebug(DType.AutoFixBase, $"Running Autofixer for total {work_list.Count} mechdefs");

            foreach (var pair in tagfixers)
            {


                foreach (var mechRecord in work_list.Where(i => !i.processed))
                    if (mechRecord.tags.Contains(pair.Key))
                    {
                        mechRecord.processed = true;
                        temp_work_list.Add(mechRecord.mech);
                    }
                Control.LogDebug(DType.AutoFixBase, $"-- tag:{pair.Key} mechdefs:{temp_work_list.Count} af:{pair.Value.Count}");
                foreach (var autoFixerDelegate in pair.Value)
                {
                    try
                    {
                        autoFixerDelegate(temp_work_list, null);
                    }
                    catch (Exception e)
                    {
                        Control.LogError($"Exception in {pair.Key} Autofixer {autoFixerDelegate.Method.Name}", e);
                    }
                }
            }

            temp_work_list = work_list.Where(i => !i.processed).Select(i => i.mech).ToList();

            Control.LogDebug(DType.AutoFixBase, $"-- default: mechdefs:{temp_work_list.Count} af:{fixers.Count}");
            foreach (var autoFixerDelegate in fixers)
            {
                try
                {
                    autoFixerDelegate(temp_work_list, null);
                }
                catch (Exception e)
                {
                    Control.LogError($"Exception in default Autofixer {autoFixerDelegate.Method.Name}", e);
                }
            }

            if (Control.Settings.DEBUG_ValidateMechDefs)
            {



                DEBUG_ValidateAll.Validate(temp_work_list);

            }

            Control.LogDebug(DType.AutoFixBase, $"-- done");

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

            mechDefs = mechDefs.Where(i => i.Chassis != null).Where(i => !i.IgnoreAutofix()).ToList();

            var work_list = mechDefs.Where(i => i != null && i.Chassis != null).Select(i => new mech_record(i))
                .ToList();
            var temp_work_list = new List<MechDef>();

            foreach (var pair in tagsavefixers)
            {
                foreach (var mechRecord in work_list.Where(i => !i.processed))
                    if (mechRecord.tags.Contains(pair.Key))
                    {
                        mechRecord.processed = true;
                        temp_work_list.Add(mechRecord.mech);
                    }

                foreach (var autoFixerDelegate in pair.Value)
                {
                    try
                    {
                        autoFixerDelegate(temp_work_list, null);
                    }
                    catch (Exception e)
                    {
                        Control.LogError($"Exception in {pair.Key} Autofixer {autoFixerDelegate.Method.Name}", e);
                    }
                }
            }

            temp_work_list = work_list.Where(i => !i.processed).Select(i => i.mech).ToList();

            foreach (var autoFixerDelegate in savegamefixers)
            {
                try
                {
                    autoFixerDelegate(temp_work_list, null);
                }
                catch (Exception e)
                {
                    Control.LogError($"Exception in default Autofixer {autoFixerDelegate.Method.Name}", e);
                }
            }
        }

        public void RegisterMechFixer(AutoFixerDelegate fixer)
        {
            fixers.Add(fixer);
            savegamefixers.Add(fixer);
        }

        public void RegisterMechFixer(AutoFixerDelegate fixer, string tag)
        {
            if (string.IsNullOrEmpty(tag))
            {
                RegisterMechFixer(fixer);
                return;
            }

            List<AutoFixerDelegate> list = null;
            if (!tagfixers.TryGetValue(tag, out list))
            {
                list = new List<AutoFixerDelegate>();
                tagfixers[tag] = list;
            }
            list.Add(fixer);

            if (!tagsavefixers.TryGetValue(tag, out list))
            {
                list = new List<AutoFixerDelegate>();
                tagsavefixers[tag] = list;
            }
            list.Add(fixer);

        }

        public void RegisterSaveMechFixer(AutoFixerDelegate fixer)
        {
            savegamefixers.Add(fixer);
        }

        public void RegisterSaveMechFixer(AutoFixerDelegate fixer, string tag)
        {
            if (string.IsNullOrEmpty(tag))
            {
                RegisterSaveMechFixer(fixer);
            }

            List<AutoFixerDelegate> list = null;
            if (!tagsavefixers.TryGetValue(tag, out list))
            {
                list = new List<AutoFixerDelegate>();
                tagsavefixers[tag] = list;
            }
            list.Add(fixer);
        }



        internal void RemoveEmptyRefs(List<MechDef> mechDefs, SimGameState state)
        {
            foreach (var mechDef in mechDefs)
            {
                if (mechDef.Inventory.All(i => i?.Def != null))
                {
                    continue;
                }

                Control.LogError($"Found NULL in {mechDef.Name}({mechDef.Description.Id})");

                foreach (var r in mechDef.Inventory)
                {
                    if (r.Def == null)
                        Control.LogError($"--- NULL --- {r.ComponentDefID}");
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
}