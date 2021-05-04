using System.Collections.Generic;

namespace CustomComponents.Changes
{
    public interface IChange_Optimize : IChange_Apply
    {
        void DoOptimization(List<IChange_Apply> current);
    }
}