using BattleTech;

namespace CustomComponents;

public interface IAllowedLocations
{
    ChassisLocations GetLocationsFor(MechDef mech);
}

[CustomComponent("AllowedLocations")]
public class EquipLocations: SimpleCustomComponent, IAllowedLocations
{
    internal string Tag { get; set; }

    public ChassisLocations GetLocationsFor(MechDef mech)
    {
        return EquipLocationController.Instance[mech, Def];
    }
}

public interface IChassisAllowedLocations
{
    string Tag { get; }
    ChassisLocations Locations { get;  }
}


[CustomComponent("ChassisAllowedLocations", true)]
public class CEquipLocations : SimpleCustomChassis, IChassisAllowedLocations
{
    public string Tag { get; set; }
    public ChassisLocations Locations { get; set; }
}