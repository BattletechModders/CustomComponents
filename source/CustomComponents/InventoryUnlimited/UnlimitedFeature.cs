using System.Collections.Generic;
using BattleTech;

namespace CustomComponents.InventoryUnlimited;

internal static class UnlimitedFeature
{
    internal static void Register(MechComponentDef def)
    {
        UnlimitedItems[$"Item.{def.GetType().Name}.{def.Description.Id}"] = def;
    }
    internal static void Clear()
    {
        UnlimitedItems.Clear();
    }

    internal static IReadOnlyCollection<string> UnlimitedItemStatIds => UnlimitedItems.Keys;
    internal static IReadOnlyCollection<MechComponentDef> UnlimitedItemDefs => UnlimitedItems.Values;
    private static readonly Dictionary<string, MechComponentDef> UnlimitedItems = new();

    internal static bool IsUnlimitedByStatItemId(string statItemId)
    {
        return UnlimitedItems.ContainsKey(statItemId);
    }

    internal const int UnlimitedCount = int.MaxValue / 3 + 441235;
}