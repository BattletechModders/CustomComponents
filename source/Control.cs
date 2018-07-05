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
        private static Dictionary<string, CustomComponentDescriptor> descriptors = new Dictionary<string, CustomComponentDescriptor>();
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


                RegisterCustomTypes(Assembly.GetExecutingAssembly());

                if (settings.LoadDefaultValidators)
                {
                    Validator.RegisterMechValidator(WeighLimitedController.ValidateMech, WeighLimitedController.ValidateMechCanBeFielded);
                    Validator.RegisterMechValidator(CategoryController.ValidateMech, CategoryController.ValidateMechCanBeFielded);
                    Validator.RegisterMechValidator(LinkedController.ValidateMech, LinkedController.ValidateMechCanBeFielded);

                    Validator.RegisterDropValidator(CategoryController.ValidateDrop);
                    Validator.RegisterDropValidator(WeighLimitedController.ValidateDrop);
                    Validator.RegisterDropValidator(LinkedController.ValidateDrop);

                    MechLabFilter.AddFilter(WeighLimitedController.Filter);
                }

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

        internal static ICustomComponent CreateNew(string custom_type)
        {
            return descriptors.TryGetValue(custom_type, out var descriptor) ? descriptor.CreateNew() : null;
        }

        /// <summary>
        /// register custom types for your assamble
        /// </summary>
        /// <param name="assembly"></param>
        public static void RegisterCustomTypes(Assembly assembly)
        {
            var desc_list = from type in assembly.GetTypes()
                            let attributes = type.GetCustomAttributes(typeof(CustomAttribute), true)
                            where attributes != null && attributes.Length > 0
                            let attribute = attributes[0] as CustomAttribute
                            select new CustomComponentDescriptor(attribute.CustomType, type);

            foreach (var item in desc_list)
            {
                CustomComponentDescriptor temp_desc;
                if (descriptors.TryGetValue(item.CustomName, out temp_desc))
                    descriptors[item.CustomName] = item;
                else
                    descriptors.Add(item.CustomName, item);
            }
        }

        private static readonly Regex CustomTypeRegex = new Regex(@"""CustomType""\s*:\s*""([^""]+)""", RegexOptions.Compiled);

        public static bool LoaderPatch<T>(DataManager.ResourceLoadRequest<T> loader,
            string json, ref T resource)
            where T : class
        {
            var id = Regex.Match(json, "\"UIName\" : \"(.+?)\"");

            //Control.mod.Logger.Log("loading " + (id.Success ? id.Result("$1") : "not found"));

            var custom = CustomTypeRegex.Match(json);
            if (!custom.Success)
                return true;

            string custom_type = custom.Groups[1].Value;

            Logger.LogDebug("Loading custom: " + custom_type);


            var custom_obj = Control.CreateNew(custom_type) as ICustomComponent;
            if (custom_obj == null || !(custom_obj is T))
            {
                Logger.LogError("Error: Create new return null");
                throw new ArgumentNullException("json");
            }

            custom_obj.FromJson(json);

            if (custom_obj is ICategory)
            {
                var cat = custom_obj as ICategory;
                cat.CategoryDescriptor = GetCategory(cat.CategoryID);
            }

            //string new_json = custom_obj.ToJson();
            //Control.mod.Logger.Log(new_json);

            resource = custom_obj as T;
            Traverse.Create(loader).Method("TryLoadDependencies", resource).GetValue();
            if (custom_obj is MechComponentDef)
                Logger.LogDebug("Loaded: " + (custom_obj as MechComponentDef).Description.Id);

            return false;
        }

        public static bool JsonPatch<T>(T value, ref string result)
        {
            if (!(value is ICustomComponent))
                return true;

            var custom = value as ICustomComponent;
            result = custom.ToJson();
            return false;
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
