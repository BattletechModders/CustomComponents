using System.Diagnostics;
using System.Linq;
using BattleTech;

namespace CustomComponents
{
    internal class DEBUGTOOLS
    {
        public static bool NEEDTOSHOW = false;

        [Conditional("CCDEBUG")]
        public static void ShowInventory(MechDef mech)
        {
            Control.Log($"SHOW INVENTORY FOR {mech.Name}");
            foreach (var comp in mech.Inventory.OrderBy(i=>i.MountedLocation))
            {
                Control.Log($" -- {comp.MountedLocation} -- {comp.ComponentDefID} -- F:{comp.IsFixed} -- {comp.SimGameUID}");
            }
            Control.Log($"========== done ============");
        }
    }
}