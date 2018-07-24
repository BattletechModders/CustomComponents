using BattleTech;
using BattleTech.Data;
using Harmony;
using System;


namespace CustomComponents
{
    [HarmonyPatch(typeof(GameInstance))]
    [HarmonyPatch("DataManager", PropertyMethod.Setter)]
    public static class GameInstance_Constructor_Patch_noarg
    {
        public static void Postfix(DataManager value)
        {
            if(Control.Logger != null)
                Control.Logger.LogDebug($"GameInstance.ctor(), dm is null = {value == null}");
            Database.DataManager = value;
        }
    }
}
