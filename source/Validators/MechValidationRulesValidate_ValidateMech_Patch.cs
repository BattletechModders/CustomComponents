using System.Collections.Generic;
using System.Linq;
using BattleTech;
using Harmony;

namespace CustomComponents
{
    [HarmonyPatch(typeof(MechValidationRules), "ValidateMechDef")]
    internal static class MechValidationRulesValidate_ValidateMech_Patch
    {
        public static void Postfix(Dictionary<MechValidationType, List<string>> __result,
            MechValidationLevel validationLevel, MechDef mechDef)
        {
            //Control.Logger.Log($"{UnityEngine.Time.realtimeSinceStartup} CC.Validator start");

            Validator.ValidateMech(__result, validationLevel, mechDef);
            foreach (var component in mechDef.Inventory.Where(i => i.Def != null)
                .Select(i => i.Def)
                .GroupBy(i => i.Description.Id)
                .Select(i => i.First())
                .OfType<IMechValidate>())
            {
                component.ValidateMech(__result, validationLevel, mechDef);
            }
            //Control.Logger.Log($"{UnityEngine.Time.realtimeSinceStartup} CC.Validator end");
        }
    }
}