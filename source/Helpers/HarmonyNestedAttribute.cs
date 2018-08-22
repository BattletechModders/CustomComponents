using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Harmony;

namespace CustomComponents
{
    /// <summary>
    /// mark a patch for private nested class
    /// </summary>
    public class HarmonyNestedAttribute : HarmonyPatch
    {
        public HarmonyNestedAttribute(Type baseType, string nestedType, string method, Type[] parameters = null)
            : base(null, method, null)
        {
            this.info.declaringType = baseType.GetNestedType(nestedType, BindingFlags.Static |
                                                   BindingFlags.Instance |
                                                   BindingFlags.Public |
                                                   BindingFlags.NonPublic);
            this.info.argumentTypes = parameters;
            this.info.methodName = method;

            Control.Logger.LogDebug($"Type: {this.info.declaringType}\tMethod: {this.info.methodName}");
        }
    }
}
