using BattleTech;

namespace CustomComponents
{
    public interface IDefault
    {
        ChassisLocations Location { get; }
        string CategoryID { get; }
        bool AnyLocation { get; }
        string DefID { get;  }

        MechComponentRef GetReplace(MechDef mechDef, SimGameState state);
        bool AddItems(MechDef mechDef, SimGameState state);
        bool NeedReplaceExistDefault(MechDef mechDef, MechComponentRef item);
    }
}