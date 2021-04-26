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
        private static readonly List<ICustomFactory> Factories = new List<ICustomFactory>();
        private static readonly HashSet<string> SimpleIdentifiers = new HashSet<string>();

        private static Dictionary<Type, CustomComponentAttribute> attributes = new Dictionary<Type, CustomComponentAttribute>() ;

        public static CustomComponentAttribute GetAttributeByType(Type type)
        {
            if (attributes.TryGetValue(type, out var result))
                return result;
            return null;
        }

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
            foreach (var tuple in types.Select(t => new { type = t, typeWithGenericType = GetTypeWithGenericType(t, scGenericType) }).Where(t => t.typeWithGenericType != null))
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
                    Control.Log($"SimpleCustom {name} already registered");
                    continue;
                }

                attributes.Add(tuple.type, customAttribute);

                var typeWithGenericType = tuple.typeWithGenericType;
                var defType = typeWithGenericType.GetGenericArguments()[0];

                var factoryGenericType = typeof(SimpleCustomFactory<,>);
                var genericTypes = new[] { type, defType };
                var factoryType = factoryGenericType.MakeGenericType(genericTypes);
                var factory = Activator.CreateInstance(factoryType, name) as ICustomFactory;
                Factories.Add(factory);
                Control.Log($"SimpleCustom {name} registered for type {defType}");
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

        internal static void ProcessCustomFactories(object target, Dictionary<string, object> values, bool replace = true)
        {
            if (target == null)
            {
                Control.LogError($"NULL item loaded");
                foreach (var value in values)
                {
                    Control.LogError($"- {value.Key}: {value.Value}");
                }
            }

            var identifier = Database.Identifier(target);

            if (identifier == null)
            {
                return;
            }

            Control.LogDebug(DType.CCLoading, $"ProcessCustomCompontentFactories for {target.GetType()} ({target.GetHashCode()})");
            Control.LogDebug(DType.CCLoading, $"- {identifier}");

            if (replace)
            {
                foreach (var preProcessor in PreProcessors)
                {
                    preProcessor.PreProcess(target, values);
                }
            }

#if CCDEBUG
            bool loaded = false;
#endif
            foreach (var factory in Factories)
            {
                foreach (var component in factory.Create(target, values))
                {
#if CCDEBUG
                    loaded = true;
#endif
                    if (component == null)
                    {
                        continue;
                    }

                    Control.LogDebug(DType.CCLoading, $"Created {component} for {identifier}");

                    if (Database.SetCustomWithIdentifier(identifier, component, replace) )
                    {
                        if (component is IAfterLoad load)
                        {
                            Control.LogDebug(DType.CCLoading, $"IAfterLoad: {identifier}");
                            load.OnLoaded(values);
                        }

                        if (component is IAdjustDescriptionED ed)
                        {
                            Control.DelayLoading(ed.AdjustDescription);
                        }
                    }
                }
            }

#if CCDEBUG
            if (loaded)
            {
                Control.LogDebug(DType.CCLoadingSummary, $"ProcessCustomCompontentFactories for {target.GetType()} ({target.GetHashCode()})");
                Control.LogDebug(DType.CCLoadingSummary, $"- {identifier}");

                Control.LogDebug(DType.CCLoadingSummary, "- Loaded:");

                foreach (var custom in Database.GetCustoms<ICustom>(target))
                {
                    Control.LogDebug(DType.CCLoadingSummary, $"--- {custom}");
                }

                Control.LogDebug(DType.CCLoadingSummary, "- done");
            }

#endif
        }
    }
}