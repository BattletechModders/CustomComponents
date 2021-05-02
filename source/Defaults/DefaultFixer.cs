//undef CCDEBUG
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
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
                    _instance = new DefaultFixer();
                return _instance;
            }
        }

        public List<inv_change> GetMultiChange(MechDef mech, IEnumerable<InvItem> inventory)
        {
            var defaults = DefaultsDatabase.Instance[mech];

            if (defaults?.Multi == null || !defaults.Multi.HasRecords)
                return null;

            var result = new List<inv_change>();

            var usage = defaults.Multi.Defaults
                .Select(i => new usage_record(i));

            var free = defaults.Multi.UsedCategories.ToDictionary(
                i => i.Key,
                i => i.Value.LocationLimits.Where(a => a.Value.Min > 0)
                    .Select(a => new free_record()
                    {
                        free = a.Value.Min == 0 ? 9999 : a.Value.Min,
                        location = a.Key,
                        limit = a.Value
                    }).ToArray());


            foreach (var invItem in inventory)
            {
                var item = usage.FirstOrDefault(i => !i.used_now && i.DefId == invItem.item.ComponentDefID
                                                     && i.Location == invItem.location);
                if (item != null)
                    item.item = invItem.item;
                else if (item.item.IsModuleFixed(mech) || !item.item.Flags<CCFlags>().CategoryDefault)
                {
                    foreach (var catid in defaults.Multi.UsedCategories.Keys)
                        if (invItem.item.IsCategory(catid, out var cat))
                            foreach (var freeRecord in free[catid].Where(i => i.location.HasFlag(item.Location)))
                                freeRecord.free -= cat.Weight;
                }
            }

            var used_records = new List<(free_record record, int value)>();

            foreach (var usageRecord in usage)
            {
                var fit = true;

                used_records.Clear();

                foreach (var pair in usageRecord.mrecord.CategoryRecords)
                {
                    var freerecords = free[pair.Key];
                    var num = pair.Value.category.Weight;

                    foreach (var record in freerecords)
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

                //if (usageRecord.used_after != usageRecord.used_now)
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
        public List<inv_change> GetDefaultsChange(MechDef mech, IEnumerable<InvItem> inventory, string category)
        {
            //Control.Log($"GetDefaultsChange {category} {mech.ChassisID}");

            var record = DefaultsDatabase.Instance[mech];


            if (!record.Defaults.TryGetValue(category, out var defaults) || defaults?.Defaults == null)
            {
                //Control.Log($"- empty defaults list, clearing");
                return inventory
                    .Where(i => i.item.IsCategory(category) && i.item.Flags<CCFlags>().CategoryDefault && !i.item.IsModuleFixed(mech))
                    .Select(i => inv_change.Remove(i.item.ComponentDefID, i.location))
                    .ToList();
            }

            //Control.Log($"- usage create");

            var usage = defaults.Defaults
                .Select(i => new usage_record(i))
                .ToList();
            //Control.Log($"-- {(usage == null ? "null" : usage.Count.ToString() + " items")}");

            //Control.Log($"- free create");
            var free = defaults.CategoryRecord.LocationLimits
                .Where(i => i.Value.Min > 0)
                .Select(i =>
                    new free_record
                    {
                        location = i.Key,
                        free = i.Value.Min
                    })
                .ToList();

            //Control.Log($"-- {(free == null ? "null" : free.Count.ToString() + " items")}");


            //Control.Log($"- start inventory");
            foreach (var invItem in inventory)
            {
                var item = usage.FirstOrDefault(i => i.DefId == invItem.item.ComponentDefID
                                                     && i.Location == invItem.location && !i.used_now);
                if (item != null)
                {
                    //Control.Log($"-- {invItem.item.ComponentDefID} is in defaults, added");
                    item.item = invItem.item;
                }
                else if (invItem.item.IsCategory(category, out var cat))
                {
                    //Control.Log($"-- {invItem.item.ComponentDefID} not in defaults, decrease free space");
                    foreach (var freeRecord in free.Where(i => i.location.HasFlag(invItem.location)))
                        freeRecord.free -= cat.Weight;
                }
            }
            //Control.Log($"- end inventory");
            var used_records = new List<(free_record record, int value)>();
            var result = new List<inv_change>();

            //Control.Log($"- start usage");
            foreach (var usageRecord in usage)
            {
                //Control.Log($"-- {(usageRecord.DefId)} {usageRecord.Location} {usageRecord.crecord.Category.Weight}");

                used_records.Clear();
                bool fit = true;
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
                        result.Add(inv_change.Add(usageRecord.DefId, usageRecord.Type, usageRecord.Location));
                    else
                        result.Add(inv_change.Remove(usageRecord.DefId, usageRecord.Location));
                }
            }
            //Control.Log($"- done");


            return result;
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
                    Control.LogError($"Error in Default autofixer for {mechDef.Description.Id}", e);
                }
            }
        }

        private void FixMech(SimGameState simgame, MechDef mechDef)
        {
            var defaults = DefaultsDatabase.Instance[mechDef];
            if (defaults == null)
                return;

            var inv = mechDef.Inventory.ToInvItems().ToList();
            bool changed = false;
            var changes = GetMultiChange(mechDef, inv);
            if (changes != null && changes.Count > 0)
            {
                changed = true;
                apply_changes(changes, inv, simgame);
            }

            foreach (var cat_id in defaults.Defaults.Keys)
            {
                changes = GetDefaultsChange(mechDef, inv, cat_id);
                if (changes != null && changes.Count > 0)
                {
                    changed = true;
                    apply_changes(changes, inv, simgame);
                }
            }

            if (changed)
                mechDef.SetInventory(inv.Select(i => i.item).ToArray());
        }

        private void apply_changes(List<inv_change> changes, List<InvItem> inv, SimGameState simgame)
        {
            var dm = UnityGameInstance.BattleTechGame.DataManager;
            foreach (var invChange in changes)
            {
                if (invChange.IsAdd)
                {
                    var r = DefaultHelper.CreateRef(invChange.Id, invChange.Type, dm, simgame);
                    r.SetData(invChange.Location, 0, ComponentDamageLevel.Functional, true);
                    inv.Add(new RefInvItem(r, r.MountedLocation));
                }
                else if (invChange.IsRemove)
                {
                    var to_remove = inv.FirstOrDefault(i =>
                        i.location == invChange.Location && i.item.ComponentDefID == invChange.Id);
                    if (to_remove != null)
                        inv.Remove(to_remove);
                }
            }
        }

        public void OnInstalled(WorkOrderEntry_InstallComponent order, SimGameState state, MechDef mech)
        {
            FixMech(state, mech);
        }
    }
}