using System.Collections.Generic;
using BattleTech;

namespace CustomComponents
{
    public delegate void ClearInventoryDelegate(MechDef mech, List<MechComponentRef> result, SimGameState state);

    public interface IClearInventory
    {
        void ClearInventory(MechDef mech, List<MechComponentRef> result, SimGameState state, MechComponentRef source);
    }
}