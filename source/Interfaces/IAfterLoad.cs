using System.Collections.Generic;

namespace CustomComponents
{
    public interface IAfterLoad
    {
        void OnLoaded(Dictionary<string, object> values);
    }
}
