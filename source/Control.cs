using Harmony;
using System;
using System.Reflection;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using fastJSON;
using BattleTech;
using HBS.Extensions;
using HBS.Logging;
using HBS.Util;


namespace CustomComponents
{
    public static class Control
    {
        public static CustomComponentSettings Settings = new CustomComponentSettings();

        private static ILog Logger;
        private static FileLogAppender logAppender;

        internal const string CustomSectionName = "Custom";
        internal static string LogPrefix = "[CC]";

        public static void Init(string directory, string settingsJSON)
        {
            Logger = HBS.Logging.Logger.GetLogger("CustomComponents", LogLevel.Debug);
            SetupLogging(directory);
            
            try
            {
                try
                {

                    Settings = new CustomComponentSettings();
                    JSONSerializationUtility.FromJSON(Settings, settingsJSON);
                    HBS.Logging.Logger.SetLoggerLevel(Logger.Name, Settings.LogLevel);
                }
                catch (Exception e)
                {
                    Logger.LogError("Couldn't load settings", e);
                    Settings = new CustomComponentSettings();
                }
                Logger.LogError($"{Settings.TestEnableAllTags}");


                Settings.Complete();

                LogDebug(DType.ShowConfig, JSONSerializationUtility.ToJSON(Settings));


                var harmony = HarmonyInstance.Create("io.github.denadan.CustomComponents");
                harmony.PatchAll(Assembly.GetExecutingAssembly());

                //Registry.RegisterPreProcessor(new AdjustDescriptionPreProcessor());

                Registry.RegisterSimpleCustomComponents(Assembly.GetExecutingAssembly());
                Validator.RegisterMechValidator(CategoryController.Shared.ValidateMech, CategoryController.Shared.ValidateMechCanBeFielded);

                Logger.Log("Loaded CustomComponents v3.0.1 for bt 1.9.1");

                Validator.RegisterMechValidator(TagRestrictionsHandler.Shared.ValidateMech, TagRestrictionsHandler.Shared.ValidateMechCanBeFielded);
                Validator.RegisterMechValidator(FlagsController.Instance.ValidateMech, FlagsController.Instance.CanBeFielded);
                Validator.RegisterDropValidator(pre: TagRestrictionsHandler.Shared.ValidateDrop);
                if(Control.Settings.CheckWeaponCount)
                {
                    Validator.RegisterMechValidator(WeaponsCountFix.CheckWeapons, WeaponsCountFix.CheckWeaponsFielded);
                }

                if (Settings.RunAutofixer)
                {
                    if (Settings.FixDeletedComponents)
                        AutoFixer.Shared.RegisterMechFixer(AutoFixer.Shared.RemoveEmptyRefs);

                    if (Settings.FixSaveGameMech)
                    {
                        AutoFixer.Shared.RegisterSaveMechFixer(AutoFixer.Shared.ReAddFixed);
                        AutoFixer.Shared.RegisterSaveMechFixer(CategoryController.Shared.RemoveExcessDefaults);
                    }

                    if (Settings.FixDefaults)
                        AutoFixer.Shared.RegisterMechFixer(DefaultFixer.Shared.FixMechs);

                    if (Settings.DebugInfo.HasFlag(DType.AutoFixFAKE))
                        AutoFixer.Shared.RegisterMechFixer(DEBUGTOOLS.SHOWTAGS);

                    if(Settings.ignoreAutofixTags != null && Settings.ignoreAutofixTags.Count > 0)
                        foreach (var tag in Settings.ignoreAutofixTags)
                            AutoFixer.Shared.RegisterMechFixer(AutoFixer.Shared.EmptyFixer, tag);
                }

                FlagsController.Instance.RegisterFlag("autorepair");
                FlagsController.Instance.RegisterFlag("no_remove");
                FlagsController.Instance.RegisterFlag("hide");
                FlagsController.Instance.RegisterFlag("no_salvage");
                FlagsController.Instance.RegisterFlag("default",(item) => item.Is<IDefault>(), new[] { "autorepair", "no_remove", "hide", "no_salvage" });
                FlagsController.Instance.RegisterFlag("not_broken");
                FlagsController.Instance.RegisterFlag("not_destroyed");
                FlagsController.Instance.RegisterFlag("invalid");
                FlagsController.Instance.RegisterFlag("hide_remove_message");
                FlagsController.Instance.RegisterFlag("vital");

                Logger.LogDebug("done");
            }
            catch (Exception e)
            {
                Logger.LogError(e);
            }
        }

        public static void FinishedLoading(Dictionary<string, Dictionary<string, VersionManifestEntry>> customResources)
        {
            Control.LogDebug(DType.CustomResource, "Custom Resource Load started");
            CategoryController.Shared.Setup(customResources);
            DefaultsDatabase.Instance.Setup(customResources);
            TagRestrictionsHandler.Shared.Setup(customResources);
            if (customResources.TryGetValue("CustomSVGIcon", out var icons))
                IconController.LoadIcons(icons);
            Control.LogDebug(DType.CustomResource, " - done");

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
