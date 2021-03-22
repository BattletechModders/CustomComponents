using System.Collections.Generic;
using BattleTech;
using Newtonsoft.Json;

namespace CustomComponents
{
    public class MultiCategoryDefault
    {
        public string[] Categories { get; set; }
        public string DefID { get; set; }
        public ComponentType ComponentType { get; set; }

        [JsonIgnore]
        public MechComponentDef Component;

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

        private MechDefaultInfo CreateDefaultRecord(MechDef mech)
        {
            return null;
        }
    }
}