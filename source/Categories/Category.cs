using System;
using System.Collections.Generic;
using System.Linq;
using BattleTech;
using BattleTech.UI;
using fastJSON;


namespace CustomComponents
{
    /// <summary>
    /// component use category logic
    /// </summary>
    [CustomComponent("Category", true)]
    public class Category : SimpleCustomComponent, IAfterLoad, IOnInstalled, IReplaceValidateDrop,
        IPreValidateDrop, IPostValidateDrop, IReplaceIdentifier, IAdjustDescription, IOnItemGrabbed,
        IClearInventory, IAdjustValidateDrop
    {
        /// <summary>
        /// name of category
        /// </summary>
        public string CategoryID { get; set; }
        /// <summary>
        /// optional tag for AllowMixTags, if not set defid will used
        /// </summary>
        public string Tag { get; set; }

        public string ReplaceID => CategoryID;


        public string GetTag()
        {
            if (string.IsNullOrEmpty(Tag))
                return Def.Description.Id;
            else
                return Tag;
        }

        //       public bool Placeholder { get; set; } // if true, item is invalid

        [JsonIgnore]
        public CategoryDescriptor CategoryDescriptor { get; set; }

        public void OnLoaded(Dictionary<string, object> values)
        {
            CategoryDescriptor = CategoryController.Shared.GetOrCreateCategory(CategoryID);

            if (CategoryDescriptor.Defaults == null)
            {
                return;
            }

            Registry.ProcessCustomFactories(Def, CategoryDescriptor.Defaults, false);
        }

        public void OnInstalled(WorkOrderEntry_InstallComponent order, SimGameState state, MechDef mech)
        {
            Control.Logger.LogDebug($"- Category");
            if (order.PreviousLocation != ChassisLocations.None)
            {
#if CCDEBUG
                Control.Logger.LogDebug("-- removing");
#endif
                MechComponentRef def_replace = DefaultFixer.Shared.GetReplaceFor(mech, CategoryID, order.PreviousLocation, state);
                if (def_replace != null)
                {
#if CCDEBUG
                    Control.Logger.LogDebug($"--- added {def_replace.ComponentDefID}");
#endif
                    var inv = mech.Inventory.ToList();
                    inv.Add(def_replace);
                    mech.SetInventory(inv.ToArray());
                }
            }

            if (order.DesiredLocation == ChassisLocations.None)
                return;

            if (!CategoryDescriptor.AutoReplace || CategoryDescriptor.MaxEquiped < 0 && CategoryDescriptor.MaxEquipedPerLocation < 0)
            {
#if CCDEBUG
                var c1 = CategoryDescriptor;
                Control.Logger.LogDebug($"-- {c1.DisplayName} r:{c1.AutoReplace}  max:{c1.MaxEquiped} mpr:{c1.MaxEquipedPerLocation} = not requre replace");
#endif
                return;
            }


            int n1 = mech.Inventory.Count(i => i.IsCategory(CategoryID));
            int n2 = mech.Inventory.Count(i => i.MountedLocation == order.DesiredLocation && i.IsCategory(CategoryID));

#if CCDEBUG
            Control.Logger.LogDebug("--- list:");
            foreach (var def in mech.Inventory.Where(i => i.MountedLocation == order.DesiredLocation && i.IsCategory(CategoryID)).Select(i => i.Def))
            {
                Control.Logger.LogDebug($"---- list: {def.Description.Id}: Default:{def.IsDefault()}");
            }
#endif


            Control.Logger.LogDebug($"-- total {n1}/{CategoryDescriptor.MaxEquiped}  location: {n2}/{CategoryDescriptor.MaxEquipedPerLocation}");

            var replace = mech.Inventory.FirstOrDefault(i => !i.IsModuleFixed(mech) && (i.MountedLocation == order.DesiredLocation || CategoryDescriptor.ReplaceAnyLocation) && i.IsCategory(CategoryID) && i.IsDefault());

            Control.Logger.LogDebug($"-- possible replace: {(replace == null ? "not found" : replace.ComponentDefID)}");

            if (replace == null)
                return;

            bool need_replace = (CategoryDescriptor.MaxEquiped > 0 && n1 > CategoryDescriptor.MaxEquiped) ||
                (CategoryDescriptor.MaxEquipedPerLocation > 0 && n2 > CategoryDescriptor.MaxEquipedPerLocation);

            Control.Logger.LogDebug($"-- need_repalce: {need_replace}");

            if (need_replace)
                DefaultHelper.RemoveInventory(replace.ComponentDefID, mech, replace.MountedLocation, replace.ComponentDefType);

        }

        public string ReplaceValidateDrop(MechLabItemSlotElement drop_item, LocationHelper location,
            ref MechLabItemSlotElement current_replace)
        {
#if CCDEBUG
            Control.Logger.LogDebug($"-- Category {CategoryID}");
#endif
            if (!CategoryDescriptor.AutoReplace ||
                CategoryDescriptor.MaxEquiped <= 0 && CategoryDescriptor.MaxEquipedPerLocation <= 0)
            {
#if CCDEBUG
                Control.Logger.LogDebug($"--- no replace needed");
#endif
                return String.Empty;
            }

            if (current_replace != null)
            {
#if CCDEBUG
                Control.Logger.LogDebug($"--- replace already found");
#endif
                return string.Empty;
            }

#if CCDEBUG
            foreach (var c in location.LocalInventory.Select(i => i.ComponentRef.Def).SelectMany(def => def.GetComponents<Category>()))
            {
                //                var c = def.GetComponent<Category>();
                string error = c.Def.Description.Id;
                error += $" : {c.CategoryID}, M:{c.CategoryDescriptor.MaxEquiped}, MPL: {c.CategoryDescriptor.MaxEquipedPerLocation} cd is null: {c.CategoryDescriptor.DefaultCustoms == null}";
                Control.Logger.LogDebug($"---- {error}");
            }
#endif
            var mech = location.mechLab.activeMechDef;

            if (CategoryDescriptor.MaxEquiped > 0)
            {
#if CCDEBUG
                Control.Logger.LogDebug($"--- MaxEquiped: {CategoryDescriptor.MaxEquiped}");
#endif
                var n = location.mechLab.activeMechDef.Inventory.Count(i => i.Def.IsCategory(CategoryID));
#if CCDEBUG
                Control.Logger.LogDebug($"--- current: {n}");
#endif

                if (n >= CategoryDescriptor.MaxEquiped)
                {
                    var replace = location.LocalInventory
                        .FirstOrDefault(i => i.ComponentRef.Def.IsCategory(CategoryID) && !i.ComponentRef.IsModuleFixed(mech));

                    if (CategoryDescriptor.ReplaceAnyLocation && replace == null)
                    {
                        var mechlab = new MechLabHelper(location.mechLab);
                        foreach (var widget in mechlab.GetWidgets())
                        {
                            if (widget.loadout.Location == location.widget.loadout.Location)
                                continue;

                            var loc_helper = new LocationHelper(widget);
                            replace = loc_helper.LocalInventory
                                .FirstOrDefault(i => i.ComponentRef.Def.IsCategory(CategoryID) && !i.ComponentRef.IsModuleFixed(mech));
                            if (replace != null)
                                break;
                        }
                    }

#if CCDEBUG
                    Control.Logger.LogDebug($"--- replace: {(replace == null ? "none" : replace.ComponentRef.ComponentDefID)}");
#endif

                    if (replace == null)
                    {
#if CCDEBUG
                        Control.Logger.LogDebug($"--- return error");
#endif

                        if (CategoryDescriptor.MaxEquiped > 1)
                            return string.Format(CategoryDescriptor.AddMaximumReached, CategoryDescriptor.displayName,
                                n);
                        else
                            return string.Format(CategoryDescriptor.AddAlreadyEquiped, CategoryDescriptor.displayName);
                    }
#if CCDEBUG
                    Control.Logger.LogDebug($"--- return replace");
#endif
                    current_replace = replace;
                    return string.Empty;
                }
            }

            if (CategoryDescriptor.MaxEquipedPerLocation > 0)
            {
#if CCDEBUG
                Control.Logger.LogDebug($"--- MaxEquipedPerLocation: {CategoryDescriptor.MaxEquipedPerLocation}");
#endif
                var n = location.LocalInventory.Count(i => i.ComponentRef.Def.IsCategory(CategoryID));
#if CCDEBUG
                Control.Logger.LogDebug($"--- current: {n}");
#endif
                if (n >= CategoryDescriptor.MaxEquipedPerLocation)
                {


                    var replace = location.LocalInventory
                        .FirstOrDefault(i => i.ComponentRef.Def.IsCategory(CategoryID) && !i.ComponentRef.IsModuleFixed(mech));
#if CCDEBUG
                    Control.Logger.LogDebug($"--- replace: {(replace == null ? "none" : replace.ComponentRef.ComponentDefID)}");
#endif
                    if (replace == null)
                    {
#if CCDEBUG
                        Control.Logger.LogDebug($"--- return error");
#endif

                        if (CategoryDescriptor.MaxEquipedPerLocation > 1)
                            return string.Format(CategoryDescriptor.AddMaximumLocationReached,
                                CategoryDescriptor.displayName, n, location.LocationName);
                        else
                            return string.Format(CategoryDescriptor.AddAlreadyEquipedLocation,
                                CategoryDescriptor.displayName, location.LocationName);
                    }
#if CCDEBUG
                    Control.Logger.LogDebug($"--- return replace");
#endif
                    current_replace = replace;
                }

            }
            return string.Empty;

        }

        public string ReplaceValidateDrop(MechLabItemSlotElement drop_item, LocationHelper location, List<IChange> changes)
        {
#if CCDEBUG
            Control.Logger.LogDebug($"-- Category {CategoryID}");
#endif
            if (!CategoryDescriptor.AutoReplace ||
                CategoryDescriptor.MaxEquiped <= 0 && CategoryDescriptor.MaxEquipedPerLocation <= 0)
            {
#if CCDEBUG
                Control.Logger.LogDebug($"--- no replace needed");
#endif
                return String.Empty;
            }

            if (changes.OfType<RemoveChange>().Select(i => i.item.ComponentRef).Any(i => i.IsCategory(CategoryID)))
            {
#if CCDEBUG
                Control.Logger.LogDebug($"--- replace already found");
#endif
                return string.Empty;
            }


            var mech = location.mechLab.activeMechDef;

            if (CategoryDescriptor.MaxEquiped > 0)
            {
#if CCDEBUG
                Control.Logger.LogDebug($"--- MaxEquiped: {CategoryDescriptor.MaxEquiped}");
#endif
                var n = location.mechLab.activeMechDef.Inventory.Count(i => i.Def.IsCategory(CategoryID));
#if CCDEBUG
                Control.Logger.LogDebug($"--- current: {n}");
#endif

                if (n >= CategoryDescriptor.MaxEquiped)
                {
                    var replace = location.LocalInventory
                        .FirstOrDefault(i => i.ComponentRef.Def.IsCategory(CategoryID) && !i.ComponentRef.IsModuleFixed(mech));

                    if (CategoryDescriptor.ReplaceAnyLocation && replace == null)
                    {
                        var mechlab = new MechLabHelper(location.mechLab);
                        foreach (var widget in mechlab.GetWidgets())
                        {
                            if (widget.loadout.Location == location.widget.loadout.Location)
                                continue;

                            var loc_helper = new LocationHelper(widget);
                            replace = loc_helper.LocalInventory
                                .FirstOrDefault(i => i.ComponentRef.Def.IsCategory(CategoryID) && !i.ComponentRef.IsModuleFixed(mech));
                            if (replace != null)
                                break;
                        }
                    }

#if CCDEBUG
                    Control.Logger.LogDebug($"--- replace: {(replace == null ? "none" : replace.ComponentRef.ComponentDefID)}");
#endif

                    if (replace == null)
                    {
#if CCDEBUG
                        Control.Logger.LogDebug($"--- return error");
#endif

                        if (CategoryDescriptor.MaxEquiped > 1)
                            return string.Format(CategoryDescriptor.AddMaximumReached, CategoryDescriptor.displayName,
                                n);
                        else
                            return string.Format(CategoryDescriptor.AddAlreadyEquiped, CategoryDescriptor.displayName);
                    }
#if CCDEBUG
                    Control.Logger.LogDebug($"--- return replace");
#endif

                    changes.Add(new RemoveChange(replace.MountedLocation, replace));

                    return string.Empty;
                }
            }

            if (CategoryDescriptor.MaxEquipedPerLocation > 0)
            {
#if CCDEBUG
                Control.Logger.LogDebug($"--- MaxEquipedPerLocation: {CategoryDescriptor.MaxEquipedPerLocation}");
#endif
                var n = location.LocalInventory.Count(i => i.ComponentRef.Def.IsCategory(CategoryID));
#if CCDEBUG
                Control.Logger.LogDebug($"--- current: {n}");
#endif
                if (n >= CategoryDescriptor.MaxEquipedPerLocation)
                {


                    var replace = location.LocalInventory
                        .FirstOrDefault(i => i.ComponentRef.Def.IsCategory(CategoryID) && !i.ComponentRef.IsModuleFixed(mech));
#if CCDEBUG
                    Control.Logger.LogDebug($"--- replace: {(replace == null ? "none" : replace.ComponentRef.ComponentDefID)}");
#endif
                    if (replace == null)
                    {
#if CCDEBUG
                        Control.Logger.LogDebug($"--- return error");
#endif

                        if (CategoryDescriptor.MaxEquipedPerLocation > 1)
                            return string.Format(CategoryDescriptor.AddMaximumLocationReached,
                                CategoryDescriptor.displayName, n, location.LocationName);
                        else
                            return string.Format(CategoryDescriptor.AddAlreadyEquipedLocation,
                                CategoryDescriptor.displayName, location.LocationName);
                    }
#if CCDEBUG
                    Control.Logger.LogDebug($"--- return replace");
#endif
                    changes.Add(new RemoveChange(replace.MountedLocation, replace));
                }
            }
            return string.Empty;
        }

        public string PreValidateDrop(MechLabItemSlotElement item, LocationHelper location, MechLabHelper mechlab)
        {
#if CCDEBUG
            Control.Logger.LogDebug($"-- Category {CategoryID}");
#endif

            if (!CategoryDescriptor.AllowMixTags && mechlab.MechLab.activeMechDef.Inventory.Any(i => i.Def.IsCategory(CategoryID, out var c) && GetTag() != c.GetTag()))
                return string.Format(CategoryDescriptor.AddMixed, CategoryDescriptor.DisplayName);

            return string.Empty;
        }

        public string PostValidateDrop(MechLabItemSlotElement drop_item, MechDef mech, List<InvItem> new_inventory, List<IChange> changes)
        {
#if CCDEBUG
            Control.Logger.LogDebug($"-- Category {CategoryID}");
#endif

            if (CategoryDescriptor.MaxEquiped > 0)
            {
                var total = new_inventory.Count(i => i.item.Def.IsCategory(CategoryID));
#if CCDEBUG
                Control.Logger.LogDebug($"---- {total}/{CategoryDescriptor.MaxEquiped}");
#endif
                if (total > CategoryDescriptor.MaxEquiped)
                    if (CategoryDescriptor.MaxEquiped > 1)
                        return string.Format(CategoryDescriptor.AddMaximumReached, CategoryDescriptor.displayName, CategoryDescriptor.MaxEquiped);
                    else
                        return string.Format(CategoryDescriptor.AddAlreadyEquiped, CategoryDescriptor.displayName);
            }

            if (CategoryDescriptor.MaxEquipedPerLocation > 0)
            {
                var total = new_inventory
                    .Where(i => i.item.Def.IsCategory(CategoryID))
                    .Select(i =>
                    {
                        i.item.Def.IsCategory(CategoryID, out var component);
                        return new { l = i.location, c = component };
                    })
                    .GroupBy(i => i.l)
                    .FirstOrDefault(i => i.Count() > CategoryDescriptor.MaxEquipedPerLocation);

                if (total != null)
                    if (CategoryDescriptor.MaxEquipedPerLocation > 1)
                        return string.Format(CategoryDescriptor.AddMaximumLocationReached, CategoryDescriptor.DisplayName,
                           CategoryDescriptor.MaxEquipedPerLocation, total.Key);
                    else
                        return string.Format(CategoryDescriptor.AddAlreadyEquipedLocation,
                            CategoryDescriptor.DisplayName, total.Key);
            }

            return string.Empty;
        }

        public string AdjustDescription(string Description)
        {
            if (CategoryDescriptor.AddCategoryToDescription)
            {
                var color_start = $"<b><color={Control.Settings.CategoryDescriptionColor}>[";
                var color_end = "]</color><b>";
                if (Description.Contains(color_start))
                {
                    int pos = Description.IndexOf(color_start, 0) + color_start.Length;
                    Description = Description.Substring(0, pos) + CategoryDescriptor.DisplayName + ", " +
                                  Description.Substring(pos);
                }
                else
                    Description = Description + "\n" + color_start + CategoryDescriptor.DisplayName + color_end;
            }

            return Description;
        }


        public override string ToString()
        {
            return "Category: " + CategoryID;
        }

        public void ClearInventory(MechDef mech, List<MechComponentRef> result, SimGameState state, MechComponentRef source)
        {
            var item = DefaultFixer.Shared.GetReplaceFor(mech, CategoryID, source.MountedLocation, state);
            if(item != null)
                result.Add(item);
        }

        public void OnItemGrabbed(IMechLabDraggableItem item, MechLabPanel mechLab, MechLabLocationWidget widget)
        {
            Control.Logger.LogDebug($"- Category {CategoryID}");
            Control.Logger.LogDebug($"-- search replace for {item.ComponentRef.ComponentDefID}");

            var replace = DefaultFixer.Shared.GetReplaceFor(mechLab.activeMechDef, CategoryID, widget.loadout.Location, mechLab.sim);

            if (replace == null)
            {
                Control.Logger.LogDebug($"-- no replacement, skipping");
                return;
            }

            DefaultHelper.AddMechLab(replace, new MechLabHelper(mechLab));
            Control.Logger.LogDebug($"-- added {replace.ComponentDefID} to {replace.MountedLocation}");
            mechLab.ValidateLoadout(false);
        }

        public IEnumerable<IChange> ValidateDropOnAdd(MechLabItemSlotElement item, LocationHelper location, MechLabHelper mechlab, List<IChange> changes)
        {
            yield break;
        }

        public IEnumerable<IChange> ValidateDropOnRemove(MechLabItemSlotElement item, LocationHelper location, MechLabHelper mechlab, List<IChange> changes)
        {
            var replace = DefaultFixer.Shared.GetReplaceFor(mechlab.MechLab.activeMechDef, CategoryID, item.MountedLocation, mechlab.MechLab.sim);
            if (replace == null)
            {
                Control.Logger.LogDebug($"--- Category {CategoryID} - no replace, return");
                yield break;
            }

            foreach (var addChange in changes.OfType<AddChange>())
            {
                if (addChange.item.ComponentRef.IsCategory(CategoryID))
                {
                    Control.Logger.LogDebug($"--- Category {CategoryID} - replace already added");
                    yield break;
                }
            }

            Control.Logger.LogDebug($"--- Category {CategoryID} - add replace {replace.ComponentDefID}");

            yield return new AddChange(replace.MountedLocation, DefaultHelper.CreateSlot(replace, mechlab.MechLab));
        }
    }
}