using System.Collections.Generic;

namespace CustomComponents
{
    public interface ICustomFactory
    {
        IEnumerable<ICustom> Create(object target, Dictionary<string, object> values);
    }
}