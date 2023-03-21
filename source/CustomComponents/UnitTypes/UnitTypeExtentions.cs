using System.Collections.Generic;
using BattleTech;

namespace CustomComponents;

public static class UnitTypeExtentions
{
    public static HashSet<string> GetUnitTypes(this MechDef mech)
    {
        return UnitTypeDatabase.Instance.GetUnitTypes(mech);
    }

    public static HashSet<string> GetUnitTypes(this ChassisDef chassis)
    {
        return UnitTypeDatabase.Instance.GetUnitTypes(chassis);
    }
}