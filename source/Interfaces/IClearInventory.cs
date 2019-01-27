using System.Collections.Generic;
using BattleTech;

namespace CustomComponents
{
    public interface IClearInventory
    {
        void ClearInventory(MechDef mech, List<MechComponentRef> result, SimGameState state, MechComponentRef source);
    }
}