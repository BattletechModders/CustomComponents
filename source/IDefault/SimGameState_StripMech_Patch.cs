using BattleTech;
using Harmony;
using System.Linq;

namespace CustomComponents
{
    [HarmonyPatch(typeof(SimGameState), "StripMech")]
    internal static class SimGameState_StripMech_Patch
    {
        public static void Prefix(MechDef def)
        {
            Control.Logger.LogDebug("SimGameState.StripMech - clear ICannotRemove");
            Control.Logger.LogDebug("SimGameState.StripMech - before clear");
            foreach(var item in def.Inventory)
            {
                Control.Logger.LogDebug(item.ComponentDefID);
            }


            def.SetInventory(def.Inventory.Where(i => !(i.Def is ICannotRemove)).ToArray());


            Control.Logger.LogDebug("SimGameState.StripMech - after clear");
            foreach (var item in def.Inventory)
            {
                Control.Logger.LogDebug(item.ComponentDefID);
            }
        }
    }
}
