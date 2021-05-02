using System.Collections.Generic;

namespace CustomComponents.Changes
{
    public interface IOptimizableChange : IInvChange
    {
        void DoOptimization(List<IInvChange> current);
    }
}