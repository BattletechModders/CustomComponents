namespace CustomComponents
{
    public interface IChange
    {
        void DoChange(MechLabHelper mechLab, LocationHelper loc);
        void CancelChange(MechLabHelper mechLab, LocationHelper loc);
    }
}