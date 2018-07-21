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
            RegisterSimpleCustomComponents(assembly.GetTypes());
        }

        public static void RegisterSimpleCustomComponents(params Type[] types)
        {
            var sccType = typeof(SimpleCustomComponent);
            foreach (var type in types.Where(t => sccType.IsAssignableFrom(t)))
            {
                var customAttribute = type.GetCustomAttributes(false).OfType<CustomComponentAttribute>().FirstOrDefault();
                if (customAttribute == null)
                {
                    continue;
                }

                var name = customAttribute.Name;
                if (Factories.Any(f => f.ComponentSectionName == name))
                {
                    continue;
                }

                var factoryGenericType = typeof(SimpleCustomComponentFactory<>);
                var genericTypes = new[] { type };
                var factoryType = factoryGenericType.MakeGenericType(genericTypes);
                var factory = Activator.CreateInstance(factoryType) as ICustomComponentFactory;
                // ReSharper disable once PossibleNullReferenceException
                factory.ComponentSectionName = name;
                Factories.Add(factory);
            }
        }

        // can be used by post or preprocessors
        public static void SetCustomComponent(MechComponentDef def, ICustomComponent component)
        {
            Database.SetCustomComponent(def, component);
        }

        internal static void ProcessCustomCompontentFactories(object target, Dictionary<string, object> values)
        {
            if (!(target is MechComponentDef componentDef))
            {
                return;
            }

            foreach (var preProcessor in PreProcessors)
            {
                preProcessor.PreProcess(componentDef, values);
            }

            foreach (var factory in Factories)
            {
                var component = factory.Create(componentDef, values);
                if (component == null)
                {
                    continue;
                }

                //Control.Logger.LogDebug($"Created {factory.ComponentSectionName} for {componentDef.Description.Id}");
                SetCustomComponent(componentDef, component);
            }

            foreach (var postProcessor in PostProcessors)
            {
                postProcessor.PostProcess(componentDef, values);
            }
        }
    }
}
