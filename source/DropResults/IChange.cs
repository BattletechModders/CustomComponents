using System.Collections.Generic;

namespace CustomComponents
{
    public interface IChange
    {

    }


    public interface IApplyChange : IChange
    {
        void DoChange();
    }

    public interface ICancelChange : IChange
    {
        void CancelChange();
    }

    public interface IAdjustChange : IChange
    {
        public string ChangeID { get; }

        void DoAdjust(Queue<IChange> changes);
    }
}