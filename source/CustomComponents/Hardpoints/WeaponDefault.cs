using BattleTech;

namespace CustomComponents;

public interface IWeaponDefault
{
    string DefID { get; }
    ChassisLocations Location { get; }
    ComponentType Type { get;}
    string[] ReplaceCategories { get;  }
}

[CustomComponent("WeaponDefault", true)]
public class WeaponDefault : SimpleCustomChassis, IWeaponDefault
{
    public string DefID { get; set; }
    public ChassisLocations Location { get; set; }
    public ComponentType Type { get; set; } = ComponentType.Weapon;
    public string[] ReplaceCategories { get; set; } = null;
}