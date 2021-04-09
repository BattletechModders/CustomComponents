using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BattleTech;

namespace CustomComponents
{
    public class EquipLocationTag
    {
        public class record
        {
            public string UnitType { get; set; }
            public ChassisLocations Location = ChassisLocations.All;
        }

        public string Tag { get; set; }
        public ChassisLocations Default = ChassisLocations.All;
        public record[] UnitTypes;
    }
}
