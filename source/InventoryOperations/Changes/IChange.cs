namespace CustomComponents.Changes;

public interface IChange
{
    bool Initial { get; set; }
    void AdjustChange(InventoryOperationState state);
}