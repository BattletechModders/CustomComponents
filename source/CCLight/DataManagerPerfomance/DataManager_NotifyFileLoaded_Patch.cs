using System;
using System.Collections.Generic;
using BattleTech.Data;
using Harmony;

namespace CustomComponents.DataManagerPerfomance
{
    //[HarmonyPatch(typeof(DataManager), "NotifyFileLoaded")]
    public static class DataManager_NotifyFileLoaded_Patch
    {
        private static List<DataManager.DataManagerLoadRequest> foregroundRequests;

        public static void Prefix(List<DataManager.DataManagerLoadRequest> ___foregroundRequests)
        {
            foregroundRequests = ___foregroundRequests;
        }

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return instructions
                .MethodReplacer(
                    AccessTools.Method(typeof(List<DataManager.DataManagerLoadRequest>), "Clear"),
                    AccessTools.Method(typeof(DataManager_NotifyFileLoaded_Patch), "Clear")
                );
        }

        public static void Clear(List<DataManager.DataManagerLoadRequest> list)
        {
            //Control.Logger.LogDebug($"NotifyFileLoaded clear {list.Count} items");
            list.Clear();
            DMPHelper.ClearRequests();
        }
    }
}