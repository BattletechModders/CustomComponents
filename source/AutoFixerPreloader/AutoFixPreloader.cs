using System;
using System.Collections.Generic;
using BattleTech;
using BattleTech.Data;
using Harmony;

namespace CustomComponents
{
    // makes sure all mech components are loaded in advanced (otherwise only components already installed in mechs are loaded)
    public static class AutoFixPreloader
    {
        internal static IEnumerable<CodeInstruction> ReplaceCreateLoadRequest(IEnumerable<CodeInstruction> instructions)
        {
            return instructions.MethodReplacer(
                AccessTools.Method(typeof(DataManager), nameof(DataManager.CreateLoadRequest)),
                AccessTools.Method(typeof(AutoFixPreloader), nameof(CreateLoadRequest))
            );
        }

        public static LoadRequest CreateLoadRequest(DataManager dataManager, Action<LoadRequest> loadCompleteCallback, bool filterByOwnership)
        {
            var loadRequest = dataManager.CreateLoadRequest(loadCompleteCallback, filterByOwnership);

            loadRequest.AddAllOfTypeLoadRequest<AmmunitionBoxDef>(BattleTechResourceType.AmmunitionBoxDef, null, true);
            loadRequest.AddAllOfTypeLoadRequest<HeatSinkDef>(BattleTechResourceType.HeatSinkDef, null, true);
            loadRequest.AddAllOfTypeLoadRequest<JumpJetDef>(BattleTechResourceType.JumpJetDef, null, true);
            loadRequest.AddAllOfTypeLoadRequest<UpgradeDef>(BattleTechResourceType.UpgradeDef, null, true);
            loadRequest.AddAllOfTypeLoadRequest<WeaponDef>(BattleTechResourceType.WeaponDef, null, true);

            return loadRequest;
        }
    }
}