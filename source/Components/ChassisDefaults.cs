using BattleTech;

namespace CustomComponents
{
    [CustomComponent("ChassisDefaults", true)]
    public class ChassisDefaults : SimpleCustomChassis, IDefault
    {
        public string CategoryID { get; set; }
        public DefaultsInfoRecord[] Defaults { get; set; }

        public override string ToString()
        {
            return $"ChassisDefaults: {CategoryID}, {Defaults?.Length ?? 0} items";
        }

    }

    [CustomComponent("MultiDefaults", true)]
    public class MultiDefaults : SimpleCustomChassis, IMultiCategoryDefault
    {
        public ChassisLocations Location { get; set; }
        public string DefID { get; set; }
        public string[] Categories { get; set; }
        public ComponentType ComponentType { get; set; }
        public override string ToString()
        {
            return $"MultiDefaults: {DefID}";
        }
    }
}