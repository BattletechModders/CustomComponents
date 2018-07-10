using BattleTech;
using BattleTech.UI;
using System.Linq;

namespace CustomComponents
{
    public class Link
    {
        public ChassisLocations Location;
        public string ApendixID;
        public ComponentType BaseType;
    }

    [CustomComponent("Linked")]
    public class AutoLinked : SimpleCustomComponent, IOnItemGrabbed
    {
        public Link[] Links { get; set; }

        public void OnItemGrabbed(IMechLabDraggableItem item, MechLabPanel mechLab, MechLabLocationWidget w)
        {
            if (Links != null && Links.Length > 0)
            {
                RemoveLinked(item, mechLab);
            }
        }

        public void RemoveLinked(IMechLabDraggableItem item, MechLabPanel mechLab)
        {
            var helper = new MechLabHelper(mechLab);
            foreach (var r_link in Links)
            {
                var target = helper.GetLocationWidget(r_link.Location);
                if (target != null)
                {
                    Control.Logger.LogDebug($"{r_link.ApendixID} from {r_link.Location}");
                    var location = new LocationHelper(target);

                    var remove = location.LocalInventory.FirstOrDefault(e =>
                        e?.ComponentRef?.ComponentDefID == r_link.ApendixID);

                    if (remove != null)
                    {
                        target.OnRemoveItem(remove, true);
                        if (remove.ComponentRef.Is<Flags>(out var f) && f.Default)
                        {
                            remove.thisCanvasGroup.blocksRaycasts = true;
                            mechLab.dataManager.PoolGameObject(MechLabPanel.MECHCOMPONENT_ITEM_PREFAB, item.GameObject);
                        }
                        else
                        {
                            Control.Logger.LogDebug($"removed");
                            mechLab.ForceItemDrop(remove);
                            helper.SetDragItem(item as MechLabItemSlotElement);
                        }
                    }
                    else
                        Control.Logger.LogDebug($"not found");
                }
            }
        }
    }
}