using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using BattleTech;

namespace CustomComponents;

public class Database
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static CCFlags GetCCFlags(object target, ref List<object> customs)
    {
        var count = CustomsCountAndEnsureCustomsField(target, ref customs);
        if (count == 0)
        {
            return null;
        }
        // CCFlags is kept in front to improve combat performance
        return customs[0] as CCFlags;
    }

    internal static T GetCustom<T>(object target, ref List<object> customs)
    {
        var count = CustomsCountAndEnsureCustomsField(target, ref customs);
        if (count == 0)
        {
            return default;
        }

        for (var index = 0; index < count; index++)
        {
            if (customs[index] is T csT)
            {
                return csT;
            }
        }

        return default;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool Is<T>(object target, ref List<object> customs, out T value)
    {
        value = GetCustom<T>(target, ref customs);
        return value != null;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool Is<T>(object target, ref List<object> customs)
    {
        return GetCustom<T>(target, ref customs) != null;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int CustomsCountAndEnsureCustomsField(object target, ref List<object> customs)
    {
        if (customs == null)
        {
            var identifier = Identifier(target);

            if (identifier == null)
            {
                return 0;
            }

            if (!Customs.TryGetValue(identifier, out customs))
            {
                Customs[identifier] = customs = [];
            }
        }
        return customs.Count;
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

    internal static IEnumerable<T> GetCustoms<T>(object target, ref List<object> customs)
    {
        var count = CustomsCountAndEnsureCustomsField(target, ref customs);
        return count == 0
            ? []
            : GetCustomsIter<T>(customs, count);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static IEnumerable<T> GetCustomsIter<T>(List<object> customs, int count)
    {
        for (var index = 0; index < count; index++)
        {
            if (customs[index] is T csT)
            {
                yield return csT;
            }
        }
    }

    private static readonly Dictionary<string, List<object>> Customs = new(StringComparer.Ordinal);
    internal static bool AddCustom(object target, ICustom cc)
    {
        var identifier = Identifier(target);

        if (identifier == null)
        {
            return false;
        }

        if (!Customs.TryGetValue(identifier, out var customs))
        {
            Customs[identifier] = customs = [];
        }

        Log.CCLoading.Trace?.Log($"{nameof(AddCustom)} identifier={identifier} cc={cc} cc.type={cc.GetType()}");

        var attribute = GetAttributeByType(cc.GetType());
        Log.CCLoading.Trace?.Log($"--{nameof(CustomComponentAttribute)} {nameof(attribute.Name)}={attribute.Name} {nameof(attribute.AllowArray)}={attribute.AllowArray}");

        if (customs is { Count: > 0 } && !attribute.AllowArray)
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
        if (cc.GetType() == typeof(CCFlags))
        {
            // CCFlags is kept in front to improve combat performance
            customs.Insert(0, cc);
        }
        else
        {
            customs.Add(cc);
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