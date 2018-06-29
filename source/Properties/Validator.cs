using BattleTech;
using BattleTech.UI;
using Harmony;
using System;
using System.Collections.Generic;
using System.Text;

namespace CustomComponents.Properties
{
    public delegate  bool ValidateAddDelegate(MechComponentDef component,
            MechLabLocationWidget widget, bool current_result, ref string errorMessage,
            MechLabPanel mechlab, int usedslots, int maxslots, List<MechLabItemSlotElement> localInventory);


    public static class Validator
    {
        static Dictionary<Type, ValidateAddDelegate> add_validators = new Dictionary<Type, ValidateAddDelegate>();

        public static void RegisterAddValidator(Type type, ValidateAddDelegate validator)
        {
            
        }

        internal static bool ValidateAdd(MechComponentDef component,
            MechLabLocationWidget widget, ref bool result, ref string errorMessage,
            MechLabPanel mechlab, int usedslots, int maxslots, List<MechLabItemSlotElement> localInventory
        )
        {

        }
    }

    [HarmonyPatch(typeof(MechLabLocationWidget), "ValidateAdd", new Type[] { typeof(MechComponentDef) })]
    internal static class MechLabLocationWidget_ValidateAdd_Patch
    {
        internal static void Postfix(MechComponentDef newComponentDef,
            MechLabLocationWidget __instance, ref bool __result, ref string ___dropErrorMessage,
            MechLabPanel ___mechLab, int ___usedSlots, int ___maxSlots, List<MechLabItemSlotElement> ___localInventory
        )
        {
            if (__result)
                return;

            if (newComponentDef is IWeightLimited)
            {
                var limit = newComponentDef as IWeightLimited;

                if (___mechLab.activeMechDef.Chassis.Tonnage != limit.AllowedTonnage)
                {
                    __result = false;
                    ___dropErrorMessage = string.Format("{0} designed for {1}t mech", newComponentDef.Description.Name.ToUpper(), limit.AllowedTonnage);
                }

            }
        }
    }
}
