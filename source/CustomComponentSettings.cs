using System.Collections.Generic;
using HBS.Logging;

namespace CustomComponents
{
    public class CustomComponentSettings
    {
        public LogLevel LogLevel = LogLevel.Debug;
        public List<CategoryDescriptor> Categories = new List<CategoryDescriptor>();
        public List<TagRestrictions> TagRestrictions = new List<TagRestrictions>();
        public bool TestEnableAllTags = false;
    }
}
