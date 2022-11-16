using BattleTech;

namespace CustomComponents;

public static class FlagsExtentions
{
    public static T Flags<T>(this MechComponentDef item)
        where T : class, new()
    {
        return FlagsController<T>.Shared[item];
    }
    public static T Flags<T>(this MechComponentRef item)
        where T : class, new()
    {
        return FlagsController<T>.Shared[item?.Def];
    }

    public static T Flags<T>(this BaseComponentRef item)
        where T : class, new()
    {
        return FlagsController<T>.Shared[item?.Def];
    }

    public static bool IsDefault(this MechComponentDef item)
    {
        if (item == null)
            return false;
        var f = item.Flags<CCFlags>();
        if (f == null)
            return false;
        return f.Default;

    }
    public static bool IsDefault(this MechComponentRef item)
    {
        if (item?.Def == null)
            return false;
        var f = item.Flags<CCFlags>();
        if (f == null)
            return false;
        return f.Default;
    }

    public static bool IsDefault(this BaseComponentRef item)
    {
        if (item?.Def == null)
            return false;
        var f = item.Flags<CCFlags>();
        if (f == null)
            return false;
        return f.Default;
    }

}