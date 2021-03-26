using BattleTech;

namespace CustomComponents
{
    public interface IDefault
    {
        public ChassisLocations Location { get;  }
        public string CategoryID { get;  }
        public string DefID { get;  }
        public ComponentType Type { get;  }
        public bool AnyLocation { get; } 

    }
}