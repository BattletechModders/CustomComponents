using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BattleTech;

namespace CustomComponents;

public class Database
{
    internal static void AddCustom(object target, ICustom cc)
    {
        var identifier = Identifier(target);
        AddCustom(identifier, cc);
    }

    internal static bool AddCustom(string identifier, ICustom cc)
    {
        return Shared.AddCustomInternal(identifier, cc);
    }

    public static IEnumerable<T> GetCustoms<T>(object target)
    {
        var identifier = Identifier(target);
        return Shared.GetCustomsInternal<T>(identifier);
    }

    public static T GetCustom<T>(object target)
    {
        var identifier = Identifier(target);
        return Shared.GetCustomInternalFast<T>(identifier);
    }

    internal static bool Is<T>(object target, out T value)
    {
        value = GetCustom<T>(target);
        return value != null;
    }

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

        // this is for faster access
        switch (target)
        {
            case MechComponentDef def:
                return def.Description.Id;
            case BaseDescriptionDef def:
                return def.Id;
            case MechDef def:
                return def.Description.Id;
            case ChassisDef def:
                return def.Description.Id;
            case EffectData data:
                return data.Description?.Id;
            case VehicleChassisDef def:
                return def.Description.Id;
            case LanceDef def:
                return def.Description.Id;
            case PilotDef def:
                return def.Description.Id;
            case AmmunitionDef def:
                return def.Description.Id;
            case MovementCapabilitiesDef def:
                return def.Description.Id;
            case AbilityDef def:
                return def.Description.Id;
            case PathingCapabilitiesDef def:
                return def.Description.Id;
            case HeraldryDef def:
                return def.Description.Id;
        }

        var targetType = target.GetType();
        if (!_reflectionCache.TryGetValue(targetType, out var propertyInfo))
        {
            propertyInfo = targetType.GetProperty(nameof(MechComponentDef.Description));
            if (propertyInfo != null && typeof(BaseDescriptionDef).IsAssignableFrom(propertyInfo.PropertyType))
            {
                _reflectionCache[targetType] = propertyInfo;
                Log.Main.Warning?.Log($"Don't know {targetType} but found a Description property, might want to add it to the fast access path");
            }
            else
            {

                _reflectionCache[targetType] = null;
            }
        }
        var description = propertyInfo?.GetValue(target, null) as BaseDescriptionDef;
        return description?.Id;
    }
    private static readonly Dictionary<Type, PropertyInfo> _reflectionCache = new();

    internal static void Clear()
    {
        Shared.ClearCustoms();
    }

    private static readonly Database Shared = new();

    private readonly Dictionary<string, List<ICustom>> customs = new();

    private IEnumerable<T> GetCustomsInternal<T>(string key)
    {
        if (key == null || !customs.TryGetValue(key, out var ccs))
        {
            return Enumerable.Empty<T>();
        }

        return ccs.OfType<T>();
    }

    private T GetCustomInternalFast<T>(string key)
    {
        if (key != null && customs.TryGetValue(key, out var ccs))
        {
            for (var index = 0; index < ccs.Count; index++)
            {
                if (ccs[index] is T csT)
                {
                    return csT;
                }
            }
        }
        return default;
    }

    private List<ICustom> GetOrCreateCustomsList(string key)
    {
        if (!customs.TryGetValue(key, out var ccs))
        {
            ccs = new();
            customs[key] = ccs;
        }
        return ccs;
    }

    private bool AddCustomInternal(string identifier, ICustom cc)
    {
        Log.CCLoading.Trace?.Log($"{nameof(AddCustomInternal)} identifier={identifier} cc={cc} cc.type={cc.GetType()}");

        var attribute = GetAttributeByType(cc.GetType());
        Log.CCLoading.Trace?.Log($"--{nameof(CustomComponentAttribute)} {nameof(attribute.Name)}={attribute.Name} {nameof(attribute.AllowArray)}={attribute.AllowArray}");

        var ccs = GetOrCreateCustomsList(identifier);

        if (ccs.Count > 0 && !attribute.AllowArray)
        {
            var isDuplicate = ccs
                .Select(custom => GetAttributeByType(custom.GetType()))
                .Any(attribute2 => attribute.Name == attribute2.Name);

            if (isDuplicate)
            {
                Log.CCLoading.Warning?.Log($"Not adding duplicate for identifier={identifier} {nameof(attribute.Name)}={attribute.Name} {nameof(attribute.AllowArray)}={attribute.AllowArray}");
                return false;
            }
        }

        Log.CCLoading.Trace?.Log("--added");
        ccs.Add(cc);
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

    private void ClearCustoms()
    {
        customs.Clear();
    }
}