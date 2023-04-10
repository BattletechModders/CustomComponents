using System;
using BattleTech;

namespace CustomComponents;

public static class FlagsExtentions
{
    [Obsolete("use FlagExtensions.CCFlags()")]
    public static T Flags<T>(this MechComponentDef def) where T : class
    {
        return def.CCFlags() as T ?? throw new ArgumentException("Only CCFlags are currently supported");
    }
    [Obsolete("use FlagExtensions.CCFlags()")]
    public static T Flags<T>(this MechComponentRef item) where T : class
    {
        return Flags<T>(item?.Def);
    }
}