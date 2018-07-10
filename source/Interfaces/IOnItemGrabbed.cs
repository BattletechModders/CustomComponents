using BattleTech.UI;

namespace CustomComponents
{
    public interface IOnItemGrabbed
    {
        void OnItemGrabbed(IMechLabDraggableItem item, MechLabPanel mechLab, MechLabLocationWidget widget);
    }
}