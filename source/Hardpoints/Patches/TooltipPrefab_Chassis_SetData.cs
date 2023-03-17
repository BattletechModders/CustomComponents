using BattleTech;
using BattleTech.UI.TMProWrapper;
using BattleTech.UI.Tooltips;

namespace CustomComponents.Debug;

[HarmonyPatch(typeof(TooltipPrefab_Chassis))]
[HarmonyPatch("SetData")]
public static class TooltipPrefab_Chassis_SetData
{
    [HarmonyPostfix]
    [HarmonyWrapSafe]
    public static void Postfix(object data, TooltipPrefab_Chassis __instance, LocalizableText ___jumpjetHPText)
    {
        //Control.Log("tooltip mech");
        var handler = __instance.GetComponent<TooltipHPHandler>();
        if (handler == null)
        {
            //Control.Log("creating");
            handler = __instance.gameObject.AddComponent<TooltipHPHandler>();
            handler.Init(__instance, ___jumpjetHPText.transform.parent.gameObject);
        }

        var chassis = data as ChassisDef;
        if (chassis != null)
        {
            //Control.Log($"set data for {mech.Description.Id}");
            var usage = chassis.GetHardpoints();
            handler.SetDataTotal(usage);
            handler.SetJJ(chassis);
        }
    }
}


[HarmonyPatch(typeof(TooltipPrefab_Chassis))]
[HarmonyPatch("SetHardpointData")]
public static class TooltipPrefab_Chassis_SetHardpointData
{
    [HarmonyPrefix]
    public static void Prefix(ref bool __runOriginal)
    {
        __runOriginal = false;
    }
}