using BattleTech;

namespace CustomComponents;

//    [HarmonyPatch(typeof(Contract), "AddToFinalSalvage")]
internal static class Contract_AddToFilnaSalvagePatch
{
    [HarmonyPrefix]
    [HarmonyWrapSafe]
    public static void Prefix(ref bool __runOriginal, ref SalvageDef def)
    {
        if (!__runOriginal)
        {
            return;
        }

        __runOriginal = AddToFinalSalvage(ref def);
    }

    public static bool AddToFinalSalvage(ref SalvageDef def)
    {
        if (def.MechComponentDef == null)
        {
            return true;
        }


        if (!def.MechComponentDef.Flags<CCFlags>().NoSalvage)
        {
            return true;
        }

        var lootable = def.MechComponentDef.GetComponent<LootableDefault>();

        if (lootable == null)
        {
            return false;
        }

        MechComponentDef component = null;

        switch (def.ComponentType)
        {
            case ComponentType.AmmunitionBox:
                if(UnityGameInstance.BattleTechGame.DataManager.AmmoBoxDefs.Exists(lootable.ItemID))
                {
                    component = UnityGameInstance.BattleTechGame.DataManager.AmmoBoxDefs.Get(lootable.ItemID);
                }

                break;

            case ComponentType.Weapon:
                if (UnityGameInstance.BattleTechGame.DataManager.WeaponDefs.Exists(lootable.ItemID))
                {
                    component = UnityGameInstance.BattleTechGame.DataManager.WeaponDefs.Get(lootable.ItemID);
                }

                break;

            case ComponentType.Upgrade:
                if (UnityGameInstance.BattleTechGame.DataManager.UpgradeDefs.Exists(lootable.ItemID))
                {
                    component = UnityGameInstance.BattleTechGame.DataManager.UpgradeDefs.Get(lootable.ItemID);
                }

                break;

            case ComponentType.HeatSink:
                if (UnityGameInstance.BattleTechGame.DataManager.HeatSinkDefs.Exists(lootable.ItemID))
                {
                    component = UnityGameInstance.BattleTechGame.DataManager.HeatSinkDefs.Get(lootable.ItemID);
                }

                break;
            case ComponentType.JumpJet:
                if (UnityGameInstance.BattleTechGame.DataManager.JumpJetDefs.Exists(lootable.ItemID))
                {
                    component = UnityGameInstance.BattleTechGame.DataManager.JumpJetDefs.Get(lootable.ItemID);
                }

                break;
        }

        if (component == null || component.Flags<CCFlags>().NoSalvage)
        {
            return false;
        }

        var salvageDef = new SalvageDef();
        salvageDef.MechComponentDef = component;
        salvageDef.Description = new(component.Description);
        salvageDef.RewardID = def.RewardID;
        salvageDef.Type = SalvageDef.SalvageType.COMPONENT;
        salvageDef.ComponentType = def.ComponentType;
        salvageDef.Damaged = false;
        salvageDef.Weight = def.Weight;
        salvageDef.Count = 1;

        def = salvageDef;

        return true;
    }
}