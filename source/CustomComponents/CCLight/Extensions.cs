using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using BattleTech;

namespace CustomComponents;

// TODO how to improve performance?
// 0. (/) reduce supported anchor types by publicly accessible ones
// 1. (/) for each Def, add the list of possible customs as a private field!
// 2. (?) convert array access to hardcoded if/else dynamic method and ReferenceEquals
public static class MechComponentDefExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T GetComponent<T>(this MechComponentDef target)
    {
        return Database.GetCustom<T>(target.ccCustoms);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IEnumerable<T> GetComponents<T>(this MechComponentDef target)
    {
        return Database.GetCustoms<T>(target.ccCustoms);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Is<T>(this MechComponentDef target, out T res)
    {
        // strongly used during combat/ai due to ME ignore_damage
        // TODO migrate combat related flags to dedicated SimpleInjector fields
        //  requires changes in calling mods!
        if (typeof(T) == typeof(Flags))
        {
            res = (T)target.ccFlags;
            return res != null;
        }

        return Database.Is(target.ccCustoms, out res);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Is<T>(this MechComponentDef target)
    {
        return Database.Is<T>(target.ccCustoms);
    }

    public static T AddComponent<T>(this MechComponentDef target, T component) where T : ICustom
    {
        if (component is SimpleCustom<MechComponentDef> simple)
        {
            simple.Def = target;
        }
        Database.AddCustom(target.Description.Id, ref target.ccCustoms, component);
        return component;
    }

    public static T GetOrCreate<T>(this MechComponentDef target, Func<T> factory) where T : ICustom
    {
        var result = target.GetComponent<T>();
        if (result is ExtendedDetails.ExtendedDetails ed && ed.Def != target.Description)
        {
            ed.Def = target.Description;
        }
        return result ?? target.AddComponent(factory.Invoke());
    }
}

public static class VehicleExtentions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T GetComponent<T>(this VehicleChassisDef target)
    {
        return Database.GetCustom<T>(target.ccCustoms);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IEnumerable<T> GetComponents<T>(this VehicleChassisDef target)
    {
        return Database.GetCustoms<T>(target.ccCustoms);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Is<T>(this VehicleChassisDef target, out T res)
    {
        return Database.Is(target.ccCustoms, out res);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Is<T>(this VehicleChassisDef target)
    {
        return Database.Is<T>(target.ccCustoms);
    }
}

public static class MechDefExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T GetComponent<T>(this MechDef target)
    {
        return Database.GetCustom<T>(target.ccCustoms);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IEnumerable<T> GetComponents<T>(this MechDef target)
    {
        return Database.GetCustoms<T>(target.ccCustoms);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Is<T>(this MechDef target, out T res)
    {
        return Database.Is(target.ccCustoms, out res);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Is<T>(this MechDef target)
    {
        return Database.Is<T>(target.ccCustoms);
    }

    public static bool IsBroken(this MechDef def)
    {
        try
        {
            if (def == null)
            {
                Log.Main.Error?.Log("MECHDEF IS NULL!");
                return true;
            }
            if (def.Chassis == null)
            {
                Log.Main.Error?.Log($"Chassis of {def.Description.Id} IS NULL!");
                return true;
            }
            if (def.MechTags == null)
            {
                Log.Main.Error?.Log($"Mechtags of {def.Description.Id} IS NULL!");
                return true;
            }
            if (def.Chassis.ChassisTags == null)
            {
                Log.Main.Error?.Log($"Chassistags of {def.Description.Id} IS NULL!");
                return true;
            }

            return false;
        }
        catch
        {
            Log.Main.Error?.Log("5.GOT NRE!!!!");
            Log.Main.Error?.Log($"{def}");
            return false;
        }
    }
}

public static class ChassisDefExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T GetComponent<T>(this ChassisDef target)
    {
        return Database.GetCustom<T>(target.ccCustoms);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IEnumerable<T> GetComponents<T>(this ChassisDef target)
    {
        return Database.GetCustoms<T>(target.ccCustoms);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Is<T>(this ChassisDef target, out T res)
    {
        return Database.Is(target.ccCustoms, out res);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Is<T>(this ChassisDef target)
    {
        return Database.Is<T>(target.ccCustoms);
    }

    public static T AddComponent<T>(this ChassisDef target, T component) where T : ICustom
    {
        if (component is SimpleCustom<ChassisDef> simple)
        {
            simple.Def = target;
        }
        Database.AddCustom(target.Description.Id, ref target.ccCustoms, component);
        return component;
    }

    public static T GetOrCreate<T>(this ChassisDef target, Func<T> factory) where T : ICustom
    {
        var result = target.GetComponent<T>();
        if ((result is ExtendedDetails.ExtendedDetails ed) && ed.Def != target.Description)
        {
            ed.Def = target.Description;
        }

        return result ?? target.AddComponent(factory.Invoke());
    }
}

public static class MechComponentRefExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T GetComponent<T>(this BaseComponentRef target)
    {
        RefreshDef(target);
        return target.Def.GetComponent<T>();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IEnumerable<T> GetComponents<T>(this BaseComponentRef target)
    {
        RefreshDef(target);
        return target.Def.GetComponents<T>();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Is<T>(this BaseComponentRef target, out T res)
    {
        RefreshDef(target);
        return target.Def.Is(out res);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Is<T>(this BaseComponentRef target)
    {
        RefreshDef(target);
        return target.Def.Is<T>();
    }

    internal static void RefreshDef(this BaseComponentRef target)
    {
        if (target == null)
        {
            return;
        }
        if (target.Def != null)
        {
            return;
        }
        target.DataManager ??= UnityGameInstance.BattleTechGame.DataManager;
        target.RefreshComponentDef();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T GetComponent<T>(this MechComponentRef target)
    {
        RefreshDef(target);
        return target.Def.GetComponent<T>();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IEnumerable<T> GetComponents<T>(this MechComponentRef target)
    {
        RefreshDef(target);
        return target.Def.GetComponents<T>();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Is<T>(this MechComponentRef target, out T res)
    {
        RefreshDef(target);
        return target.Def.Is(out res);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Is<T>(this MechComponentRef target)
    {
        RefreshDef(target);
        return target.Def.Is<T>();
    }
}