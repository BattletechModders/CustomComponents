using System.Collections.Generic;

namespace CustomComponents;

public interface IPreProcessor
{
    void PreProcess(object target, Dictionary<string, object> values);
}