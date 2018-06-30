﻿using System.Collections.Generic;
using System.Linq;
using BattleTech;
using BattleTech.UI;

namespace CustomComponents
{
    public static class CategoryController
    {
        public static void ValidateMech(Dictionary<MechValidationType, List<string>> errors,
            MechValidationLevel validationLevel, MechDef mechDef)
        {
            var items_by_category = (from item in mechDef.Inventory
                                     where item.Def is ICategory
                                     let def = item.Def as ICategory
                                     select new
                                     {
                                         category = def.CategoryDescriptor,
                                         itemdef = item.Def,
                                         itemref = item
                                     }).GroupBy(i => i.category).ToDictionary(i => i.Key, i => i.ToList());

            foreach (var category in Control.GetCategories().Where(i => i.Requred))
            {
                if (!items_by_category.ContainsKey(category) || items_by_category[category].Count < category.MinEquiped)
                    if (category.MinEquiped == 1)
                        errors[MechValidationType.InvalidInventorySlots].Add(string.Format(category.ValidateRequred, category.DisplayName.ToUpper(), category.DisplayName));
                    else
                        errors[MechValidationType.InvalidInventorySlots].Add(string.Format(category.ValidateMinimum, category.DisplayName.ToUpper(), category.DisplayName, category.MinEquiped));
            }

            foreach (var pair in items_by_category)
            {
                if (pair.Key.MaxEquiped > 0 && pair.Value.Count > pair.Key.MaxEquiped)
                    if (pair.Key.MaxEquiped == 1)
                        errors[MechValidationType.InvalidInventorySlots].Add(string.Format(pair.Key.ValidateUnique,
                            pair.Key.DisplayName.ToUpper(), pair.Key.DisplayName));
                    else
                        errors[MechValidationType.InvalidInventorySlots].Add(string.Format(pair.Key.ValidateMaximum,
                            pair.Key.DisplayName.ToUpper(), pair.Key.DisplayName, pair.Key.MaxEquiped));

                if (!pair.Key.AllowMix)
                {
                    string def = pair.Value[0].itemdef.Description.Id;
                    bool flag = pair.Value.Select(i => i.itemdef.Description.Id).Any(d => def != d);
                    if (flag)
                    {
                        errors[MechValidationType.InvalidInventorySlots].Add(string.Format(pair.Key.ValidateMixed,
                            pair.Key.DisplayName.ToUpper(), pair.Key.DisplayName));
                    }
                }

                if (pair.Key.MaxEquipedPerLocation > 0)
                {
                    var max = pair.Value.GroupBy(i => i.itemref.MountedLocation).Max(i => i.Count());
                    if(max > pair.Key.MaxEquipedPerLocation)
                        if (pair.Key.MaxEquipedPerLocation == 1)
                            if (pair.Key.MaxEquiped == 1)
                                errors[MechValidationType.InvalidInventorySlots].Add(string.Format(pair.Key.ValidateUniqueLocation,
                                    pair.Key.DisplayName.ToUpper(), pair.Key.DisplayName));
                            else
                                errors[MechValidationType.InvalidInventorySlots].Add(string.Format(pair.Key.ValidateMaximumLocation,
                                    pair.Key.DisplayName.ToUpper(), pair.Key.DisplayName, pair.Key.MaxEquipedPerLocation ));
                }
            }




            //var items_by_category = (
            //    from itemRef in mechDef.Inventory
            //    let info = itemRef.GetUniqueItem()
            //    where info != null
            //    group info by info.ReplaceTag
            //    into g
            //    select new { tag = g.Key, count = g.Count() }
            //).ToDictionary(i => i.tag, i => i.count);

            //foreach (var category in Control.settings.UniqueCategories)
            //{
            //    int n = 0;
            //    if (items_by_category.TryGetValue(category.Tag, out n))
            //    {
            //        if (n > 1)
            //        {
            //            errorMessages[MechValidationType.InvalidInventorySlots]
            //                .Add(string.Format(category.ErrorToMany, category.Tag.ToUpper(), category.Tag));
            //        }
            //    }
            //    else
            //    {
            //        if (category.Required)
            //            errorMessages[MechValidationType.InvalidInventorySlots]
            //                .Add(string.Format(category.ErrorMissing, category.Tag.ToUpper(), category.Tag));
            //    }
            //}
        }

        public static bool ValidateAdd(MechComponentDef component, MechLabLocationWidget widget,
            bool current_result, ref string errorMessage, MechLabPanel mechlab)
        {
            return current_result;
        }
    }
}