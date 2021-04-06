using BattleTech;

namespace CustomComponents
{
    internal interface IIsDestroyed
    {
        bool IsMechDestroyed(MechComponentRef item, MechDef mech);
    }
}