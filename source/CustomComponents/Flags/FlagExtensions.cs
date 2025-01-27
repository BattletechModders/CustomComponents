using BattleTech;

namespace CustomComponents;

public static class FlagExtensions
{
    private static readonly CCFlags s_nullCCFlags = new();
    public static CCFlags CCFlags(this MechComponentDef def)
    {
        return def.Is<Flags>(out var f) ? f.CCFlags : s_nullCCFlags;
    }
}