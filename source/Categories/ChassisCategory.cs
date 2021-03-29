using BattleTech;
using Newtonsoft.Json;

namespace CustomComponents
{
    public class ChassisCategory : SimpleCustomChassis, ICategoryDescriptorRecord
    {
        public int MaxEquiped { get; set; }
        public int MaxEquipedPerLocation { get; set; }
        public int MinEquiped { get; set; }
        public ChassisLocations AllowEquip { get; set; }
        
        [JsonIgnore]
        public bool Unique
        {
            get
            {
                return MaxEquiped == 1;
            }
        }
        
        [JsonIgnore]
        public bool UniqueForLocation
        {
            get
            {
                return MaxEquipedPerLocation == 1;
            }
        }

        [JsonIgnore]
        public bool Required => MinEquiped > 0;

        [JsonIgnore]
        public bool NotAllowed => MaxEquiped == 0 || AllowEquip == ChassisLocations.None;

    }
}