using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using BattleTech;
using HBS.Collections;
using UnityEngine;
using Object = System.Object;

namespace CustomComponents
{
    internal class DEBUGTOOLS
    {
        public static bool NEEDTOSHOW = false;

        [Conditional("CCDEBUG")]
        public static void ShowInventory(MechDef mech)
        {
            Control.Log($"SHOW INVENTORY FOR {mech.Name}");
            foreach (var comp in mech.Inventory.OrderBy(i => i.MountedLocation))
            {
                Control.Log(
                    $" -- {comp.MountedLocation} -- {comp.ComponentDefID} -- F:{comp.IsFixed} -- {comp.SimGameUID}");
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

        public static void ShowHierarchy(GameObject go)
        {
            if (go == null)
                return;
            Control.Log($"show hierarchy for {go.name}");
            walk(go, "");
        }

        private static void walk(GameObject root, string prefix)
        {
            if (root == null)
                return;
            Control.Log(prefix + $" GameObject:[{root.name}]");
            Control.Log(prefix + $" |Components:");

            var rect = root.GetComponent<RectTransform>();
            if ( rect != null)
            {
                Control.Log(prefix + $" |- Rect pos:({rect.anchoredPosition.x}, {rect.anchoredPosition.y}) size:{rect.sizeDelta.x}, {rect.sizeDelta.y})");
            }

            foreach (var component in root.GetComponents<Object>())
            {
                Control.Log(prefix + $" |- {component?.GetType()}");
            }

            if (root.transform.childCount > 0)
            {
                Control.Log(prefix + $" |Childs:");
                foreach (Transform transform in root.transform)
                {
                    walk(transform.gameObject, prefix + "  ");
                }
            }
        }
    }
}