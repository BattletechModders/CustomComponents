using System.Collections.Generic;
using System.Linq;
using BattleTech;

namespace CustomComponents;

public class UnitTypeDatabase
{
    private static UnitTypeDatabase _instance;

    List<IUnitType> types = new();
    Dictionary<string, HashSet<string>> known =new();


    public static UnitTypeDatabase Instance
    {
        get
        {
            if(_instance ==null)
                _instance = new();
            return _instance;
        }
    }

    private UnitTypeDatabase()
    {


    }

    public void Setup(Dictionary<string, Dictionary<string, VersionManifestEntry>> customResources)
    {
        foreach (var unitType in SettingsResourcesTools.Enumerate<TagUnitType>("CCUnitTypes", customResources))
        {
            RegisterUnitType(unitType);
        }
    }

    public void RegisterUnitType(IUnitType unitType)
    {
        if (unitType == null)
        {
            Log.Main.Error?.Log("Null unit type, skipping");
        }

        var old = types.FirstOrDefault(i => i.Name == unitType.Name);
        if (old != null)
        {
            Log.UnitType.Trace?.Log("-- removed old one");
            types.Remove(old);
        }

        types.Add(unitType);
        Log.Main.Info?.Log("UnitType " + unitType.Name + " Registred");
    }

    public HashSet<string> GetUnitTypes(ChassisDef chassis)
    {
        if (chassis == null)
            return new();

        if (known.TryGetValue(chassis.Description.Id, out var result))
            return result;

        var mechid = GetMechIDFromChassisID(chassis.Description.Id);
        var mech = UnityGameInstance.BattleTechGame.DataManager.MechDefs.Get(mechid);
        if(mech == null)
            return new();

        return BuildUnitTypes(mech);
    }



    public HashSet<string> GetUnitTypes(MechDef mech)
    {
        if (mech == null)
            return new();

        if (known.TryGetValue(mech.ChassisID, out var result))
            return result;

        return BuildUnitTypes(mech);
    }

    //FOR PATCHES!!!
    public string GetMechIDFromChassisID(string descriptionId)
    {
        return descriptionId.Replace("chassisdef_", "mechdef_");
    }

    private HashSet<string> BuildUnitTypes(MechDef mech)
    {
        HashSet<string> result;
        if (mech.Is<UnitTypeCustom>(out var unitTypes))
        {
            result = unitTypes.Types;
        }
        else
        {
            result = types
                .Where(ut => ut.IsThisType(mech))
                .Select(ut => ut.Name)
                .ToHashSet();
        }

        if (mech.Is<UnitTypeAddCustom>(out var unitTypesToAdd))
        {
            result = result.Concat(unitTypesToAdd.Types).ToHashSet();
        }

        known[mech.ChassisID] = result;
        return result;
    }

    public void ShowRegistredTypes()
    {
        if (Log.UnitType.Debug != null)
        {
            foreach (var unitType in types)
            {
                Log.UnitType.Debug.Log(unitType.ToString());
            }
        }
    }
}