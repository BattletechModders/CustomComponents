using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BattleTech;

namespace CustomComponents
{
    public static class Registry
    {
        private static readonly List<IPreProcessor> PreProcessors = new List<IPreProcessor>();
        private static readonly List<ICustomComponentFactory> Factories = new List<ICustomComponentFactory>();
        private static readonly List<IPostProcessor> PostProcessors = new List<IPostProcessor>();

        public static void RegisterPreProcessor(IPreProcessor preProcessor)
        {
            PreProcessors.Add(preProcessor);
        }

        public static void RegisterFactory(ICustomComponentFactory factory)
        {
            Factories.Add(factory);
        }

        public static void RegisterPostProcessor(IPostProcessor postProcessor)
        {
            PostProcessors.Add(postProcessor);
        }

        public static void RegisterSimpleCustomComponents(Assembly assembly)
        {
            var sccType = typeof(SimpleCustomComponent);
            foreach (var type in assembly.GetTypes().Where(t => sccType.IsAssignableFrom(t)))
            {
                var custom_attribute = type.GetCustomAttributes(false).OfType<CustomComponentAttribute>().FirstOrDefault();
                if (custom_attribute == null)
                    continue;

                var factoryGenericType = typeof(SimpleCustomComponentFactory<>);
                var genericTypes = new[] { type };
                var factoryType = factoryGenericType.MakeGenericType(genericTypes);
                var factory = Activator.CreateInstance(factoryType) as ICustomComponentFactory;
                factory.ComponentSectionName = custom_attribute.Name;
                Factories.Add(factory);
            }
        }

        internal static void ProcessCustomCompontentFactories(object target, Dictionary<string, object> values)
        {
            if (!(target is MechComponentDef componentDef))
            {
                return;
            }

            foreach (var preProcessor in PreProcessors)
            {
                foreach (var component in preProcessor.PreProcess(componentDef, values) ?? Enumerable.Empty<ICustomComponent>())
                {
                    Database.SetCustomComponent(componentDef, component);
                }
            }

            foreach (var factory in Factories)
            {
                var component = factory.Create(componentDef, values);
                if (component == null)
                {
                    continue;
                }
                Control.Logger.LogDebug($"LOAD: {factory.ComponentSectionName} to {componentDef.Description.Id}");
                Database.SetCustomComponent(componentDef, component);
            }

            foreach (var postProcessor in PostProcessors)
            {
                foreach (var component in postProcessor.PostProcess(componentDef, values) ?? Enumerable.Empty<ICustomComponent>())
                {
                    Database.SetCustomComponent(componentDef, component);
                }
            }
        }
    }
}
