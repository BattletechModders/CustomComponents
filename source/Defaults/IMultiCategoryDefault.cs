using BattleTech;

namespace CustomComponents
{
    public interface IMultiCategoryDefault
    {
        string[] Categories { get;  }
        ComponentType ComponentType { get; }
        string DefID { get; }
        ChassisLocations Location { get; }
        public bool AnyLocation { get; }

    }
}