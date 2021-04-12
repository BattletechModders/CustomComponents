using System;
using System.Collections.Generic;
using System.Linq;
using BattleTech;
using BattleTech.UI;
using CustomComponents.ExtendedDetails;
using fastJSON;

namespace CustomComponents
{
    /// <summary>
    /// component use category logic
    /// </summary>
    [CustomComponent("Category", true)]
    public class Category : SimpleCustomComponent, IAfterLoad, IOnInstalled, IReplaceValidateDrop,
        IReplaceIdentifier, IAdjustDescription, IOnItemGrabbed,
        IClearInventory, IAdjustValidateDrop
    {

        private class free_record
        {
            public ChassisLocations locations;
            public CategoryLimit limit;
            public int free;
            public int can_free;
            public List<(MechComponentRef item, int weight)> items = new List<(MechComponentRef item, int weight)>();
        }

        /// <summary>
        /// name of category
        /// </summary>
        public string CategoryID { get; set; }
        /// <summary>
        /// optional tag for AllowMixTags, if not set defid will used
        /// </summary>
        public string Tag { get; set; }

        public int Weight { get; set; } = 1;

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
            Control.LogDebug(DType.ComponentInstall, $"- Category");
            if (order.PreviousLocation != ChassisLocations.None)
            {
                Control.LogDebug(DType.ComponentInstall, "-- removing");
                MechComponentRef def_replace = DefaultFixer.Shared.GetReplaceFor(mech, CategoryID, order.PreviousLocation, state);
                if (def_replace != null)
                {
                    Control.LogDebug(DType.ComponentInstall, $"--- added {def_replace.ComponentDefID}");
                    var inv = mech.Inventory.ToList();
                    inv.Add(def_replace);
                    mech.SetInventory(inv.ToArray());
                }
            }

            if (order.DesiredLocation == ChassisLocations.None)
                return;

            var record = CategoryDescriptor[mech];
            if (record == null)
            {
                Control.LogDebug(DType.ComponentInstall,
                    $"-- {CategoryDescriptor.DisplayName} r:{CategoryDescriptor.AutoReplace}  record:null = not requre replace");

                return;
            }

            if (!CategoryDescriptor.AutoReplace || record.MaxEquiped < 0 && record.MaxEquipedPerLocation < 0)
            {
                Control.LogDebug(DType.ComponentInstall, $"-- {CategoryDescriptor.DisplayName} r:{CategoryDescriptor.AutoReplace}  max:{record.MaxEquiped} mpr:{record.MaxEquipedPerLocation} = not requre replace");

                return;
            }


            int n1 = mech.Inventory.Count(i => i.IsCategory(CategoryID));
            int n2 = mech.Inventory.Count(i => i.MountedLocation == order.DesiredLocation && i.IsCategory(CategoryID));

#if CCDEBUG
            if (Control.Settings.DebugInfo.HasFlag(DType.ComponentInstall))
            {
                Control.LogDebug(DType.ComponentInstall, "--- list:");
                foreach (var def in mech.Inventory
                    .Where(i => i.MountedLocation == order.DesiredLocation && i.IsCategory(CategoryID))
                    .Select(i => i.Def))
                {
                    Control.LogDebug(DType.ComponentInstall,
                        $"---- list: {def.Description.Id}: Default:{def.IsDefault()}");
                }
            }
#endif

            Control.LogDebug(DType.ComponentInstall, $"-- total {n1}/{record.MaxEquiped}  location: {n2}/{record.MaxEquipedPerLocation}");

            var replace = mech.Inventory.FirstOrDefault(i => !i.IsModuleFixed(mech) && (i.MountedLocation == order.DesiredLocation || CategoryDescriptor.ReplaceAnyLocation) && i.IsCategory(CategoryID) && i.IsDefault());

            Control.LogDebug(DType.ComponentInstall, $"-- possible replace: {(replace == null ? "not found" : replace.ComponentDefID)}");

            if (replace == null)
                return;

            bool need_replace = (record.MaxEquiped > 0 && n1 > record.MaxEquiped) ||
                (record.MaxEquipedPerLocation > 0 && n2 > record.MaxEquipedPerLocation);

            Control.LogDebug(DType.ComponentInstall, $"-- need_repalce: {need_replace}");

            if (need_replace)
                DefaultHelper.RemoveInventory(replace.ComponentDefID, mech, replace.MountedLocation, replace.ComponentDefType);

        }

        public string ReplaceValidateDrop(MechLabItemSlotElement drop_item, ChassisLocations location, Queue<IChange> changes)
        {
            Control.LogDebug(DType.ComponentInstall, $"-- Category {CategoryID}");


            var mech = MechLabHelper.CurrentMechLab.MechLab.activeMechDef ;


            var record = CategoryDescriptor[mech];

            if (!CategoryDescriptor.AutoReplace || record == null ||
                !record.MaxLimited)
            {
                Control.LogDebug(DType.ComponentInstall, $"--- no replace needed");
                return String.Empty;
            }


            var removed = changes.OfType<RemoveChange>().Select(i => i.item.ComponentRef).ToList();

            var limits = record.LocationLimits.Where(i => i.Key.HasFlag(location) && i.Value.Max >= 0).ToList();
            var inventory = mech.Inventory
                .Where(i => !removed.Contains(i))
                .Select(i => new { item = i, def = i.IsDefault(), fixd = i.IsModuleFixed(mech), cat = i.IsCategory(CategoryID, out var c) ? c : null })
                .Where(i => i.cat != null)
                .ToList();

            var free_places = new List<free_record>();

            foreach (var limit in limits)
            {
                var free = new free_record();
                free.free = limit.Value.Max;
                free.locations = limit.Key;
                free.limit = limit.Value;
                foreach (var item_info in inventory.Where(i => limit.Key.HasFlag(i.item.MountedLocation)))
                {
                    if (item_info.fixd)
                        free.free -= item_info.cat.Weight;
                    else if (!item_info.def)
                    {
                        free.free -= item_info.cat.Weight;
                        free.can_free += item_info.cat.Weight;
                        free.items.Add( (item_info.item, item_info.cat.Weight) );
                    }
                }
                free.items.Sort((a,b) => a.weight.CompareTo(b.weight));
                free_places.Add(free);
            }

            var to_remove = new List<(MechComponentRef item, int weight)>();

            foreach (var free in free_places)
            {
                foreach (var item in to_remove.Where(i => free.locations.HasFlag(i.item.MountedLocation)))
                {
                    free.free += item.weight;
                    free.can_free -= item.weight;
                    free.items.RemoveAll(i => i.item == item.item);
                }

                if (free.free >= Weight)
                    continue;

                if (free.free + free.can_free < Weight)
                {
                    // 0 - Display Name, 1 - maximum, 2 - Mech Uiname, 3 - Mech Name
                    // 4 - Location, 5 - item name, 6 - item uiname
                    return new Localize.Text(CategoryDescriptor.AddMaximumReached, CategoryDescriptor.DisplayName,
                        free.limit.Max, mech.Description.UIName, mech.Description.Name,
                        free.locations == ChassisLocations.All ? "All Locations" : free.locations.ToString(),
                        drop_item.ComponentRef.Def.Description.Name, drop_item.ComponentRef.Def.Description.UIName
                        ).ToString();
                }

                int need_free = Weight - free.free;
                foreach(var item in free.items)
                {
                    if (need_free <= 0)
                        break;
                    to_remove.Add(item);
                    need_free -= item.weight;
                }
            }

            foreach (var item in to_remove)
            {
                var slot = MechLabHelper.CurrentMechLab.FullInventory.FirstOrDefault(i => i.ComponentRef == item.item);
                if (slot == null)
                {
                    Control.LogError($"Cannot find slot to remove for {item.item.ComponentDefID} in {item.item.MountedLocation}");
                }
                else
                    changes.Enqueue(new RemoveChange(slot.MountedLocation, slot));
            }

            return string.Empty;
        }

        public override string ToString()
        {
            return "Category: " + CategoryID;
        }

        public void ClearInventory(MechDef mech, List<MechComponentRef> result, SimGameState state, MechComponentRef source)
        {
            var item = DefaultFixer.Shared.GetReplaceFor(mech, CategoryID, source.MountedLocation, state);
            if (item != null)
                result.Add(item);
        }

        public void OnItemGrabbed(IMechLabDraggableItem item, MechLabPanel mechLab, MechLabLocationWidget widget)
        {
            Control.LogDebug(DType.ComponentInstall, $"- Category {CategoryID}");
            Control.LogDebug(DType.ComponentInstall, $"-- search replace for {item.ComponentRef.ComponentDefID}");

            var replace = DefaultFixer.Shared.GetReplaceFor(mechLab.activeMechDef, CategoryID, widget.loadout.Location, mechLab.sim);

            if (replace == null)
            {
                Control.LogDebug(DType.ComponentInstall, $"-- no replacement, skipping");
                return;
            }

            DefaultHelper.AddMechLab(replace);
            Control.LogDebug(DType.ComponentInstall, $"-- added {replace.ComponentDefID} to {replace.MountedLocation}");
            mechLab.ValidateLoadout(false);
        }



        public void AdjustDescription()
        {
            if (this.CategoryDescriptor.AddCategoryToDescription)
            {
                var ed = ExtendedDetails.ExtendedDetails.GetOrCreate(Def);
                var detail =
                    ed.GetDetails().FirstOrDefault(i => i.Identifier == "Category") as
                        ExtendedDetails.ExtendedDetailList ??
                    new ExtendedDetailList()
                    {
                        Index = 10,
                        Identifier = "Category",
                        OpenBracket = $"\n<b><color={Control.Settings.CategoryDescriptionColor}>[",
                        CloseBracket = "]</color></b>\n"
                    };

                detail.Values.Add(this.CategoryDescriptor.DisplayName);
                ed.AddDetail(detail);
            }
        }
        public bool ValidateDropOnAdd(MechLabItemSlotElement item, ChassisLocations location, Queue<IChange> changes, List<SlotInvItem> inventory)
        {
            if (CategoryDescriptor[MechLabHelper.CurrentMechLab.ActiveMech].MaxLimited)
                changes.Enqueue(new CategoryDefaultsAdjust(CategoryID));

            return false;

        }

        public bool ValidateDropOnRemove(MechLabItemSlotElement item, ChassisLocations location, Queue<IChange> changes, List<SlotInvItem> inventory)
        {
            if (CategoryDescriptor[MechLabHelper.CurrentMechLab.ActiveMech].MinLimited)
                changes.Enqueue(new CategoryDefaultsAdjust(CategoryID));

            return false;
        }
    }
}