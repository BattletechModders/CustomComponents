using System.Collections.Generic;
using System.Linq;
using BattleTech;
using Newtonsoft.Json;

namespace CustomComponents
{
    public class MultiCategoryDefault : IMultiCategoryDefault
    {
        public ChassisLocations Location { get; set; }
        public string[] Categories { get; set; }
        public string DefID { get; set; }
        public ComponentType ComponentType { get; set; }

        public bool AnyLocation { get; set; }

        [JsonIgnore]
        public MechComponentDef Component { get; set; }
        [JsonIgnore]
        public Dictionary<string, KeyValuePair<Category, CategoryDescriptorRecord>> CategoryRecords { get; set; }

    }

    public class CategoryDefaultRecord
    {
        public MechComponentDef Item { get; set; }
        public Category Category { get; set; }
    }


    public class CategoryDefault
    {
        public CategoryDescriptorRecord CategoryRecord { get; set; }
        public CategoryDescriptor CategoryDescriptor { get; set; }

        public Dictionary<ChassisLocations, List<CategoryDefaultRecord>> DefaultsPerLocation { get; set; }
    }

    public class DefaultRecord
    {
        public ChassisLocations Location { get; set; }
        public string CategoryID { get; set; }
        public string DefID { get; set; }
        public ComponentType Type { get; set; }
        public bool AnyLocation { get; set; } = true;

        [JsonIgnore] public bool Ready { get; private set; } = false;
        public bool Invalid => Ready && _def == null;

        private MechComponentDef _def;
        private Category _cat;

        [JsonIgnore]
        public MechComponentDef Def
        {
            get
            {
                if (!Ready)
                    Init();
                return _def;
            }
        }

        [JsonIgnore]
        public Category Category
        {
            get
            {
                if (!Ready)
                    Init();
                return _cat;
            }
        }

        private void Init()
        {
            Ready = true;
            _def = DefaultHelper.GetComponentDef(DefID, Type);
            if (_def == null)
                return;
            if (!_def.IsCategory(CategoryID, out _cat))
            {
                _def = null;
                return;
            }
        }

    }


    public class MechDefaultInfo
    {
        public List<MultiCategoryDefault> Multi;
        public Dictionary<string, CategoryDefault> Defaults;
    }

    public class DefaultsDatabase
    {
        private static DefaultsDatabase _instance;
        private Dictionary<string, MechDefaultInfo> database = new Dictionary<string, MechDefaultInfo>();
        private Dictionary<string, MechComponentDef> def_cache = new Dictionary<string, MechComponentDef>();
        private List<DefaultsInfo> load_defaults = new List<DefaultsInfo>();
        private DefaultsInfoRecord[] default_defaults;

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
                load_defaults.Add(entry);
            }
            var item = load_defaults.FirstOrDefault(i => i.UnitType == "*");
            {
                load_defaults.Remove(item);
                default_defaults = item.Records;
            }
            load_defaults.Sort((i, j) => -i.Priority.CompareTo(j.Priority));
        }
        private IEnumerable<IDefault> GetMechDefaults(MechDef mech)
        {
            return mech.Chassis.GetComponents<IDefault>();
        }

        public IEnumerable<IMultiCategoryDefault> GetMechMultiDefauls(MechDef mech)
        {
            return mech.Chassis.GetComponents<IMultiCategoryDefault>();
        }

        public IEnumerable<ICategoryDescriptorRecord> GetMechCategories(MechDef mech)
        {
            return mech.Chassis.GetComponents<ICategoryDescriptorRecord>();
        }


        private MechDefaultInfo CreateDefaultRecord(MechDef mech)
        {
            void process_defaults(MechDefaultInfo result, IEnumerable<IDefault> defaults)
            {
                var seen = new HashSet<string>(result.Defaults.Keys);

                foreach (var item in defaults.Where(i => !seen.Contains(i.CategoryID)))
                {
                    if (!result.Defaults.TryGetValue(item.CategoryID, out var catdefault))
                    {
                        catdefault = new CategoryDefault();
                        catdefault.CategoryDescriptor = CategoryController.Shared.GetCategory(item.CategoryID);
                        if (catdefault.CategoryDescriptor == null)
                            return;
                        catdefault.CategoryRecord = catdefault.CategoryDescriptor[mech];
                        if (catdefault.CategoryRecord.MinEquiped <= 0)
                        {
                            Control.LogError($"{mech.ChassisID} have default of category {item.CategoryID} which minequiped == 0, skipped");
                            continue;
                        }
                        result.Defaults[item.CategoryID] = catdefault;
                    }
                    var def = DefaultHelper.GetComponentDef(item.DefID, item.Type);
                    if (def == null)
                    {
                        Control.LogError($"{mech.ChassisID} have unexisting default {item.DefID}[{item.Type}]");
                        continue;
                    }
                    if (!def.IsCategory(item.CategoryID, out var c))
                    {
                        Control.LogError($"{mech.ChassisID} default {item.DefID} dont have category {item.CategoryID}");
                        continue;
                    }
                    var cr = new CategoryDefaultRecord()
                    {
                        Category = c,
                        Item = def
                    };
                    if (!catdefault.DefaultsPerLocation.TryGetValue(item.Location, out var list))
                    {
                        list = new List<CategoryDefaultRecord>();
                        catdefault.DefaultsPerLocation[item.Location] = list;
                    }
                    list.Add(cr);
                }
            }

            var result = new MechDefaultInfo();

            result.Multi = new List<MultiCategoryDefault>();
            var mech_multi = GetMechMultiDefauls(mech);
            if (mech_multi != null)
                foreach (var m in mech_multi)
                {
                    var info = new MultiCategoryDefault
                    {
                        Categories = m.Categories,
                        DefID = m.DefID,
                        ComponentType = m.ComponentType,
                        Location = m.Location,
                        CategoryRecords = new Dictionary<string, KeyValuePair<Category, CategoryDescriptorRecord>>(),
                        Component = DefaultHelper.GetComponentDef(m.DefID, m.ComponentType)

                    };


                    if (info.Categories == null || info.Categories.Length == 0)
                    {
                        Control.LogError($"MultiDefault record for {mech.Description.Id} have empty category list for {m.DefID}");
                        continue;
                    }

                    if (info.Component == null)
                    {
                        Control.LogError($"MultiDefault record for {mech.Description.Id} have unknown component {m.DefID}");
                        continue;
                    }

                    foreach (var category in info.Categories)
                    {
                        if (info.Component.IsCategory(category, out var c))
                        {
                            var cr = c.CategoryDescriptor[mech];
                            if (cr == null || cr.MinEquiped == 0)
                            {
                                Control.LogError($"MultiDefault record for {mech.Description.Id}, component {m.DefID}, category {category} have MinEquiped = 0, so defaults will be ignored");
                                continue;
                            }

                            info.CategoryRecords[category] = new KeyValuePair<Category, CategoryDescriptorRecord>(c, cr);

                        }
                        else
                        {
                            Control.LogError($"MultiDefault record for {mech.Description.Id} have unknown category [{category}] for {m.DefID}");
                            continue;
                        }
                    }

                    if (info.CategoryRecords.Count == 0)
                    {
                        Control.LogError($"MultiDefault record for {mech.Description.Id} have no applicable categories for {m.DefID}");
                        continue;
                    }

                    result.Multi.Add(info);
                }

            result.Defaults = new Dictionary<string, CategoryDefault>();
            var defaults = GetMechDefaults(mech);
            process_defaults(result, defaults);
            var mechut = UnitTypeDatabase.Instance.GetUnitTypes(mech);
            if (mechut != null)
                foreach (var item in load_defaults.Where(i => mechut.Contains(i.UnitType)))
                    process_defaults(result, item.Records);
            process_defaults(result, default_defaults);

            return result;
        }
    }
}