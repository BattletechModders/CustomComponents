using System.Collections.Generic;

namespace CustomComponents;

internal interface IPostProcessor
{
    void PostProcess(object target, Dictionary<string, object> values);
}