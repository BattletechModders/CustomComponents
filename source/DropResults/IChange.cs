using System.Collections.Generic;

namespace CustomComponents
{
    public interface IChange
    {
        bool DoAdjust(Queue<IChange> changes, List<InvItem> inventory);
    }


    public interface IApplyChange : IChange
    {
        void DoChange();
        void PreviewChange(List<InvItem> inventory);

    }

    public interface ICancelChange : IChange
    {
        void CancelChange();
    }

    public interface IDelayChange : IChange
    {
        public string ChangeID { get; }

    }
}