using BattleTech;
using BattleTech.Data;
using System;
using SVGImporter;

namespace CustomComponents
{
    public class BTLoadUtils
    {
        internal static LoadRequest CreateLoadRequest(DataManager dataManager, Action<LoadRequest> loadCompleteCallback, bool filterByOwnership)
        {
            var loadRequest = dataManager.CreateLoadRequest(loadCompleteCallback, filterByOwnership);
            loadRequest.AddAllOfTypeBlindLoadRequest(GetResourceType(nameof(BattleTechResourceType.HeatSinkDef)), true);
            loadRequest.AddAllOfTypeBlindLoadRequest(GetResourceType(nameof(BattleTechResourceType.UpgradeDef)), true);
            loadRequest.AddAllOfTypeBlindLoadRequest(GetResourceType(nameof(BattleTechResourceType.WeaponDef)), true);
            loadRequest.AddAllOfTypeBlindLoadRequest(GetResourceType(nameof(BattleTechResourceType.AmmunitionBoxDef)), true);
            loadRequest.AddAllOfTypeBlindLoadRequest(GetResourceType(nameof(BattleTechResourceType.JumpJetDef)), true);
            return loadRequest;
        }

        // BattleTechResourceType is generated and doesn't have to have deterministic values between releases
        // this makes sure to return the correct type

        public static BattleTechResourceType GetResourceType(string name)
        {
            return (BattleTechResourceType)Enum.Parse(typeof(BattleTechResourceType), name);
        }
    }
}
