using BattleTech;
using fastJSON;
using UnityEngine;

namespace CustomComponents
{
    [SerializeField]
    public class DefaultsInfoRecord : IDefault
    {
        public ChassisLocations Location { get; set; }
        public string CategoryID { get; set; }
        public string DefID { get; set; }
        public ComponentType Type { get; set; }

        public override string ToString()
        {
            return "DefaultsInfo: " + DefID;
        }
    }


    [SerializeField]
    public class DefaultsInfo
    {
        public int Priority { get; set; }
        public string UnitType { get; set; }
        public DefaultsInfoRecord[] Records;
    }
}