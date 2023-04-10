using BattleTech;

namespace CustomComponents;

public static class FlagExtensions
{
    private static readonly CCFlags s_nullCCFlags = new();
    public static CCFlags CCFlags(this MechComponentDef def)
    {
        return def.GetComponent<Flags>()?.CCFlags ?? s_nullCCFlags;
    }
}