using Harmony;
using System;
using System.Reflection;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using fastJSON;
using HBS.Logging;
using HBS.Util;


namespace CustomComponents
{
    public static class Control
    {
        private static readonly Dictionary<string, CategoryDescriptor> Categories = new Dictionary<string, CategoryDescriptor>();
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

                LogDebug(DType.ShowConfig, JSONSerializationUtility.ToJSON(Settings));


                var harmony = HarmonyInstance.Create("io.github.denadan.CustomComponents");
                harmony.PatchAll(Assembly.GetExecutingAssembly());

                // make sure category is always run first, as it contains default customs
                //                Registry.RegisterSimpleCustomComponents(typeof(Category));
                Registry.RegisterSimpleCustomComponents(Assembly.GetExecutingAssembly());
                Validator.RegisterMechValidator(CategoryController.ValidateMech, CategoryController.ValidateMechCanBeFielded);

                Logger.Log("Loading CustomComponents v0.9.1.5 for bt 1.4");
                foreach (var categoryDescriptor in Settings.Categories)
                {
                    AddCategory(categoryDescriptor);

                }

                Validator.RegisterMechValidator(TagRestrictionsHandler.Shared.ValidateMech, TagRestrictionsHandler.Shared.ValidateMechCanBeFielded);
                Validator.RegisterDropValidator(check: TagRestrictionsHandler.Shared.ValidateDrop);
                foreach (var restriction in Settings.TagRestrictions)
                {
                    TagRestrictionsHandler.Shared.Add(restriction);
                }
                Logger.LogDebug("done");
            }
            catch (Exception e)
            {
                Logger.LogError(e);
            }
        }

        public static void AddTagRestrictions(TagRestrictions restrictions)
        {
            TagRestrictionsHandler.Shared.Add(restrictions);
        }

        internal static void AddNewCategory(string category)
        {
            if (Categories.TryGetValue(category, out _))
            {
                   return;
            }

            var c = new CategoryDescriptor { Name = category };
            Categories.Add(category, c);
        }

        public static void AddCategory(CategoryDescriptor category)
        {
            if (Categories.TryGetValue(category.Name, out var c))
            {
                c.Apply(category);
            }
            else
            {
                Categories.Add(category.Name, category);
                category.InitDefaults();
            }
        }

        public static CategoryDescriptor GetOrCreateCategory(string name)
        {
            if (Categories.TryGetValue(name, out var c))
                return c;
            c = new CategoryDescriptor { Name = name };
            Categories.Add(name, c);
            return c;
        }

        public static CategoryDescriptor GetCategory(string name)
        {
            return Categories.TryGetValue(name, out var c) ? c : null;
        }

        public static IEnumerable<CategoryDescriptor> GetCategories()
        {
            return Categories.Values;
        }

        #region LOGGING
        [Conditional("CCDEBUG")]
        public static void LogDebug(DType type, string message)
        {
            if (Settings.DebugInfo.Contains(type))
                Logger.LogDebug(message);
        }
        [Conditional("CCDEBUG")]
        public static void LogDebug(DType type, string message, Exception e = null)
        {
            if (Settings.DebugInfo.Contains(type))
                Logger.LogDebug(message, e);
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
