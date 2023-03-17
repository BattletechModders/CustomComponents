using BattleTech;

namespace CustomComponents;

[HarmonyPatch(typeof(Contract),"AddMechComponentToSalvage")]
public static class Contract_AddMechComponentToSalvage
{
    [HarmonyPrefix]
    [HarmonyWrapSafe]
    public static void Prefix(ref bool __runOriginal, ref MechComponentDef def)
    {
        if (!__runOriginal)
        {
            return;
        }

        __runOriginal = CheckDefaults(ref def);
    }

    public static bool CheckDefaults(ref MechComponentDef def)
    {
        if (def == null)
        {
            Log.Main.Error?.Log("Get NULL component in salvage!");
            return false;
        }


        if (!def.Flags<CCFlags>().NoSalvage)
            return true;

        var lootable = def.GetComponent<LootableDefault>();

        if (lootable == null)
        {
            Log.SalvageProcess.Trace?.Log("---- default, no lootable - skipped");

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

        if (component == null || component.Flags<CCFlags>().NoSalvage)
        {
            Log.SalvageProcess.Trace?.Log($"---- default, lootable {lootable.ItemID} not found or notsalvagable - skipped");
            return false;
        }

        Log.SalvageProcess.Trace?.Log($"---- default, lootable {lootable.ItemID} replaced");

        def = component;

        return true;
    }
}