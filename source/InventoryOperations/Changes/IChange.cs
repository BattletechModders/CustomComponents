using System.Collections.Generic;
using BattleTech;

namespace CustomComponents.Changes
{
    public interface IChange
    {
        void AdjustChange(InventoryOperationState state);
    }
}