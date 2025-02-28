using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using BattleTech;

namespace CustomComponents;

public class Database
{
    internal static T GetCustom<T>(object target)
    {
        var identifier = Identifier(target);

        if (identifier == null)
        {
            return default;
        }

        if (!Customs.TryGetValue(identifier, out var customs))
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool Is<T>(object target, out T value)
    {
        value = GetCustom<T>(target);
        return value != null;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool Is<T>(object target)
    {
        return GetCustom<T>(target) != null;
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

    internal static IEnumerable<T> GetCustoms<T>(object target)
    {
        var identifier = Identifier(target);

        if (identifier == null)
        {
            yield break;
        }

        if (!Customs.TryGetValue(identifier, out var customs))
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

    private static readonly Dictionary<string, object[]> Customs = new(StringComparer.Ordinal);
    internal static bool AddCustom(object target, ICustom cc)
    {
        var identifier = Identifier(target);
        if (identifier == null)
        {
            return false;
        }

        Customs.TryGetValue(identifier, out var customs);

        Log.CCLoading.Trace?.Log($"{nameof(AddCustom)} identifier={identifier} cc={cc} cc.type={cc.GetType()}");

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
            Customs[identifier] = [cc];
        }
        else
        {
            var newCustoms = new object[customs.Length + 1];
            Array.Copy(customs, newCustoms, customs.Length);
            newCustoms[customs.Length] = cc;
            Customs[identifier] = newCustoms;
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