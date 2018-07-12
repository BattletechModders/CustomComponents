using System.Collections.Generic;
using HBS.Logging;
using Newtonsoft.Json;

namespace CustomComponents
{
    public class CustomComponentSettings
    {
        public LogLevel LogLevel = LogLevel.Debug;
        public List<CategoryDescriptor> Categories = new List<CategoryDescriptor>();
        public bool TestEnableAllTags = false;
    }
}
