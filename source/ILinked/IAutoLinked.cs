using BattleTech;

namespace CustomComponents
{
    public class Link
    {
        public ChassisLocations Location;
        public string ApendixID;
        public ComponentType BaseType;
    }

    public interface IAutoLinked
    {
        Link[] Links { get; }
    }
}