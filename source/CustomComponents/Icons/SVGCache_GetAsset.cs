using BattleTech.Data;
using SVGImporter;

namespace CustomComponents.Icons;

[HarmonyPatch(typeof(SVGCache))]
[HarmonyPatch("GetAsset")]
public static class SVGCache_GetAsset
{
    [HarmonyPrefix]
    [HarmonyWrapSafe]
    public static void Prefix(ref bool __runOriginal, string id, ref SVGAsset __result)
    {
        if (!__runOriginal)
        {
            return;
        }

        if (string.IsNullOrEmpty(id) || id[0] != '@')
        {
            return;
        }

        __result = IconController.Get(id);
        if (__result == null)
        {
            Log.Main.Error?.Log($"Custom icon {id} not found!");
        }
        __runOriginal = false;
    }
}