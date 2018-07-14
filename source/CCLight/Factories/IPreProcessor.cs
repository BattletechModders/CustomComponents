using System.Collections.Generic;
using BattleTech;

namespace CustomComponents
{
    public interface IPreProcessor
    {
        IEnumerable<ICustomComponent> PreProcess(MechComponentDef target, Dictionary<string, object> values);
    }
}