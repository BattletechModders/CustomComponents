namespace CustomComponents
{
    [CustomComponent("Lootable")]
    public class LootableDefault : SimpleCustomComponent, IValueComponent<string>
    {
        public string ItemID { get; set; }
        public void LoadValue(string value)
        {
            ItemID = value;
        }
    }
}