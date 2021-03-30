using BattleTech;
using Newtonsoft.Json;

namespace CustomComponents
{
    public class ChassisCategory : SimpleCustomChassis, ICategoryDescriptorRecord
    {

        public string Category { get; set; }
        public int MaxEquiped { get; set; }
        public int MaxEquipedPerLocation { get; set; }
        public int MinEquiped { get; set; }
        public ChassisLocations AllowEquip { get; set; }
       
    }
}