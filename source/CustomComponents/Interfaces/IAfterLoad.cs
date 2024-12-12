using System;
using System.Collections.Generic;

namespace CustomComponents;

[Obsolete("Use IOnLoaded")]
public interface IAfterLoad
{
    void OnLoaded(Dictionary<string, object> values);
}