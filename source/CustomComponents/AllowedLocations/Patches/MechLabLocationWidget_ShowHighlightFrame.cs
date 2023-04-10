using System.Linq;
using BattleTech;
using BattleTech.UI;

namespace CustomComponents.Patches;

[HarmonyPatch(typeof(MechLabLocationWidget))]
[HarmonyPatch("ShowHighlightFrame")]
[HarmonyPatch(new[] {typeof(MechComponentRef), typeof(WeaponDef), typeof(bool), typeof(bool)})]
public static class MechLabLocationWidget_ShowHighlightFrame
{
    [HarmonyPrefix]
    [HarmonyWrapSafe]
    public static void Prefix(ref bool __runOriginal, MechLabLocationWidget __instance, MechComponentRef cRef, bool isOriginalLocation, bool canBeAdded)
    {
        if (!__runOriginal)
        {
            return;
        }

        if (cRef == null)
        {
            __instance.ShowHighlightFrame(false);
            __runOriginal = false;
            return;
        }

        var show = !cRef.Def.CCFlags().NoRemove;
        if (show)
        {
            show = !cRef.IsFixed;
        }

        var location = __instance.loadout.Location;
        if (show)
        {
            var mech = MechLabHelper.CurrentMechLab.ActiveMech;
            var allowed = cRef.Is<IAllowedLocations>(out var al) ? al.GetLocationsFor(mech) : cRef.Def.AllowedLocations;
            show = (allowed & location) > ChassisLocations.None;
        }

        if (show)
        {
            var use_hp = cRef.Def.GetComponent<UseHardpointCustom>();
            var replace = cRef.GetComponent<ReplaceHardpoint>();


            if (use_hp != null && !use_hp.WeaponCategory.Is_NotSet || replace != null && replace.Valid)
            {
                var lhelper = MechLabHelper.CurrentMechLab.GetLocationHelper(location);
                var wc = use_hp == null ? replace.UseWeaponCategory : use_hp.WeaponCategory;

                show = lhelper.HardpointsUsage?.Any(i => i.hpInfo.CompatibleID.Contains(wc.ID)) ?? false;
            }
        }

        __instance.ShowHighlightFrame(show, isOriginalLocation ? UIColor.Blue : UIColor.Gold);
        __runOriginal = false;
    }
}