using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CustomComponents;

public static class Registry
{
    private static readonly List<IPreProcessor> PreProcessors = new();
    private static readonly List<IPostProcessor> PostProcessors = new();
    private static readonly List<ICustomFactory> Factories = new();
    private static readonly HashSet<string> FactoryIdentifiers = new();

    public static void RegisterPreProcessor(IPreProcessor preProcessor)
    {
        PreProcessors.Add(preProcessor);
    }

    internal static void RegisterPostProcessor(IPostProcessor postProcessor)
    {
        PostProcessors.Add(postProcessor);
    }

    public static void RegisterSimpleCustomComponents(Assembly assembly)
    {
        RegisterSimpleCustomComponents(assembly.GetTypes());
    }

    public static void RegisterSimpleCustomComponents(params Type[] types)
    {
        var scGenericType = typeof(SimpleCustom<>);
        foreach (var tuple in types
                     .Select(t => new { type = t, typeWithGenericType = GetTypeWithGenericType(t, scGenericType) })
                     .Where(t => t.typeWithGenericType != null))
        {
            var type = tuple.type;
            var customAttribute =
                type.GetCustomAttributes(false).OfType<CustomComponentAttribute>().FirstOrDefault();
            if (customAttribute == null)
            {
                continue;
            }

            var name = customAttribute.Name;
            if (!FactoryIdentifiers.Add(name))
            {
                Log.Main.Info?.Log($"SimpleCustom {name} already registered");
                continue;
            }

            var typeWithGenericType = tuple.typeWithGenericType;
            var defType = typeWithGenericType.GetGenericArguments()[0];
            Type factoryGenericType = null;
            Type[] genericTypes = null;

            foreach (var intType in type.GetInterfaces().Where(i => i.IsGenericType))
            {
                if (intType.GetGenericTypeDefinition() == typeof(IValueComponent<>))
                {
                    var argument = intType.GetGenericArguments()[0];
                    factoryGenericType = argument.IsEnum ? typeof(EnumValueCustomFactory<,,>) : typeof(ValueCustomFactory<,,>);
                    genericTypes = new[] { type, defType,argument };
                    break;
                }

                if (intType.GetGenericTypeDefinition() == typeof(IListComponent<>))
                {
                    var argument = intType.GetGenericArguments()[0];
                    factoryGenericType = argument.IsEnum ? typeof(EnumListCustomFactory<,,>) : typeof(ListCustomFactory<,,>);
                    genericTypes = new[] { type, defType, argument };
                    break;
                }
            }

            if(factoryGenericType == null)
            {
                factoryGenericType = typeof(SimpleCustomFactory<,>);
                genericTypes = new[] { type, defType };
            }

            var factoryType = factoryGenericType.MakeGenericType(genericTypes);
            var factory = Activator.CreateInstance(factoryType, name) as ICustomFactory;
            Factories.Add(factory);
            Log.Main.Info?.Log($"SimpleCustom {name} registered for type {defType}");
        }
    }

    private static Type GetTypeWithGenericType(Type givenType, Type genericType)
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

    internal static void ProcessCustomFactories(object target, Dictionary<string, object> values)
    {
        if (target == null)
        {
            Log.Main.Error?.Log("NULL item loaded");
            foreach (var value in values)
            {
                Log.Main.Error?.Log($"- {value.Key}: {value.Value}");
            }
            return;
        }

        var identifier = Database.Identifier(target);

        if (identifier == null)
        {
            return;
        }

        Log.CCLoading.Trace?.Log($"ProcessCustomCompontentFactories for {target.GetType()} ({target.GetHashCode()})");
        Log.CCLoading.Trace?.Log($"- {identifier}");

        foreach (var preProcessor in PreProcessors)
        {
            preProcessor.PreProcess(target, values);
        }

        var loaded = false;
        foreach (var factory in Factories)
        {
            try
            {
                ProcessCustomFactory(factory, target, values, identifier, ref loaded);
            }
            catch (Exception e)
            {
                Log.Main.Error?.Log($"Error when processing custom factory {factory.CustomName} for {target.GetType()} ({target.GetHashCode()})", e);
            }
        }

        foreach (var postProcessor in PostProcessors)
        {
            postProcessor.PostProcess(target, values);
        }

#if DEBUG
            if (loaded)
            {
                Log.CCLoadingSummary.Trace?.Log($"ProcessCustomCompontentFactories for {target.GetType()} ({target.GetHashCode()})");
                Log.CCLoadingSummary.Trace?.Log($"- {identifier}");

                Log.CCLoadingSummary.Trace?.Log("- Loaded:");

                foreach (var custom in Database.GetCustoms<ICustom>(target))
                {
                    Log.CCLoadingSummary.Trace?.Log($"--- {custom}");
                }

                Log.CCLoadingSummary.Trace?.Log("- done");
            }
#endif

    }

    private static void ProcessCustomFactory(
        ICustomFactory factory,
        object target,
        Dictionary<string, object> values,
        string identifier,
        ref bool loaded)
    {
        foreach (var component in factory.Create(target, values))
        {
            loaded = true;
            if (component == null)
            {
                continue;
            }

            Log.CCLoading.Trace?.Log($"Created {component} for {identifier}");

            if (Database.AddCustom(identifier, component))
            {
                if (component is IAfterLoad load)
                {
                    Log.CCLoading.Trace?.Log($"IAfterLoad: {identifier}");
                    load.OnLoaded(values);
                }

                if (component is IOnLoaded onLoaded)
                {
                    Log.CCLoading.Trace?.Log($"IOnLoaded: {identifier}");
                    onLoaded.OnLoaded();
                }

                if (component is IAdjustDescription ed)
                {
                    ed.AdjustDescription();
                }
            }
        }
    }
}