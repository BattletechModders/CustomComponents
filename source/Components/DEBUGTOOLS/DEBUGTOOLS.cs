using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using BattleTech;
using HBS.Collections;

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

        internal static void SHOWTAGS(List<MechDef> mechDefs, SimGameState simgame)
        {
            string list_to_string(TagSet list)
            {
                var builder = new StringBuilder("[");
                foreach (var tag in list)
                    builder.Append(tag + " ");
                builder.Append("]");
                return builder.ToString();
            }

            foreach (var def in mechDefs)
            {
                Control.LogDebug(DType.AutoFixFAKE, $"Mech: {def.Description.Id}");
                Control.LogDebug(DType.AutoFixFAKE, "-- mech tags: " + list_to_string(def.MechTags));
                Control.LogDebug(DType.AutoFixFAKE, "-- chassis tags: " + list_to_string(def.Chassis.ChassisTags));

            }
        }
    }
}