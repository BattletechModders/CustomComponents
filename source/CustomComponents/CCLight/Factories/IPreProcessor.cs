using System.Collections.Generic;

namespace CustomComponents;

internal interface IPreProcessor
{
    void PreProcess(object target, Dictionary<string, object> values);
}