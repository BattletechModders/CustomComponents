using System;
using System.Collections.Generic;
using BattleTech;
using BattleTech.UI;

namespace CustomComponents
{
    public static class TColorExtentions
    {
        public static void SetTColor(this UIColorRefTracker color_text, UIColorRefTracker color_icon, MechComponentRef cref)
        {

            Control.LogDebug(DType.Color, $"TextColor for {cref.ComponentDefID}");
            if (cref.Is<ITColorComponent>(out var color))
            {
                Control.LogDebug(DType.Color, $"-- color found set to {color.UIColor}/{color.RGBColor}");

                color_text.SetCustomColor(color.UIColor, color.RGBColor);
                if (color_icon != null)
                    if (color.Icon)
                        color_icon.SetCustomColor(color.UIColor, color.RGBColor);
                    else
                        color_icon.SetUIColor(UIColor.White);
            }
            else
            {
                Control.LogDebug(DType.Color, $"-- no color set to white");
                color_text.SetUIColor(UIColor.White);
                if (color_icon != null)
                    color_icon.SetUIColor(UIColor.White);
            }
        }

        public static void SetTColor(this UIColorRefTracker color_text, UIColorRefTracker color_icon, MechComponentDef cdef)
        {
            if (cdef.Is<ITColorComponent>(out var color))
            {
                color_text.SetCustomColor(color.UIColor, color.RGBColor);
                if (color_icon != null)
                    if (color.Icon)
                        color_icon.SetCustomColor(color.UIColor, color.RGBColor);
                    else
                        color_icon.SetUIColor(UIColor.White);
            }
            else
            {
                color_text.SetUIColor(UIColor.White);
                if (color_icon != null)
                    color_icon.SetUIColor(UIColor.White);
            }
        }

        public static void SetTColor(this IEnumerable<UIColorRefTracker> color_text, UIColorRefTracker color_icon, MechComponentRef cref)
        {
            if (cref.Is<ITColorComponent>(out var color))
            {
                color_text.SetCustomColor(color.UIColor, color.RGBColor);
                if (color_icon != null)
                    if (color.Icon)
                        color_icon.SetCustomColor(color.UIColor, color.RGBColor);
                    else
                        color_icon.SetUIColor(UIColor.White);
            }
            else
            {
                foreach (var color_tracker in color_text)
                    color_tracker.SetUIColor(UIColor.White);
                if (color_icon != null)
                    color_icon.SetUIColor(UIColor.White);
            }
        }
        
        public static void SetTColor(this IEnumerable<UIColorRefTracker> color_text, UIColorRefTracker color_icon, MechComponentDef cdef)
        {
            if (cdef.Is<ITColorComponent>(out var color))
            {
                color_text.SetCustomColor(color.UIColor, color.RGBColor);
                if (color_icon != null)
                    if (color.Icon)
                        color_icon.SetCustomColor(color.UIColor, color.RGBColor);
                    else
                        color_icon.SetUIColor(UIColor.White);
            }
            else
            {
                foreach (var color_tracker in color_text)
                    color_tracker.SetUIColor(UIColor.White);
                if (color_icon != null)
                    color_icon.SetUIColor(UIColor.White);
            }
        }

        internal static void ChangeTextIconColor(MechComponentDef cdef, InventoryItemElement_NotListView theWidget)
        {
            var color_text = theWidget.itemName.GetComponent<UIColorRefTracker>();
            var color_icon = theWidget.icon.GetComponent<UIColorRefTracker>();
            if (color_icon == null)
            {
                color_icon = theWidget.icon.gameObject.AddComponent<UIColorRefTracker>();
            }

            if (cdef.Is<ITColorComponent>(out var color))
            {
                color_text.SetCustomColor(color.UIColor, color.RGBColor);


                if (color_icon != null)
                    if (color.Icon)
                        color_icon.SetCustomColor(color.UIColor, color.RGBColor);
                    else
                        color_icon.SetUIColor(UIColor.White);
            }
            else
            {
                color_text.SetUIColor(UIColor.White);
                color_icon.SetUIColor(UIColor.White);
            }
        }

        public static void ChangeTextIconColor(MechComponentDef cdef, InventoryItemElement theWidget)
        {
            var color_text = theWidget.itemName.GetComponent<UIColorRefTracker>();
            var color_icon = theWidget.icon.GetComponent<UIColorRefTracker>();
            if (color_icon == null)
            {
                color_icon = theWidget.icon.gameObject.AddComponent<UIColorRefTracker>();
            }

            if (cdef.Is<ITColorComponent>(out var color))
            {
                color_text.SetCustomColor(color.UIColor, color.RGBColor);


                if (color_icon != null)
                    if (color.Icon)
                        color_icon.SetCustomColor(color.UIColor, color.RGBColor);
                    else
                        color_icon.SetUIColor(UIColor.White);
            }
            else
            {
                color_text.SetUIColor(UIColor.White);
                color_icon.SetUIColor(UIColor.White);
            }
        }
    }
}