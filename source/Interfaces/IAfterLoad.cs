using System;
using System.Collections.Generic;
using System.Text;

namespace CustomComponents
{
    public interface IAfterLoad
    {
        void OnLoaded(Dictionary<string, object> values);
    }
}
