using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using BattleTech;
using BattleTech.Data;
using BattleTech.UI;
using Harmony;

namespace CustomComponents
{
    internal static class DefaultHelper
    {
        #region EXTENSIONS

        public static bool IsDefault(this MechComponentDef cdef)
        {
            return cdef.Is<Flags>(out var f) && f.Default;
        }
        public static bool IsDefault(this MechComponentRef cref)
        {
            return cref.Is<Flags>(out var f) && f.Default;
        }

        #endregion


        private static MechComponentRef CreateRef(string id, ComponentType type, DataManager datamanager, SimGameState state)
        {
            var component_ref = new MechComponentRef(id, string.Empty, type, ChassisLocations.None);
            component_ref.DataManager = datamanager;
            component_ref.RefreshComponentDef();
            if (state != null)
                component_ref.SetSimGameUID(state.GenerateSimGameUID());
            return component_ref;
        }

        public static void RemoveInventory(string defaultID, MechDef mech, ChassisLocations location, ComponentType type)
        {
            var item = mech.Inventory.FirstOrDefault(i => i.MountedLocation == location && i.ComponentDefID == defaultID);
            if (item != null)
            {
                var inv = mech.Inventory.ToList();
                inv.Remove(item);
                mech.SetInventory(inv.ToArray());
            }
        }

        public static void AddInventory(string defaultID, MechDef mech, ChassisLocations location, ComponentType type, SimGameState state)
        {
            var r = CreateRef(defaultID, type, mech.DataManager, state);
            if (r != null)
            {
                r.SetData(location, -1, ComponentDamageLevel.Functional);
                var inv = mech.Inventory.ToList();
                inv.Add(r);
                mech.SetInventory(inv.ToArray());
            }
        }

        public static MechLabItemSlotElement CreateSlot(string id, ComponentType type, MechLabPanel mechLab)
        {
            var component_ref = new MechComponentRef(id, string.Empty, type, ChassisLocations.None);
            component_ref.DataManager = mechLab.dataManager;

            if (!component_ref.IsDefault())
            {
                Control.Logger.LogError($"CreateDefault: {id} not default or not exist");
            }

            if (mechLab.IsSimGame)
                component_ref.SetSimGameUID(mechLab.Sim.GenerateSimGameUID());

            return mechLab.CreateMechComponentItem(component_ref, false, ChassisLocations.None, mechLab);
        }

        public static void AddMechLab(string id, ComponentType type, MechLabHelper mechLab, ChassisLocations location)
        {

            var target = mechLab.GetLocationWidget(location);
            if (target == null)
            {
                Control.Logger.LogError($"DefaultHelper: Cannot add {id} to {location} - wrong location ");
                return;
            }

            var slot = CreateSlot(id, type, mechLab.MechLab);
            target.OnAddItem(slot, false);
        }

        public static void RemoveMechLab(string id, ComponentType type, MechLabHelper mechLab, ChassisLocations location)
        {
            var widget = mechLab.GetLocationWidget(location);
            if (widget == null)
            {
                Control.Logger.LogError($"DefaultHelper: Cannot remove {id} from {location} - wrong location ");
                return;
            }
            var helper = new LocationHelper(widget);

            var remove = helper.LocalInventory.FirstOrDefault(e => e.ComponentRef.ComponentDefID == id);
            if (remove == null)
            {
                Control.Logger.LogDebug($"- not found");
            }
            else if (!remove.ComponentRef.IsDefault())
            {
                Control.Logger.LogDebug($"- not default");
            }
            else
            {
                widget.OnRemoveItem(remove, true);
                Control.Logger.LogDebug($"- removed");
                remove.thisCanvasGroup.blocksRaycasts = true;
                mechLab.MechLab.dataManager.PoolGameObject(MechLabPanel.MECHCOMPONENT_ITEM_PREFAB, remove.GameObject);
            }
        }

        #region repair

        private static bool repair_state;
        private static MechLabLocationWidget repair_widget;

        public static bool OnRemoveItemRepair(this MechLabLocationWidget widget, IMechLabDraggableItem item, bool validate)
        {
            var component = item.ComponentRef.Def;

            if (component.Is<Flags>(out var f) && f.AutoRepair)
            {
                Control.Logger.LogDebug($"AutoRepair: {component.Description.Id} ");
                item.ComponentRef.DamageLevel = ComponentDamageLevel.Penalized;
                var t = Traverse.Create(item).Method("RefreshDamageOverlays").GetValue();
                item.RepairComponent(true);
                repair_state = false;
                return true;
            }

            repair_widget = widget;


            var mechlab = widget.parentDropTarget as MechLabPanel;
            repair_state = true;
            foreach (var validator in component.GetComponents<IOnItemGrab>())
            {
                repair_state = validator.OnItemGrab(item, mechlab, out _);
                if (!repair_state)
                    return true;
            }
            return widget.OnRemoveItem(item, validate);
        }


        public static void ForceItemDropRepair(this MechLabPanel mechlab, MechLabItemSlotElement item)
        {
            if (!repair_state)
            {
                foreach (var validator in item.ComponentRef.Def.GetComponents<IOnItemGrabbed>())
                {
                    validator.OnItemGrabbed(item, mechlab, strip_widget);
                }
                mechlab.ForceItemDrop(item);
            }
        }
        #endregion

        #region strip

        private static bool strip_state;
        private static MechLabLocationWidget strip_widget;

        public static bool OnRemoveItemStrip(this MechLabLocationWidget widget, IMechLabDraggableItem item,
            bool validate)
        {
            var component = item.ComponentRef.Def;
            strip_widget = widget;

            Control.Logger.LogDebug($"Removing {component.Description.Id} ");

            var mechlab = widget.parentDropTarget as MechLabPanel;

            strip_state = true;

            foreach (var validator in component.GetComponents<IOnItemGrab>())
            {
                Control.Logger.LogDebug($"- {validator.GetType()}");
                strip_state = validator.OnItemGrab(item, mechlab, out _);
                if (!strip_state)
                {
                    Control.Logger.LogDebug($"-- Canceled");
                    return true;
                }
            }

            return widget.OnRemoveItem(item, validate);
        }

        public static void ForceItemDropStrip(this MechLabPanel mechlab, MechLabItemSlotElement item)
        {
            Control.Logger.LogDebug($"Dropping {item.ComponentRef.Def.Description.Id} ");
            if (strip_state)
            {
                foreach (var validator in item.ComponentRef.Def.GetComponents<IOnItemGrabbed>())
                {
                    validator.OnItemGrabbed(item, mechlab, strip_widget);
                }
                mechlab.ForceItemDrop(item);
            }
        }

        public static MechComponentRef[] ClearInventory(MechDef source, SimGameState state)
        {
            Control.Logger.LogDebug("Clearing Inventory");

            var list = source.Inventory.ToList();

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
                    var ref_item = CreateRef(replace.ComponentDefId, list[i].ComponentDefType, list[i].DataManager, state);
                    ref_item.SetData(list[i].MountedLocation, list[i].HardpointSlot, list[i].DamageLevel);
                    ref_item.SetSimGameUID(state.GenerateSimGameUID());
                    result_list.Add(ref_item);
                    Control.Logger.LogDebug($"-- Replace with {ref_item.ComponentDefID} - {ref_item.SimGameUID}");
                }


                if (list[i].Is<AutoLinked>(out var link) && link.Links != null)
                {
                    foreach (var l in link.Links)
                    {
                        result_list.RemoveAll(item => item.ComponentDefID == l.ComponentDefId && item.MountedLocation == l.Location);
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