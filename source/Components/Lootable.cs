namespace CustomComponents
{
    [CustomComponent("Lootable")]
    public class LootableDefault : SimpleCustomComponent, IValueComponent
    {
        public string ItemID { get; set; }
        public void LoadValue(object value)
        {
            ItemID = value.ToString();
        }
    }
}