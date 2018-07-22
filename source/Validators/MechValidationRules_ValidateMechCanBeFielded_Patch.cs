#undef CCDEBUG

using System;
using System.Linq;
using BattleTech;
using Harmony;



namespace CustomComponents
{
    [HarmonyPatch(typeof(MechValidationRules), "ValidateMechCanBeFielded")]
    internal static class MechValidationRules_ValidateMechCanBeFielded_Patch
    {
        public static void Postfix(MechDef mechDef, ref bool __result)
        {
            try
            {
                if (mechDef == null)
                {
#if CCDEBUG
                    Control.Logger.LogDebug($"Mech validation for NULL return");
#endif
                    return;
                }

#if CCDEBUG
                Control.Logger.LogDebug($"Mech validation for {mechDef.Name} starter current resutl {__result}");
#endif

                if (!__result)
                {
#if CCDEBUG
                    Control.Logger.LogDebug($"- ended at base validation");
#endif
                    return;
                }

#if CCDEBUG
                Control.Logger.LogDebug($"- statrted fixed validation");
#endif
                if (!Validator.ValidateMechCanBeFielded(mechDef))
                {
                    __result = false;
#if CCDEBUG
                    Control.Logger.LogDebug($"- ended at fixed validation");
#endif
                    return;
                }
#if CCDEBUG
                Control.Logger.LogDebug($"- statrted component validation");
#endif
                foreach (var component in mechDef.Inventory)
                {
                    foreach (var mechValidate in component.GetComponents<IMechValidate>())
                    {

#if CCDEBUG
                        Control.Logger.LogDebug($"-- {mechValidate.GetType()}");
#endif
                        if (!mechValidate.ValidateMechCanBeFielded(mechDef, component))
                        {
                            __result = false;
#if CCDEBUG
                            Control.Logger.LogDebug($"- ended at component validation");
#endif
                            return;
                        }
                    }
                }

#if CCDEBUG
                Control.Logger.LogDebug($"- and done");
#endif
            }

            catch (Exception e)
            {
                Control.Logger.LogError(e);
            }
        }
    }
}