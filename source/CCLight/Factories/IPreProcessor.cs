using System.Collections.Generic;
using BattleTech;

namespace CustomComponents
{
    public interface IPreProcessor
    {
        void PreProcess(MechComponentDef target, Dictionary<string, object> values);
    }
}