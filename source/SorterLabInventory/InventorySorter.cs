
namespace CustomComponents
{
    [CustomComponent("InventorySorter")]
    public class InventorySorter : SimpleCustomComponent, IValueComponent 
    {
        public string SortKey { get; set; }
        public void LoadValue(object value)
        {
            SortKey = value.ToString();
        }
    }
}