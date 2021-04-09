using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BattleTech;

namespace CustomComponents
{
    [CustomComponent("AllowedLocations")]
    public class EquipLocations
    {
        public ChassisLocations Locations { get; set; } = ChassisLocations.All;
    }

    [CustomComponent("AllowedLocationsTag")]
    public class EquipLocationsTag
    {
        public string Tag { get; set; }
    }
}
