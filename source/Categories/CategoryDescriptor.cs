using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using BattleTech;
using Newtonsoft.Json;

namespace CustomComponents
{
    public struct CategoryLimit
    {
        public CategoryLimit(int min, int max)
        {
            Min = min;
            Max = max;
        }


        public int Max { get; set; }
        public int Min { get; set; }
    }



    public class CategoryDescriptorRecord
    {
        private class record
        {
            public ChassisLocations Location { get; set; } = ChassisLocations.All;
            public int Min { get; set; } = 0;
            public int Max { get; set; } = 1;

            public override bool Equals(object obj)
            {
                var r = obj as record;
                if (r == null)
                    return false;

                return Location == r.Location;
            }

            public override int GetHashCode()
            {
                return Location.GetHashCode();
            }
        }
        
        public string UnitType;

        private record[] Limits;

        [JsonIgnore]
        public Dictionary<ChassisLocations, CategoryLimit> LocationLimits;
        [JsonIgnore]
        public bool MinLimited { get; set; }
        [JsonIgnore]
        public bool MaxLimited { get; set; }


        public void Complete()
        {
            if (Limits == null || Limits.Length == 0)
                LocationLimits = new Dictionary<ChassisLocations, CategoryLimit>();
            else
                LocationLimits = Limits.Distinct().ToDictionary(i => i.Location, i => new CategoryLimit(i.Min, i.Max));

            MinLimited = LocationLimits.Count > 0 && LocationLimits.Values.Any(i => i.Min > 0);
            MaxLimited = LocationLimits.Count > 0 && LocationLimits.Values.Any(i => i.Max >= 0);

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

        public string AddMaximumReached;
        public string AddMixed;

        public string ValidateMinimum;
        public string ValidateMaximum;
        public string ValidateMixed;

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

                var chassis_limits = GetMechCategoryCustom(mech);
                if (chassis_limits != null)
                    result = new CategoryDescriptorRecord()
                    {
                        LocationLimits = chassis_limits
                    };
                else
                {
                    var ut = UnitTypeDatabase.Instance.GetUnitTypes(mech);
                    result = UnitTypes.FirstOrDefault(i => ut.Contains(i.UnitType));
                    if (result == null)
                        result = default_record;
                    if (result.LocationLimits == null)
                        result.Complete();
                }

                records[mech.ChassisID] = result;
                return result;
            }
        }

        private Dictionary<ChassisLocations, CategoryLimit> GetMechCategoryCustom(MechDef mech)
        {
            var custom = mech.Chassis.GetComponents<ChassisCategory>().FirstOrDefault(i => i.Category == Name);

            if (custom != null)
                return custom.LocationLimits;

            return null;
        }

        public void Apply(CategoryDescriptor source)
        {
            DisplayName = source.DisplayName;
            AllowMixTags = source.AllowMixTags;
            AutoReplace = source.AutoReplace;
            DefaultCustoms = source.DefaultCustoms;
            ReplaceAnyLocation = source.ReplaceAnyLocation;
            AddCategoryToDescription = source.AddCategoryToDescription;

            if (!string.IsNullOrEmpty(source.AddMaximumReached))
                AddMaximumReached = source.AddMaximumReached;
            if (!string.IsNullOrEmpty(source.AddMixed))
                AddMixed = source.AddMixed;

            if (!string.IsNullOrEmpty(source.ValidateMinimum))
                ValidateMinimum = source.ValidateMinimum;
            if (!string.IsNullOrEmpty(source.ValidateMaximum))
                ValidateMaximum = source.ValidateMaximum;
            if (!string.IsNullOrEmpty(source.ValidateMixed))
                ValidateMixed = source.ValidateMixed;

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

            if (UnitTypes == null)
                UnitTypes = new List<CategoryDescriptorRecord>();

            default_record = UnitTypes.FirstOrDefault(i => i.UnitType == "*");

            if (default_record == null)
                default_record = new CategoryDescriptorRecord();
            else
                UnitTypes.Remove(default_record);

            if (string.IsNullOrEmpty(AddMaximumReached))
                AddMaximumReached = Control.Settings.Message.Category_MaximumReached;
            if (string.IsNullOrEmpty(AddMixed))
                AddMixed = Control.Settings.Message.Category_Mixed;

            if (string.IsNullOrEmpty(ValidateMinimum))
                ValidateMinimum = Control.Settings.Message.Category_ValidateMinimum;
            if (string.IsNullOrEmpty(ValidateMaximum))
                ValidateMaximum = Control.Settings.Message.Category_ValidateMaximum;
            if (string.IsNullOrEmpty(ValidateMixed))
                ValidateMixed = Control.Settings.Message.Category_ValidateMixed;
        }
    }

    /// <summary>
    /// Category settings
    /// </summary>
}