using System;
using System.Collections.Generic;
using BattleTech.Data;
using Harmony;

namespace CustomComponents.DataManagerPerfomance
{
    //[HarmonyPatch(typeof(DataManager), "RequestResource_Internal")]
    public static class DataManager_RequestResource_Internal_Patch
    {
        private static string _id;
        private static List<DataManager.DataManagerLoadRequest> foregroundRequests;

        public static void Prefix(string identifier, List<DataManager.DataManagerLoadRequest> ___foregroundRequests)
        {
            foregroundRequests = ___foregroundRequests;
            _id = identifier;
        }
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return instructions
                .MethodReplacer(
                    AccessTools.Method(typeof(List<DataManager.DataManagerLoadRequest>), "Find"),
                    AccessTools.Method(typeof(DataManager_RequestResource_Internal_Patch), "Find")
                ).MethodReplacer(
                    AccessTools.Method(typeof(List<DataManager.DataManagerLoadRequest>), "Add"),
                    AccessTools.Method(typeof(DataManager_RequestResource_Internal_Patch), "Add")
                ); ;
        }

        public static DataManager.DataManagerLoadRequest Find(List<DataManager.DataManagerLoadRequest> list, Predicate<DataManager.DataManagerLoadRequest> predicate)
        {
            //Control.Logger.LogDebug($"RequestResource_Internal search {_id}, {list.Count} items");
            return DMPHelper.Get(_id);
        }

        public static void Add(List<DataManager.DataManagerLoadRequest> list, DataManager.DataManagerLoadRequest dataManagerLoadRequest)
        {
            //Control.Logger.LogDebug($"RequestResource_Internal GraduateBackgroundRequest adding {_id}, {list.Count} items");
            list.Add(dataManagerLoadRequest);
            DMPHelper.Add(dataManagerLoadRequest, _id);
        }
    }
}