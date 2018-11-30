using System.Collections.Generic;
using BattleTech;

namespace CustomComponents
{
    public interface IClearInventory
    {
        void ClearInventory(List<MechComponentRef> result, SimGameState state, MechComponentRef source);
    }
}