#undef CCDEBUG

using Harmony;
using System;
using System.Reflection;
using System.Collections.Generic;
using System.IO;
using BattleTech.UI;
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
            Logger = HBS.Logging.Logger.GetLogger("CustomComponents", LogLevel.Debug);
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

                Settings.Complete();


                SetupLogging(directory);
#if CCDEBUG
                var str = JsonConvert.SerializeObject(Settings, Formatting.Indented);
                Logger.LogDebug(str);

#endif  
                var harmony = HarmonyInstance.Create("io.github.denadan.CustomComponents");
                harmony.PatchAll(Assembly.GetExecutingAssembly());

                // make sure category is always run first, as it contains default customs
                Registry.RegisterSimpleCustomComponents(typeof(Category));
                Registry.RegisterSimpleCustomComponents(Assembly.GetExecutingAssembly());
                Validator.RegisterMechValidator(CategoryController.ValidateMech, CategoryController.ValidateMechCanBeFielded);

                Logger.Log("Loaded CustomComponents v0.8.5.0 for bt 1.3.2");
#if CCDEBUG
                Logger.LogDebug("Loading Categories");
#endif  
                foreach (var categoryDescriptor in Settings.Categories)
                {
                    AddCategory(categoryDescriptor);
#if CCDEBUG
                    Logger.LogDebug(categoryDescriptor.Name + " - " + categoryDescriptor.DisplayName);
#endif  
                }

                Validator.RegisterMechValidator(TagRestrictionsHandler.Shared.ValidateMech, TagRestrictionsHandler.Shared.ValidateMechCanBeFielded);
                Validator.RegisterDropValidator(check: TagRestrictionsHandler.Shared.ValidateDrop);
                foreach (var restriction in Settings.TagRestrictions)
                {
                    TagRestrictionsHandler.Shared.Add(restriction);
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

        public static void AddTagRestrictions(TagRestrictions restrictions)
        {
            TagRestrictionsHandler.Shared.Add(restrictions);
        }

        internal static void AddNewCategory(string category)
        {
#if CCDEBUG
            Logger.LogDebug($"Create new category: {category}");
#endif
            if (Categories.TryGetValue(category, out _))
            {
#if CCDEBUG
                Logger.LogDebug("Already exist");
#endif
                return;
            }

            var c = new CategoryDescriptor { Name = category };
            Categories.Add(category, c);
        }

        public static void AddCategory(CategoryDescriptor category)
        {
#if CCDEBUG
            Logger.LogDebug($"Add Category: {category.Name}");
#endif
            if (Categories.TryGetValue(category.Name, out var c))
            {
#if CCDEBUG
                Logger.LogDebug($"Already have, apply: {category.Name}");
#endif
                c.Apply(category);
            }
            else
            {
#if CCDEBUG
                Logger.LogDebug($"Adding new: {category.Name}");
#endif
                Categories.Add(category.Name, category);
            }

#if CCDEBUG
            Logger.LogDebug($"Current Categories");
            foreach (var categoryDescriptor in Categories)
            {
                Logger.LogDebug($" - {categoryDescriptor.Value.Name}");
            }
#endif
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
