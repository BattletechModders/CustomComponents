using System.Collections.Generic;
using BattleTech.Data;
using BattleTech.UI;
using Harmony;

namespace CustomComponents.DataManagerPerfomance
{
    //[HarmonyPatch(typeof(DataManager), "GraduateBackgroundRequest")]
    public static class DataManager_GraduateBackgroundRequest_Patch
    {
        private static List<DataManager.DataManagerLoadRequest> foregroundRequests;
        private static string _id = "";

        public static void Prefix(List<DataManager.DataManagerLoadRequest> ___foregroundRequests, string id)
        {
            foregroundRequests = ___foregroundRequests;
            _id = id;
        }

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return instructions
                .MethodReplacer(
                    AccessTools.Method(typeof(List<DataManager.DataManagerLoadRequest>), "Add"),
                    AccessTools.Method(typeof(DataManager_GraduateBackgroundRequest_Patch), "Add")
                );

        }

        public static void Add(List<DataManager.DataManagerLoadRequest> list,  DataManager.DataManagerLoadRequest dataManagerLoadRequest)
        {
            Control.Logger.LogDebug($"GraduateBackgroundRequest adding {_id}, {list.Count} items");
            list.Add(dataManagerLoadRequest);
            DMPHelper.Add(dataManagerLoadRequest, _id);
        }
    }
}