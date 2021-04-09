using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BattleTech;

namespace CustomComponents
{
    public interface IAllowedLocations
    {
        public ChassisLocations GetLocationsFor(MechDef mech);
    }

    [CustomComponent("AllowedLocationsTAG", group: "AllowedLocation")]
    public class EquipLocationsTAG : SimpleCustomComponent, IAllowedLocations
    {
        internal string Tag { get; set; }

        public ChassisLocations GetLocationsFor(MechDef mech)
        {
            return EquipLocationController.Instance[mech, Def];
        }
    }

    [CustomComponent("AllowedLocationsTAG", group: "AllowedLocation")]
    public class EquipLocationsUT : SimpleCustomComponent, IAllowedLocations
    {
        internal EquipLocationTag.record[] UnitTypes;
        internal ChassisLocations Default = ChassisLocations.All;

        public ChassisLocations GetLocationsFor(MechDef mech)
        {
            return EquipLocationController.Instance[mech, Def];
        }
    }
}
