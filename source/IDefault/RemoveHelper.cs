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

            if (component is ICategory cat)
                if (cat.CategoryDescriptor.AutoRepair)
                {
                    do_Repair();
                    return true;
                }
                else if (!string.IsNullOrEmpty(cat.CategoryDescriptor.Default))
                {
                    if (cat.CategoryDescriptor.Default == item.ComponentRef.ComponentDefID)
                    {
                        do_Repair();
                        return true;
                    }

                    var new_ref = CreateHelper.Ref(cat.CategoryDescriptor.Default, item.ComponentRef.ComponentDefType, mechlab.dataManager);
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
            var mechlab = widget.parentDropTarget as MechLabPanel;

            if (component is ICannotRemove)
                return true;

            if (component is ICategory cat)
            {
                if (!cat.CategoryDescriptor.AllowRemove)
                    return true;
                else if (!string.IsNullOrEmpty(cat.CategoryDescriptor.Default))
                {
                    if (cat.CategoryDescriptor.Default == item.ComponentRef.ComponentDefID)
                        return true;

                    var new_ref = CreateHelper.Ref(cat.CategoryDescriptor.Default, item.ComponentRef.ComponentDefType, mechlab.dataManager);
                    if (new_ref != null)
                    {
                        var new_item = CreateHelper.Slot(mechlab, new_ref, widget.loadout.Location);
                        widget.OnAddItem(new_item, false);
                    }
                }
            }

            if (component is IAutoLinked linked)
            {
                LinkedController.RemoveLinked(mechlab, item, linked);
            }

            return widget.OnRemoveItem(item, validate);
        }

        public static void ForceItemDropStrip(this MechLabPanel mechlab, MechLabItemSlotElement item)
        {
            var component = item.ComponentRef.Def;
            if (component is ICannotRemove)
                return;

            if (component is ICategory cat)
            {
                if (!cat.CategoryDescriptor.AllowRemove)
                    return;
                else if (!string.IsNullOrEmpty(cat.CategoryDescriptor.Default))
                {
                    if (cat.CategoryDescriptor.Default == component.Description.Id)
                        return;
                }
            }
            mechlab.ForceItemDrop(item);
        }

    }
}