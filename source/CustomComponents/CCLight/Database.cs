using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using BattleTech;

namespace CustomComponents;

public class Database
{
    internal static bool AddCustom(string identifier, object target, ICustom cc)
    {
        ref var customs = ref DefToCustoms(target);
        return AddCustomInternal(identifier, ref customs, cc);
    }
    private static ref object[] DefToCustoms(object target)
    {
        switch (target)
        {
            case MechComponentDef def1:
                return ref def1.ccCustoms;
            case MechDef def2:
                return ref def2.ccCustoms;
            case ChassisDef def3:
                return ref def3.ccCustoms;
            case VehicleChassisDef def4:
                return ref def4.ccCustoms;
            default:
                throw new ArgumentException();
        }
    }

    internal static bool AddCustom(string identifier, ref object[] customs, ICustom cc)
    {
        return AddCustomInternal(identifier, ref customs, cc);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static IEnumerable<T> GetCustoms<T>(object[] customs)
    {
        return GetCustomsInternal<T>(customs);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static T GetCustom<T>(object[] customs)
    {
        return GetCustomInternalFast<T>(customs);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool Is<T>(object[] customs, out T value)
    {
        value = GetCustom<T>(customs);
        return value != null;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool Is<T>(object[] customs)
    {
        return GetCustom<T>(customs) != null;
    }

    internal static string Identifier(object target)
    {
        if (target == null)
        {
            Log.Main.Error?.Log("Null object passed, therefore missing identifier!");
            return null;
        }
        return target switch
        {
            MechComponentDef def => def.Description.Id,
            MechDef def => def.Description.Id,
            ChassisDef def => def.Description.Id,
            VehicleChassisDef def => def.Description.Id,
            _ => null
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static IEnumerable<T> GetCustomsInternal<T>(object[] customs)
    {
        if (customs == null)
        {
            yield break;
        }

        for (var index = 0; index < customs.Length; index++)
        {
            if (customs[index] is T csT)
            {
                yield return csT;
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static T GetCustomInternalFast<T>(object[] customs)
    {
        if (customs == null)
        {
            return default;
        }

        for (var index = 0; index < customs.Length; index++)
        {
            if (customs[index] is T csT)
            {
                return csT;
            }
        }

        return default;
    }

    private static bool AddCustomInternal(string identifier, ref object[] customs, ICustom cc)
    {
        Log.CCLoading.Trace?.Log($"{nameof(AddCustomInternal)} identifier={identifier} cc={cc} cc.type={cc.GetType()}");

        var attribute = GetAttributeByType(cc.GetType());
        Log.CCLoading.Trace?.Log($"--{nameof(CustomComponentAttribute)} {nameof(attribute.Name)}={attribute.Name} {nameof(attribute.AllowArray)}={attribute.AllowArray}");

        if (customs is { Length: > 0 } && !attribute.AllowArray)
        {
            var isDuplicate = customs
                .Select(custom => GetAttributeByType(custom.GetType()))
                .Any(attribute2 => attribute.Name == attribute2.Name);

            if (isDuplicate)
            {
                Log.CCLoading.Warning?.Log(
                    $"Not adding duplicate for identifier={identifier} {nameof(attribute.Name)}={attribute.Name} {nameof(attribute.AllowArray)}={attribute.AllowArray}");
                return false;
            }
        }

        Log.CCLoading.Trace?.Log("--added");
        if (customs == null)
        {
            customs = [cc];
        }
        else
        {
            var newCustoms = new object[customs.Length + 1];
            Array.Copy(customs, newCustoms, customs.Length);
            newCustoms[customs.Length] = cc;
            customs = newCustoms;
        }
        return true;
    }

    private static readonly Dictionary<Type, CustomComponentAttribute> Attributes = new();

    private static CustomComponentAttribute GetAttributeByType(Type type)
    {
        if (!Attributes.TryGetValue(type, out var attribute))
        {
            attribute = type.GetCustomAttributes(false).OfType<CustomComponentAttribute>().FirstOrDefault();
            Attributes[type] = attribute ?? throw new($"{type} is missing CustomComponentAttribute");
        }

        return attribute;
    }
}