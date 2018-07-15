using System.Collections.Generic;
using BattleTech.UI;
using Harmony;

namespace CustomComponents
{
    [HarmonyPatch(typeof(MechLabLocationWidget), "SetData")]
    public static class MechLabLocationWidget_SetData_Patch
    {
        public static void Postfix(List<MechLabItemSlotElement> ___localInventory)
        {
            var inventory = new List<MechLabItemSlotElement>(___localInventory);
            ___localInventory.Clear();
            foreach (var element in inventory)
            {
                if (element.ComponentRef.Def.Is<Sorter>(out var s) && s.Order >=0 && s.Order < ___localInventory.Count)
                {
                    ___localInventory.Insert(s.Order, element);
                    element.gameObject.transform.SetSiblingIndex(s.Order);
                }
                else
                {
                    ___localInventory.Add(element);
                    element.gameObject.transform.SetSiblingIndex(___localInventory.Count - 1);
                }
            }
        }
    }
}