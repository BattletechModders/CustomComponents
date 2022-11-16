using BattleTech.UI;
using BattleTech;

namespace CustomComponents;

public interface IPreValidateDrop
{
    string PreValidateDrop(MechLabItemSlotElement item, ChassisLocations location);
}

public delegate string PreValidateDropDelegate(MechLabItemSlotElement item, ChassisLocations location);