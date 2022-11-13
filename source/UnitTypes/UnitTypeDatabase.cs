using System.Collections.Generic;
using System.Linq;
using BattleTech;

namespace CustomComponents
{
    public class UnitTypeDatabase
    {
        private static UnitTypeDatabase _instance;

        List<IUnitType> types = new List<IUnitType>();
        Dictionary<string, HashSet<string>> known =new Dictionary<string, HashSet<string>>();


        public static UnitTypeDatabase Instance
        {
            get
            {
                if(_instance ==null)
                    _instance = new UnitTypeDatabase();
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
                Logging.Error?.Log("Null unit type, skipping");
            }

            var old = types.FirstOrDefault(i => i.Name == unitType.Name);
            if (old != null)
            {
                Logging.Debug?.LogDebug(DType.UnitType, $"-- removed old one");
                types.Remove(old);
            }

            types.Add(unitType);
            Logging.Info?.Log("UnitType " + unitType.Name + " Registred");
        }

        public HashSet<string> GetUnitTypes(ChassisDef chassis)
        {
            if (chassis == null)
                return new HashSet<string>();

            if (known.TryGetValue(chassis.Description.Id, out var result))
                return result;

            var mechid = GetMechIDFromChassisID(chassis.Description.Id);
            var mech = UnityGameInstance.BattleTechGame.DataManager.MechDefs.Get(mechid);
            if(mech == null)
                return new HashSet<string>();

            return BuildUnitTypes(mech);
        }



        public HashSet<string> GetUnitTypes(MechDef mech)
        {
            if (mech == null)
                return new HashSet<string>();

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
            if (!Control.Settings.DebugInfo.HasFlag(DType.UnitType))
                return;
            foreach (var unitType in types)
            {
                Logging.Debug?.LogDebug(DType.UnitType, unitType.ToString());
            }
        }
    }

}