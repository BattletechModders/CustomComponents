using System;
using System.Collections.Generic;
using System.Linq;
using BattleTech;
using BattleTech.Data;
using BattleTech.UI;
using Harmony;
using HBS.Extensions;


namespace CustomComponents
{
    public static class DefaultHelper
    {
        #region EXTENSIONS

        private static Dictionary<string, bool> defaults = new Dictionary<string, bool>();



        public static bool IsModuleFixed(this MechComponentRef item, MechDef mech)
        {
            Control.LogDebug(DType.FixedCheck, $"IsModuleFixed: {item.ComponentDefID}");
            if (!item.IsFixed)
            {
                Control.LogDebug(DType.FixedCheck, $"-- false: not fixed");
                return false;

            }


            if (mech.Chassis.FixedEquipment != null && mech.Chassis.FixedEquipment.Length > 0)
                foreach (var mref in mech.Chassis.FixedEquipment)
                {

                    if (mref.MountedLocation == item.MountedLocation && item.ComponentDefID == mref.ComponentDefID)
                    {
                        Control.LogDebug(DType.FixedCheck, $"-- true!");
                        return true;
                    }
                }
            Control.LogDebug(DType.FixedCheck, $"-- false: not really fixed");


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

        public static MechComponentDef GetComponentDef(string id, ComponentType type)
        {
            var dm = UnityGameInstance.BattleTechGame.DataManager;
            switch (type)
            {
                case ComponentType.Weapon:
                    dm.WeaponDefs.TryGet(id, out var weapon);
                    return weapon;
                case ComponentType.AmmunitionBox:
                    dm.AmmoBoxDefs.TryGet(id, out var ammobox);
                    return ammobox;
                case ComponentType.HeatSink:
                    dm.HeatSinkDefs.TryGet(id, out var hs);
                    return hs;
                case ComponentType.JumpJet:
                    dm.JumpJetDefs.TryGet(id, out var jj);
                    return jj;
                case ComponentType.Upgrade:
                    dm.UpgradeDefs.TryGet(id, out var upgrade);
                    return upgrade;
                default:
                    CustomComponents.Control.LogError($"Cannot find {id} of type {type}");
                    return null;
            }
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

            }
        }

        public static MechLabItemSlotElement CreateSlot(string id, ComponentType type)
        {
            var component_ref = new MechComponentRef(id, string.Empty, type, ChassisLocations.None, isFixed: true);
            component_ref.DataManager = UnityGameInstance.BattleTechGame.DataManager;
            component_ref.RefreshComponentDef();

            if (!component_ref.IsDefault())
            {
                Control.LogError($"CreateDefault: {id} not default or not exist");
            }

            var mechLab = MechLabHelper.CurrentMechLab.MechLab;
            if (mechLab.IsSimGame)
                component_ref.SetSimGameUID(mechLab.Sim.GenerateSimGameUID());

            return mechLab.CreateMechComponentItem(component_ref, false, ChassisLocations.None, mechLab);
        }

        public static MechLabItemSlotElement CreateSlot(MechComponentRef item)
        {
            return MechLabHelper.CurrentMechLab.MechLab.CreateMechComponentItem(item, false, ChassisLocations.None, MechLabHelper.CurrentMechLab.MechLab);
        }

        internal static void AddMechLab(MechComponentRef replace)
        {
            Control.LogDebug(DType.DefaultHandle, $"DefaultHelper.AddMechLab: adding {replace.ComponentDefID} to {replace.MountedLocation}");

            var target = MechLabHelper.CurrentMechLab.GetLocationWidget(replace.MountedLocation);

            if (target == null)
            {
                Control.LogDebug(DType.DefaultHandle, $"DefaultHelper: Cannot add - wrong location ");
                return;
            }

            var slot = CreateSlot(replace);
            slot.MountedLocation = replace.MountedLocation;
            target.OnAddItem(slot, false);
        }

        public static void AddMechLab(string id, ComponentType type, ChassisLocations location)
        {
            if (!MechLabHelper.CurrentMechLab.InMechLab)
                return;
            Control.LogDebug(DType.DefaultHandle, $"DefaultHelper.AddMechLab: adding {id} to {location}");

            var target = MechLabHelper.CurrentMechLab.GetLocationWidget(location);
            if (target == null)
            {
                Control.LogDebug(DType.DefaultHandle, $"DefaultHelper: Cannot add {id} to {location} - wrong location ");
                return;
            }

            var slot = CreateSlot(id, type);
            slot.MountedLocation = location;
            target.OnAddItem(slot, false);
        }

        public static void RemoveMechLab(ChassisLocations location, MechLabItemSlotElement slot)
        {
            if (slot == null)
                return;

            var helper = MechLabHelper.CurrentMechLab.GetLocationHelper(location);

            if (helper.LocalInventory.Contains(slot))
            {

                helper.widget.OnRemoveItem(slot, true);
                Control.LogDebug(DType.DefaultHandle, $"- removed");
                slot.thisCanvasGroup.blocksRaycasts = true;
                MechLabHelper.CurrentMechLab.MechLab.dataManager.PoolGameObject(MechLabPanel.MECHCOMPONENT_ITEM_PREFAB, slot.GameObject);
            }
            else
                Control.LogDebug(DType.DefaultHandle, $"DefaultHelper: Cannot remove {slot.ComponentRef.ComponentDefID} from {helper.LocationName} - wrong location ");
        }

        public static void RemoveMechLab(string id, ComponentType type, ChassisLocations location)
        {
            var helper = MechLabHelper.CurrentMechLab.GetLocationHelper(location);

            var remove = helper.LocalInventory.FirstOrDefault(e => e.ComponentRef.ComponentDefID == id);
            if (remove == null)
            {
                Control.LogDebug(DType.DefaultHandle, $"- not found");
            }
            else if (!remove.ComponentRef.IsDefault())
            {
                Control.LogDebug(DType.DefaultHandle, $"- not default");
            }
            else
            {
                helper.widget.OnRemoveItem(remove, true);
                Control.LogDebug(DType.DefaultHandle, $"- removed");
                remove.thisCanvasGroup.blocksRaycasts = true;
                MechLabHelper.CurrentMechLab.MechLab.dataManager.PoolGameObject(MechLabPanel.MECHCOMPONENT_ITEM_PREFAB, remove.GameObject);
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

            if (component.IsDefault() || component.HasFlag(CCF.AutoRepair))
            {
                Control.LogDebug(DType.DefaultHandle, $"AutoRepair: {component.Description.Id} ");
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

                Control.LogDebug(DType.DefaultHandle, $"Repair-Remove for {item.ComponentRef.ComponentDefID}");
                if (repair_state)
                {
                    foreach (var validator in item.ComponentRef.Def.GetComponents<IOnItemGrabbed>())
                    {
                        Control.LogDebug(DType.DefaultHandle, $" -  {validator.GetType()}");
                        validator.OnItemGrabbed(item, mechlab, repair_widget.loadout.Location);
                    }
                    mechlab.ForceItemDrop(item);
                }
            }
            catch (Exception e)
            {
                Control.LogError($"ERROR", e);
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

            Control.LogDebug(DType.DefaultHandle, $"Removing {component.Description.Id} ");

            var mechlab = widget.parentDropTarget as MechLabPanel;

            strip_state = true;

            foreach (var validator in component.GetComponents<IOnItemGrab>())
            {
                Control.LogDebug(DType.DefaultHandle, $"- {validator.GetType()}");
                strip_state = validator.OnItemGrab(item, mechlab, out _);
                if (!strip_state)
                {
                    Control.LogDebug(DType.DefaultHandle, $"-- Canceled");
                    return true;
                }
            }

            return widget.OnRemoveItem(item, validate);
        }

        internal static void ForceItemDropStrip(this MechLabPanel mechlab, MechLabItemSlotElement item)
        {
            Control.LogDebug(DType.DefaultHandle, $"Dropping {item.ComponentRef.Def.Description.Id} ");
            if (strip_state)
            {
                foreach (var validator in item.ComponentRef.Def.GetComponents<IOnItemGrabbed>())
                {
                    validator.OnItemGrabbed(item, mechlab, strip_widget.loadout.Location);
                }
                mechlab.ForceItemDrop(item);
            }
        }

        public static MechComponentRef[] ClearInventory(MechDef source, SimGameState state)
        {

            Control.LogDebug(DType.ClearInventory, "Clearing Inventory");

            var list = source.Inventory.ToList();
            var result_list = source.Inventory
                .Where(i => i.IsFixed || i.IsDefault() ||
                            (i.SimGameUID != null && i.SimGameUID.StartsWith("FixedEquipment")))
                .Select(i =>
                    {
                        var result = new MechComponentRef(i, i.SimGameUID);
                        result.DataManager = i.DataManager;
                        result.RefreshComponentDef();
                        if (i.IsFixed || i.IsDefault() ||
                           i.SimGameUID != null && i.SimGameUID.StartsWith("FixedEquipment"))
                            result.SetData(i.HardpointSlot, ComponentDamageLevel.Functional, true);
                        return result;
                    }
                    ).ToList();


            for (int i = list.Count - 1; i >= 0; i--)
            {
                Control.LogDebug(DType.ClearInventory, $"- {list[i].ComponentDefID} - {(list[i].Def == null ? "NULL" : list[i].SimGameUID)}");

                if (list[i].Def == null)
                {
                    list[i].RefreshComponentDef();
                    list[i].SetSimGameUID(state.GenerateSimGameUID());
                }

                if (list[i].IsFixed || list[i].IsDefault() || list[i].SimGameUID != null && list[i].SimGameUID.StartsWith("FixedEquipment"))
                {
                    Control.LogDebug(DType.ClearInventory, "-- fixed - skipping");
                    continue;
                }

                foreach (var clear in list[i].GetComponents<IClearInventory>())
                {
                    clear.ClearInventory(source, result_list, state, list[i]);
                }
            }

            foreach (var clearInventoryDelegate in Validator.clear_inventory)
            {
                clearInventoryDelegate(source, result_list, state);
            }

            Control.LogDebug(DType.ClearInventory, $"- setting guids");
            foreach (var item in result_list)
            {
                if (string.IsNullOrEmpty(item.SimGameUID))
                    item.SetSimGameUID(state.GenerateSimGameUID());
                Control.LogDebug(DType.ClearInventory, $"-- {item.ComponentDefID} - {item.SimGameUID}");
            }

            return result_list.ToArray();
        }


        #endregion

    }
}