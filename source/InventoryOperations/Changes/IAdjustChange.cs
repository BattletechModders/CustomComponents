namespace CustomComponents.Changes
{
    public interface IAdjustChange : IChange
    {
        string ChangeID { get; }
    }
}