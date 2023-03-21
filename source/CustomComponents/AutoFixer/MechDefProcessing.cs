using System.Collections.Generic;
using BattleTech;

namespace CustomComponents;

public class MechDefProcessing
{
    public static readonly MechDefProcessing Instance = new();

    private readonly List<IMechDefProcessor> _processors = new();
    public void Register(IMechDefProcessor processor)
    {
        _processors.Add(processor);
    }

    internal void Process(List<MechDef> mechDefs)
    {
        foreach (var mechDef in mechDefs)
        {
            if (mechDef.ChassisID == null)
            {
                mechDef.Refresh();
            }
        }

        foreach (var processor in _processors)
        {
            processor.ProcessMechDefs(mechDefs);
        }
    }
}