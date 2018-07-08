namespace CustomComponents
{
    public enum ValidateDropStatus
    {
        Continue, Handled
    }

    public interface IValidateDropResult
    {
        ValidateDropStatus Status { get; } // not really necessary, but nice for semantics
    }


    public interface IChange
    {
        void DoChange(MechLabHelper mechLab, LocationHelper loc);
    }
}
