using System;
using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json;

namespace CustomComponents
{
    /// <summary>
    /// Category settings
    /// </summary>
    public class CategoryDescriptor
    {
        /// <summary>
        /// Name to display in error messages
        /// </summary>
        [DefaultValue(""), JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
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
        [DefaultValue(true), JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public bool AllowMixTags = true;
        /// <summary>
        /// auto replace item if maximum reached
        /// </summary>
        [DefaultValue(false), JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public bool AutoReplace = false;

        /// <summary>
        /// max of items per mech
        /// </summary>
        [DefaultValue(-1), JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int MaxEquiped = -1;
        /// <summary>
        /// max of items per location
        /// </summary>
        [DefaultValue(-1), JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int MaxEquipedPerLocation = -1;
        /// <summary>
        /// Minimum item per mech(required items)
        /// </summary>
        [DefaultValue(0), JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int MinEquiped = 0;


        public string[] Forbidden;

        public Dictionary<string, object> DefaultCustoms = null;

        [DefaultValue("{0} already installed on mech"), JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string AddAlreadyEquiped = "{0} already installed on mech";

        [DefaultValue("{0} already installed on {1}"), JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string AddAlreadyEquipedLocation = "{0} already installed on {1}";

        [DefaultValue("Mech already have {1} of {0} installed"), JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string AddMaximumReached = "Mech already have {1} of {0} installed";

        [DefaultValue("Mech can have obly {1} of {0} installed at {2}"), JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string AddMaximumLocationReached = "Mech already have {1} of {0} installed at {2}";

        [DefaultValue("Mech can have only one type of {0}"), JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string AddMixed = "Mech can have only one type of {0}";


        [DefaultValue("MISSING {0}: This mech must mount a {1}"), JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string ValidateRequred = "MISSING {0}: This mech must mount a {1}";

        [DefaultValue("MISSING {0}: This mech must mount at least {2} of {1}"), JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string ValidateMinimum = "MISSING {0}: This mech must mount at least {2} of {1}";

        [DefaultValue("WRONG {0}: Mech can have only one type of {1}"), JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string ValidateMixed = "WRONG {0}: Mech can have only one type of {1}";

        [DefaultValue("EXCESS {0}: This mech can't mount more then one {1}"), JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string ValidateUnique = "EXCESS {0}: This mech can't mount more then one {1}";

        [DefaultValue("EXCESS {0}: This mech can't mount more then {2} of {1}"), JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string ValidateMaximum = "EXCESS {0}: This mech can't mount more then {2} of {1}";

        [DefaultValue("EXCESS {0}: This mech can't mount more then one {1} at any location"), JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string ValidateUniqueLocation = "EXCESS {0}: This mech can't mount more then one {1} at any location";

        [DefaultValue("EXCESS {0}: This mech can't mount more then {2} of {1} any location"), JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string ValidateMaximumLocation = "EXCESS {0}: This mech can't mount more then {2} of {1} any location";


        [DefaultValue("Cannot use {0} and {1} together"), JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string ValidateForbidden = "Cannot use {0} and {1} together";


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


        public CategoryDescriptor()
        {
        }


        public void Apply(CategoryDescriptor category)
        {
            DisplayName = category.DisplayName;
            AllowMixTags = category.AllowMixTags;
            AutoReplace = category.AutoReplace;

            MaxEquiped = category.MaxEquiped;
            MaxEquipedPerLocation = category.MaxEquipedPerLocation;
            MinEquiped = category.MinEquiped;

            AddAlreadyEquiped = category.AddAlreadyEquiped;
            AddAlreadyEquipedLocation = category.AddAlreadyEquipedLocation;
            AddMaximumReached = category.AddMaximumReached;
            AddMaximumLocationReached = category.AddMaximumLocationReached;
            AddMixed = category.AddMixed;
            Forbidden = category.Forbidden;
            DefaultCustoms = category.DefaultCustoms;



            ValidateRequred = category.ValidateRequred;
            ValidateMinimum = category.ValidateMinimum;
            ValidateMixed = category.ValidateMixed;
            ValidateUnique = category.ValidateUnique;
            ValidateMaximum = category.ValidateMaximum;
            ValidateUniqueLocation = category.ValidateUniqueLocation;
            ValidateMaximumLocation = category.ValidateMaximumLocation;
            ValidateForbidden = category.ValidateForbidden;
        }
    }
}