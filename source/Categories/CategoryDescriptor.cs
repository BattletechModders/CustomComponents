using System.Collections.Generic;
using System.Linq;
using System.Text;
using BattleTech;
using fastJSON;

namespace CustomComponents
{
    public struct CategoryLimit
    {
        public CategoryLimit(int min, int max, bool snapToMin)
        {
            Min = min;
            Max = max;
            SnapToMin = snapToMin;
        }

        public bool SnapToMin { get; set; }

        public int Max { get; set; }
        public int Min { get; set; }

        public int Limit => SnapToMin ? Min : Max;
        public bool Limited => SnapToMin ? Min > 0 : Max >= 0;
    }



    public class CategoryDescriptorRecord
    {
        public class _record
        {
            public ChassisLocations Location { get; set; } = ChassisLocations.All;
            public int Min { get; set; } = 0;
            public int Max { get; set; } = -1;

            public override bool Equals(object obj)
            {
                var r = obj as _record;
                if (r == null)
                    return false;

                return Location == r.Location;
            }

            public override int GetHashCode()
            {
                return Location.GetHashCode();
            }

            public _record()
            {
            }

            public _record(ChassisLocations location, int min, int max)
            {
                Location = location;
                Max = max;
                Min = min;
            }

            public _record(_record source)
            {
                Location = source.Location;
                Max = source.Max;
                Min = source.Min;
            }
        }

        public string UnitType;

        internal _record[] Limits;

        public bool PartialOverride = false;

        [Newtonsoft.Json.JsonIgnore]
        public Dictionary<ChassisLocations, CategoryLimit> LocationLimits;

        public CategoryDescriptorRecord()
        {
        }

        public CategoryDescriptorRecord(_record[] baseLimits)
        {
            Limits = baseLimits;
        }

        [Newtonsoft.Json.JsonIgnore] public bool MinLimited { get; set; }
        [Newtonsoft.Json.JsonIgnore] public bool MaxLimited { get; set; }
        [Newtonsoft.Json.JsonIgnore] public bool Limited => MaxLimited || MinLimited;


        public void Complete(CategoryDescriptor cat_info)
        {
            if (LocationLimits == null)
                if (Limits == null || Limits.Length == 0)
                    LocationLimits = new Dictionary<ChassisLocations, CategoryLimit>();
                else
                    LocationLimits = Limits.Distinct().ToDictionary(i => i.Location, i => new CategoryLimit(i.Min, i.Max, cat_info?.ReplaceDefaultsFirst ?? true));

            MinLimited = LocationLimits.Count > 0 && LocationLimits.Values.Any(i => i.Min > 0);
            MaxLimited = LocationLimits.Count > 0 && LocationLimits.Values.Any(i => i.Max >= 0);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            if (!string.IsNullOrEmpty(UnitType))
                sb.Append(UnitType + ": ");
            sb.AppendLine($"MinL: {MinLimited}, MaxL: {MaxLimited}");
            if (LocationLimits != null)
                foreach (var pair in LocationLimits)
                    sb.AppendLine($"--- {pair.Key}: min: {pair.Value.Min}, max: {pair.Value.Max}, limit:{pair.Value.Limit}");

            return sb.ToString();
        }
    }

    public class CategoryDescriptor
    {
        public string DisplayName = "";
        public string Name { get; set; }
        [Newtonsoft.Json.JsonIgnore]
        public string _DisplayName
        {
            get => string.IsNullOrEmpty(DisplayName) ? Name : DisplayName;
            set => DisplayName = value;
        }

        public bool AllowMaxOverflow = false;
        public bool AllowMinOverflow = true;
        public bool AllowMixTagsMechlab = true;
        public bool ReplaceDefaultsFirst = true;

        //public bool AutoReplace = false;
        public bool AddCategoryToDescription = Control.Settings.CategoryDescriptionAddedByDefault;
        public bool AllowMixTags = true;
        public Dictionary<string, object> DefaultCustoms = null;

        public string AddMaximumReached = null;
        public string AddMixed = null;

        public string ValidateMinimum = null;
        public string ValidateMaximum = null;
        public string ValidateMixed = null;


        [JsonIgnore]
        public CategoryDescriptorRecord DefaultLimits { get; set; }

        private CategoryDescriptorRecord._record[] BaseLimits { get; set; }
        public List<CategoryDescriptorRecord> UnitLimits = new List<CategoryDescriptorRecord>();
        [Newtonsoft.Json.JsonIgnore] private Dictionary<string, CategoryDescriptorRecord> records = new Dictionary<string, CategoryDescriptorRecord>();

        public CategoryDescriptorRecord this[MechDef mech]
        {
            get
            {
                if (mech == null)
                    return DefaultLimits;

                if (records.TryGetValue(mech.ChassisID, out var result))
                    return result;

                var chassis_limits = GetMechCategoryCustom(mech);
                if (chassis_limits != null)
                {
                    result = new CategoryDescriptorRecord()
                    {
                        LocationLimits = chassis_limits
                    };
                }
                else
                {
                    var ut = UnitTypeDatabase.Instance.GetUnitTypes(mech);
                    if (ut != null)
                    {
                        var candidates = UnitLimits.Where(i => ut.Contains(i.UnitType)).ToList();

                        result = candidates.FirstOrDefault(x => !x.PartialOverride);

                        if (result == null && candidates.Count > 0 && DefaultLimits != null)
                        {
                            { // copy default limits into result
                                result = new CategoryDescriptorRecord(DefaultLimits.Limits);
                                result.Complete(this);
                            }
                            // overwrite values and last one wins
                            foreach (var limit in candidates)
                            {
                                limit.Complete(this);
                                foreach (var locationLimit in limit.LocationLimits)
                                {
                                    result.LocationLimits[locationLimit.Key] = locationLimit.Value;
                                }
                            }
                        }
                    }
                }
                
                if (result == null)
                    result = DefaultLimits;

                if (result != null)
                {
                    result.Complete(this);

                    //foreach (var limit in result.LocationLimits)
                    //{
                    //    Control.Log($"Limit Name={Name} ChassisID={mech.ChassisID} limitKey={limit.Key} Limit={limit.Value.Limit} Max={limit.Value.Max}");
                    //}
                }

                records[mech.ChassisID] = result;

                return result;
            }
        }

        private Dictionary<ChassisLocations, CategoryLimit> GetMechCategoryCustom(MechDef mech)
        {
            var custom = mech.Chassis.GetComponents<ChassisCategory>().FirstOrDefault(i => i.CategoryID == Name);

            if (custom != null)
                return custom.LocationLimits;

            return null;
        }

        public void Apply(CategoryDescriptor source)
        {
            _DisplayName = source._DisplayName;
            AllowMixTags = source.AllowMixTags;
            DefaultCustoms = source.DefaultCustoms;
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

            UnitLimits = source.UnitLimits;
            BaseLimits = source.BaseLimits;
        }

        public void Init()
        {
            UnitLimits ??= new List<CategoryDescriptorRecord>();

            var limits = UnitLimits.FirstOrDefault(i => i.UnitType == "*");
            if (limits != null)
                UnitLimits.Remove(limits);

            if (BaseLimits == null)
            {
                if (limits == null)
                    DefaultLimits = new CategoryDescriptorRecord();
                else
                    DefaultLimits = limits;
            }
            else
                DefaultLimits = new CategoryDescriptorRecord(BaseLimits);



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

        public override string ToString()
        {
            var sb = new StringBuilder("Category: " + Name + "(" + _DisplayName + ")");
            sb.AppendLine();
            sb.AppendLine(
                $"- AddToDescription: {AddCategoryToDescription},  AllowMixTags/Mechlab: {AllowMixTags}/{AllowMixTagsMechlab}");
            sb.AppendLine(
                $"- MinOverflow: {AllowMinOverflow}, MaxOverflow: {AllowMaxOverflow}, DefaultsFirst: {ReplaceDefaultsFirst}");
            sb.AppendLine("- Errors");
            sb.AppendLine("-- AddMaximumReached: " + AddMaximumReached);
            sb.AppendLine("-- AddMixed: " + AddMixed);
            sb.AppendLine("-- ValidateMinimum: " + ValidateMinimum);
            sb.AppendLine("-- ValidateMaximum: " + ValidateMaximum);
            sb.AppendLine("-- ValidateMixed: " + ValidateMixed);
            if (DefaultLimits != null)
            {
                if (DefaultLimits.LocationLimits == null)
                    DefaultLimits.Complete(this);
                sb.Append("- DefaultLimits: " + DefaultLimits.ToString());
            }

            if (UnitLimits != null)
                foreach (var record in UnitLimits)
                {
                    if (record.LocationLimits == null)
                        record.Complete(this);
                    sb.Append("-- " + record.ToString());
                }

            return sb.ToString();
        }
    }

    /// <summary>
    /// Category settings
    /// </summary>
}