using System;
using System.Collections.Generic;
using System.Linq;
using BattleTech;
using Harmony;
using UnityEngine;

namespace CustomComponents;

[HarmonyPatch(typeof(Contract), "GenerateSalvage")]
public static class Contract_GenerateSalvage
{
    public static bool IsDestroyed(MechDef mech)
    {
        if (mech.IsDestroyed)
            return true;

        if (Control.Settings.CheckCriticalComponent && mech.Inventory.Any(i =>
                i.Def.CriticalComponent && i.DamageLevel == ComponentDamageLevel.Destroyed))
            return true;

        return mech.Inventory.Any(item => (item.DamageLevel == ComponentDamageLevel.Destroyed && item.Flags<CCFlags>().Vital) || item.GetComponents<IIsDestroyed>().Any(isDestroyed => isDestroyed.IsMechDestroyed(item, mech)));
    }

    [HarmonyPrefix]
    [HarmonyPriority(Priority.Low)]
    public static bool GenerateSalvage(List<UnitResult> enemyMechs, List<VehicleDef> enemyVehicles,
        List<UnitResult> lostUnits, bool logResults,
        Contract __instance, ref List<SalvageDef> ___finalPotentialSalvage)
    {
        if (!Control.Settings.OverrideSalvageGeneration)
            return true;

        try
        {
            Log.SalvageProcess.Trace?.Log($"Start GenerateSalvage for {__instance.Name}");

            ___finalPotentialSalvage = new List<SalvageDef>();

            var contract = __instance;

            contract.SalvagedChassis = new List<SalvageDef>();
            contract.LostMechs = new List<MechDef>();
            contract.SalvageResults = new List<SalvageDef>();

            var simgame = __instance.BattleTechGame.Simulation;
            if (simgame == null)
            {
                Log.Main.Error?.Log("No simgame - cancel salvage");
                return false;
            }

            var Constants = simgame.Constants;

            Log.SalvageProcess.Trace?.Log($"- Lost Units {__instance.Name}");
            for (int i = 0; i < lostUnits.Count; i++)
            {
                var mech = lostUnits[i].mech;

                if (!IsDestroyed(mech))
                {
                    Log.SalvageProcess.Trace?.Log($"-- {mech.Name} not destroyed, skiping");
                    continue;

                }


                if (Control.Settings.OverrideRecoveryChance)
                {
                    Log.SalvageProcess.Trace?.Log($"-- Recovery {mech.Name} CC method");

                    float chance = Constants.Salvage.DestroyedMechRecoveryChance;

                    chance -= mech.IsLocationDamaged(ChassisLocations.Head)
                        ? Control.Settings.HeadRecoveryPenaly
                        : 0;

                    chance -= mech.IsLocationDestroyed(ChassisLocations.LeftTorso)
                        ? Control.Settings.TorsoRecoveryPenalty
                        : 0;
                    chance -= mech.IsLocationDestroyed(ChassisLocations.CenterTorso)
                        ? Control.Settings.TorsoRecoveryPenalty
                        : 0;
                    chance -= mech.IsLocationDestroyed(ChassisLocations.RightTorso)
                        ? Control.Settings.TorsoRecoveryPenalty
                        : 0;

                    chance -= mech.IsLocationDestroyed(ChassisLocations.RightArm)
                        ? Control.Settings.LimbRecoveryPenalty
                        : 0;
                    chance -= mech.IsLocationDestroyed(ChassisLocations.RightLeg)
                        ? Control.Settings.LimbRecoveryPenalty
                        : 0;
                    chance -= mech.IsLocationDestroyed(ChassisLocations.LeftArm)
                        ? Control.Settings.LimbRecoveryPenalty
                        : 0;
                    chance -= mech.IsLocationDestroyed(ChassisLocations.LeftLeg)
                        ? Control.Settings.LimbRecoveryPenalty
                        : 0;

                    chance += lostUnits[i].pilot.HasEjected
                        ? Control.Settings.EjectRecoveryBonus
                        : 0;


                    float num = simgame.NetworkRandom.Float(0f, 1f);

                    lostUnits[i].mechLost = chance < num;

                    if (lostUnits[i].mechLost)
                    {
                        Log.SalvageProcess.Trace?.Log($"--- {chance:0.00} < {num:0.00} - roll failed, no recovery");
                    }
                    else
                    {
                        Log.SalvageProcess.Trace?.Log($"--- {chance:0.00} >= {num:0.00} - roll success, recovery");
                    }
                }
                else
                {
                    Log.SalvageProcess.Trace?.Log($"-- Recovery {mech.Name} vanila method");
                    float num = simgame.NetworkRandom.Float(0f, 1f);

                    if (mech.IsLocationDestroyed(ChassisLocations.CenterTorso))
                    {
                        Log.SalvageProcess.Trace?.Log($"--- CenterTorso Destroyed - no recovery");
                        lostUnits[i].mechLost = true;
                    }
                    else
                    {
                        lostUnits[i].mechLost = Constants.Salvage.DestroyedMechRecoveryChance < num;
                        if (lostUnits[i].mechLost)
                        {
                            Log.SalvageProcess.Trace?.Log($"--- {Constants.Salvage.DestroyedMechRecoveryChance:0.00} < {num:0.00} - roll failed, no recovery");
                        }
                        else
                        {
                            Log.SalvageProcess.Trace?.Log($"--- {Constants.Salvage.DestroyedMechRecoveryChance:0.00} >= {num:0.00} - roll success, recovery");
                        }
                    }
                }

                if (lostUnits[i].mechLost)
                {
                    if (Control.Settings.SalvageUnrecoveredMech)
                        AddMechToSalvage(mech, contract, simgame, Constants, ___finalPotentialSalvage);
                    else
                    {
                        int old_diff = __instance.Override.finalDifficulty;

                        float old_rare_u = Constants.Salvage.RareUpgradeChance;
                        float old_rare_w = Constants.Salvage.RareWeaponChance;
                        float old_vrare_i = Constants.Salvage.VeryRareUpgradeChance;
                        float old_vrare_w = Constants.Salvage.VeryRareWeaponChance;

                        Constants.Salvage.RareUpgradeChance = 0;
                        Constants.Salvage.RareWeaponChance = 0;
                        Constants.Salvage.VeryRareUpgradeChance = 0;
                        Constants.Salvage.VeryRareWeaponChance = 0;

                        __instance.Override.finalDifficulty = 0;

                        AddMechToSalvage(mech, contract, simgame, Constants, __instance.SalvageResults);


                        Constants.Salvage.RareUpgradeChance = old_rare_u;
                        Constants.Salvage.RareWeaponChance = old_rare_w;
                        Constants.Salvage.VeryRareUpgradeChance = old_vrare_i;
                        Constants.Salvage.VeryRareWeaponChance = old_vrare_w;

                        __instance.Override.finalDifficulty = old_diff;


                    }
                }

            }


            Log.SalvageProcess.Trace?.Log($"- Enemy Mechs {__instance.Name}");
            foreach (var unit in enemyMechs)
            {
                if (unit.pilot.IsIncapacitated || IsDestroyed(unit.mech) || unit.pilot.HasEjected)
                    AddMechToSalvage(unit.mech, contract, simgame, Constants, ___finalPotentialSalvage);
                else
                {
                    Log.SalvageProcess.Trace?.Log($"-- Salvaging {unit.mech.Name}");
                    Log.SalvageProcess.Trace?.Log($"--- not destroyed, skipping");
                }
            }

            Log.SalvageProcess.Trace?.Log($"- Enemy Vechicle {__instance.Name}");
            foreach (var vechicle in enemyVehicles)
            {
                Log.SalvageProcess.Trace?.Log($"-- Salvaging {vechicle?.Chassis?.Description?.Name}");
                foreach (var component in vechicle.Inventory.Where(item =>
                             item.DamageLevel != ComponentDamageLevel.Destroyed))
                {
                    Log.SalvageProcess.Trace?.Log($"--- Adding {component.ComponentDefID}");
                    contract.AddMechComponentToSalvage(___finalPotentialSalvage, component.Def, ComponentDamageLevel.Functional, false,
                        Constants, simgame.NetworkRandom, true);
                }
            }

            contract.FilterPotentialSalvage(___finalPotentialSalvage);
            int num2 = __instance.SalvagePotential;
            float num3 = Constants.Salvage.VictorySalvageChance;
            float num4 = Constants.Salvage.VictorySalvageLostPerMechDestroyed;
            if (__instance.State == Contract.ContractState.Failed)
            {
                num3 = Constants.Salvage.DefeatSalvageChance;
                num4 = Constants.Salvage.DefeatSalvageLostPerMechDestroyed;
            }
            else if (__instance.State == Contract.ContractState.Retreated)
            {
                num3 = Constants.Salvage.RetreatSalvageChance;
                num4 = Constants.Salvage.RetreatSalvageLostPerMechDestroyed;
            }
            float num5 = num3;
            float num6 = (float)num2 * __instance.PercentageContractSalvage;
            if (num2 > 0)
            {
                num6 += (float)Constants.Finances.ContractFloorSalvageBonus;
            }
            num3 = Mathf.Max(0f, num5 - num4 * (float)lostUnits.Count);
            int num7 = Mathf.FloorToInt(num6 * num3);
            if (num2 > 0)
            {
                num2 += Constants.Finances.ContractFloorSalvageBonus;
            }

            contract.FinalSalvageCount = num7;
            contract.FinalPrioritySalvageCount = Math.Min(7, Mathf.FloorToInt((float)num7 * Constants.Salvage.PrioritySalvageModifier));

        }
        catch (Exception e)
        {
            Log.Main.Error?.Log("Unhandled error in salvage", e);
        }

        return false;
    }

    private static void AddMechToSalvage(MechDef mech, Contract contract, SimGameState simgame, SimGameConstants constants, List<SalvageDef> salvage)
    {
        Log.SalvageProcess.Trace?.Log($"-- Salvaging {mech.Name}");

        int numparts = 0;

        if (Control.Settings.OverrideMechPartCalculation)
        {
            if (mech.IsLocationDestroyed(ChassisLocations.CenterTorso))
                numparts = Control.Settings.CenterTorsoDestroyedParts;
            else
            {
                float total = Control.Settings.SalvageArmWeight * 2 + Control.Settings.SalvageHeadWeight +
                              Control.Settings.SalvageLegWeight * 2 + Control.Settings.SalvageTorsoWeight * 2 + 1;

                float val = total;

                val -= mech.IsLocationDestroyed(ChassisLocations.Head) ? Control.Settings.SalvageHeadWeight : 0;

                val -= mech.IsLocationDestroyed(ChassisLocations.LeftTorso)
                    ? Control.Settings.SalvageTorsoWeight
                    : 0;
                val -= mech.IsLocationDestroyed(ChassisLocations.RightTorso)
                    ? Control.Settings.SalvageTorsoWeight
                    : 0;

                val -= mech.IsLocationDestroyed(ChassisLocations.LeftLeg) ? Control.Settings.SalvageLegWeight : 0;
                val -= mech.IsLocationDestroyed(ChassisLocations.RightLeg) ? Control.Settings.SalvageLegWeight : 0;

                val -= mech.IsLocationDestroyed(ChassisLocations.LeftArm) ? Control.Settings.SalvageArmWeight : 0;
                val -= mech.IsLocationDestroyed(ChassisLocations.LeftLeg) ? Control.Settings.SalvageArmWeight : 0;

                numparts = (int)(constants.Story.DefaultMechPartMax * val / total + 0.5f);
                if (numparts <= 0)
                    numparts = 1;
                if (numparts > constants.Story.DefaultMechPartMax)
                    numparts = constants.Story.DefaultMechPartMax;
            }
        }
        else
        {
            numparts = 3;
            if (mech.IsLocationDestroyed(ChassisLocations.CenterTorso))
                numparts = 1;
            else if (mech.IsLocationDestroyed(ChassisLocations.LeftLeg) &&
                     mech.IsLocationDestroyed(ChassisLocations.RightLeg))
                numparts = 2;
        }

        try
        {
            Log.SalvageProcess.Trace?.Log($"--- Adding {numparts} parts");
            contract.CreateAndAddMechPart(constants, mech, numparts, salvage);
        }
        catch (Exception e)
        {
            Log.Main.Error?.Log("Error in adding parts", e);
        }

        try
        {
            if (Control.Settings.NoLootCTDestroyed && mech.IsLocationDestroyed(ChassisLocations.CenterTorso))
            {
                Log.SalvageProcess.Trace?.Log($"--- CT Destroyed - no component loot");
            }
            else
                foreach (var component in mech.Inventory.Where(item =>
                             !mech.IsLocationDestroyed(item.MountedLocation) &&
                             item.DamageLevel != ComponentDamageLevel.Destroyed))
                {
                    Log.SalvageProcess.Trace?.Log($"--- Adding {component.ComponentDefID}");
                    contract.AddMechComponentToSalvage(salvage, component.Def, ComponentDamageLevel.Functional, false,
                        constants, simgame.NetworkRandom, true);
                }
        }
        catch (Exception e)
        {
            Log.Main.Error?.Log("Error in adding component", e);
        }
    }
}