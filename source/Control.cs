using BattleTech;
using Harmony;
using HBS.Logging;
using HBS.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;


namespace CustomComponents
{

    public static class Control
    {
        public static CustomComponentSettings Settings = new();

        private static ILog Logger;
        private static FileLogAppender logAppender;

        internal const string CustomSectionName = "Custom";
        internal static string LogPrefix = "[CC]";

        public static void Init(string directory, string defaultsSettingsJson)
        {
            Logger = HBS.Logging.Logger.GetLogger("CustomComponents", LogLevel.Debug);
            SetupLogging(directory);

            try
            {
                try
                {
                    JSONSerializationUtility.FromJSON(Settings, defaultsSettingsJson);
                }
                catch (Exception e)
                {
                    Logger.LogError("Couldn't load default settings", e);
                    Settings = new CustomComponentSettings();
                }

                var settingsPath = Path.Combine(directory, "settings.json");
                if (File.Exists(settingsPath))
                {
                    try
                    {
                        var settingsJson = File.ReadAllText(settingsPath);
                        JSONSerializationUtility.FromJSON(Settings, settingsJson);
                    }
                    catch (Exception e)
                    {
                        Logger.LogError("Couldn't load settings", e);
                    }
                }

                HBS.Logging.Logger.SetLoggerLevel(Logger.Name, Settings.LogLevel);
                Settings.Complete();

                if (Control.Settings.DEBUG_ShowConfig)
                    Log(JSONSerializationUtility.ToJSON(Settings));

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
                    Control.LogError("AmbiguousMatchException\n" + values);
                }

                Registry.RegisterPreProcessor(new CategoryDefaultCustomsPreProcessor());
                Registry.RegisterSimpleCustomComponents(Assembly.GetExecutingAssembly());

                var version = Assembly.GetExecutingAssembly().GetName().Version;
                Logger.Log($"Loaded CustomComponents {version.ToString(3)} for bt 1.9.1");
                Logger.Log("DebugInfo: " + Settings.DebugInfo);
                Logger.Log("- DumpMechDefs:" + Settings.DEBUG_DumpMechDefs);
                Logger.Log("-- MechDefsDir: " + Settings.DEBUG_MechDefsDir);
                Logger.Log("- ValidateMechDefs: " + Settings.DEBUG_ValidateMechDefs);
                Logger.Log("-- ShowOnlyErrors: " + Settings.DEBUG_ShowOnlyErrors);
                Logger.Log("- ShowAllUnitTypes: " + Settings.DEBUG_ShowAllUnitTypes);
                Logger.Log("- EnableAllTags: " + Settings.DEBUG_EnableAllTags);
                Logger.Log("- ShowConfig: " + Settings.DEBUG_ShowConfig);
                Logger.Log("- ShowLoadedCategory: " + Settings.DEBUG_ShowLoadedCategory);
                Logger.Log("- ShowLoadedDefaults: " + Settings.DEBUG_ShowLoadedDefaults);
                Logger.Log("- ShowLoadedAlLocations: " + Settings.DEBUG_ShowLoadedAlLocations);
                Logger.Log("- ShowMechUT: " + Settings.DEBUG_ShowMechUT);
                Logger.Log("- ShowLoadedHardpoints: " + Settings.DEBUG_ShowLoadedHardpoints);


                Validator.RegisterMechValidator(CategoryController.Shared.ValidateMech, CategoryController.Shared.ValidateMechCanBeFielded);
                Validator.RegisterMechValidator(TagRestrictionsHandler.Shared.ValidateMech, TagRestrictionsHandler.Shared.ValidateMechCanBeFielded);
                Validator.RegisterMechValidator(CCFlags.ValidateMech, CCFlags.CanBeFielded);
                Validator.RegisterMechValidator(EquipLocationController.Instance.ValidateMech, EquipLocationController.Instance.CanBeFielded);
                Validator.RegisterMechValidator(HardpointController.Instance.ValidateMech, HardpointController.Instance.CanBeFielded);
                Validator.RegisterMechValidator(AutoLinked.ValidateMech, AutoLinked.CanBeFielded);
                if (Control.Settings.CheckWeaponCount)
                {
                    Validator.RegisterMechValidator(WeaponsCountFix.CheckWeapons, WeaponsCountFix.CheckWeaponsFielded);
                }


                Validator.RegisterDropValidator(check: CategoryController.Shared.ValidateDrop);
                Validator.RegisterDropValidator(pre: TagRestrictionsHandler.Shared.ValidateDrop);
                Validator.RegisterDropValidator(check: HardpointController.Instance.PostValidatorDrop);
                Validator.RegisterDropValidator(EquipLocationController.Instance.PreValidateDrop);


                if (Settings.RunAutofixer)
                {
                    if (Settings.FixDeletedComponents)
                        AutoFixer.Shared.RegisterMechFixer(AutoFixer.Shared.RemoveEmptyRefs);
                    if (Settings.FixSaveGameMech)
                        AutoFixer.Shared.RegisterSaveMechFixer(AutoFixer.Shared.ReAddFixed);
                    if (Settings.FixDefaults)
                    {
                        AutoFixer.Shared.RegisterMechFixer(DefaultFixer.Instance.FixMechs);
                        AutoFixer.Shared.RegisterMechFixer(HardpointController.Instance.FixMechs);
                    }
                }

                Logger.LogDebug("done");
            }
            catch (Exception e)
            {
                Logger.LogError(e);
            }
        }

        public static bool Loaded { get; private set; } = false;
        public static void FinishedLoading(Dictionary<string, Dictionary<string, VersionManifestEntry>> customResources)
        {
            var Manifests = customResources;

            Control.LogDebug(DType.CustomResource, "Custom Resource Load started");

            UnitTypeDatabase.Instance.Setup(Manifests);
            CategoryController.Shared.Setup(Manifests);
            DefaultsDatabase.Instance.Setup(Manifests);
            TagRestrictionsHandler.Shared.Setup(Manifests);
            EquipLocationController.Instance.Setup(Manifests);
            HardpointController.Instance.Setup(Manifests);

            if (Manifests.TryGetValue("CustomSVGIcon", out var icons))
                IconController.LoadIcons(icons);
            Control.LogDebug(DType.CustomResource, " - done");
            Loaded = true;
        }



        #region LOGGING
        [Conditional("CCDEBUG")]
        public static void LogDebug(DType type, string message)
        {
            if (Settings.DebugInfo.HasFlag(type))
                Logger.LogDebug(LogPrefix + message);
        }
        [Conditional("CCDEBUG")]
        public static void LogDebug(DType type, string message, Exception e)
        {
            if (Settings.DebugInfo.HasFlag(type))
                Logger.LogDebug(LogPrefix + message, e);
        }
        [Conditional("CCDEBUG")]
        public static void LogDebug(DType type, string message, params object[] objs)
        {
            if (Settings.DebugInfo.HasFlag(type))
                Logger.LogDebug(LogPrefix + string.Format(message, objs));
        }
        public static void LogError(string message)
        {
            Logger.LogError(LogPrefix + message);
        }
        public static void LogError(string message, Exception e)
        {
            Logger.LogError(LogPrefix + message, e);
        }
        public static void LogError(Exception e)
        {
            Logger.LogError(LogPrefix, e);
        }

        public static void Log(string message)
        {
            Logger.Log(LogPrefix + message);
        }



        internal static void SetupLogging(string Directory)
        {
            var logFilePath = Path.Combine(Directory, "log.txt");

            try
            {
                ShutdownLogging();
                AddLogFileForLogger(logFilePath);
            }
            catch (Exception e)
            {
                Logger.Log("CustomComponents: can't create log file", e);
            }
        }

        internal static void ShutdownLogging()
        {
            if (logAppender == null)
            {
                return;
            }

            try
            {
                HBS.Logging.Logger.ClearAppender("CustomComponents");
                logAppender.Flush();
                logAppender.Close();
            }
            catch
            {
            }

            logAppender = null;
        }

        private static void AddLogFileForLogger(string logFilePath)
        {
            try
            {
                logAppender = new FileLogAppender(logFilePath, FileLogAppender.WriteMode.INSTANT);
                HBS.Logging.Logger.AddAppender("CustomComponents", logAppender);

            }
            catch (Exception e)
            {
                Logger.Log("CustomComponents: can't create log file", e);
            }
        }

        #endregion
    }
}
