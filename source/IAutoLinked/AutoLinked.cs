using BattleTech;

namespace CustomComponents
{
    public class Link
    {
        public ChassisLocations Location;
        public string ApendixID;
        public ComponentType BaseType;
    }

    [CustomComponent("Linked")]
    public class AutoLinked : SimpleCustomComponent
    {
        public Link[] Links { get; set; }
    }
}