using BattleTech;

namespace CustomComponents;

public interface IIsDestroyed
{
    bool IsMechDestroyed(MechComponentRef item, MechDef mech);
}