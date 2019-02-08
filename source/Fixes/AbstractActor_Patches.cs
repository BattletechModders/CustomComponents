using BattleTech;
using Harmony;

namespace CustomComponents.Fixes
{
    [HarmonyPatch(typeof(Mech), "FlagForDeath")]
    public static class Mech_FlagForDeath
    {
        [HarmonyPrefix]
        public static void logPrefix(string reason, DeathMethod deathMethod, DamageType damageType, int location,
            int stackItemID, string attackerID, bool isSilent, Mech __instance)
        {
            Control.Logger.LogDebug($"FlagForDeath: {__instance.DisplayName} reason:{reason}");
            Control.Logger.LogDebug($"--  reason:{reason} DeathMethid:{deathMethod}");
        }

        [HarmonyPostfix]
        public static void logPostfix( AbstractActor __instance)
        {
            Control.Logger.LogDebug($"complete: FlagForDeath for {__instance.DisplayName}");
            Control.Logger.LogDebug($"-- Flagged: {__instance.IsFlaggedForDeath} DeathMethod {__instance.DeathMethod} Location: {__instance.DeathLocation} Handled: {__instance.HasHandledDeath}");
        }
    }

    [HarmonyPatch(typeof(Mech), "HandleDeath")]
    public static class Mech_HandleDeath
    {
        [HarmonyPrefix]
        public static void logPrefix(AbstractActor __instance)
        {
            Control.Logger.LogDebug($"HandleDeath: {__instance.DisplayName}");
            Control.Logger.LogDebug($"-- Flagged: {__instance.IsFlaggedForDeath} DeathMethod {__instance.DeathMethod} Location: {__instance.DeathLocation} Handled: {__instance.HasHandledDeath}");
        }

        [HarmonyPostfix]
        public static void logPostfix(AbstractActor __instance)
        {
            Control.Logger.LogDebug($"complete: HandleDeath for {__instance.DisplayName}");
            Control.Logger.LogDebug($"-- Flagged: {__instance.IsFlaggedForDeath} DeathMethod {__instance.DeathMethod} Location: {__instance.DeathLocation} Handled: {__instance.HasHandledDeath}");
        }
    }

    [HarmonyPatch(typeof(StatisticEffect), "OnEffectEnd")]
    public static class StatisticEffect_OnEffectEnd
    {
        [HarmonyPrefix]
        public static void logPrefix(StatisticEffect __instance, EffectData ___effectData)
        {
            Control.Logger.LogDebug($"OnEffectEnd: {__instance.id} from {__instance.creatorID} to {__instance.targetID}");
            Control.Logger.LogDebug($"-- EffectData: {___effectData}, targeting: {___effectData?.targetingData}, statistic: {___effectData?.statisticData}");
        }
    }
}