using BattleTech;

namespace CustomComponents
{
    [CustomComponent("WeaponDefault", true)]
    public class WeaponDefault : SimpleCustomChassis
    {
        public string DefID { get; set; }
        public ChassisLocations Location { get; set; }
        public ComponentType Type { get; set; } = ComponentType.Weapon;
        public string[] ReplaceTags { get; set; } = null;
    }
}