using System.Diagnostics;
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
            foreach (var comp in mech.Inventory)
            {
                Control.Log($" -- {comp.MountedLocation} -- {comp.ComponentDefID} -- {comp.SimGameUID}");
            }
            Control.Log($"========== done ============");
        }
    }
}