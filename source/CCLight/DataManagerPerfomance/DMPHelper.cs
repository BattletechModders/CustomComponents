using System;
using System.Collections.Generic;
using BattleTech.Data;

namespace CustomComponents.DataManagerPerfomance
{
    public static class DMPHelper
    {
        private static Dictionary<string, DataManager.DataManagerLoadRequest> requests;

        public static void ClearRequests()
        {
            requests = new Dictionary<string, DataManager.DataManagerLoadRequest>();
        }

        public static void Add(DataManager.DataManagerLoadRequest dataManagerLoadRequest, string id)
        {
            try
            {
                if (requests == null || requests.TryGetValue(id, out var _))
                    return;
                requests.Add(id, dataManagerLoadRequest);
            }
            catch (Exception e)
            {
                Control.Logger.LogDebug($"Error in DMPHeler",e);
            }
        }

        public static DataManager.DataManagerLoadRequest Get(string id)
        {
            if (requests.TryGetValue(id, out var item))
                return item;
            return null;
        }
    }
}