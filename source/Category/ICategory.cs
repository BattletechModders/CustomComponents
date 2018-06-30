using fastJSON;

namespace CustomComponents
{
    public interface ICategory
    {
        string Category { get; }

        [JsonIgnore]
        CategoryDescriptor CategoryDescriptor { get; set; }
    }
}