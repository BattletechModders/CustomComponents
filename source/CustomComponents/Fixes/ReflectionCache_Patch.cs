using System;
using System.Collections.Generic;
using System.Reflection;
using HBS.Reflection;

namespace CustomComponents;

[HarmonyPatch(typeof(ReflectionCache), nameof(ReflectionCache.Get))]
internal static class ReflectionCache_Get_Patch
{
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        return ReflectionCache_Patch.ReplaceNameInstructions(instructions);
    }
}

[HarmonyPatch(typeof(ReflectionCache), nameof(ReflectionCache.Set))]
internal static class ReflectionCache_Set
{
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        return ReflectionCache_Patch.ReplaceNameInstructions(instructions);
    }
}

[HarmonyPatch(typeof(ReflectionCache), nameof(ReflectionCache.Invoke))]
internal static class ReflectionCache_Invoke_Patch
{
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        return ReflectionCache_Patch.ReplaceNameInstructions(instructions);
    }
}

[HarmonyPatch]
internal static class ReflectionCache_TryCacheMember_Patch
{
    public static MethodInfo TargetMethod()
    {
        return typeof(ReflectionCache)
            .GetMethod(
                "TryCacheMember",
                BindingFlags.NonPublic | BindingFlags.Instance,
                null,
                new[] {typeof(Type), typeof(string), typeof(string).MakeByRefType()},
                null
            );
    }

    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        return ReflectionCache_Patch.ReplaceNameInstructions(instructions);
    }
}

[HarmonyPatch]
internal static class ReflectionCache_TryCacheMethod_Patch
{
    public static MethodInfo TargetMethod()
    {
        return typeof(ReflectionCache)
            .GetMethod(
                "TryCacheMethod",
                BindingFlags.Public | BindingFlags.Instance
            );
    }

    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        return ReflectionCache_Patch.ReplaceNameInstructions(instructions);
    }
}

internal static class ReflectionCache_Patch
{
    internal static IEnumerable<CodeInstruction> ReplaceNameInstructions(IEnumerable<CodeInstruction> instructions)
    {
        return instructions.MethodReplacer(
            AccessTools.Property(typeof(MemberInfo), nameof(MemberInfo.Name)).GetGetMethod(),
            AccessTools.Method(typeof(ReflectionCache_Patch), nameof(GetName))
        );
    }

    internal static string GetName(this MemberInfo type)
    {
        //Control.Logger.LogDebug(type.ToString());
        return type.ToString();
    }
}