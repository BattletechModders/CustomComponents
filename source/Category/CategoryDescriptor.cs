namespace CustomComponents
{
    public class CategoryDescriptor
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }

        public bool AllowMix = true;
        public bool AutoReplace = false;

        public int MaxEquiped = -1;
        public int MaxEquipedPerLocation = -1;
        public int MinEquiped = 0;

        public string AddAlreadyEquiped = "{0} already fitted on mech";
        public string AddAlreadyEquipedLocation = "{0} already present at {1}";
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

        public bool Unique
        {
            get
            {
                return MaxEquiped == 1;
            }
        }
        public bool UniqueForLocation
        {
            get
            {
                return MaxEquipedPerLocation == 1;
            }
        }
        public bool Requred
        {
            get { return MinEquiped > 0; }
        }


        public CategoryDescriptor(string name)
        {
            this.Name = name;
            this.DisplayName = name;
        }

        public void Apply(CategoryDescriptor category)
        {
            DisplayName = category.DisplayName;
            AllowMix = category.AllowMix;
            AutoReplace = category.AutoReplace;

            MaxEquiped = category.MaxEquiped;
            MaxEquipedPerLocation = category.MaxEquipedPerLocation;
            MinEquiped = category.MinEquiped;

            AddAlreadyEquiped =category.AddAlreadyEquiped;
            AddAlreadyEquipedLocation = category.AddAlreadyEquipedLocation;
            AddMaximumReached = category.AddMaximumReached;
            AddMaximumLocationReached = category.AddMaximumLocationReached;
            AddMixed = category.AddMixed;

            ValidateRequred = category.ValidateRequred;
            ValidateMinimum = category.ValidateMinimum;
            ValidateMixed = category.ValidateMixed;
            ValidateUnique = category.ValidateUnique;
            ValidateMaximum = category.ValidateMaximum;
            ValidateUniqueLocation = category.ValidateUniqueLocation;
            ValidateMaximumLocation = category.ValidateMaximumLocation;
        }
    }
}