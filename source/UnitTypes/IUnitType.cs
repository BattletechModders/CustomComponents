using BattleTech;

namespace CustomComponents.UnitTypes
{
    public interface IUnitType
    {
        public string Name { get; }
        public bool IsThisType(MechDef mechdef);
    }
}