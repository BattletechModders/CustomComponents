using BattleTech;
using BattleTech.UI.TMProWrapper;
using BattleTech.UI.Tooltips;

namespace CustomComponents.Patches;

[HarmonyPatch(typeof(TooltipPrefab_Mech))]
[HarmonyPatch("SetData")]
public static class TooltipPrefab_Mech_SetData
{
    [HarmonyPostfix]
    [HarmonyWrapSafe]
    public static void Postfix(object data, TooltipPrefab_Mech __instance, LocalizableText ___JumpjetHP)
    {
        //Control.Log("tooltip mech");
        var handler = __instance.GetComponent<TooltipHPHandler>();
        if (handler == null)
        {
            //Control.Log("creating");
            handler = __instance.gameObject.AddComponent<TooltipHPHandler>();
            handler.Init(__instance, ___JumpjetHP.transform.parent.gameObject);
        }

        var mech = data as MechDef;
        if (mech != null)
        {
            //Control.Log($"set data for {mech.Description.Id}");
            var usage = mech.GetHardpointUsage();
            handler.SetData(usage);
            handler.SetJJ(mech);
        }
    }
}

[HarmonyPatch(typeof(TooltipPrefab_Mech))]
[HarmonyPatch("SetHardpointData")]
public static class TooltipPrefab_Mech_SetHardpointData
{
    [HarmonyPrefix]
    [HarmonyWrapSafe]
    public static void Prefix(ref bool __runOriginal, TooltipPrefab_Mech __instance)
    {
        __runOriginal = false;
    }
}