using System.Collections.Generic;
using BattleTech;

namespace CustomComponents;

public interface IMechDefProcessor
{
    void ProcessMechDefs(List<MechDef> mechDefs);
}