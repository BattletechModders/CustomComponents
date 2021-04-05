using BattleTech;

namespace CustomComponents
{
    [CustomComponent("ChassisDefaults", true)]
    public class ChassisDefaults : SimpleCustomChassis, IDefault
    {
        public ChassisLocations Location { get; set; }
        public string CategoryID { get; set; }
        public string DefID { get; set; }
        public ComponentType Type { get; set; }

        public override string ToString()
        {
            return $"ChassisDefaults: {DefID}";
        }
    }

    [CustomComponent("MultyDefaults", true)]
    public class MultyDefaults : SimpleCustomChassis, IMultiCategoryDefault
    {
        public ChassisLocations Location { get; set; }
        public string DefID { get; set; }
        public string[] Categories { get; set; }
        public ComponentType ComponentType { get; set; }
        public override string ToString()
        {
            return $"MultyDefaults: {DefID}";
        }
    }
}