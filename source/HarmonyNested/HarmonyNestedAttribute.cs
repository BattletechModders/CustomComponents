using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Harmony;

namespace CustomComponents
{
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

            Control.mod.Logger.Log(string.Format("Type: {0}\tMethod: {1}",
                this.info.originalType, this.info.methodName));
        }
    }
}
