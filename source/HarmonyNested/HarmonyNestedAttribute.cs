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
            this.info.originalType = baseType.GetNestedType(nestedType, BindingFlags.Static |
                                                   BindingFlags.Instance |
                                                   BindingFlags.Public |
                                                   BindingFlags.NonPublic);
            this.info.parameter = parameters;
            this.info.methodName = method;

            Control.mod.Logger.LogDebug(string.Format("Type: {0}\tMethod: {1}",
                this.info.originalType, this.info.methodName));
        }
    }
}
