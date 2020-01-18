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
            loadRequest.AddAllOfTypeLoadRequest<HeatSinkDef>(BattleTechResourceType.HeatSinkDef, null);
            loadRequest.AddAllOfTypeLoadRequest<UpgradeDef>(BattleTechResourceType.UpgradeDef, null);
            loadRequest.AddAllOfTypeLoadRequest<WeaponDef>(BattleTechResourceType.WeaponDef, null);
            loadRequest.AddAllOfTypeLoadRequest<AmmunitionBoxDef>(BattleTechResourceType.AmmunitionBoxDef, null);
            loadRequest.AddAllOfTypeLoadRequest<JumpJetDef>(BattleTechResourceType.AmmunitionBoxDef, null);
            return loadRequest;
        }
    }
}
