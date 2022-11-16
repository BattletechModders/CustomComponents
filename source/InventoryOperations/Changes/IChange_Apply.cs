using System.Collections.Generic;
using BattleTech;

namespace CustomComponents.Changes;

public interface IChange_Apply : IChange
{
    void PreviewApply(InventoryOperationState state);
    void ApplyToInventory(MechDef mech, List<MechComponentRef> inventory);
    void ApplyToMechlab();
}