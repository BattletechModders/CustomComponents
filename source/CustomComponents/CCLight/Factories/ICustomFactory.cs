using System.Collections.Generic;

namespace CustomComponents;

public interface ICustomFactory
{
    string CustomName { get; }
    IEnumerable<ICustom> Create(object target, Dictionary<string, object> values);
}