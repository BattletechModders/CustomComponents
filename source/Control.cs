using DynModLib;
using Harmony;
using System;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using BattleTech.Data;
using System.Text.RegularExpressions;
using BattleTech;

namespace CustomComponents
{
    public static class Control
    {
        public static Mod mod;
        public static CustomCompoentSettings settings = new CustomCompoentSettings();

        public static void Init(string directory, string settingsJSON)
        {
            mod = new Mod(directory);

            try
            {
                //   mod.LoadSettings(settings);
                mod.LoadSettings(settings);

                var harmony = HarmonyInstance.Create("io.github.denadan.CustomComponents");
                harmony.PatchAll(Assembly.GetExecutingAssembly());
                RegisterCustomTypes(Assembly.GetExecutingAssembly());

                if (settings.LoadDefaultValidators)
                {
                    Validator.RegisterValidator(WeighLimitedController.ValidateMech);
                    Validator.RegisterAddValidator(typeof(IWeightLimited), WeighLimitedController.ValidateAdd);
                }

                // logging output can be found under BATTLETECH\BattleTech_Data\output_log.txt
                // or also under yourmod/log.txt
                mod.Logger.Log("Loaded " + mod.Name);
            }
            catch (Exception e)
            {
                mod.Logger.LogError(e);
            }
        }

        internal static ICustomComponent CreateNew(string custom_type)
        {
            CustomComponentDescriptor descriptor = null;
            if (descriptors.TryGetValue(custom_type, out descriptor))
            {
                return descriptor.CreateNew();
            }
            return null;
        }

        private static Dictionary<string, CustomComponentDescriptor> descriptors = new Dictionary<string, CustomComponentDescriptor>();


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


        public static bool LoaderPatch<T>(DataManager.ResourceLoadRequest<T> loader,
            string json, ref T resource)
            where T: class
        {
            var id = Regex.Match(json, "\"UIName\" : \"(.+?)\"");

            //Control.mod.Logger.Log("loading " + (id.Success ? id.Result("$1") : "not found"));

            var custom = Regex.Match(json, "\"CustomType\" : \"(.+?)\"");
            if (!custom.Success)
                return true;

            string custom_type = custom.Result("$1");

            Control.mod.Logger.LogDebug("Loading custom: " + custom_type);

            var custom_obj = Control.CreateNew(custom_type) as ICustomComponent;
            if (custom_obj == null || !(custom_obj is T))
            {
                Control.mod.Logger.LogError("Error: Create new return null");
                throw new ArgumentNullException("json");
            }

            custom_obj.FromJson(json);

            string new_json = custom_obj.ToJson();
            //Control.mod.Logger.Log(new_json);

            resource = custom_obj as T;
            Traverse.Create(loader).Method("TryLoadDependencies", resource).GetValue();
            if(custom_obj is MechComponentDef)
                Control.mod.Logger.LogDebug("Loaded: " + (custom_obj as MechComponentDef).Description.Id);

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
    }
}
