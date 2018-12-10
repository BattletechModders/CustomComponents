using BattleTech;

namespace CustomComponents
{
    internal class DEBUGTOOLS
    {
        public static bool NEEDTOSHOW = false;

        public static void ShowInventory(MechDef mech)
        {

            Control.Logger.LogDebug($"SHOW INVENTORY FOR {mech.Name}");
            foreach (var comp in mech.Inventory)
            {
                Control.Logger.LogDebug($" -- {comp.MountedLocation} -- {comp.ComponentDefID} -- {comp.SimGameUID}");
            }
            Control.Logger.LogDebug($"========== done ============");
        }
    }
}