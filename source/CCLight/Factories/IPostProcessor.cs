using System.Collections.Generic;
using BattleTech;

namespace CustomComponents
{
    public interface IPostProcessor
    {
        IEnumerable<ICustomComponent> PostProcess(MechComponentDef target, Dictionary<string, object> values);
    }
}