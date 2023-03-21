using BattleTech;

namespace CustomComponents;

public interface IOnAdd
{
    void OnAdd(ChassisLocations location, InventoryOperationState state);
}

public interface IOnRemove
{
    void OnRemove(ChassisLocations location, InventoryOperationState state);
}