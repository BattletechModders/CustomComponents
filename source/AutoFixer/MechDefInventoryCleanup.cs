using System.Collections.Generic;
using System.Linq;
using BattleTech;

namespace CustomComponents;

internal static class MechDefInventoryCleanup
{
    internal static void RemoveEmptyRefs(List<MechDef> mechDefs)
    {
        foreach (var mechDef in mechDefs)
        {
            if (mechDef.Inventory.All(i => i?.Def != null))
            {
                continue;
            }

            Log.Main.Error?.Log($"Found NULL in {mechDef.Name}({mechDef.Description.Id})");

            foreach (var r in mechDef.Inventory)
            {
                if (r.Def == null)
                {
                    Log.Main.Error?.Log($"--- NULL --- {r.ComponentDefID}");
                }
            }

            mechDef.SetInventory(mechDef.Inventory.Where(i => i.Def != null).ToArray());
        }
    }
}