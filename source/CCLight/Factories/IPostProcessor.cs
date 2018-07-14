using System.Collections.Generic;
using BattleTech;

namespace CustomComponents
{
    public interface IPostProcessor
    {
        void PostProcess(MechComponentDef target, Dictionary<string, object> values);
    }
}