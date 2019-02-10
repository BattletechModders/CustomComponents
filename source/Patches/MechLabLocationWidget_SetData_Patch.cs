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
            Sorter.SortWidgetInventory(___localInventory);
        }

        public static readonly MechLabItemSlotElementSorter Sorter = new MechLabItemSlotElementSorter();
    }

    public class MechLabItemSlotElementSorter : IComparer<MechLabItemSlotElement>
    {
        private static int Order(MechLabItemSlotElement element)
        {
            return element.ComponentRef.Def.GetComponent<ISorter>()?.Order ?? 100;
        }

        public int Compare(MechLabItemSlotElement x, MechLabItemSlotElement y)
        {
            return Order(x) - Order(y);
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