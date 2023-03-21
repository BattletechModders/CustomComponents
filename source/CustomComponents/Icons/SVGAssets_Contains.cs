using BattleTech.Data;

namespace CustomComponents.Icons;

[HarmonyPatch(typeof(SVGCache))]
[HarmonyPatch("Contains")]
public static class SVGAssets_Contains
{
    [HarmonyPrefix]
    [HarmonyWrapSafe]
    public static void Prefix(ref bool __runOriginal, string id, ref bool __result)
    {
        if (!__runOriginal)
        {
            return;
        }

        if (string.IsNullOrEmpty(id) || id[0] != '@')
        {
            return;
        }

        __result = IconController.Contains(id);
        if (!__result)
        {
            Log.Main.Error?.Log($"Custom icon {id} not exists!");
        }
        __runOriginal = false;
    }
}