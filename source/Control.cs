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
        private static Dictionary<string, CategoryDescriptor> categories = new Dictionary<string, CategoryDescriptor>();

        public static CustomComponentSettings settings = new CustomComponentSettings();


        internal static ILog Logger;
        private static FileLogAppender logAppender;


        public static void Init(string directory, string settingsJSON)
        {

            Logger = HBS.Logging.Logger.GetLogger("CustomComponents", LogLevel.Debug);
            SetupLogging(directory);

            try
            {
                //   mod.LoadSettings(settings);
                try
                {
                    settings = JsonConvert.DeserializeObject<CustomComponentSettings>(settingsJSON);
                }
                catch (Exception)
                {
                    settings = new CustomComponentSettings();
                }

                var harmony = HarmonyInstance.Create("io.github.denadan.CustomComponents");
                harmony.PatchAll(Assembly.GetExecutingAssembly());


                Registry.RegisterSimpleCustomComponents(Assembly.GetExecutingAssembly());
                Validator.RegisterMechValidator(CategoryController.ValidateMech, CategoryController.ValidateMechCanBeFielded);

                Logger.Log("Loaded CustomComponents");
                Logger.LogDebug("Loading Categories");
                foreach (var categoryDescriptor in settings.Categories)
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
            if (categories.TryGetValue(category, out _))
            {
                Logger.LogDebug("Already exist");
                return;
            }

            var c = new CategoryDescriptor { Name = category };
            categories.Add(category, c);
        }

        public static void AddCategory(CategoryDescriptor category)
        {
            Logger.LogDebug($"Add Category: {category.Name}");
            if (categories.TryGetValue(category.Name, out var c))
            {
                Logger.LogDebug($"Already have, apply: {category.Name}");
                c.Apply(category);
            }
            else
            {
                Logger.LogDebug($"Adding new: {category.Name}");
                categories.Add(category.Name, category);
            }
        }

        public static CategoryDescriptor GetCategory(string name)
        {
            if (categories.TryGetValue(name, out var c))
                return c;
            c = new CategoryDescriptor { Name = name };
            categories.Add(name, c);
            return c;
        }

        public static IEnumerable<CategoryDescriptor> GetCategories()
        {
            return categories.Values;
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
