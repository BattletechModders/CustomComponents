using BattleTech.UI;
using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BattleTech;

namespace CustomComponents.Patches
{
    [HarmonyPatch(typeof(MechLabLocationWidget))]
    [HarmonyPatch("ShowHighlightFrame")]
    public static class MechLabLocationWidget_ShowHighlightFrame
    {
        [HarmonyPrefix]
        public static bool ShowHighlightFrame(MechLabLocationWidget __instance, MechComponentRef cRef, WeaponDef wDef, bool isOriginalLocation, bool canBeAdded)
        {
            if (cRef == null)
            {
                __instance.ShowHighlightFrame(false, UIColor.Gold);
                return false;
            }

            var mech = MechLabHelper.CurrentMechLab.ActiveMech;

            var allowed = cRef.Is<IAllowedLocations>(out var al) ? al.GetLocationsFor(mech) : cRef.Def.AllowedLocations;

            var show = (allowed & __instance.loadout.Location) > ChassisLocations.None;
            if (wDef != null)
            {
                wDef.WeaponCategoryValue.ID;

            }


            return false;
        }
    }
}
