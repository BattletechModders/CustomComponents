using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using BattleTech;
using Harmony;
using Newtonsoft.Json;
using UnityEngine.Playables;

namespace CustomComponents
{
    public class MultiCategoryDefault : IMultiCategoryDefault
    {

        public ChassisLocations Location { get; set; }
        public string[] Categories { get; set; }
        public string DefID { get; set; }
        public ComponentType ComponentType { get; set; }
        [JsonIgnore]
        public MechComponentDef Component { get; set; }
        [JsonIgnore]
        public Dictionary<string, (Category category, CategoryDescriptorRecord record)> CategoryRecords { get; set; }

    }

    public class CategoryDefaultRecord
    {
        public MechComponentDef Item { get; set; }
        public Category Category { get; set; }

        public ChassisLocations Location { get; set; }
    }


    public class CategoryDefault
    {
        public CategoryDescriptorRecord CategoryRecord { get; set; }
        public CategoryDescriptor CategoryDescriptor { get; set; }
        public List<CategoryDefaultRecord> Defaults { get; set; } = new List<CategoryDefaultRecord>();
    }


    public class DefaultsDatabase
    {
        public static ChassisLocations[] SingleLocations = new[]
        {
            ChassisLocations.Head,
            ChassisLocations.CenterTorso,

            ChassisLocations.LeftTorso,
            ChassisLocations.LeftArm,
            ChassisLocations.LeftLeg,

            ChassisLocations.RightTorso,
            ChassisLocations.RightArm,
            ChassisLocations.RightLeg,
        };

        private class temp_idefault : IDefault
        {
            public string CategoryID { get; set; }
            public DefaultsInfoRecord[] Defaults { get; set; }
        }

        public class MechDefaultInfo
        {
            public MultiRecord Multi { get; set; }
            public Dictionary<string, CategoryDefault> Defaults { get; set; }

            private Dictionary<string, HashSet<string>> all_id;
            private Dictionary<string, HashSet<string>> single_id;

            public void Complete()
            {
                single_id = new Dictionary<string, HashSet<string>>();
                all_id = new Dictionary<string, HashSet<string>>();
                
                if (Multi != null && Multi.HasRecords)
                    foreach (var d in Multi.Defaults)
                    {
                        foreach (var cat in d.Categories)
                        {
                            if (!all_id.TryGetValue(cat, out var h))
                            {
                                h = new HashSet<string>();
                                all_id[cat] = h;
                            }

                            h.Add(d.DefID);
                        }
                    }

                if (Defaults != null && Defaults.Count > 0)
                    foreach (var def in Defaults.Where(def => def.Value?.Defaults != null && def.Value.Defaults.Count > 0))
                    {
                        if (!all_id.TryGetValue(def.Key, out var ha))
                        {
                            ha = new HashSet<string>();
                            all_id[def.Key] = ha;
                        }

                        var hs = new HashSet<string>();
                        single_id[def.Key] = hs;
                        foreach (var cdr in def.Value.Defaults)
                        {
                            ha.Add(cdr.Item.Description.Id);
                            hs.Add(cdr.Item.Description.Id);
                        }
                    }
            }

            public bool IsCatDefault(string id)
            {
                return all_id.Any(i => i.Value.Contains(id));
            }

            public bool IsCatDefault(string id, string catid)
            {
                return all_id.TryGetValue(catid, out var hash) && hash.Contains(id);
            }

            public bool IsSingleCatDefault(string id, string catid)
            {
                return single_id.TryGetValue(catid, out var hash) && hash.Contains(id);
            }

            public override string ToString()
            {
                string result = "MechDefaultInfo:";
                if (Multi != null && Multi.HasRecords)
                {
                    result += "\n- MultiRecords";
                    foreach (var mcd in Multi.Defaults)
                        result += $"\n-- {mcd.DefID} => {mcd.Location}";
                    result += "\n--- Categories: " + Multi.UsedCategories.Keys.Join();
                }

                if (Defaults != null)
                {
                    result += "\n- Defaults";
                    foreach (var cd in Defaults)
                    {
                        result += $"\n-- {cd.Key}";
                        foreach (var cdr in cd.Value.Defaults)
                        {
                            result += $"\n--- {cdr.Item.Description.Id} => {cdr.Location}";
                        }
                    }
                }
                return result;
            }
        }

        public class MultiRecord
        {
            public List<MultiCategoryDefault> Defaults { get; set; }
            public Dictionary<string, CategoryDescriptorRecord> UsedCategories { get; set; }

            public bool HasRecords => Defaults != null && Defaults.Count > 0;
        }

        private static DefaultsDatabase _instance;
        private Dictionary<string, MechDefaultInfo> database = new Dictionary<string, MechDefaultInfo>();
        private Dictionary<string, MechComponentDef> def_cache = new Dictionary<string, MechComponentDef>();
        private Dictionary<string, DefaultsInfo> defaults_by_category = new Dictionary<string, DefaultsInfo>();

        public static DefaultsDatabase Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new DefaultsDatabase();
                return _instance;
            }
        }

        public MechDefaultInfo this[MechDef mech]
        {
            get
            {
                if (mech == null)
                    return null;

                if (!database.TryGetValue(mech.ChassisID, out var result))
                {
                    result = CreateDefaultRecord(mech);
                    database[mech.ChassisID] = result;
                }

                return result;
            }
        }

        internal void Setup(Dictionary<string, Dictionary<string, VersionManifestEntry>> customResources)
        {
            foreach (var entry in SettingsResourcesTools.Enumerate<DefaultsInfo>("CCDefaults", customResources))
            {
                if(string.IsNullOrEmpty(entry.CategoryID))
                    continue;

                defaults_by_category[entry.CategoryID] = entry;

                Control.Log($"Defaults for {entry.CategoryID} registered");

                entry.Complete();
                if(Control.Settings.DEBUG_ShowLoadedDefaults)
                    Control.Log(entry.ToString());
            }
        }

        private IEnumerable<IDefault> GetMechDefaults(MechDef mech)
        {
            return mech.Chassis.GetComponents<IDefault>();
        }

        public IEnumerable<IMultiCategoryDefault> GetMechMultiDefauls(MechDef mech)
        {
            return mech.Chassis.GetComponents<IMultiCategoryDefault>();
        }

        private MechDefaultInfo CreateDefaultRecord(MechDef mech)
        {
            Control.LogDebug(DType.DefaultsBuild, "CreateDefaultRecord for {0}", mech?.ChassisID);

            void process_defaults(MechDefaultInfo result, IEnumerable<IDefault> defaults)
            {
                if(defaults == null)
                    return;

                var seen = new HashSet<string>(result.Defaults.Keys);

                foreach (var item in defaults.Where(i => !seen.Contains(i.CategoryID)))
                {
                    //    Control.Log($"item: {item}" );
                    //    Control.Log($"item: {item.CategoryID}, {item.Defaults?.Length}");


                    seen.Add(item.CategoryID);
                    if (item.Defaults == null || item.Defaults.Length == 0)
                    {
                        result.Defaults[item.CategoryID] = null;
                        continue;
                    }

                    var category = new CategoryDefault();
                    category.CategoryDescriptor = CategoryController.Shared.GetCategory(item.CategoryID);
                    if (category.CategoryDescriptor == null)
                    {
                        Control.LogError($"Defaults for {mech.ChassisID} have unknown category {item.CategoryID}, skipped");
                        continue;
                    }


                    category.CategoryRecord = category.CategoryDescriptor[mech];


                    if (!category.CategoryRecord.MinLimited)
                    {
                        Control.LogError($"{mech.ChassisID} have default of category {item.CategoryID} which not have minimum limit, skipped");
                        continue;
                    }


                    foreach (var default_record in item.Defaults)
                    {
                        if (!SingleLocations.Contains(default_record.Location))
                        {
                            Control.LogError(
                                $"{mech.ChassisID} have default in group location for {default_record.DefID}, skipped");
                            continue;
                        }

                        var def = GetComponentDef(default_record.DefID, default_record.Type);
                        if (def == null)
                        {
                            Control.LogError($"{mech.ChassisID} have unexisting default {default_record.DefID}[{default_record.Type}]");
                            continue;
                        }
                        if (!def.IsCategory(item.CategoryID, out var c))
                        {
                            Control.LogError($"{mech.ChassisID} default {default_record.DefID} dont have category {item.CategoryID}");
                            continue;
                        }
                        if (!def.Flags<CCFlags>().Default)
                        {
                            Control.LogError($"{mech.ChassisID} default {default_record.DefID} dont have `default` flag");
                            continue;
                        }

                        var cr = new CategoryDefaultRecord()
                        {
                            Category = c,
                            Item = def,
                            Location = default_record.Location
                        };

                        category.Defaults.Add(cr);
                    }

                    if (category.Defaults.Count > 0)
                        result.Defaults[item.CategoryID] = category;
                    else
                        result.Defaults[item.CategoryID] = null;
                }
            }

            var result = new MechDefaultInfo();

            var multi = new MultiRecord() { Defaults = new List<MultiCategoryDefault>() };

            var mech_multi = GetMechMultiDefauls(mech);

            if (mech_multi != null)
            {
                Control.LogDebug(DType.DefaultsBuild, "- MultiRecords");

                foreach (var m in mech_multi)
                {
                    Control.LogDebug(DType.DefaultsBuild, "-- {0} => {1}", m.DefID, m.Location);

                    var info = new MultiCategoryDefault
                    {
                        Categories = m.Categories,
                        DefID = m.DefID,
                        ComponentType = m.ComponentType,
                        Location = m.Location,
                        CategoryRecords =
                            new Dictionary<string, (Category category, CategoryDescriptorRecord record)>(),
                        Component = DefaultHelper.GetComponentDef(m.DefID, m.ComponentType)

                    };


                    if (info.Categories == null || info.Categories.Length == 0)
                    {
                        Control.LogError(
                            $"MultiDefault _record for {mech.Description.Id} have empty category list for {m.DefID}");
                        continue;
                    }

                    if (info.Component == null)
                    {
                        Control.LogError(
                            $"MultiDefault _record for {mech.Description.Id} have unknown component {m.DefID}");
                        continue;
                    }

                    foreach (var category in info.Categories)
                    {
                        if (info.Component.IsCategory(category, out var c))
                        {
                            var cr = c.CategoryDescriptor[mech];
                            if (cr == null || !cr.MinLimited)
                            {
                                Control.LogError(
                                    $"MultiDefault _record for {mech.Description.Id}, component {m.DefID}, category {category} dont have minimum limit, so defaults will be ignored");
                                continue;
                            }

                            info.CategoryRecords[category] = (c, cr);

                        }
                        else
                        {
                            Control.LogError(
                                $"MultiDefault _record for {mech.Description.Id} have unknown category [{category}] for {m.DefID}");
                            continue;
                        }
                    }

                    if (info.CategoryRecords.Count == 0)
                    {
                        Control.LogError(
                            $"MultiDefault _record for {mech.Description.Id} have no applicable categories for {m.DefID}");
                        continue;
                    }

                    multi.Defaults.Add(info);
                }
            }

            if (multi.HasRecords)
            {
                Control.LogDebug(DType.DefaultsBuild, "- Build UsedCategories");
                multi.UsedCategories = multi.Defaults
                    .SelectMany(i => i.CategoryRecords)
                    .GroupBy(i => i.Key)
                    .ToDictionary(a => a.Key, a => a.First().Value.record);

                result.Multi = multi;
                Control.LogDebug(DType.DefaultsBuild, "- done");
            }
            else
            {
                Control.LogDebug(DType.DefaultsBuild, "- No MultiRecords");
            }

            result.Defaults = new Dictionary<string, CategoryDefault>();
            var defaults = GetMechDefaults(mech);

            //if(defaults != null &&)
            //    foreach (var a in defaults)
            //        Control.Log(a.ToString());

            process_defaults(result, defaults);

            var mechut = UnitTypeDatabase.Instance.GetUnitTypes(mech);
            process_defaults(result, defaults_by_category
                .Select(i => new temp_idefault()
                    {
                        Defaults = i.Value.GetDefault(mechut),
                        CategoryID = i.Key
                    }));

            result.Complete();
            if(Control.Settings.DEBUG_ShowLoadedDefaults)
                Control.LogDebug(DType.DefaultsBuild, "result: {0}", result);

            return result;
        }

        private MechComponentDef GetComponentDef(string defId, ComponentType type)
        {
            if (def_cache.TryGetValue(defId, out var def))
            {
                if (def.ComponentType == type)
                    return def;

                return null;
            }

            def = DefaultHelper.GetComponentDef(defId, type);
            if (def != null)
                def_cache[def.Description.Id] = def;

            return def;
        }
    }
}