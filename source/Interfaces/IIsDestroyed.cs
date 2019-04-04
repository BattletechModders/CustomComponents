using BattleTech;

namespace CustomComponents
{
    public interface IIsDestroyed
    {
        bool IsMechDestroyed(MechComponentRef component, MechDef mech);
    }
}