using System;
using BattleTech;
using BattleTech.Data;
using Harmony;

namespace CustomComponents
{
    [HarmonyPatch(typeof(SimGameState), "AddItemStat", new [] {typeof(string), typeof(Type), typeof(bool)})]
    public static class SimGameState_AddItemStat_Patch
    {
        public static bool Prefix(string id, Type type, bool damaged, SimGameState __instance)
        {
            if (__instance == null)
                return true;

            var def = GetComponentByType(id, type, __instance.DataManager);
            if (!(def is IDefault))
                return true;

            return false;
        }

        private static MechComponentDef GetComponentByType(string id, Type type, DataManager data)
        {
            switch (type)
            {
                case Type t when t == typeof(WeaponDef):
                    if (data.WeaponDefs.TryGet(id, out var weapon))
                        return weapon;
                    break;
                case Type t when t == typeof(AmmunitionBoxDef):
                    if (data.AmmoBoxDefs.TryGet(id, out var ammunition))
                        return ammunition;
                    break;
                case Type t when t == typeof(JumpJetDef):
                    if (data.JumpJetDefs.TryGet(id, out var jj))
                        return jj;
                    break;
                case Type t when t == typeof(UpgradeDef):
                    if (data.UpgradeDefs.TryGet(id, out var upgrade))
                        return upgrade;
                    break;
                case Type t when t == typeof(HeatSinkDef):
                    if (data.WeaponDefs.TryGet(id, out var heatsink))
                        return heatsink;
                    break;
            }
            return null;
        }

    }


}