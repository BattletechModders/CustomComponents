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
    [CustomComponent("Category")]
    public class Category : SimpleCustomComponent, IAfterLoad, IOnInstalled, IReplaceValidateDrop, IPreValidateDrop, IPostValidateDrop
    {
        /// <summary>
        /// name of category
        /// </summary>
        public string CategoryID { get; set; }
        /// <summary>
        /// optional tag for AllowMixTags, if not set defid will used
        /// </summary>
        public string Tag { get; set; }

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
            CategoryDescriptor = Control.GetCategory(CategoryID);

            if (CategoryDescriptor.DefaultCustoms == null)
            {
                return;
            }
            var customSection = (Dictionary<string, object>)values[Control.CustomSectionName];

            foreach (var customPair in CategoryDescriptor.DefaultCustoms)
            {
                if (!customSection.ContainsKey(customPair.Key))
                {
                    customSection[customPair.Key] = customPair.Value;

                    //Control.Logger.LogDebug($"{Def.Description.Id} added {customPair.Key}");
                }
            }
        }

        public void OnInstalled(WorkOrderEntry_InstallComponent order, SimGameState state, MechDef mech)
        {

            Control.Logger.LogDebug($"- Category");

            if (!CategoryDescriptor.AutoReplace || CategoryDescriptor.MaxEquiped < 0 && CategoryDescriptor.MaxEquipedPerLocation < 0)
            {
#if CCDEBUG
                var c1 = CategoryDescriptor;
                Control.Logger.LogDebug($"-- {c1.DisplayName} r:{c1.AutoReplace}  max:{c1.MaxEquiped} mpr:{c1.MaxEquipedPerLocation} = not requre replace");
#endif
                return;
            }

            if (order.DesiredLocation == ChassisLocations.None)
            {
                Control.Logger.LogDebug("-- removing, no additional actions");
                return;
            }

            int n1 = mech.Inventory.Count(i => i.Is<Category>(out var cat) && CategoryID == cat.CategoryID);
            int n2 = mech.Inventory.Count(i => i.MountedLocation == order.DesiredLocation && i.IsCategory(CategoryID));

#if CCDEBUG
            Control.Logger.LogDebug("--- list:");
            foreach (var def in mech.Inventory.Where(i => i.MountedLocation == order.DesiredLocation && i.IsCategory(CategoryID)).Select(i => i.Def))
            {
                Control.Logger.LogDebug($"---- list: {def.Description.Id}: Default:{def.IsDefault()}");
            }
#endif


            Control.Logger.LogDebug($"-- total {n1}/{CategoryDescriptor.MaxEquiped}  location: {n2}/{CategoryDescriptor.MaxEquipedPerLocation}");

            var replace = mech.Inventory.FirstOrDefault(i => i.MountedLocation == order.DesiredLocation && i.IsCategory(CategoryID) && i.IsDefault());

            Control.Logger.LogDebug($"-- possible replace: {(replace == null ? "not found" : replace.ComponentDefID)}");

            if (replace == null)
                return;

            bool need_replace = (CategoryDescriptor.MaxEquiped > 0 && n1 > CategoryDescriptor.MaxEquiped) ||
                (CategoryDescriptor.MaxEquipedPerLocation > 0 && n2 > CategoryDescriptor.MaxEquipedPerLocation);

            Control.Logger.LogDebug($"-- need_repalce: {need_replace}");

            if (need_replace)
                DefaultHelper.RemoveInventory(replace.ComponentDefID, mech, order.DesiredLocation, replace.ComponentDefType);

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
            foreach (var def in location.LocalInventory.Select(i => i.ComponentRef.Def))
            {
                var c = def.GetComponent<Category>();
                string error = def.Description.Id;
                if (c == null)
                    error += " : not category";
                else
                {
                    error =
                        $"{error} : {c.CategoryID}, M:{c.CategoryDescriptor.MaxEquiped}, MPL: {c.CategoryDescriptor.MaxEquipedPerLocation} cd is null: {c.CategoryDescriptor.DefaultCustoms == null}";
                }
                Control.Logger.LogDebug($"---- {error}");

            }
#endif

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
                        .FirstOrDefault(i => i.ComponentRef.Def.IsCategory(CategoryID));
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
                        .FirstOrDefault(i => i.ComponentRef.Def.IsCategory(CategoryID));
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

        public string PreValidateDrop(MechLabItemSlotElement item, LocationHelper location, MechLabHelper mechlab)
        {
            Control.Logger.LogDebug("-- Category");

            if (!CategoryDescriptor.AllowMixTags && mechlab.MechLab.activeMechDef.Inventory.Any(i => i.Def.Is<Category>(out var c) && c.CategoryID == CategoryID && GetTag() != c.GetTag()))
                return string.Format(CategoryDescriptor.AddMixed, CategoryDescriptor.DisplayName);

            if (CategoryDescriptor.Forbidden != null && CategoryDescriptor.Forbidden.Length > 0)
            {
                foreach (var forbidden in CategoryDescriptor.Forbidden)
                {
                    var f_item =
                        mechlab.MechLab.activeMechDef.Inventory.FirstOrDefault(i => i.Def.IsCategory(forbidden));
                    if (f_item != null)
                    {
                        var c = f_item.Def.GetComponent<Category>();
                        return string.Format(CategoryDescriptor.ValidateForbidden, CategoryDescriptor.DisplayName,
                            c.CategoryDescriptor.DisplayName);
                    }
                }
            }

            return string.Empty;
        }

        public string PostValidateDrop(MechLabItemSlotElement drop_item, MechDef mech, List<InvItem> new_inventory, List<IChange> changes)
        {
            Control.Logger.LogDebug("-- Category");

            if (CategoryDescriptor.MaxEquiped > 0)
            {
                var total = new_inventory.Count(i => i.item.Def.IsCategory(CategoryID));

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
                    .Select(i => new { l = i.location, c = i.item.Def.GetComponent<Category>() })
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
    }
}