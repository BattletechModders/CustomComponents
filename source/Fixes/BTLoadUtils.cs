using BattleTech;
using BattleTech.Data;
using System;

namespace CustomComponents
{
    class BTLoadUtils
    {
        internal static LoadRequest CreateLoadRequest(DataManager dataManager, Action<LoadRequest> loadCompleteCallback, bool filterByOwnership)
        {
            var loadRequest = dataManager.CreateLoadRequest(loadCompleteCallback, filterByOwnership);
            loadRequest.AddAllOfTypeBlindLoadRequest(BattleTechResourceType.HeatSinkDef, true);
            loadRequest.AddAllOfTypeBlindLoadRequest(BattleTechResourceType.UpgradeDef, true);
            loadRequest.AddAllOfTypeBlindLoadRequest(BattleTechResourceType.WeaponDef, true);
            loadRequest.AddAllOfTypeBlindLoadRequest(BattleTechResourceType.AmmunitionBoxDef, true);
            loadRequest.AddAllOfTypeBlindLoadRequest(BattleTechResourceType.JumpJetDef, true);
            return loadRequest;
        }
    }
}
