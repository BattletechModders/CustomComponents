using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using BattleTech;
using Newtonsoft.Json;

namespace CustomComponents
{
    public interface ICategoryDescriptorRecord
    {
        int MaxEquiped { get;  }
        int MaxEquipedPerLocation { get;  }
        int MinEquiped { get;  }
    }

    public class CategoryDescriptorRecord : ICategoryDescriptorRecord
    {
        public string UnitType;

        public int MaxEquiped { get; set; } = -1;
        public int MaxEquipedPerLocation { get; set; } = -1;
        public int MinEquiped { get; set; } = 0;

        [JsonIgnore]
        public bool Unique
        {
            get
            {
                return MaxEquiped == 1;
            }
        }
        [JsonIgnore]

        public bool UniqueForLocation
        {
            get
            {
                return MaxEquipedPerLocation == 1;
            }
        }

        [JsonIgnore]
        public bool Required => MinEquiped > 0;

        [JsonIgnore]
        public bool NotAllowed => MaxEquiped <= 0;


        public CategoryDescriptorRecord()
        {

        }

        public CategoryDescriptorRecord(ICategoryDescriptorRecord source)
        {
            this.MinEquiped = source.MinEquiped;
            this.MaxEquiped = source.MaxEquiped;
            this.MaxEquipedPerLocation = source.MaxEquipedPerLocation;
            
        }
    }

    public class CategoryDescriptor
    {
        public string displayName = "";
        public string Name { get; set; }
        [JsonIgnore]
        public string DisplayName
        {
            get => string.IsNullOrEmpty(displayName) ? Name : displayName;
            set => displayName = value;
        }
        public bool AutoReplace = false;
        public bool ReplaceAnyLocation = false;
        public bool AddCategoryToDescription = true;
        public bool AllowMixTags = true;
        public Dictionary<string, object> DefaultCustoms = null;

        public string AddAlreadyEquiped;
        public string AddAlreadyEquipedLocation;
        public string AddMaximumReached; 

        public string AddMaximumLocationReached;
        public string AddMixed;
        public string ValidateRequred;

        public string ValidateMinimum;
        public string ValidateMixed;
        public string ValidateUnique;

        public string ValidateMaximum;
        public string ValidateUniqueLocation;
        public string ValidateMaximumLocation;

        public List<CategoryDescriptorRecord> UnitTypes = new List<CategoryDescriptorRecord>();

        [JsonIgnore] private Dictionary<string, CategoryDescriptorRecord> records;
        [JsonIgnore] private CategoryDescriptorRecord default_record;

        public CategoryDescriptorRecord this[MechDef mech]
        {
            get
            {
                if (mech == null)
                    return default_record;

                if (records.TryGetValue(mech.ChassisID, out var result))
                    return result;

                var iface = GetMechCategoryCustom(mech);
                if(iface != null)
                    result = new CategoryDescriptorRecord(iface);
                else
                {
                    var ut = UnitTypeDatabase.Instance.GetUnitTypes(mech);
                    result = UnitTypes.FirstOrDefault(i => ut.Contains(i.UnitType));
                    if (result == null)
                        result = default_record;
                }

                records[mech.ChassisID] = result;
                return result;
            }
        }

        private ICategoryDescriptorRecord GetMechCategoryCustom(MechDef mech)
        {
            return mech.Chassis.GetComponents<ChassisCategory>().FirstOrDefault(i => i.Category == Name);
        }

        public void Apply(CategoryDescriptor source)
        {
            DisplayName = source.DisplayName;
            AllowMixTags = source.AllowMixTags;
            AutoReplace = source.AutoReplace;
            DefaultCustoms = source.DefaultCustoms;
            ReplaceAnyLocation = source.ReplaceAnyLocation;
            AddCategoryToDescription = source.AddCategoryToDescription;

            if (!string.IsNullOrEmpty(source.AddAlreadyEquiped))
                AddAlreadyEquiped = source.AddAlreadyEquiped;
            if (!string.IsNullOrEmpty(source.AddAlreadyEquipedLocation))
                AddAlreadyEquipedLocation = source.AddAlreadyEquipedLocation;
            if (!string.IsNullOrEmpty(source.AddMaximumReached))
                AddMaximumReached = source.AddMaximumReached;

            if (!string.IsNullOrEmpty(source.AddMaximumLocationReached))
                AddMaximumLocationReached = source.AddMaximumLocationReached;
            if (!string.IsNullOrEmpty(source.AddMixed))
                AddMixed = source.AddMixed;
            if (!string.IsNullOrEmpty(source.ValidateRequred))
                ValidateRequred = source.ValidateRequred;

            if (!string.IsNullOrEmpty(source.ValidateMinimum))
                ValidateMinimum = source.ValidateMinimum;
            if (!string.IsNullOrEmpty(source.ValidateMixed))
                ValidateMixed = source.ValidateMixed;
            if (!string.IsNullOrEmpty(source.ValidateUnique))
                ValidateUnique = source.ValidateUnique;

            if (!string.IsNullOrEmpty(source.ValidateMaximum))
                ValidateMaximum = source.ValidateMaximum;
            if (!string.IsNullOrEmpty(source.ValidateUniqueLocation))
                ValidateUniqueLocation = source.ValidateUniqueLocation;
            if (!string.IsNullOrEmpty(source.ValidateMaximumLocation))
                ValidateMaximumLocation = source.ValidateMaximumLocation;

            UnitTypes = source.UnitTypes;
            default_record = source.default_record;
        }

        [JsonIgnore]
        public Dictionary<string, object> Defaults = null;

        public void Init()
        {
            if (DefaultCustoms == null)
            {
                Defaults = null;
                return;
            }

            Defaults = new Dictionary<string, object>();
            Defaults.Add(Control.CustomSectionName, DefaultCustoms);

            if(UnitTypes == null)
                UnitTypes = new List<CategoryDescriptorRecord>();

            default_record = UnitTypes.FirstOrDefault(i => i.UnitType == "*");
            
            if (default_record == null)
                default_record = new CategoryDescriptorRecord();
            else
                UnitTypes.Remove(default_record);

            if (string.IsNullOrEmpty(AddAlreadyEquiped))
                AddAlreadyEquiped = Control.Settings.Message.Category_AlreadyEquiped;
            if (string.IsNullOrEmpty(AddAlreadyEquipedLocation))
                AddAlreadyEquipedLocation = Control.Settings.Message.Category_AlreadyEquiped;
            if (string.IsNullOrEmpty(AddMaximumReached))
                AddMaximumReached = Control.Settings.Message.Category_AlreadyEquiped;

            if (string.IsNullOrEmpty(AddMaximumLocationReached))
                AddMaximumLocationReached = Control.Settings.Message.Category_AlreadyEquiped;
            if (string.IsNullOrEmpty(AddMixed))
                AddMixed = Control.Settings.Message.Category_AlreadyEquiped;
            if (string.IsNullOrEmpty(ValidateRequred))
                ValidateRequred = Control.Settings.Message.Category_AlreadyEquiped;
            
            if (string.IsNullOrEmpty(ValidateMinimum))
                ValidateMinimum = Control.Settings.Message.Category_AlreadyEquiped;
            if (string.IsNullOrEmpty(ValidateMixed))
                ValidateMixed = Control.Settings.Message.Category_AlreadyEquiped;
            if (string.IsNullOrEmpty(ValidateUnique))
                ValidateUnique = Control.Settings.Message.Category_AlreadyEquiped;
           
            if (string.IsNullOrEmpty(ValidateMaximum))
                ValidateMaximum = Control.Settings.Message.Category_AlreadyEquiped;
            if (string.IsNullOrEmpty(ValidateUniqueLocation))
                ValidateUniqueLocation = Control.Settings.Message.Category_AlreadyEquiped;
            if (string.IsNullOrEmpty(ValidateMaximumLocation))
                ValidateMaximumLocation = Control.Settings.Message.Category_AlreadyEquiped;
        }
    }

    /// <summary>
    /// Category settings
    /// </summary>
}