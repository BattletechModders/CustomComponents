using BattleTech.Data;
using Harmony;

namespace CustomComponents.DataManagerPerfomance
{
    //[HarmonyPatch(typeof(DataManager),"Clear")]
    public static class DataManager_Clear_Patch
    {
        public static void Prefix()
        {
            DMPHelper.ClearRequests();
        }
    }
}