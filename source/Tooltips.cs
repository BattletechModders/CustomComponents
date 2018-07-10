
using System;
using System.Collections.Generic;
using System.Reflection;
using BattleTech;
using BattleTech.UI.Tooltips;
using Harmony;

namespace CustomComponents
{
    public static class TooltipHelper
    {
        public static Type GetTooltipType(MechComponentDef def)
        {
            var defHandler = TooltipUtilities.MechComponentDefHandlerForTooltip(def) as MechComponentDef;
            return defHandler?.GetType();
        }
    }

    [HarmonyPatch(typeof(TooltipManager), "SetActiveTooltip")]
    public static class TooltipManagerSetActiveTooltipPatch
    {
        private static ICustomComponent_old customComponent;

        public static void Prefix(object data)
        {
            customComponent = data as ICustomComponent_old;
        }

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return instructions
                .MethodReplacer(
                    AccessTools.Property(typeof(MemberInfo), "Name").GetGetMethod(),
                    AccessTools.Method(typeof(TooltipManagerSetActiveTooltipPatch), "OverrideName")
                );
        }

        public static void Postfix()
        {
            customComponent = null;
        }

        public static string OverrideName(this MemberInfo @this)
        {
            var name = customComponent == null ? @this.Name : customComponent.TooltipType.Name;
            return name;
        }
    }

    [HarmonyPatch(typeof(TooltipUtilities), "MechComponentDefHandlerForTooltip")]
    public static class TooltipUtilitiesMechComponentDefHandlerForTooltipPatch
    {
        public static bool Prefix(MechComponentDef baseDef, ref object __result)
        {
            __result = baseDef as ICustomComponent;
            return __result == null;
        }
    }
}
