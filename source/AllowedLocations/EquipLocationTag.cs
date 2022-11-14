using System.Text;
using BattleTech;

namespace CustomComponents
{
    public class EquipLocationTag
    {
        public class _record
        {
            public string UnitType { get; set; }
            public ChassisLocations Location = ChassisLocations.All;
        }

        public string Tag { get; set; }
        public ChassisLocations Default = ChassisLocations.All;
        public _record[] UnitTypes;

        public override string ToString()
        {
            var sb = new StringBuilder("Allowed Locations for " + Tag);
            sb.Append("\n- Default: " + Default);
            if(UnitTypes != null && UnitTypes.Length > 0)
                foreach (var unitType in UnitTypes)
                    sb.Append("\n- " + unitType.UnitType + ": " + unitType.Location);
            return sb.ToString();
        }
    }
}
