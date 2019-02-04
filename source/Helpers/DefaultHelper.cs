//#undef CCDEBUG

using System;
using System.Collections.Generic;
using System.Linq;
using BattleTech;
using BattleTech.Data;
using BattleTech.UI;
using Harmony;


namespace CustomComponents
{
    public static class DefaultHelper
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

        public static bool IsModuleFixed(this MechComponentRef item, MechDef mech)
        {
#if CCDEBUG1
            Control.Logger.LogDebug($"IsModuleFixed: {item.ComponentDefID}");
#endif
            if (!item.IsFixed)
            {
#if CCDEBUG1
                Control.Logger.LogDebug($"-- false: not fixed");
#endif

                return false;

            }

            
            if (mech.Chassis.FixedEquipment != null && mech.Chassis.FixedEquipment.Length > 0)
                foreach (var mref in mech.Chassis.FixedEquipment)
                {

                    if (mref.MountedLocation == item.MountedLocation && item.ComponentDefID == mref.ComponentDefID)
                    {
#if CCDEBUG1
                        Control.Logger.LogDebug($"-- true!");
#endif
                        return true;
                    }
                }
#if CCDEBUG1
            Control.Logger.LogError($"-- false: not really fixed");
#endif

            return false;
        }

        #endregion


        public static MechComponentRef CreateRef(string id, ComponentType type, DataManager datamanager, SimGameState state)
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
                r.SetData(location, -1, ComponentDamageLevel.Functional, true);
                var inv = mech.Inventory.ToList();
                inv.Add(r);
                mech.SetInventory(inv.ToArray());
#if CCDEBUG1
                var flag = r.GetComponent<Flags>();
                Control.Logger.LogDebug($"AddInventory: {r.Def.Description.Id} isdefult:{r.Def.IsDefault()} isfixed:{r.IsFixed} isFlag:{flag == null}");
                if (flag == null)
                {
                    Control.Logger.LogDebug($"-- NO FLAGS!");
                }
                else
                {
                    Control.Logger.LogDebug($"-- default: {flag.IsSet("default")} isdefault:{flag.Default}");
                }
                foreach (var simpleCustomComponent in r.GetComponents<SimpleCustomComponent>())
                {
                    Control.Logger.LogDebug($"-- {simpleCustomComponent}");
                }
#endif
            }
        }

        public static MechLabItemSlotElement CreateSlot(string id, ComponentType type, MechLabPanel mechLab)
        {
            var component_ref = new MechComponentRef(id, string.Empty, type, ChassisLocations.None, isFixed: true);

            if (!component_ref.IsDefault())
            {
                Control.Logger.LogError($"CreateDefault: {id} not default or not exist");
            }

            if (mechLab.IsSimGame)
                component_ref.SetSimGameUID(mechLab.Sim.GenerateSimGameUID());

            return mechLab.CreateMechComponentItem(component_ref, false, ChassisLocations.None, mechLab);
        }

        public static MechLabItemSlotElement CreateSlot(MechComponentRef item, MechLabPanel mechLab)
        {
            return mechLab.CreateMechComponentItem(item, false, ChassisLocations.None, mechLab);
        }

        internal static void AddMechLab(MechComponentRef replace, MechLabHelper mechLab)
        {
            Control.Logger.LogDebug($"DefaultHelper.AddMechLab: adding {replace.ComponentDefID} to {replace.MountedLocation}");

            var target = mechLab.GetLocationWidget(replace.MountedLocation);

            if (target == null)
            {
                Control.Logger.LogError($"DefaultHelper: Cannot add - wrong location ");
                return;
            }

            var slot = CreateSlot(replace, mechLab.MechLab);
            slot.MountedLocation = replace.MountedLocation;
            target.OnAddItem(slot, false);
        }

        public static void AddMechLab(string id, ComponentType type, MechLabHelper mechLab, ChassisLocations location)
        {
            Control.Logger.LogDebug($"DefaultHelper.AddMechLab: adding {id} to {location}");

            var target = mechLab.GetLocationWidget(location);
            if (target == null)
            {
                Control.Logger.LogError($"DefaultHelper: Cannot add {id} to {location} - wrong location ");
                return;
            }

            var slot = CreateSlot(id, type, mechLab.MechLab);
            slot.MountedLocation = location;
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

        internal static bool OnRemoveItemRepair(this MechLabLocationWidget widget, IMechLabDraggableItem item, bool validate)
        {
            var component = item.ComponentRef.Def;

            repair_state = true;
            repair_widget = widget;

            if (component.Is<Flags>(out var f) && f.AutoRepair)
            {
                Control.Logger.LogDebug($"AutoRepair: {component.Description.Id} ");
                item.ComponentRef.DamageLevel = ComponentDamageLevel.Penalized;
                var t = Traverse.Create(item).Method("RefreshDamageOverlays").GetValue();
                item.RepairComponent(true);
                repair_state = false;
                return true;
            }

            repair_state = true;
            var mechlab = widget.parentDropTarget as MechLabPanel;
            foreach (var validator in component.GetComponents<IOnItemGrab>())
            {
                repair_state = validator.OnItemGrab(item, mechlab, out _);
                if (!repair_state)
                    return true;
            }

            return widget.OnRemoveItem(item, validate);
        }


        internal static void ForceItemDropRepair(this MechLabPanel mechlab, MechLabItemSlotElement item)
        {
            try
            {

                Control.Logger.Log($"Repair-Remove for {item.ComponentRef.ComponentDefID}");
                if (repair_state)
                {
                    foreach (var validator in item.ComponentRef.Def.GetComponents<IOnItemGrabbed>())
                    {
                        Control.Logger.Log($" -  {validator.GetType()}");
                        validator.OnItemGrabbed(item, mechlab, repair_widget);
                    }
                    mechlab.ForceItemDrop(item);
                }
            }
            catch (Exception e)
            {
                Control.Logger.Log($"ERROR", e);
            }
        }
        #endregion

        #region strip

        private static bool strip_state;
        private static MechLabLocationWidget strip_widget;

        internal static bool OnRemoveItemStrip(this MechLabLocationWidget widget, IMechLabDraggableItem item,
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

        internal static void ForceItemDropStrip(this MechLabPanel mechlab, MechLabItemSlotElement item)
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
#if CCDEBUG
            Control.Logger.LogDebug("Clearing Inventory");
#endif
            var list = source.Inventory.ToList();

            var result_list = list.Where(i => i.IsFixed).ToList();

            for (int i = list.Count - 1; i >= 0; i--)
            {
#if CCDEBUG
                Control.Logger.LogDebug($"- {list[i].ComponentDefID} - {(list[i].Def == null ? "NULL" : list[i].SimGameUID)}");
#endif
                if (list[i].Def == null)
                {
                    list[i].RefreshComponentDef();
                    list[i].SetSimGameUID(state.GenerateSimGameUID());
                }

                if (list[i].IsFixed)
                {
#if CCDEBUG
                    Control.Logger.LogDebug("-- fixed - skipping");
#endif
                    continue;
                }

                foreach (var clear in list[i].GetComponents<IClearInventory>())
                {
                    clear.ClearInventory(source, result_list, state, list[i]);
                }
            }

            foreach (var item in result_list)
            {
                if(string.IsNullOrEmpty(item.SimGameUID))
                    item.SetSimGameUID(state.GenerateSimGameUID());
#if CCDEBUG
                    Control.Logger.LogDebug($"- {item.ComponentDefID} - {item.SimGameUID}");
#endif
            }

            return result_list.ToArray();
        }
        #endregion

    }
}