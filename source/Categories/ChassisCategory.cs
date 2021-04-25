using System.Collections.Generic;
using BattleTech;
using Newtonsoft.Json;
using System.Linq;

namespace CustomComponents
{

    [CustomComponent("ChassisCategory", true)]
    public class ChassisCategory : SimpleCustomChassis, IAfterLoad
    {
        public class record
        {
            public ChassisLocations Location { get; set; } = ChassisLocations.All;
            public int Min { get; set; } = 0;
            public int Max { get; set; } = -1;

            public override bool Equals(object obj)
            {
                var r = obj as record;
                if (r == null)
                    return false;

                return Location == r.Location;
            }

            public override int GetHashCode()
            {
                return Location.GetHashCode();
            }
        }

        public string Category { get; set; }
        private record[] Limits { get; set; }

        [JsonIgnore]
        public Dictionary<ChassisLocations, CategoryLimit> LocationLimits { get; set; }

        public void OnLoaded(Dictionary<string, object> values)
        {
            if (Limits == null || Limits.Length == 0)
                LocationLimits = new Dictionary<ChassisLocations, CategoryLimit>();
            else
                LocationLimits = Limits.Distinct().ToDictionary(i => i.Location, i => new CategoryLimit(i.Min, i.Max));

        }
    }
}