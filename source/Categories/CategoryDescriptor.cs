using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using BattleTech;
using Newtonsoft.Json;

namespace CustomComponents
{
    public class CategoryDescriptorRecord
    {
        public string UnitType;

        public int MaxEquiped = -1;
        public int MaxEquipedPerLocation = -1;
        public int MinEquiped = 0;

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

        public string AddAlreadyEquiped = "{0} already installed on mech";

        public string AddAlreadyEquipedLocation = "{0} already installed on {1}";

        public string AddMaximumReached = "Mech already have {1} of {0} installed";

        public string AddMaximumLocationReached = "Mech already have {1} of {0} installed at {2}";

        public string AddMixed = "Mech can have only one type of {0}";


        public string ValidateRequred = "MISSING {0}: This mech must mount a {1}";

        public string ValidateMinimum = "MISSING {0}: This mech must mount at least {2} of {1}";

        public string ValidateMixed = "WRONG {0}: Mech can have only one type of {1}";

        public string ValidateUnique = "EXCESS {0}: This mech can't mount more then one {1}";

        public string ValidateMaximum = "EXCESS {0}: This mech can't mount more then {2} of {1}";

        public string ValidateUniqueLocation = "EXCESS {0}: This mech can't mount more then one {1} at any location";

        public string ValidateMaximumLocation = "EXCESS {0}: This mech can't mount more then {2} of {1} any location";

        public CategoryDescriptorRecord[] UnitTypes;

        public CategoryDescriptorRecord this[string mechid]
        {
            get
            {
                var types = UnitTypeDatabase.Instance.GetUnitTypes(mechid);
                return UnitTypes.FirstOrDefault(i => types.Contains(i.UnitType));
            }
        }

        public CategoryDescriptorRecord this[MechDef mech]
        {
            get
            {
                if (mech != null)
                    return this[mech.Description.Id];
                return null;
            }
        }


        public void CreateFromOld(CategoryDescriptor_Old source)
        {
            DisplayName = source.DisplayName;
            AllowMixTags = source.AllowMixTags;
            AutoReplace = source.AutoReplace;
            AddCategoryToDescription = source.AddCategoryToDescription;
            AddAlreadyEquiped = source.AddAlreadyEquiped;
            AddAlreadyEquipedLocation = source.AddAlreadyEquipedLocation;
            AddMaximumReached = source.AddMaximumReached;
            AddMaximumLocationReached = source.AddMaximumLocationReached;
            AddMixed = source.AddMixed;
            DefaultCustoms = source.DefaultCustoms;

            ReplaceAnyLocation = source.ReplaceAnyLocation;

            ValidateRequred = source.ValidateRequred;
            ValidateMinimum = source.ValidateMinimum;
            ValidateMixed = source.ValidateMixed;
            ValidateUnique = source.ValidateUnique;
            ValidateMaximum = source.ValidateMaximum;
            ValidateUniqueLocation = source.ValidateUniqueLocation;
            ValidateMaximumLocation = source.ValidateMaximumLocation;

            var record = new CategoryDescriptorRecord()
            {
                UnitType = Control.Settings.DefaultOldCategoryType,
                MaxEquiped = source.MaxEquiped,
                MaxEquipedPerLocation = source.MaxEquipedPerLocation,
                MinEquiped = source.MinEquiped,
            };

            this.UnitTypes = new[] {record};
        }
        public void Apply(CategoryDescriptor source)
        {
            DisplayName = source.DisplayName;
            AllowMixTags = source.AllowMixTags;
            AutoReplace = source.AutoReplace;
            AddCategoryToDescription = source.AddCategoryToDescription;
            AddAlreadyEquiped = source.AddAlreadyEquiped;
            AddAlreadyEquipedLocation = source.AddAlreadyEquipedLocation;
            AddMaximumReached = source.AddMaximumReached;
            AddMaximumLocationReached = source.AddMaximumLocationReached;
            AddMixed = source.AddMixed;
            DefaultCustoms = source.DefaultCustoms;

            ReplaceAnyLocation = source.ReplaceAnyLocation;

            ValidateRequred = source.ValidateRequred;
            ValidateMinimum = source.ValidateMinimum;
            ValidateMixed = source.ValidateMixed;
            ValidateUnique = source.ValidateUnique;
            ValidateMaximum = source.ValidateMaximum;
            ValidateUniqueLocation = source.ValidateUniqueLocation;
            ValidateMaximumLocation = source.ValidateMaximumLocation;

            UnitTypes = source.UnitTypes;

        }

        [JsonIgnore]
        public Dictionary<string, object> Defaults = null;

        public void InitDefaults()
        {
            if (DefaultCustoms == null)
            {
                Defaults = null;
                return;
            }

            Defaults = new Dictionary<string, object>();
            Defaults.Add(Control.CustomSectionName, DefaultCustoms);
        }
    }

    /// <summary>
    /// Category settings
    /// </summary>
    public class CategoryDescriptor_Old
    {
        /// <summary>
        /// Name to display in error messages
        /// </summary>
        public string displayName = "";

        /// <summary>
        /// name of category (same as ICategory.CategoryID)
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Name to display in error messages
        /// </summary>
        [JsonIgnore]
        public string DisplayName
        {
            get => string.IsNullOrEmpty(displayName) ? Name : displayName;
            set => displayName = value;
        }

        /// <summary>
        /// if allow mixing items of same category with different
        /// </summary>
        public bool AllowMixTags = true;
        /// <summary>
        /// auto replace item if maximum reached
        /// </summary>
        public bool AutoReplace = false;


        public bool ReplaceAnyLocation = false;

        /// <summary>
        /// max of items per mech
        /// </summary>
        public int MaxEquiped = -1;
        /// <summary>
        /// max of items per location
        /// </summary>
        public int MaxEquipedPerLocation = -1;
        /// <summary>
        /// Minimum item per mech(required items)
        /// </summary>
        public int MinEquiped = 0;

        public bool AddCategoryToDescription = true;

        public Dictionary<string, object> DefaultCustoms = null;

        public string AddAlreadyEquiped = "{0} already installed on mech";

        public string AddAlreadyEquipedLocation = "{0} already installed on {1}";

        public string AddMaximumReached = "Mech already have {1} of {0} installed";

        public string AddMaximumLocationReached = "Mech already have {1} of {0} installed at {2}";

        public string AddMixed = "Mech can have only one type of {0}";


        public string ValidateRequred = "MISSING {0}: This mech must mount a {1}";

        public string ValidateMinimum = "MISSING {0}: This mech must mount at least {2} of {1}";

        public string ValidateMixed = "WRONG {0}: Mech can have only one type of {1}";

        public string ValidateUnique = "EXCESS {0}: This mech can't mount more then one {1}";

        public string ValidateMaximum = "EXCESS {0}: This mech can't mount more then {2} of {1}";

        public string ValidateUniqueLocation = "EXCESS {0}: This mech can't mount more then one {1} at any location";

        public string ValidateMaximumLocation = "EXCESS {0}: This mech can't mount more then {2} of {1} any location";


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


        public CategoryDescriptor_Old()
        {
        }


        public void Apply(CategoryDescriptor_Old category)
        {
            DisplayName = category.DisplayName;
            AllowMixTags = category.AllowMixTags;
            AutoReplace = category.AutoReplace;

            MaxEquiped = category.MaxEquiped;
            MaxEquipedPerLocation = category.MaxEquipedPerLocation;
            MinEquiped = category.MinEquiped;

            AddCategoryToDescription = category.AddCategoryToDescription;

            AddAlreadyEquiped = category.AddAlreadyEquiped;
            AddAlreadyEquipedLocation = category.AddAlreadyEquipedLocation;
            AddMaximumReached = category.AddMaximumReached;
            AddMaximumLocationReached = category.AddMaximumLocationReached;
            AddMixed = category.AddMixed;
            DefaultCustoms = category.DefaultCustoms;

            ReplaceAnyLocation = category.ReplaceAnyLocation;

            ValidateRequred = category.ValidateRequred;
            ValidateMinimum = category.ValidateMinimum;
            ValidateMixed = category.ValidateMixed;
            ValidateUnique = category.ValidateUnique;
            ValidateMaximum = category.ValidateMaximum;
            ValidateUniqueLocation = category.ValidateUniqueLocation;
            ValidateMaximumLocation = category.ValidateMaximumLocation;

            InitDefaults();
        }



        [JsonIgnore]
        public Dictionary<string, object> Defaults = null;

        public void InitDefaults()
        {
            if (DefaultCustoms == null)
            {
                Defaults = null;
                return;
            }

            Defaults = new Dictionary<string, object>();
            Defaults.Add(Control.CustomSectionName, DefaultCustoms);
        }
    }
}