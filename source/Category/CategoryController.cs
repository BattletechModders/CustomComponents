using System.Collections.Generic;
using System.Linq;
using BattleTech;
using BattleTech.UI;
using Harmony;
using TMPro;

namespace CustomComponents
{
    internal enum CategoryError
    {
        None,
        AreadyEquiped,
        MaximumReached,
        AlreadyEquipedLocation,
        MaximumReachedLocation,
        AllowMix
    }

    public static class CategoryController
    {
        public static void ValidateMech(Dictionary<MechValidationType, List<string>> errors,
            MechValidationLevel validationLevel, MechDef mechDef)
        {
            var items_by_category = (from item in mechDef.Inventory
                                     where item.Def is ICategory
                                     let def = item.Def as ICategory
                                     select new
                                     {
                                         category = def.CategoryDescriptor,
                                         itemdef = item.Def,
                                         itemref = item
                                     }).GroupBy(i => i.category).ToDictionary(i => i.Key, i => i.ToList());

            foreach (var category in Control.GetCategories().Where(i => i.Requred))
            {
                if (!items_by_category.ContainsKey(category) || items_by_category[category].Count < category.MinEquiped)
                    if (category.MinEquiped == 1)
                        errors[MechValidationType.InvalidInventorySlots].Add(string.Format(category.ValidateRequred, category.DisplayName.ToUpper(), category.DisplayName));
                    else
                        errors[MechValidationType.InvalidInventorySlots].Add(string.Format(category.ValidateMinimum, category.DisplayName.ToUpper(), category.DisplayName, category.MinEquiped));
            }

            foreach (var pair in items_by_category)
            {
                if (pair.Key.MaxEquiped > 0 && pair.Value.Count > pair.Key.MaxEquiped)
                    if (pair.Key.MaxEquiped == 1)
                        errors[MechValidationType.InvalidInventorySlots].Add(string.Format(pair.Key.ValidateUnique,
                            pair.Key.DisplayName.ToUpper(), pair.Key.DisplayName));
                    else
                        errors[MechValidationType.InvalidInventorySlots].Add(string.Format(pair.Key.ValidateMaximum,
                            pair.Key.DisplayName.ToUpper(), pair.Key.DisplayName, pair.Key.MaxEquiped));

                if (!pair.Key.AllowMix)
                {
                    string def = pair.Value[0].itemdef.Description.Id;
                    bool flag = pair.Value.Select(i => i.itemdef.Description.Id).Any(d => def != d);
                    if (flag)
                    {
                        errors[MechValidationType.InvalidInventorySlots].Add(string.Format(pair.Key.ValidateMixed,
                            pair.Key.DisplayName.ToUpper(), pair.Key.DisplayName));
                    }
                }

                if (pair.Key.MaxEquipedPerLocation > 0)
                {
                    var max = pair.Value.GroupBy(i => i.itemref.MountedLocation).Max(i => i.Count());
                    if (max > pair.Key.MaxEquipedPerLocation)
                        if (pair.Key.MaxEquipedPerLocation == 1)
                            if (pair.Key.MaxEquiped == 1)
                                errors[MechValidationType.InvalidInventorySlots].Add(string.Format(pair.Key.ValidateUniqueLocation,
                                    pair.Key.DisplayName.ToUpper(), pair.Key.DisplayName));
                            else
                                errors[MechValidationType.InvalidInventorySlots].Add(string.Format(pair.Key.ValidateMaximumLocation,
                                    pair.Key.DisplayName.ToUpper(), pair.Key.DisplayName, pair.Key.MaxEquipedPerLocation));
                }
            }
        }

        internal static CategoryError ValidateAdd(ICategory component, MechLabLocationWidget widget,
            MechLabPanel mechlab, out int count, out string location)
        {
            var category = component.CategoryDescriptor;

            var items = mechlab.activeMechDef.Inventory
                .Where(i => (i.Def is ICategory) && (i.Def as ICategory).CategoryDescriptor == category).ToList();

            count = 0;
            location = "";

            if (category.MaxEquiped > 0)
            {
                if (items.Count >= category.MaxEquiped)
                {
                    if (category.Unique)
                        return CategoryError.AreadyEquiped;
                    count = items.Count;
                    return CategoryError.MaximumReached;
                }
            }

            if (category.MaxEquipedPerLocation > 0)
            {
                int count_per_location = items.Count(i => i.MountedLocation == widget.loadout.Location);
                if (count_per_location >= category.MaxEquipedPerLocation)
                {
                    var helper = new LocationHelper(widget);
                    location = helper.LocationName;
                    if (category.UniqueForLocation)
                        return CategoryError.AlreadyEquipedLocation;

                    count = count_per_location;
                    return CategoryError.MaximumReachedLocation;
                }
            }

            if (!category.AllowMix)
            {
                if (items.Any(i => i.Def.Description.Id != (component as MechComponentDef).Description.Id))
                    return CategoryError.AllowMix;
            }

            return CategoryError.None;
        }

        public static bool ValidateAdd(MechComponentDef component, MechLabLocationWidget widget,
            bool current_result, ref string errorMessage, MechLabPanel mechlab)
        {
            if (!current_result)
                return false;

            if (!(component is ICategory))
                return current_result;

            int count;
            string location_name;

            var error = ValidateAdd(component as ICategory, widget, mechlab, out count, out location_name);

            if (error == CategoryError.None)
                return true;

            var category = (component as ICategory).CategoryDescriptor;

            switch (error)
            {
                case CategoryError.AreadyEquiped:
                    errorMessage = string.Format(category.AddAlreadyEquiped, category.DisplayName);
                    return false;
                case CategoryError.MaximumReached:
                    errorMessage = string.Format(category.AddMaximumReached, category.DisplayName, count);
                    return false;
                case CategoryError.AlreadyEquipedLocation:
                    errorMessage = string.Format(category.AddAlreadyEquipedLocation, category.DisplayName, location_name);
                    return false;
                case CategoryError.MaximumReachedLocation:
                    errorMessage = string.Format(category.AddMaximumLocationReached, category.DisplayName, location_name, location_name);
                    return false;
                case CategoryError.AllowMix:
                    errorMessage = string.Format(category.AddMixed, category.DisplayName);
                    return false;
                default:
                    return true;
            }
        }
    }

    [HarmonyPatch(typeof(MechLabLocationWidget), "OnMechLabDrop")]
    public static class MechLabLocationWidget_OnDrop_Patch_Unique
    {
        public static bool Prefix(MechLabLocationWidget __instance, ref string ___dropErrorMessage,
            List<MechLabItemSlotElement> ___localInventory,
            int ___usedSlots,
            int ___maxSlots,
            TextMeshProUGUI ___locationName,
            MechLabPanel ___mechLab)
        {
            if (!Control.settings.LoadDefaultValidators)
                return true;

            if (!___mechLab.Initialized)
            {
                return false;
            }
            if (___mechLab.DragItem == null)
            {
                return false;
            }

            var drag_item = ___mechLab.DragItem;

            if (drag_item.ComponentRef == null)
            {
                return false;
            }

            bool flag = __instance.ValidateAdd(drag_item.ComponentRef);
            if (flag) return true;

            Control.mod.Logger.LogDebug("Dropped item: " + drag_item.ComponentRef.ComponentDefID);

            var item = drag_item.ComponentRef.Def as ICategory;

            if (item == null || !item.CategoryDescriptor.AutoReplace || (item.CategoryDescriptor.MaxEquiped <=0 && item.CategoryDescriptor.MaxEquipedPerLocation <=0))
            {
                Control.mod.Logger.LogDebug("Item not need autoreplace, exit");
                return true;
            } 


            int count;
            string name;

            var error = CategoryController.ValidateAdd(item, __instance, ___mechLab, out count, out name);
            Control.mod.Logger.LogDebug(string.Format("Error: {0} - {1}", error, ___dropErrorMessage));


            if (error == CategoryError.AllowMix || error == CategoryError.None)
                return true;

            //if (!flag && !___dropErrorMessage.EndsWith("Not enough free slots."))
            //{
            //    Control.mod.Logger.LogDebug("return by Not Enough slots?");
            //    return true;
            //}

            var n = ___localInventory.FindIndex(i => (i.ComponentRef.Def is ICategory) && (i.ComponentRef.Def as ICategory).Category == item.Category);

            Control.mod.Logger.Log("index = " + n.ToString());

            //if no - continue normal flow(add new or show "not enough slots" message
            if (n < 0)
                return true;

            if (___usedSlots - ___localInventory[n].ComponentRef.Def.InventorySize + drag_item.ComponentRef.Def.InventorySize >
                ___maxSlots)
            {
                return true;
            }

            var old_item = ___localInventory[n];
            __instance.OnRemoveItem(old_item, true);
            ___mechLab.ForceItemDrop(old_item);
            var clear = __instance.OnAddItem(drag_item, true);
            if (__instance.Sim != null)
            {
                WorkOrderEntry_InstallComponent subEntry = __instance.Sim.CreateComponentInstallWorkOrder(
                    ___mechLab.baseWorkOrder.MechID,
                    drag_item.ComponentRef, __instance.loadout.Location, drag_item.MountedLocation);
                ___mechLab.baseWorkOrder.AddSubEntry(subEntry);
            }

            drag_item.MountedLocation = __instance.loadout.Location;
            ___mechLab.ClearDragItem(clear);
            __instance.RefreshHardpointData();
            ___mechLab.ValidateLoadout(false);
            return false;
        }

    }
}