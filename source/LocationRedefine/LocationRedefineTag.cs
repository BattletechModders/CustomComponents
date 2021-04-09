using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BattleTech;

namespace CustomComponents
{
    public class LocationRedefineTag
    {
        public class LocationRedefineRecord
        {
            public string UnitType { get; set; }
            public ChassisLocations Location = ChassisLocations.All;
        }

        public string Tag { get; set; }
        public ChassisLocations Default = ChassisLocations.All;
        public LocationRedefineRecord[] UnitTypes;

    }
}
