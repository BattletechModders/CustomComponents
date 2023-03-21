
namespace CustomComponents;

[CustomComponent("InventorySorter")]
public class InventorySorter : SimpleCustomComponent, IValueComponent<string>
{
    public string SortKey { get; set; }
    public void LoadValue(string value)
    {
        SortKey = value;
    }
}