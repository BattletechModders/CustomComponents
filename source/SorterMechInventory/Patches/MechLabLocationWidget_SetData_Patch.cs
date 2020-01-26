using System;
using System.Collections.Generic;
using System.Linq;
using BattleTech.UI;
using Harmony;

namespace CustomComponents
{
    [HarmonyPatch(typeof(MechLabLocationWidget), "SetData")]
    public static class MechLabLocationWidget_SetData_Patch
    {
        public static void Postfix(List<MechLabItemSlotElement> ___localInventory)
        {
            try
            {
                Sorter.SortWidgetInventory(___localInventory);
            }
            catch (Exception e)
            {
                Control.LogError(e);
            }
        }

        public static readonly MechLabItemSlotElementSorter Sorter = new MechLabItemSlotElementSorter();
    }

    public class MechLabItemSlotElementSorter : IComparer<MechLabItemSlotElement>
    {
        private readonly SorterComparer comparer = new SorterComparer();

        public int Compare(MechLabItemSlotElement x, MechLabItemSlotElement y)
        {
            return comparer.Compare(x.ComponentRef.Def, y.ComponentRef.Def);
        }

        public void SortWidgetInventory(List<MechLabItemSlotElement> inventory)
        {
            // does a stable sort, same order elements stay in insertion order
            // every element without explicit order gets implicit order of 100
            var sortedInventory = inventory.OrderBy(element => element, this).ToList();
            inventory.Clear();

            for (var index = 0; index < sortedInventory.Count; index++)
            {
                var element = sortedInventory[index];
                inventory.Insert(index, element);
                element.gameObject.transform.SetSiblingIndex(index);

                //Control.Logger.LogDebug($"id={element.ComponentRef.Def.Description.Id} index={index} order={Order(element)}");
            }
        }
    }
}