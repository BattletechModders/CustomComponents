#undef CCDEBUG

using Harmony;
using System;
using System.Reflection;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BattleTech;
using HBS.Logging;
using HBS.Util;


namespace CustomComponents
{
    public static class Control
    {
        public static CustomComponentSettings Settings = new CustomComponentSettings();

        internal static ILog Logger;
        private static FileLogAppender logAppender;

        internal const string CustomSectionName = "Custom";

        public static void Init(string directory, string settingsJSON)
        {
            Logger = HBS.Logging.Logger.GetLogger("CustomComponents", LogLevel.Debug);
            try
            {
                try
                {

                    Settings = new CustomComponentSettings();
                    JSONSerializationUtility.FromJSON(Settings, settingsJSON);
                    HBS.Logging.Logger.SetLoggerLevel(Logger.Name, Settings.LogLevel);
                }
                catch (Exception)
                {
                        Settings = new CustomComponentSettings();
                }

                Settings.Complete();


                SetupLogging(directory);

                var harmony = HarmonyInstance.Create("io.github.denadan.CustomComponents");
                harmony.PatchAll(Assembly.GetExecutingAssembly());

                Registry.RegisterSimpleCustomComponents(Assembly.GetExecutingAssembly());
                Validator.RegisterMechValidator(CategoryController.Shared.ValidateMech, CategoryController.Shared.ValidateMechCanBeFielded);

                Logger.Log("Loaded CustomComponents v0.9.2.0 for bt 1.4");

                Validator.RegisterMechValidator(TagRestrictionsHandler.Shared.ValidateMech, TagRestrictionsHandler.Shared.ValidateMechCanBeFielded);
                Validator.RegisterDropValidator(check: TagRestrictionsHandler.Shared.ValidateDrop);

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
                }

#if CCDEBUG
                Logger.LogDebug("done");
#endif
            }
            catch (Exception e)
            {
                Logger.LogError(e);
            }
        }

        public static void FinishedLoading(Dictionary<string, Dictionary<string, VersionManifestEntry>> customResources)
        {
            CategoryController.Shared.Setup(customResources);
            DefaultFixer.Shared.Setup(customResources);
            TagRestrictionsHandler.Shared.Setup(customResources);
        }

#region LOGGING

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
