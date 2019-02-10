using BattleTech;
using Harmony;

namespace CustomComponents.Patches
{
    [HarmonyPatch(typeof(Contract),"AddMechComponentToSalvage")]
    public static class Contract_AddMechComponentToSalvage
    {
        [HarmonyPrefix]
        public static bool CheckDefaults(ref MechComponentDef def)
        {
            if (def == null)
            {
                Control.LogError("Get NULL component in salvage!");
                return false;
            }


            var flags = def.GetComponent<Flags>();

            if (flags == null || !flags.NotSalvagable)
                return true;

            var lootable = def.GetComponent<LootableDefault>();

            if (lootable == null)
            {
                Control.LogDebug(DType.SalvageProccess, $"---- default, no lootable - skipped");

                return false;
            }

            MechComponentDef component = null;

            switch (def.ComponentType)
            {
                case ComponentType.AmmunitionBox:
                    if (UnityGameInstance.BattleTechGame.DataManager.AmmoBoxDefs.Exists(lootable.ItemID))
                        component = UnityGameInstance.BattleTechGame.DataManager.AmmoBoxDefs.Get(lootable.ItemID);
                    break;

                case ComponentType.Weapon:
                    if (UnityGameInstance.BattleTechGame.DataManager.WeaponDefs.Exists(lootable.ItemID))
                        component = UnityGameInstance.BattleTechGame.DataManager.WeaponDefs.Get(lootable.ItemID);
                    break;

                case ComponentType.Upgrade:
                    if (UnityGameInstance.BattleTechGame.DataManager.UpgradeDefs.Exists(lootable.ItemID))
                        component = UnityGameInstance.BattleTechGame.DataManager.UpgradeDefs.Get(lootable.ItemID);
                    break;

                case ComponentType.HeatSink:
                    if (UnityGameInstance.BattleTechGame.DataManager.HeatSinkDefs.Exists(lootable.ItemID))
                        component = UnityGameInstance.BattleTechGame.DataManager.HeatSinkDefs.Get(lootable.ItemID);
                    break;
                case ComponentType.JumpJet:
                    if (UnityGameInstance.BattleTechGame.DataManager.JumpJetDefs.Exists(lootable.ItemID))
                        component = UnityGameInstance.BattleTechGame.DataManager.JumpJetDefs.Get(lootable.ItemID);
                    break;
            }

            if (component == null || (component.Is<Flags>(out flags) && flags.NotSalvagable))
            {
                Control.LogDebug(DType.SalvageProccess, $"---- default, lootable {lootable.ItemID} not found or notsalvagable - skipped");
                return false;
            }
            Control.LogDebug(DType.SalvageProccess, $"---- default, lootable {lootable.ItemID} replaced");

            def = component;

            return true;
        }
    }
}