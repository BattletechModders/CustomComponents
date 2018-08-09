using System.Collections.Generic;

namespace CustomComponents
{
    public interface ICustomFactory
    {
        ICustom Create(object target, Dictionary<string, object> values);
    }
}