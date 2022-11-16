using BattleTech.UI;

namespace CustomComponents;

public interface IOnItemGrab
{
    bool OnItemGrab(IMechLabDraggableItem item, MechLabPanel mechLab, out string error);
}