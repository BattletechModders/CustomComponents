using BattleTech.UI;

namespace CustomComponents
{
    public interface IOnItemGrab
    {
        bool OnItemGrab(IMechLabDraggableItem item, MechLabPanel ___mechLab, out string error);
    }
}