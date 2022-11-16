using System;
using System.Collections.Generic;
using System.Linq;
using BattleTech;
using BattleTech.UI;
using CustomComponents.Changes;
using CustomComponents.ExtendedDetails;
using fastJSON;
using Localize;

namespace CustomComponents;

/// <summary>
/// component use category logic
/// </summary>
[CustomComponent(CategoryCustomName, true)]
public class Category : SimpleCustomComponent, IAfterLoad, IReplaceValidateDrop,
    IReplaceIdentifier, IAdjustDescription, IOnRemove, IOnAdd, IPreValidateDrop
{
    internal const string CategoryCustomName = "Category";

    private class free_record
    {
        public ChassisLocations locations;
        public CategoryLimit limit;
        public int free;
        public int can_free;
        public List<(InvItem item, int weight)> items = new();
    }

    /// <summary>
    /// name of category
    /// </summary>
    public string CategoryID { get; set; }

    /// <summary>
    /// optional tag for AllowMixTags, if not set defid will used
    /// </summary>
    public string Tag { get; set; } = "*";

    public int Weight { get; set; } = 1;

    public string ReplaceID => CategoryID;


    public string GetTag()
    {
        if (string.IsNullOrEmpty(Tag))
            return Def.Description.Id;
        else
            return Tag;
    }

    [JsonIgnore]
    public CategoryDescriptor CategoryDescriptor { get; set; }

    public void OnLoaded(Dictionary<string, object> values)
    {
        CategoryDescriptor = CategoryController.Shared.GetOrCreateCategory(CategoryID);
    }

    public string ReplaceValidateDrop(MechLabItemSlotElement drop_item, ChassisLocations location, Queue<IChange> changes)
    {

        bool check_removed(InvItem item, List<Change_Remove> removed)
        {
            var found = removed.FirstOrDefault(i =>
                i.ItemID == item.Item.ComponentDefID && i.Location == item.Location);
            if (found != null)
            {
                removed.Remove(found);
                return false;
            }

            return true;
        }

        Log.ComponentInstall.Trace?.Log($"-- Category {CategoryID}");


        var mech = MechLabHelper.CurrentMechLab.MechLab.activeMechDef;


        var record = CategoryDescriptor[mech];

        if (record == null || !record.MaxLimited)
        {
            Log.ComponentInstall.Trace?.Log($"--- no replace needed");
            return String.Empty;
        }


        var removed = changes.OfType<Change_Remove>().ToList();

        var limits = record.LocationLimits.Where(i => i.Key.HasFlag(location) && i.Value.Max >= 0).ToList();

        var defaults = DefaultsDatabase.Instance[mech];

        var inventory = MechLabHelper.CurrentMechLab.FullInventory
            .Where(i => check_removed(i, removed))
            .Select(i => new
            {
                item = i,
                canfree = defaults?.IsCatDefault(i.Item.ComponentDefID, CategoryID) ?? false,
                fixd = i.Item.IsModuleFixed(mech),
                def = i.Item.IsDefault(),
                cat = i.Item.IsCategory(CategoryID, out var c) ? c : null
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
            foreach (var item_info in inventory.Where(i => limit.Key.HasFlag(i.item.Location)))
            {
                if (item_info.fixd)
                    free.free -= item_info.cat.Weight;
                else if (!item_info.canfree)
                {
                    free.free -= item_info.cat.Weight;
                    if (!item_info.def)
                    {
                        free.items.Add((item_info.item, item_info.cat.Weight));
                        free.can_free += item_info.cat.Weight;
                    }
                }
            }
            free.items.Sort((a, b) => a.weight.CompareTo(b.weight));
            free_places.Add(free);
        }

        var to_remove = new List<(InvItem item, int weight)>();

        foreach (var free in free_places)
        {
            foreach (var item in to_remove.Where(i => free.locations.HasFlag(i.item.Location)))
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
                return new Text(CategoryDescriptor.AddMaximumReached, CategoryDescriptor._DisplayName,
                    free.limit.Max, mech.Description.UIName, mech.Description.Name,
                    free.locations == ChassisLocations.All ? "All Locations" : free.locations.ToString(),
                    drop_item.ComponentRef.Def.Description.Name, drop_item.ComponentRef.Def.Description.UIName
                ).ToString();
            }

            var need_free = Weight - free.free;

            foreach (var item in free.items)
            {
                if (need_free <= 0)
                    break;
                to_remove.Add(item);
                need_free -= item.weight;
            }
        }


        foreach (var item in to_remove.Select(i => i.item))
        {
            changes.Enqueue(new Change_Remove(item.Item.ComponentDefID, location));
        }

        return string.Empty;
    }

    public override string ToString()
    {
        return "Category: " + CategoryID + (Weight > 1 ? $":{Weight}" : "") + ":" + Tag;
    }

    public string PreValidateDrop(MechLabItemSlotElement item, ChassisLocations location)
    {
        if (CategoryDescriptor.AllowMixTagsMechlab || CategoryDescriptor.AllowMixTags || Tag == "*")
            return string.Empty;

        var check = MechLabHelper.CurrentMechLab.ActiveMech.Inventory
            .Select(i => i.GetCategory(CategoryID))
            .Where(i => i != null)
            .Any(i => i.Tag != "*" && i.Tag != Tag);

        return !check ? string.Empty : (new Text(CategoryDescriptor.ValidateMixed, CategoryDescriptor._DisplayName)).ToString();
    }


    public void AdjustDescription()
    {
        if (!CategoryDescriptor.AddCategoryToDescription)
        {
            return;
        }

        var ed = ExtendedDetails.ExtendedDetails.GetOrCreate(Def);
        var detail = ed.AddIfMissing(
            new ExtendedDetailList
            {
                Index = Control.Settings.CategoryDescriptionIndex,
                Identifier = "Category",
                OpenBracket = $"\n\nCategories: <b><color={Control.Settings.CategoryDescriptionColor}>",
                CloseBracket = "</color></b>"
            }
        );

        detail.AddUnique(Control.Settings.AddWeightToCategory && Weight > 1
            ? CategoryDescriptor._DisplayName + ":" + Weight
            : CategoryDescriptor._DisplayName);

        ed.RefreshDetails();
    }


    public void OnAdd(ChassisLocations location, InventoryOperationState state)
    {
        OnInventoryChange(state);
    }

    public void OnRemove(ChassisLocations location, InventoryOperationState state)
    {
        OnInventoryChange(state);
    }

    private void OnInventoryChange(InventoryOperationState state)
    {
        var defaults = DefaultsDatabase.Instance[state.Mech];
        var record = CategoryDescriptor[state.Mech];
        if (record == null)
        {
            return;
        }
        if (!record.Limited)
        {
            return;
        }
        if (defaults.IsSingleCatDefault(Def.Description.Id, CategoryID))
        {
            return;
        }
        state.AddChange(new Change_CategoryAdjust(CategoryID));
    }

}