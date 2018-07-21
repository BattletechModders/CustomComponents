using Harmony;
using System;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using BattleTech.Data;
using System.Text.RegularExpressions;
using BattleTech;
using HBS.Logging;
using Newtonsoft.Json;


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
            Logger = HBS.Logging.Logger.GetLogger("CustomComponents", LogLevel.Error);
            try
            {

                try
                {
                    Settings = JsonConvert.DeserializeObject<CustomComponentSettings>(settingsJSON);
                    HBS.Logging.Logger.SetLoggerLevel(Logger.Name, Settings.LogLevel);
                }
                catch (Exception)
                {
                    Settings = new CustomComponentSettings();
                }
                
                SetupLogging(directory);

                var harmony = HarmonyInstance.Create("io.github.denadan.CustomComponents");
                harmony.PatchAll(Assembly.GetExecutingAssembly());

                // make sure category is always run first, as it contains default customs
                Registry.RegisterSimpleCustomComponents(typeof(Category));
                Registry.RegisterSimpleCustomComponents(Assembly.GetExecutingAssembly());
                Validator.RegisterMechValidator(CategoryController.ValidateMech, CategoryController.ValidateMechCanBeFielded);

                Logger.Log("Loaded CustomComponents");
                Logger.LogDebug("Loading Categories");
                foreach (var categoryDescriptor in Settings.Categories)
                {
                    AddCategory(categoryDescriptor);
                    Logger.LogDebug(categoryDescriptor.Name + " - " + categoryDescriptor.DisplayName);
                }
                Logger.LogDebug("done");
            }
            catch (Exception e)
            {
                Logger.LogError(e);
            }
        }

        internal static void AddNewCategory(string category)
        {
            Logger.LogDebug($"Create new category: {category}");
            if (Categories.TryGetValue(category, out _))
            {
                Logger.LogDebug("Already exist");
                return;
            }

            var c = new CategoryDescriptor { Name = category };
            Categories.Add(category, c);
        }

        public static void AddCategory(CategoryDescriptor category)
        {
            Logger.LogDebug($"Add Category: {category.Name}");
            if (Categories.TryGetValue(category.Name, out var c))
            {
                Logger.LogDebug($"Already have, apply: {category.Name}");
                c.Apply(category);
            }
            else
            {
                Logger.LogDebug($"Adding new: {category.Name}");
                Categories.Add(category.Name, category);
            }

            Logger.LogError($"Current Categories");
            foreach (var categoryDescriptor in Categories)
            {
                Logger.LogDebug($" - {categoryDescriptor.Value.Name}");
            }
        }

        public static CategoryDescriptor GetCategory(string name)
        {
            if (Categories.TryGetValue(name, out var c))
                return c;
            c = new CategoryDescriptor { Name = name };
            Categories.Add(name, c);
            return c;
        }

        public static IEnumerable<CategoryDescriptor> GetCategories()
        {
            return Categories.Values;
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
