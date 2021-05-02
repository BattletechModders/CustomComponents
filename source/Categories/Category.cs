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
    public class Category : SimpleCustomComponent, IAfterLoad,  IReplaceValidateDrop,
        IReplaceIdentifier, IAdjustDescription, IOnItemGrabbed,
        IAdjustValidateDrop
    {

        private class free_record
        {
            public ChassisLocations locations;
            public CategoryLimit limit;
            public int free;
            public int can_free;
            public List<(SlotInvItem item, int weight)> items = new List<(SlotInvItem item, int weight)>();
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


            var removed = changes.OfType<RemoveChange>().Select(i => i.item).ToList();

            var limits = record.LocationLimits.Where(i => i.Key.HasFlag(location) && i.Value.Max >= 0).ToList();
            var inventory = MechLabHelper.CurrentMechLab.FullInventory
                .Where(i => !removed.Contains(i.slot))
                .Select(i => new
                {
                    item = i, 
                    def = i.slot.ComponentRef.IsDefault(), 
                    fixd = i.slot.ComponentRef.IsModuleFixed(mech), 
                    cat = i.slot.ComponentRef.IsCategory(CategoryID, out var c) ? c : null
                })
                .Where(i => i.cat != null)
                .ToList();

            var free_places = new List<free_record>();

            foreach (var limit in limits)
            {
                var free = new free_record();
                free.free = limit.Value.Max;
                free.locations = limit.Key;
                free.limit = limit.Value;
                foreach (var item_info in inventory.Where(i => limit.Key.HasFlag(i.item.location)))
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

            var to_remove = new List<(SlotInvItem item, int weight)>();

            foreach (var free in free_places)
            {
                foreach (var item in to_remove.Where(i => free.locations.HasFlag(i.item.location)))
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


            foreach (var item in to_remove.Select(i => i.item))
            {
                changes.Enqueue(new RemoveChange(item.location, item.slot));
            }

            return string.Empty;
        }

        public override string ToString()
        {
            return "Category: " + CategoryID;
        }

        public void OnItemGrabbed(IMechLabDraggableItem item, MechLabPanel mechLab, ChassisLocations location)
        {
            void apply_changes(List<DefaultFixer.inv_change> changes)
            {

                foreach (var invChange in changes)
                {
                    if(invChange.IsAdd)
                        DefaultHelper.AddMechLab(invChange.Id, invChange.Type, invChange.Location);

                    else if (invChange.IsRemove)
                        DefaultHelper.RemoveMechLab(invChange.Id, invChange.Type, invChange.Location);
                }
            }

            Control.LogDebug(DType.ComponentInstall, $"- Category {CategoryID}");
            Control.LogDebug(DType.ComponentInstall, $"-- search replace for {item.ComponentRef.ComponentDefID}");

            var mechlab = MechLabHelper.CurrentMechLab;
            var mech = mechlab.ActiveMech;

        var record = CategoryDescriptor[mech];
            if (record?.MinLimited != true)
                return;

            var changes = DefaultFixer.Instance.GetMultiChange(mech, mech.Inventory.ToInvItems());
            if (changes != null)
                apply_changes(changes);
            changes = DefaultFixer.Instance.GetDefaultsChange(mech, mech.Inventory.ToInvItems(), CategoryID);
            if (changes != null)
                apply_changes(changes);

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
                        Identifier =  "Category",
                        OpenBracket = $"\n<b><color={Control.Settings.CategoryDescriptionColor}>[",
                        CloseBracket = "]</color></b>\n"
                    };

                detail.AddUnique((Control.Settings.AddWeightToCategory && Weight > 1) 
                    ? this.CategoryDescriptor.DisplayName + ":" + Weight.ToString() 
                    : this.CategoryDescriptor.DisplayName);
                ed.AddDetail(detail);
            }
        }
        public bool ValidateDropOnAdd(MechLabItemSlotElement item, ChassisLocations location, Queue<IChange> changes, List<SlotInvItem> inventory)
        {
            if (CategoryDescriptor[MechLabHelper.CurrentMechLab.ActiveMech].MinLimited && !Def.Flags<CCFlags>().CategoryDefault)
                changes.Enqueue(new CategoryDefaultsAdjust(CategoryID));

            return false;
        }

        public bool ValidateDropOnRemove(MechLabItemSlotElement item, ChassisLocations location, Queue<IChange> changes, List<SlotInvItem> inventory)
        {
            var f = Def.Flags<CCFlags>();
            if (CategoryDescriptor[MechLabHelper.CurrentMechLab.ActiveMech].MinLimited && !Def.Flags<CCFlags>().CategoryDefault)
                changes.Enqueue(new CategoryDefaultsAdjust(CategoryID));

            return false;
        }
    }
}