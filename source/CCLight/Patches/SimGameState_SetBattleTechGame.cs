using BattleTech;
using Harmony;

namespace CustomComponents
{
    [HarmonyPatch(typeof(SimGameState))]
    [HarmonyPatch("BattleTechGame", PropertyMethod.Setter)]
    public static class SimGameState_SetBattleTechGame
    {
        public static void Postfix(GameInstance value)
        {
#if CCDEBUG
            Control.Logger.LogDebug($"SetGameState found!, Datamanager is null: {value.DataManager == null} ");
#endif
            Database.setDataManager(value);
        }
    }
}