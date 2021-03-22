using System.Collections.Generic;

namespace CustomComponents
{
    public interface IChange
    {

    }


    public interface IApplyChange : IChange
    {
        void DoChange(MechLabHelper mechLab, LocationHelper loc);
        void CancelChange(MechLabHelper mechLab, LocationHelper loc);
    }

    public interface IAdjustChange : IChange
    {
        public string ChangeID { get; }

        void DoAdjust(MechLabHelper mechLab, LocationHelper loc, List<IChange> changes);
    }
}