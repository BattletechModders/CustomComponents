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

        public override string ToString()
        {
            var sb = new StringBuilder("Allowed Locations for " + Tag);
            sb.Append("\n- Default: " + Default.ToString());
            if(UnitTypes != null && UnitTypes.Length > 0)
                foreach (var unitType in UnitTypes)
                    sb.Append("\n- " + unitType.UnitType + ": " + unitType.Location.ToString());
            return sb.ToString();
        }
    }
}
