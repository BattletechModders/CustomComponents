using System.Collections.Generic;
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

        public Dictionary<Localization, CategoryDefaultRecord[]> DefaultsPerLocation { get; set; }
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

        public IEnumerable<IDefault> GetMechDefaults(MechDef mech)
        {
            return mech.Chassis.GetComponents<IDefault>();
        }

        public IEnumerable<IMultiCategoryDefault> GetMechMultiDefauls(MechDef mech)
        {
            return mech.Chassis.GetComponents<IMultiCategoryDefault>();
        }

        private MechDefaultInfo CreateDefaultRecord(MechDef mech)
        {
            var result = new MechDefaultInfo();

            result.Multi = new List<MultiCategoryDefault>();
            var mech_multi = GetMechMultiDefauls(mech);
            if(mech_multi != null)
                foreach(var m in mech_multi)
                {
                    var info = new MultiCategoryDefault
                    {
                        AnyLocation = m.AnyLocation,
                        Categories = m.Categories,
                        DefID = m.DefID,
                        ComponentType = m.ComponentType,
                        Location = m.Location,
                        CategoryRecords = new  Dictionary<string, KeyValuePair<Category, CategoryDescriptorRecord>>,
                        Component = DefaultHelper.GetComponentDef(m.DefID, m.ComponentType)
                        
                    };
                    

                    if(info.Categories == null || info.Categories.Length == 0)
                    {
                        Control.LogError($"MultiDefault record for {mech.Description.Id} have empty category list for {m.DefID}");
                        continue;
                    }

                    if(info.Component == null)
                    {
                        Control.LogError($"MultiDefault record for {mech.Description.Id} have unknown component {m.DefID}");
                        continue;
                    }

                    foreach (var category in info.Categories)
                    {
                        if(info.Component.IsCategory(category, out var c))
                        {
                            var cr = c.CategoryDescriptor[mech];
                            if(cr == null || cr.MinEquiped == 0)
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

                    if(info.CategoryRecords.Count == 0)
                    {
                        Control.LogError($"MultiDefault record for {mech.Description.Id} have no applicable categories for {m.DefID}");
                        continue;
                    }

                    result.Multi.Add(info);
                }



            return result;
        }
    }
}