using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using BattleTech;
using HBS.Util;

namespace CustomComponents;

public static class Control
{
    public static CustomComponentSettings Settings = new();

    internal const string CustomSectionName = "Custom";

    public static void Init(string directory, string settingsInitJson)
    {
        try
        {
            LoadSettingsAndSetupLogger(directory, settingsInitJson);

            Settings.Complete();

            if (Settings.DEBUG_ShowConfig)
                Log.Main.Info?.Log(JSONSerializationUtility.ToJSON(Settings));

            var harmony = HarmonyInstance.Create("io.github.denadan.CustomComponents");
            try
            {
                harmony.PatchAll(Assembly.GetExecutingAssembly());
            }
            catch (AmbiguousMatchException ame)
            {
                var values = "";
                foreach (DictionaryEntry dictionaryEntry in ame.Data)
                {
                    values += $"  {dictionaryEntry.Key} : {dictionaryEntry.Value}\n";
                }
                Log.Main.Error?.Log("AmbiguousMatchException\n" + values);
            }

            Registry.RegisterPreProcessor(new CategoryDefaultCustomsPreProcessor());
            Registry.RegisterSimpleCustomComponents(Assembly.GetExecutingAssembly());

            var version = Assembly.GetExecutingAssembly().GetName().Version;
            Log.Main.Info?.Log($"Loaded CustomComponents {version.ToString(3)} for bt 1.9.1");
            Log.Main.Debug?.Log("- DumpMechDefs:" + Settings.DEBUG_DumpMechDefs);
            Log.Main.Debug?.Log("-- MechDefsDir: " + Settings.DEBUG_MechDefsDir);
            Log.Main.Debug?.Log("- ValidateMechDefs: " + Settings.DEBUG_ValidateMechDefs);
            Log.Main.Debug?.Log("-- ShowOnlyErrors: " + Settings.DEBUG_ShowOnlyErrors);
            Log.Main.Debug?.Log("- ShowAllUnitTypes: " + Settings.DEBUG_ShowAllUnitTypes);
            Log.Main.Debug?.Log("- EnableAllTags: " + Settings.DEBUG_EnableAllTags);
            Log.Main.Debug?.Log("- ShowConfig: " + Settings.DEBUG_ShowConfig);
            Log.Main.Debug?.Log("- ShowLoadedCategory: " + Settings.DEBUG_ShowLoadedCategory);
            Log.Main.Debug?.Log("- ShowLoadedDefaults: " + Settings.DEBUG_ShowLoadedDefaults);
            Log.Main.Debug?.Log("- ShowLoadedAlLocations: " + Settings.DEBUG_ShowLoadedAlLocations);
            Log.Main.Debug?.Log("- ShowMechUT: " + Settings.DEBUG_ShowMechUT);
            Log.Main.Debug?.Log("- ShowLoadedHardpoints: " + Settings.DEBUG_ShowLoadedHardpoints);


            Validator.RegisterMechValidator(CategoryController.Shared.ValidateMech, CategoryController.Shared.ValidateMechCanBeFielded);
            Validator.RegisterMechValidator(TagRestrictionsHandler.Shared.ValidateMech, TagRestrictionsHandler.Shared.ValidateMechCanBeFielded);
            Validator.RegisterMechValidator(CCFlags.ValidateMech, CCFlags.CanBeFielded);
            Validator.RegisterMechValidator(EquipLocationController.Instance.ValidateMech, EquipLocationController.Instance.CanBeFielded);
            Validator.RegisterMechValidator(HardpointController.Instance.ValidateMech, HardpointController.Instance.CanBeFielded);
            Validator.RegisterMechValidator(AutoLinked.ValidateMech, AutoLinked.CanBeFielded);
            if (Settings.CheckWeaponCount)
            {
                Validator.RegisterMechValidator(WeaponsCountFix.CheckWeapons, WeaponsCountFix.CheckWeaponsFielded);
            }


            Validator.RegisterDropValidator(check: CategoryController.Shared.ValidateDrop);
            Validator.RegisterDropValidator(pre: TagRestrictionsHandler.Shared.ValidateDrop);
            Validator.RegisterDropValidator(check: HardpointController.Instance.PostValidatorDrop);
            Validator.RegisterDropValidator(EquipLocationController.Instance.PreValidateDrop);


            if (Settings.RunAutofixer)
            {
                MechDefProcessing.Instance.Register(AutoFixer.Shared);
                if (Settings.FixDeletedComponents)
                {
                    AutoFixer.Shared.RegisterMechFixer(MechDefInventoryCleanup.RemoveEmptyRefs);
                }
                if (Settings.FixDefaults)
                {
                    AutoFixer.Shared.RegisterMechFixer(DefaultFixer.Instance.FixMechs);
                    AutoFixer.Shared.RegisterMechFixer(HardpointController.Instance.FixMechs);
                }
            }

            Log.Main.Info?.Log("done");
        }
        catch (Exception e)
        {
            Log.Main.Error?.Log(e);
        }
    }

    private static void LoadSettingsAndSetupLogger(string directory, string settingsInitJson)
    {
        Exception settingsInitEx;
        try
        {
            JSONSerializationUtility.FromJSON(Settings, settingsInitJson);
            settingsInitEx = null;
        }
        catch (Exception e)
        {
            settingsInitEx = e;
            Settings = new();
        }

        Exception settingsFileEx;
        try
        {
            var settingsPath = Path.Combine(directory, "settings.json");
            if (File.Exists(settingsPath))
            {
                var settingsJson = File.ReadAllText(settingsPath);
                JSONSerializationUtility.FromJSON(Settings, settingsJson);
            }

            settingsFileEx = null;
        }
        catch (Exception e)
        {
            settingsFileEx = e;
        }

        if (settingsInitEx != null)
        {
            Log.Main.Error?.Log("Couldn't load default settings", settingsInitEx);
        }

        if (settingsFileEx != null)
        {
            Log.Main.Error?.Log("Couldn't load settings", settingsFileEx);
        }
    }

    public static bool Loaded { get; private set; }
    public static void FinishedLoading(Dictionary<string, Dictionary<string, VersionManifestEntry>> customResources)
    {
        var Manifests = customResources;

        Log.CustomResource.Trace?.Log("Custom Resource Load started");

        UnitTypeDatabase.Instance.Setup(Manifests);
        CategoryController.Shared.Setup(Manifests);
        DefaultsDatabase.Instance.Setup(Manifests);
        TagRestrictionsHandler.Shared.Setup(Manifests);
        EquipLocationController.Instance.Setup(Manifests);
        HardpointController.Instance.Setup(Manifests);

        if (Manifests.TryGetValue("CustomSVGIcon", out var icons))
            IconController.LoadIcons(icons);
        Log.CustomResource.Trace?.Log(" - done");
        Loaded = true;
    }
}