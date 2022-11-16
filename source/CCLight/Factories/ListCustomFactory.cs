using System;
using System.Collections.Generic;
using System.Linq;

namespace CustomComponents;

public interface IListComponent<TValue>
{
    void LoadList(IEnumerable<TValue> items);
}

public class ListCustomFactory<TCustom, TDef, TValue> : ICustomFactory
    where TCustom : SimpleCustom<TDef>, IListComponent<TValue>, new()
    where TDef : class
{
    public string CustomName { get; }
    public ListCustomFactory(string customName)
    {
        CustomName = customName;
    }

    private TValue toValue(object item, object def)
    {
        try
        {
            return (TValue)Convert.ChangeType(item, typeof(TValue));
        }
        catch (Exception e)
        {
            Log.Main.Error?.Log($"{item} is wrong value for {CustomName} for {Database.Identifier(def)} used {default(TValue)}", e);
            return default;
        }
    }

    public IEnumerable<ICustom> Create(object target, Dictionary<string, object> values)
    {
        if (!(target is TDef def))
        {
            yield break;
        }

        if (!values.TryGetValue(Control.CustomSectionName, out var customSettingsObject))
        {
            yield break;
        }

        if (!(customSettingsObject is Dictionary<string, object> customSettings))
        {
            yield break;
        }

        if (!customSettings.TryGetValue(CustomName, out var componentSettingsObject))
        {
            yield break;
        }

        if (componentSettingsObject == null)
        {
            yield break;
        }

        if (!(componentSettingsObject is IEnumerable<object> compList))
        {
            yield break;
        }

        var obj = new TCustom();
        obj.Def = def;
        obj.LoadList(compList.Select(i => toValue(i, def)));

        yield return obj;
    }


}

public class EnumListCustomFactory<TCustom, TDef, TValue> : ICustomFactory
    where TCustom : SimpleCustom<TDef>, IListComponent<TValue>, new()
    where TDef : class
    where TValue : struct
{
    public string CustomName { get; }
    public EnumListCustomFactory(string customName)
    {
        CustomName = customName;
    }

    private TValue parse(object val, object def)
    {
        if (val == null)
        {
            Log.Main.Error?.Log($"{CustomName} for {Database.Identifier(def)} has null, used {default(TValue)}");
            return default;
        }

        if (val is string str)
            if (Enum.TryParse(str, true, out TValue res))
                return res;
            else
            {
                Log.Main.Error?.Log($"{val} is wrong value for {CustomName} for {Database.Identifier(def)} used {default(TValue)}");
                return default;
            }

        try
        {
            return (TValue)val;
        }
        catch (Exception e)
        {
            Log.Main.Error?.Log($"{val} is wrong value for {CustomName} for {Database.Identifier(def)} used {default(TValue)}", e);
            return default;
        }
    }

    public IEnumerable<ICustom> Create(object target, Dictionary<string, object> values)
    {
        if (!(target is TDef def))
        {
            yield break;
        }

        if (!values.TryGetValue(Control.CustomSectionName, out var customSettingsObject))
        {
            yield break;
        }

        if (!(customSettingsObject is Dictionary<string, object> customSettings))
        {
            yield break;
        }

        if (!customSettings.TryGetValue(CustomName, out var componentSettingsObject))
        {
            yield break;
        }

        if (componentSettingsObject == null)
        {
            yield break;
        }

        if (!(componentSettingsObject is IEnumerable<object> compList))
        {
            yield break;
        }

        var obj = new TCustom();
        obj.Def = def;
        obj.LoadList(compList.Select(i => parse(i, def)));

        yield return obj;
    }


}