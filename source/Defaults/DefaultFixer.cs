//undef CCDEBUG
using System;
using System.Collections.Generic;
using System.Linq;
using BattleTech;
using BattleTech.UI;
using HBS.Extensions;

namespace CustomComponents
{

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
                this.Location = location;
                this.Id = id;
                this.Type = type;
            }

            public static inv_change Add(string id, ComponentType type, ChassisLocations location)
            {
                return new inv_change(true, id, location, type);
            }

            public static inv_change Remove(string id, ChassisLocations location)
            {
                return new inv_change(false, id, location);
            }
        }

        private class free_record
        {
            public CategoryLimit limit { get; set; }
            public int free { get; set; }

            public ChassisLocations location { get; set; }

            public bool HaveMin => limit.Min > 0;
        }

        private class usage_record
        {
            public MultiCategoryDefault mrecord;
            public CategoryDefaultRecord crecord;
            public bool used_now => item != null;
            public bool used_after = false;
            public MechComponentRef item;
        }

        private static DefaultFixer _instance;

        public static DefaultFixer Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new DefaultFixer();
                return _instance;
            }
        }

        public List<inv_change> GetMultiChange(MechDef mech, IEnumerable<InvItem> inventory)
        {
            var defaults = DefaultsDatabase.Instance[mech];

            if (defaults?.Multi != null && defaults.Multi.HasRecords)
                return null;

            var result = new List<inv_change>();

            var usage = defaults.Multi.Defaults
                .Select(i => new usage_record()
                {
                    mrecord = i,
                    item = inventory.FirstOrDefault(a => a.location == i.Location && a.item.ComponentDefID == i.DefID)?.item
                });

            var free = defaults.Multi.UsedCategories.Select(i => new
            {
                category = i.Key,
                value = i.Value.LocationLimits.Where(a => a.Value.Min > 0).Select(a => new free_record()
                {
                    free = a.Value.Min == 0 ? 9999 : a.Value.Min,
                    location = a.Key,
                    limit = a.Value
                }).ToArray()
            }).ToList();

            var items_by_category = inventory
                .Where(i => usage.All(a => a.item != i.item)
                            || !i.item.HasFlag(CCF.Default)
                            || i.item.IsModuleFixed(mech)
                            || i.item.HasFlag(CCF.NoRemove))
                .Select(item => new { item.item, def = item.item.GetComponents<Category>(), location = item.location })
                .Where(@t => @t.def != null)
                .SelectMany(@t => t.def.Where(c => defaults.Multi.UsedCategories.ContainsKey(c.CategoryID)).Select(item => new
                {
                    category = item.CategoryDescriptor,
                    itemdef = @t.item.Def,
                    itemref = @t.item,
                    location = t.location,
                    num = item.Weight
                }))
                .GroupBy(i => i.category)
                .ToDictionary(i => i.Key.Name, i => i.ToList());

            foreach (var free_record in free)
                if (items_by_category.TryGetValue(free_record.category, out var list))
                    foreach (var item in list)
                        foreach (var freeRecord in free_record.value)
                            if (freeRecord.location.HasFlag(item.location))
                                freeRecord.free -= item.num;

            var used_records = new List<(free_record record, int value)>();

            foreach (var usageRecord in usage)
            {
                var fit = true;

                used_records.Clear();

                foreach (var pair in usageRecord.mrecord.CategoryRecords)
                {
                    var freerecord = free.FirstOrDefault(i => i.category == pair.Key);
                    if (freerecord == null)
                    {
                        fit = false;
                        break;
                    }

                    var num = pair.Value.category.Weight;

                    foreach (var record in freerecord.value)
                    {
                        if (record.free > num)
                        {
                            fit = false;
                            break;
                        }
                        used_records.Add((record, num));
                    }

                    if (!fit)
                        break;


                }

                usageRecord.used_after = fit;
                if (fit)
                    foreach (var value in used_records)
                    {
                        value.record.free -= value.value;
                    }

                if (usageRecord.used_after != usageRecord.used_now)
                {
                    var d = usageRecord.mrecord;
                    if (usageRecord.used_after)
                        result.Add(inv_change.Add(d.DefID, d.ComponentType, d.Location));
                    else
                        result.Add(inv_change.Remove(d.DefID, d.Location));
                }
            }


            return result;
        }

        internal void FixMechs(List<MechDef> mechDefs, SimGameState simgame)
        {
            throw new NotImplementedException();
        }

        public List<inv_change> GetDefaultsChange(MechDef mech, IEnumerable<InvItem> inventory, string category)
        {
            var record = DefaultsDatabase.Instance[mech];
            if (!record.Defaults.TryGetValue(category, out var defaults))
                return inventory
                    .Where(i => i.item.IsCategory(category) && i.item.IsDefault() && !i.item.HasFlag(CCF.NoRemove))
                    .Select(i => inv_change.Remove(i.item.ComponentDefID, i.location))
                    .ToList();

            var usage = defaults.Defaults
                .Select(i => new usage_record()
                {
                    crecord = i,
                    item = inventory.FirstOrDefault(a => a.location == i.Location && a.item.ComponentDefID == i.Item.Description.Id)?.item
                });


            var items_of_category = inventory
                .Where(i => i.item.IsCategory(category))
                .Select(i => new { i.item, cat = i.item.GetCategory(category), location = i.location })
                .Select(i => new
                {
                    item = i.item,
                    def = i.item.IsDefault(),
                    num = i.cat.Weight,
                    fix = i.item.IsModuleFixed(mech),
                    remove = i.item.HasFlag(CCF.NoRemove)
                }).ToList();

            var used_records = new List<(free_record record, int value)>();
            var free = defaults.CategoryRecord.LocationLimits
                .Where(i => i.Value.Min > 0)
                .Select(i =>
                    new free_record
                    {
                        location = i.Key,
                        free = i.Value.Min
                    })
                .ToList();



            return null;
        }
    }
}