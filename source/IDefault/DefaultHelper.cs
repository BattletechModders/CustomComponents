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
        private enum atype { addwith, removewith, replacebase, replacedef }

        private class defaction
        {
            public atype type;
            public ChassisLocations location;
            public MechComponentRef main;
            public string defid;
            public bool need_remove;
        }

        private static MechLabPanel _mechLab = null;
        private static List<defaction> actions = new List<defaction>();

        public static void SetMechLab(MechLabPanel mechLab)
        {
            actions.Clear();
            _mechLab = mechLab;
        }

        public static bool OnRemoveItem(this MechLabLocationWidget widget, IMechLabDraggableItem item, bool validate)
        {
            void do_Repair()
            {
                item.ComponentRef.DamageLevel = ComponentDamageLevel.Penalized;
                var t = Traverse.Create(item).Method("RefreshDamageOverlays").GetValue();
                item.RepairComponent(true);
            }

            var component = item.ComponentRef.Def;

            if (component is IAutoRepair)
            {
                do_Repair();
                return true;
            }

            var mechlab = widget.parentDropTarget as MechLabPanel;

            if (component is IDefaultRepace replace && !string.IsNullOrEmpty(replace.DefaultID) && replace.DefaultID != item.ComponentRef.ComponentDefID)
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

        public static MechComponentRef[] ClearInventory(MechDef source, SimGameState state)
        {
            Control.Logger.LogDebug("Clearing Inventory");

            var list = source.Inventory.ToList();

            //TODO: Remove in light
            foreach (var item in list)
                if (item.Def == null)
                    item.RefreshComponentDef();

            var result_list = list.Where(i => i.Def is IDefault).ToList();

            for (int i = list.Count - 1; i >= 0; i--)
            {
                Control.Logger.LogDebug($"- {list[i].ComponentDefID} - {(list[i].Def == null ? "NULL" : list[i].SimGameUID)}");
                if (list[i].Def == null)
                {
                    list[i].RefreshComponentDef();
                    list[i].SetSimGameUID(state.GenerateSimGameUID());
                }

                if (list[i].Def is IDefault)
                {
                    Control.Logger.LogDebug("-- Default - skipping");
                    continue;
                }

                if (list[i].Def is IDefaultRepace replace)
                {
                    var ref_item = CreateHelper.Ref(replace.DefaultID, list[i].ComponentDefType, list[i].DataManager);
                    ref_item.SetData(list[i].MountedLocation, list[i].HardpointSlot, list[i].DamageLevel);
                    ref_item.SetSimGameUID(state.GenerateSimGameUID());
                    result_list.Add(ref_item);
                    Control.Logger.LogDebug($"-- Replace with {ref_item.ComponentDefID} - {ref_item.SimGameUID}");
                }


                if (list[i].Def is IAutoLinked link && link.Links != null)
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

        public static void PrefixPrune(WorkOrderEntry_MechLab baseWorkOrder)
        {

            if (_mechLab == null)
                return;

            Control.Logger.Log("-------- Prune: Prefix ----------");
            foreach (WorkOrderEntry_MechLab entry in baseWorkOrder.SubEntries)
            {
                Control.Logger.Log($"  {entry.Type} - {entry.ID} - {entry.MechID} - {(entry is WorkOrderEntry_InstallComponent install ? install.MechComponentRef.ComponentDefID : "")}");
            }


            baseWorkOrder.SubEntries.RemoveAll(i =>
                i is WorkOrderEntry_InstallComponent install && install.MechComponentRef.Def is IDefault);


            Control.Logger.Log("Prune: Prefix clean");
            foreach (WorkOrderEntry_MechLab entry in baseWorkOrder.SubEntries)
            {
                Control.Logger.Log($"  {entry.Type} - {entry.ID} - {entry.MechID} - {(entry is WorkOrderEntry_InstallComponent install ? install.MechComponentRef.ComponentDefID : "")}");
            }
        }

        public static void PostfixPrune(WorkOrderEntry_MechLab baseWorkOrder)
        {
            void add_install(int after, defaction action)
            {
                var componentRef = new MechComponentRef(action.defid, "", action.main.ComponentDefType, action.location = ChassisLocations.None);
                componentRef.DataManager = _mechLab.dataManager;
                componentRef.RefreshComponentDef();
                if (componentRef.Def == null)
                {
                    Control.Logger.LogError($"Cannot load {action.defid} to add");
                    return;
                }

                componentRef.SetSimGameUID(_mechLab.sim.GenerateSimGameUID());
                _mechLab.sim.WorkOrderComponents.Add(componentRef);

                var entry = _mechLab.sim.CreateComponentInstallWorkOrder(baseWorkOrder.MechID,
                    componentRef, action.location, ChassisLocations.None);
                entry.SetCost(0);

                var traverse = Traverse.Create(entry).Field("CBillCost");
                traverse.SetValue(action.type == atype.replacebase ? componentRef.Def.Description.Cost : 0);

                baseWorkOrder.SubEntries.Insert(after, entry);
            }

            void remove_install(int after, defaction action)
            {
                MechComponentRef componentRef = null;
                for (int i = after - 1; i >= 0; i--)
                {
                    var entry = baseWorkOrder.SubEntries[i] as WorkOrderEntry_InstallComponent;
                    if (entry != null && entry.MechComponentRef.ComponentDefID == action.defid &&
                        entry.DesiredLocation == ChassisLocations.None && entry.PreviousLocation == action.location)
                    {
                        componentRef = entry.MechComponentRef;
                        break;
                    }
                }

                if (componentRef == null)
                {
                    componentRef = _mechLab.originalMechDef.Inventory
                        .Where(i => i.MountedLocation == action.location)
                        .FirstOrDefault(i => i.ComponentDefID == action.defid);
                }

                if (componentRef == null)
                {
                    Control.Logger.LogError($"Cannot find {action.defid} to remove");
                    return;
                }

                var new_entry = _mechLab.sim.CreateComponentInstallWorkOrder(baseWorkOrder.MechID, componentRef,
                    ChassisLocations.None, action.location);
                new_entry.SetCost(0);

                var traverse = Traverse.Create(new_entry).Field("CBillCost");
                traverse.SetValue(action.type == atype.replacedef ? -componentRef.Def.Description.Cost / 2 : 0);

                baseWorkOrder.SubEntries.Insert(after, new_entry);
            }

            if (_mechLab == null)
                return;

            Control.Logger.Log("Prune: postfix");
            foreach (WorkOrderEntry_MechLab entry in baseWorkOrder.SubEntries)
            {
                Control.Logger.Log($"  {entry.Type} - {entry.ID} - {entry.MechID} - {(entry is WorkOrderEntry_InstallComponent install ? install.MechComponentRef.ComponentDefID : "")}");
            }


            foreach (var action in actions)
            {
                action.need_remove = false;
                int index = baseWorkOrder.SubEntries.FindIndex(i =>
                    i is WorkOrderEntry_InstallComponent inst && inst.MechComponentRef == action.main);

                if (index < 0)
                {
                    action.need_remove = true;
                    continue;
                }

                var install = baseWorkOrder.SubEntries[index] as WorkOrderEntry_InstallComponent;
                switch (action.type)
                {
                    case atype.addwith:
                        if (install.DesiredLocation == ChassisLocations.None)
                            action.need_remove = true;
                        else
                            add_install(index, action);
                        break;
                    case atype.removewith:
                        if (install.PreviousLocation != ChassisLocations.None)
                            action.need_remove = true;
                        else
                            remove_install(index, action);
                        break;
                    case atype.replacebase:
                        if (install.PreviousLocation == action.location)
                            add_install(index, action);
                        else
                            action.need_remove = true;
                        break;
                    case atype.replacedef:
                        if (install.DesiredLocation == action.location)
                            remove_install(index, action);
                        else
                            action.need_remove = true;
                        break;
                }
            }

            ShowWorkOrder(baseWorkOrder, "Prune Prefix End");

            actions.RemoveAll(a => a.need_remove);
        }

        private static void ShowWorkOrder(WorkOrderEntry_MechLab baseWorkOrder, string message)
        {
            Control.Logger.Log($"{message} {baseWorkOrder.SubEntries.Count}");
            try
            {
                foreach (WorkOrderEntry_MechLab entry in baseWorkOrder.SubEntries)
                {
                    if (entry is WorkOrderEntry_InstallComponent install)
                    {
                        Control.Logger.Log($"--{entry.Type} - {install.MechComponentRef.ComponentDefID} - {install.MechComponentRef.SimGameUID}");
                    }
                    else if (entry is WorkOrderEntry_RepairComponent repair)
                    {
                        Control.Logger.Log($"--{entry.Type} - {repair.MechComponentID}");
                    }
                    else
                    {
                        Control.Logger.Log($"--{entry.Type} - {entry.Description}");
                    }
                }
            }
            catch (Exception e)
            {
                Control.Logger.Log(e);
            }
        }

        public static void DefaultAddWith(MechComponentRef main, string defaultId, ChassisLocations location)
        {
            actions.Add(new defaction
            {
                type = atype.addwith,
                main = main,
                location = location,
                defid = defaultId
            });
        }

        public static void DefaultRemoveWith(MechComponentRef main, string defaultId, ChassisLocations location)
        {
            actions.Add(new defaction
            {
                type = atype.removewith,
                main = main,
                location = location,
                defid = defaultId
            });

        }

        public static void DefaultReplaceBase(MechComponentRef main, string defaultId, ChassisLocations location)
        {
            actions.Add(new defaction
            {
                type = atype.replacebase,
                main = main,
                location = location,
                defid = defaultId
            });
        }

        public static void DefautReplaceDefault(MechComponentRef main, string defaultId, ChassisLocations location)
        {
            actions.Add(new defaction
            {
                type = atype.replacedef,
                main = main,
                location = location,
                defid = defaultId
            });
        }
    }
}