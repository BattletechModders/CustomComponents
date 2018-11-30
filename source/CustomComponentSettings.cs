using System.Collections.Generic;
using BattleTech.UI;
using HBS.Logging;
using Newtonsoft.Json;
using UnityEngine;

namespace CustomComponents
{
    [SerializeField]
    public struct CCColor
    {
        public byte R;
        public byte G;
        public byte B;
        public byte A;

        public Color ToColor()
        {
            return new Color(R / 255f, G / 255f, B / 255f, A / 255f);
        }
    }

    public class CustomComponentSettings
    {
        public LogLevel LogLevel = LogLevel.Debug;
        public List<CategoryDescriptor> Categories = new List<CategoryDescriptor>();
        public List<TagRestrictions> TagRestrictions = new List<TagRestrictions>();
        public bool TagRestrictionDropValidateRequiredTags = false;
        public bool TagRestrictionDropValidateIncompatibleTags = true;

        public bool UseDefaultFixedColor = false;
        public UIColor DefaultFlagBackgroundColor = UIColor.UpgradeColor;
        public UIColor InvalidFlagBackgroundColor = UIColor.Red;

        public CCColor PreinstalledOverlayCColor = new CCColor() { A = 12, R = 255, B = 180, G = 180 };
        public CCColor DefaultFlagOverlayCColor = new CCColor() { A = 12, R = 180, B = 180, G = 255 };

        [JsonIgnore] public Color PreinstalledOverlayColor;
        [JsonIgnore] public Color DefaultFlagOverlayColor;

        public bool TestEnableAllTags = false;

        public void Complete()
        {
            PreinstalledOverlayColor = PreinstalledOverlayCColor.ToColor();
            DefaultFlagOverlayColor = DefaultFlagOverlayCColor.ToColor();
        }
    }
}
