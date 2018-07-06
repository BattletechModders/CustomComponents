using BattleTech;
using BattleTech.UI;

namespace CustomComponents
{
    internal static class RemoveHelper
    {
        public static bool OnRemoveItem(this MechLabLocationWidget widget, IMechLabDraggableItem item, bool validate)
        {
            void do_Repair()
            {
                item.ComponentRef.DamageLevel = ComponentDamageLevel.Penalized;
                item.RepairComponent(true);
            }

            var component = item.ComponentRef.Def;

            if (component is IAutoRepair)
            {
                do_Repair();
                return true;
            }

            var mechlab = widget.parentDropTarget as MechLabPanel;

            if (component is IDefaultRepace replace && !string.IsNullOrEmpty(replace.DefaultID)  && replace.DefaultID != item.ComponentRef.ComponentDefID)
            {
                var new_ref = CreateHelper.Ref(replace.DefaultID, item.ComponentRef.ComponentDefType, mechlab.dataManager);
                if (new_ref != null)
                {
                    var new_item = CreateHelper.Slot(mechlab, new_ref, widget.loadout.Location);
                    widget.OnAddItem(new_item, false);
                }
            }

            if (component is IAutoLinked linked)
            {
                LinkedController.RemoveLinked(mechlab, item, linked);
            }

            return widget.OnRemoveItem(item, validate);
            //mechlab.ValidateLoadout(false);
        }

        public static void ForceItemDropRepair(this MechLabPanel mechlab, MechLabItemSlotElement item)
        {
            if (item.ComponentRef.DamageLevel != ComponentDamageLevel.Destroyed)
                return;

            mechlab.ForceItemDrop(item);
        }


        public static bool OnRemoveItemStrip(this MechLabLocationWidget widget, IMechLabDraggableItem item,
            bool validate)
        {
            var component = item.ComponentRef.Def;

            Control.Logger.LogDebug($"==== removing {component.Description.Id} ");
            
            var mechlab = widget.parentDropTarget as MechLabPanel;

            if (component is ICannotRemove)
            {
                Control.Logger.LogDebug($"ICannotRemove - cancel");
                return true;
            }

            if (component is IDefaultRepace replace && !string.IsNullOrEmpty(replace.DefaultID) && replace.DefaultID != item.ComponentRef.ComponentDefID)
            {
                Control.Logger.LogDebug($"IDefaultRepace - search for replace");
                var new_ref = CreateHelper.Ref(replace.DefaultID, item.ComponentRef.ComponentDefType, mechlab.dataManager);
                if (new_ref != null)
                {
                    Control.Logger.LogDebug($"IDefaultRepace - adding");
                    var new_item = CreateHelper.Slot(mechlab, new_ref, widget.loadout.Location);
                    widget.OnAddItem(new_item, false);
                }
                else
                    Control.Logger.LogDebug($"IDefaultRepace - not found");

            }

            if (component is IAutoLinked linked)
            {
                Control.Logger.LogDebug($"IAutoLinked - remove linked");
                LinkedController.RemoveLinked(mechlab, item, linked);
            }

            return widget.OnRemoveItem(item, validate);
        }

        public static void ForceItemDropStrip(this MechLabPanel mechlab, MechLabItemSlotElement item)
        {
            var component = item.ComponentRef.Def;
            if (component is ICannotRemove)
                return;


            mechlab.ForceItemDrop(item);
        }

    }
}