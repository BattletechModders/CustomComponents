using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BattleTech;
using Harmony;

namespace CustomComponents
{
    public static class Registry
    {
        private static readonly List<IPreProcessor> PreProcessors = new List<IPreProcessor>();
        private static readonly List<ICustomFactory> Factories = new List<ICustomFactory>();
        private static readonly HashSet<string> SimpleIdentifiers = new HashSet<string>();

        public static void RegisterPreProcessor(IPreProcessor preProcessor)
        {
            PreProcessors.Add(preProcessor);
        }

        public static void RegisterFactory(ICustomFactory factory)
        {
            Factories.Add(factory);
        }

        public static void RegisterSimpleCustomComponents(Assembly assembly)
        {
            RegisterSimpleCustomComponents(assembly.GetTypes());
        }

        #region here be dragons

        public static void RegisterSimpleCustomComponents(params Type[] types)
        {
            var scGenericType = typeof(SimpleCustom<>);
            foreach (var tuple in types.Select(t => new {type = t, typeWithGenericType = GetTypeWithGenericType(t, scGenericType)}).Where(t => t.typeWithGenericType != null))
            {
                var type = tuple.type;
                var customAttribute = type.GetCustomAttributes(false).OfType<CustomComponentAttribute>().FirstOrDefault();
                if (customAttribute == null)
                {
                    continue;
                }

                var name = customAttribute.Name;
                if (!SimpleIdentifiers.Add(name))
                {
                    continue;
                }

                var typeWithGenericType = tuple.typeWithGenericType;
                var defType = typeWithGenericType.GetGenericArguments()[0];

                var factoryGenericType = typeof(SimpleCustomFactory<,>);
                var genericTypes = new[] {type, defType};
                var factoryType = factoryGenericType.MakeGenericType(genericTypes);
                var factory = Activator.CreateInstance(factoryType, name) as ICustomFactory;
                Factories.Add(factory);
            }
        }

        public static Type GetTypeWithGenericType(Type givenType, Type genericType)
        {
            var type = givenType.GetInterfaces().FirstOrDefault(it => it.IsGenericType && it.GetGenericTypeDefinition() == genericType);
            if (type != null)
            {
                return type;
            }

            type = givenType.BaseType;
            if (type == null)
            {
                return null;
            }

            if (type.IsGenericType && type.GetGenericTypeDefinition() == genericType)
            {
                return type;
            }

            return GetTypeWithGenericType(type, genericType);
        }

        #endregion

        internal static void ProcessCustomFactories(object target, Dictionary<string, object> values)
        {
            var identifier = Database.Identifier(target);
            if (identifier == null)
            {
                return;
            }

            //Control.Logger.LogDebug($"ProcessCustomCompontentFactories for {target.GetType()}");
            //Control.Logger.LogDebug($"- {key}");

            foreach (var preProcessor in PreProcessors)
            {
                preProcessor.PreProcess(target, values);
            }

            foreach (var factory in Factories)
            {
                var component = factory.Create(target, values);
                if (component == null)
                {
                    continue;
                }

                //Control.Logger.LogDebug($"-- Created {factory.ComponentSectionName} for {componentDef.Description.Id}");
                Database.Set(identifier, component);

                if (component is IAfterLoad load)
                {
                    //Control.Logger.LogDebug($"IAfterLoad: {obj.Def.Description.Id}");
                    load.OnLoaded(values);
                }
            }

            //Control.Logger.LogDebug("- done");
        }
    }
}