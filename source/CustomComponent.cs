using BattleTech;
using System;
using System.Reflection;

namespace CustomComponents
{

    public interface ICustomComponent
    {
        string CustomType { get; }
        void FromJson(string json);
        string ToJson();
    }

    public class CustomAttribute : Attribute
    {
        public string CustomType { get; set; }

        public CustomAttribute(string CustomType)
        {
            this.CustomType = CustomType;
        }
    }

    public class CustomComponentDescriptor
    {
        public string CustomName { get; private set; }
        public Type ActualType { get; private set; }

        private ConstructorInfo constructor;

        public CustomComponentDescriptor(string name, Type type)
        {
            this.CustomName = name;
            this.ActualType = type;
            this.constructor = type.GetConstructor(new Type[] { });
        }

        public ICustomComponent CreateNew()
        {
            return constructor.Invoke(null) as ICustomComponent;
        }
    }
}

