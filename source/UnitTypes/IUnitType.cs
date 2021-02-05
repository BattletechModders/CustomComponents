using BattleTech;

namespace CustomComponents
{
    public interface IUnitType
    {
        public string Name { get; }
        public bool IsThisType(MechDef mechdef);
    }
}