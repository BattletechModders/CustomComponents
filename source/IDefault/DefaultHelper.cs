using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using BattleTech;
using BattleTech.UI;
using Harmony;

namespace CustomComponents
{
    internal static class DefaultHelper
    {
        internal static void RemoveDefault(string defaultID, MechDef mech, ChassisLocations location, ComponentType type)
        {
            var item = mech.Inventory.FirstOrDefault(i => i.MountedLocation == location && i.ComponentDefID == defaultID);
            if (item != null)
            {
                var inv = mech.Inventory.ToList();
                inv.Remove(item);
                mech.SetInventory(inv.ToArray());
            }
        }

        internal static void AddDefault(string defaultID, MechDef mech, ChassisLocations location, ComponentType type, SimGameState state)
        {
            var r = CreateHelper.Ref(defaultID, type, mech.DataManager, state);
            if (r != null)
            {
                r.SetData(location, -1, ComponentDamageLevel.Functional);
                var inv = mech.Inventory.ToList();
                inv.Add(r);
                mech.SetInventory(inv.ToArray());
            }
        }

        #region repair
        public static bool OnRemoveItemRepair(this MechLabLocationWidget widget, IMechLabDraggableItem item, bool validate)
        {
            void do_Repair()
            {
                item.ComponentRef.DamageLevel = ComponentDamageLevel.Penalized;
                var t = Traverse.Create(item).Method("RefreshDamageOverlays").GetValue();
                item.RepairComponent(true);
            }

            var component = item.ComponentRef.Def;

            if (component.Is<Flags>(out var f) && f.AutoRepair)
            {
                do_Repair();
                return true;
            }

            var mechlab = widget.parentDropTarget as MechLabPanel;

            if (component.Is<AutoReplace>(out var replace) && !string.IsNullOrEmpty(replace.ReplaceID) && replace.ReplaceID != item.ComponentRef.ComponentDefID)
            {
                var new_ref = CreateHelper.Ref(replace.ReplaceID, item.ComponentRef.ComponentDefType, mechlab.dataManager, mechlab.sim);
                if (new_ref != null)
                {
                    var new_item = CreateHelper.Slot(mechlab, new_ref, widget.loadout.Location);
                    widget.OnAddItem(new_item, false);
                }
            }

            if (component.Is<AutoLinked>(out var linked))
            {
                linked.RemoveLinked(item, mechlab);
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
        #endregion

        #region strip

        public static bool OnRemoveItemStrip(this MechLabLocationWidget widget, IMechLabDraggableItem item,
            bool validate)
        {
            var component = item.ComponentRef.Def;

            Control.Logger.LogDebug($"==== removing {component.Description.Id} ");

            var mechlab = widget.parentDropTarget as MechLabPanel;

            if (component.Is<Flags>(out var f) && f.CannotRemove)
            {
                Control.Logger.LogDebug($"ICannotRemove - cancel");
                return true;
            }

            if (component.Is<AutoReplace>(out var replace) && !string.IsNullOrEmpty(replace.ReplaceID) && replace.ReplaceID != item.ComponentRef.ComponentDefID)
            {
                Control.Logger.LogDebug($"IDefaultRepace - search for replace");
                var new_ref = CreateHelper.Ref(replace.ReplaceID, item.ComponentRef.ComponentDefType, mechlab.dataManager, mechlab.sim);
                if (new_ref != null)
                {
                    Control.Logger.LogDebug($"IDefaultRepace - adding");
                    var new_item = CreateHelper.Slot(mechlab, new_ref, widget.loadout.Location);
                    widget.OnAddItem(new_item, false);
                }
                else
                    Control.Logger.LogDebug($"IDefaultRepace - not found");

            }

            if (component.Is<AutoLinked>(out var linked))
            {
                Control.Logger.LogDebug($"IAutoLinked - remove linked");
                linked.RemoveLinked(item, mechlab);
            }

            return widget.OnRemoveItem(item, validate);
        }

        public static void ForceItemDropStrip(this MechLabPanel mechlab, MechLabItemSlotElement item)
        {
            var component = item.ComponentRef.Def;
            if (component.Is<Flags>(out var f) && f.CannotRemove)
                return;


            mechlab.ForceItemDrop(item);
        }

        public static MechComponentRef[] ClearInventory(MechDef source, SimGameState state)
        {
            Control.Logger.LogDebug("Clearing Inventory");

            var list = source.Inventory.ToList();

            //TODO: Remove in light
            //foreach (var item in list)
            //if (item.Def == null)
            //    item.RefreshComponentDef();

            var result_list = list.Where(i => i.Is<Flags>(out var f) && f.CannotRemove).ToList();

            for (int i = list.Count - 1; i >= 0; i--)
            {
                Control.Logger.LogDebug($"- {list[i].ComponentDefID} - {(list[i].Def == null ? "NULL" : list[i].SimGameUID)}");
                if (list[i].Def == null)
                {
                    list[i].RefreshComponentDef();
                    list[i].SetSimGameUID(state.GenerateSimGameUID());
                }

                if (list[i].Is<Flags>(out var f) && f.CannotRemove)
                {
                    Control.Logger.LogDebug("-- Default - skipping");
                    continue;
                }

                if (list[i].Is<AutoReplace>(out var replace))
                {
                    var ref_item = CreateHelper.Ref(replace.ReplaceID, list[i].ComponentDefType, list[i].DataManager, state);
                    ref_item.SetData(list[i].MountedLocation, list[i].HardpointSlot, list[i].DamageLevel);
                    ref_item.SetSimGameUID(state.GenerateSimGameUID());
                    result_list.Add(ref_item);
                    Control.Logger.LogDebug($"-- Replace with {ref_item.ComponentDefID} - {ref_item.SimGameUID}");
                }


                if (list[i].Is<AutoLinked>(out var link) && link.Links != null)
                {
                    foreach (var l in link.Links)
                    {
                        result_list.RemoveAll(item => item.ComponentDefID == l.ApendixID && item.MountedLocation == l.Location);
                    }
                }
            }

            foreach (var item in result_list)
            {
                item.SetSimGameUID(state.GenerateSimGameUID());
                Control.Logger.LogDebug($"- {item.ComponentDefID} - {item.SimGameUID}");
            }

            return result_list.ToArray();
        }

        #endregion

    }
}