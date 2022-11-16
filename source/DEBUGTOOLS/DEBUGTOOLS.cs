using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using BattleTech;
using HBS.Collections;
using UnityEngine;
using Object = System.Object;

namespace CustomComponents;

internal class DEBUGTOOLS
{
    public static bool NEEDTOSHOW = false;

    [Conditional("DEBUG")]
    public static void ShowInventory(MechDef mech)
    {
        Log.Main.Info?.Log($"SHOW INVENTORY FOR {mech.Name}");
        foreach (var comp in mech.Inventory.OrderBy(i => i.MountedLocation))
        {
            Log.Main.Info?.Log($" -- {comp.MountedLocation} -- {comp.ComponentDefID} -- F:{comp.IsFixed} -- {comp.SimGameUID}");
        }

        Log.Main.Info?.Log("========== done ============");
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
            Log.AutoFixFAKE.Trace?.Log($"Mech: {def.Description.Id}");
            Log.AutoFixFAKE.Trace?.Log("-- mech tags: " + list_to_string(def.MechTags));
            Log.AutoFixFAKE.Trace?.Log("-- chassis tags: " + list_to_string(def.Chassis.ChassisTags));

        }
    }

    public static void ShowHierarchy(GameObject go)
    {
        if (go == null)
            return;
        Log.Main.Info?.Log($"show hierarchy for {go.name}");
        walk(go, "");
    }

    private static void walk(GameObject root, string prefix)
    {
        if (root == null)
            return;
        Log.Main.Info?.Log(prefix + $" GameObject:[{root.name}]");
        Log.Main.Info?.Log(prefix + " |Components:");

        var rect = root.GetComponent<RectTransform>();
        if ( rect != null)
        {
            Log.Main.Info?.Log(prefix + $" |- Rect pos:({rect.anchoredPosition.x}, {rect.anchoredPosition.y}) size:{rect.sizeDelta.x}, {rect.sizeDelta.y})");
        }

        foreach (var component in root.GetComponents<Object>())
        {
            Log.Main.Info?.Log(prefix + $" |- {component?.GetType()}");
        }

        if (root.transform.childCount > 0)
        {
            Log.Main.Info?.Log(prefix + " |Childs:");
            foreach (Transform transform in root.transform)
            {
                walk(transform.gameObject, prefix + "  ");
            }
        }
    }
}