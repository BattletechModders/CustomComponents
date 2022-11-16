//undef DEBUG

using System;
using System.Collections.Generic;
using System.Linq;
using BattleTech;
using CustomComponents.Changes;

namespace CustomComponents;

public class DefaultFixer
{
    public class inv_change
    {
        private bool add;
        public bool IsAdd => add;
        public bool IsRemove => !add;

        public string Id { get; set; }
        public ChassisLocations Location { get; set; }
        public ComponentType Type { get; set; }

        private inv_change(bool add, string id, ChassisLocations location, ComponentType type = ComponentType.NotSet)
        {
            this.add = add;
            Location = location;
            Id = id;
            Type = type;
        }

        public static inv_change Add(string id, ComponentType type, ChassisLocations location)
        {
            return new(true, id, location, type);
        }

        public static inv_change Remove(string id, ChassisLocations location)
        {
            return new(false, id, location);
        }
    }

    private class free_record
    {
//            public CategoryLimit limit { get; set; }
        public int free { get; set; }

        public ChassisLocations location { get; set; }

        //           public bool HaveMin => limit.Min > 0;
    }

    private class usage_record
    {
        public MultiCategoryDefault mrecord;
        public CategoryDefaultRecord crecord;
        public bool used_now => item != null;
        public bool used_after = false;
        public MechComponentRef item;

        public bool mode { get; private set; }

        public string DefId => mode ? mrecord.Component.Description.Id : crecord.Item.Description.Id;
        public ComponentType Type => mode ? mrecord.Component.ComponentType : crecord.Item.ComponentType;
        public ChassisLocations Location => mode ? mrecord.Location : crecord.Location;

        public usage_record(MultiCategoryDefault mrecord)
        {
            this.mrecord = mrecord;
            mode = true;
        }

        public usage_record(CategoryDefaultRecord crecord)
        {
            this.crecord = crecord;
            mode = false;
        }


    }

    private static DefaultFixer _instance;

    public static DefaultFixer Instance
    {
        get
        {
            if (_instance == null)
                _instance = new();
            return _instance;
        }
    }

    internal void FixMechs(List<MechDef> mechDefs, SimGameState simgame)
    {
        foreach (var mechDef in mechDefs)
        {
            try
            {
                FixMech(simgame, mechDef);
            }
            catch (Exception e)
            {
                Log.Main.Error?.Log($"Error in Default autofixer for {mechDef.Description.Id}", e);
            }
        }
    }

    private void FixMech(SimGameState simgame, MechDef mechDef)
    {
        var defaults = DefaultsDatabase.Instance[mechDef];
        if (defaults == null)
            return;

        var changes = new Queue<IChange>();
        var used = new List<string>();
        if (defaults.Multi != null && defaults.Multi.HasRecords)
            used.AddRange(defaults.Multi.UsedCategories.Keys);

        if (defaults.Defaults != null && defaults.Defaults.Count > 0)
            used.AddRange(defaults.Defaults.Keys);

        foreach (var category in used.Distinct())
            changes.Enqueue(new Change_CategoryAdjust(category));

        if (changes.Count == 0)
            return;

        var state = new InventoryOperationState(changes, mechDef);
        state.DoChanges();
        state.ApplyInventory();
    }


    //private static int count = 0;

    public void DoMultiChange(InventoryOperationState state)
    {
        var mech = state.Mech;
        var defaults = DefaultsDatabase.Instance[mech];

        if (defaults?.Multi == null || !defaults.Multi.HasRecords)
            return;

        var usage = defaults.Multi.Defaults
            .Select(i => new usage_record(i))
            .ToList();

        var free = defaults.Multi.UsedCategories.ToDictionary(
            i => i.Key,
            i => i.Value.LocationLimits
                .Where(a => a.Value.Limited)
                .Select(a => new free_record()
                {
                    free = a.Value.Limit,
                    location = a.Key,
                }).ToArray());


        //Control.Log("-- Inv Scan");

        foreach (var invItem in state.Inventory)
        {
            var item = usage.FirstOrDefault(i => !i.used_now && i.DefId == invItem.Item.ComponentDefID
                                                             && i.Location == invItem.Location);
            if (item != null)
            {
                item.item = invItem.Item;
                //Control.Log($"--- Found {item.DefId} {item.used_now} {item.item}");
            }
            else if (invItem.Item.IsModuleFixed(mech) || !defaults.IsCatDefault(invItem.Item.ComponentDefID))
            {
                //Control.Log($"--- Not Found {invItem.Item.ComponentDefID}");
                foreach (var catid in defaults.Multi.UsedCategories.Keys)
                    if (invItem.Item.IsCategory(catid, out var cat))
                    {
                        foreach (var freeRecord in free[catid].Where(i => i.location.HasFlag(invItem.Location)))
                            freeRecord.free -= cat.Weight;
                    }
            }
            else
            {
                //Control.Log($"--- Skipped {invItem.Item.ComponentDefID}");
            }
        }

        var used_records = new List<(free_record record, int value)>();

        foreach (var usageRecord in usage)
        {
            var fit = true;

            used_records.Clear();
            ////Control.Log($"-- before {usageRecord.DefId} - {usageRecord.Location} - now:{usageRecord.used_now} - after:{usageRecord.used_after}");
            //foreach (var pair in free)
            //{
            //    //Control.Log("--- " + pair.Key);
            //    foreach (var freeRecord in pair.Value)
            //    {
            //        Control.Log($"---- {freeRecord.location}: {freeRecord.free}");
            //    }
            //}

            foreach (var pair in usageRecord.mrecord.CategoryRecords)
            {
                var freerecords = free[pair.Key];
                var num = pair.Value.category.Weight;

                foreach (var record in freerecords.Where(i => i.location.HasFlag(usageRecord.Location)))
                {
                    if (record.free < num)
                    {
                        fit = false;
                        break;
                    }

                    used_records.Add((record, num));
                }

                if (!fit)
                    break;
            }

            fit = fit && used_records.Count > 0;

            usageRecord.used_after = fit;
            //Control.Log($"--- used records : {used_records.Count}");

            if (fit)
                foreach (var value in used_records)
                {
                    //Control.Log($"---- {value.record.location}: {value.record.free}-{value.value}");
                    value.record.free -= value.value;
                }

            //Control.Log($"-- after {usageRecord.DefId} - {usageRecord.Location} - now:{usageRecord.used_now} - after:{usageRecord.used_after}");
            if (usageRecord.used_after != usageRecord.used_now)
            {
                var d = usageRecord.mrecord;
                if (usageRecord.used_after)
                {
                    //Control.Log($"-- Add {d.DefID} => {d.Location}");
                    state.AddChange(new Change_Add(d.DefID, d.ComponentType, d.Location));
                }
                else
                {
                    //Control.Log($"-- Remove {d.DefID} =X {d.Location}");
                    state.AddChange(new Change_Remove(d.DefID, d.Location));
                }
            }

        }
    }

    public void DoDefaultsChange(InventoryOperationState state, string categoryId)
    {
        //Control.Log($"- DoDefaultsChange");
        var mech = state.Mech;
        var record = DefaultsDatabase.Instance[mech];


        if (!record.Defaults.TryGetValue(categoryId, out var defaults) || defaults?.Defaults == null)
        {
            //Control.Log($"- empty defaults list, clearing");
            foreach (var item in state.Inventory)
            {
                if (item.Item.IsCategory(categoryId) && record.IsSingleCatDefault(item.Item.ComponentDefID, categoryId) &&
                    !item.Item.IsModuleFixed(mech))
                {
                    state.AddChange(new Change_Remove(item.Item.ComponentDefID, item.Location));
                }
            }

            return;
        }

        //Control.Log($"- usage create");

        var usage = defaults.Defaults
            .Select(i => new usage_record(i))
            .ToList();
        //Control.Log($"-- {(usage == null ? "null" : usage.Count.ToString() + " items")}");

        //Control.Log($"- free create");
        var free = defaults.CategoryRecord.LocationLimits
            .Where(i => i.Value.Limited)
            .Select(i =>
                new free_record
                {
                    location = i.Key,
                    free = i.Value.Limit
                })
            .ToList();

        //Control.Log($"-- {(free == null ? "null" : free.Count.ToString() + " items")}");


        //Control.Log($"- start inventory");
        foreach (var invItem in state.Inventory)
        {
            var item = usage.FirstOrDefault(i => i.DefId == invItem.Item.ComponentDefID
                                                 && i.Location == invItem.Location && !i.used_now);
            if (item != null)
            {
                //Control.Log($"-- {invItem.Item.ComponentDefID} is in defaults, added");
                item.item = invItem.Item;
            }
            else if (invItem.Item.IsCategory(categoryId, out var cat))
            {
                //Control.Log($"-- {invItem.Item.ComponentDefID} not in defaults, decrease free space");
                foreach (var freeRecord in free.Where(i => i.location.HasFlag(invItem.Location)))
                    freeRecord.free -= cat.Weight;
            }
        }
        //Control.Log($"- end inventory");
        var used_records = new List<(free_record record, int value)>();

        //Control.Log($"- start usage");
        foreach (var usageRecord in usage)
        {
            //Control.Log($"-- {(usageRecord.DefId)} {usageRecord.Location} {usageRecord.crecord.Category.Weight}");

            used_records.Clear();
            var fit = true;
            foreach (var freeRecord in free.Where(i => i.location.HasFlag(usageRecord.Location)))
            {
                //Control.Log($"-- free {freeRecord.location} {freeRecord.free}");

                if (freeRecord.free >= usageRecord.crecord.Category.Weight)
                    used_records.Add((freeRecord, usageRecord.crecord.Category.Weight));
                else
                {
                    fit = false;
                    break;
                }
                //Control.Log($"-- category {(usageRecord.crecord.Category.Weight)}");
            }
            //Control.Log($"-- {usageRecord.DefId} fit: {fit}");

            fit = fit && used_records.Count > 0;

            usageRecord.used_after = fit;
            if (fit)
            {
                //Control.Log($"-- decrease free space");
                foreach (var used in used_records)
                    used.record.free -= used.value;
            }

            if (usageRecord.used_after != usageRecord.used_now)
            {
                //Control.Log($"-- create change");
                if (usageRecord.used_after)
                {
                    //Control.Log($"-- Add {usageRecord.DefId} => {usageRecord.Location}");
                    state.AddChange(new Change_Add(usageRecord.DefId, usageRecord.Type, usageRecord.Location));
                }
                else
                {
                    //Control.Log($"-- Remove {usageRecord.DefId} =X {usageRecord.Location}");
                    state.AddChange(new Change_Remove(usageRecord.DefId, usageRecord.Location));
                }
            }
        }
        //Control.Log($"- done");
    }
}