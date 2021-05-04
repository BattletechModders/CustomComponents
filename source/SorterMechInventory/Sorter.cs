namespace CustomComponents
{
    [CustomComponent("Sorter")]
    public class Sorter : SimpleCustomComponent, ISorter, IValueComponent
    {
        public int Order { get; set; }
        public void LoadValue(object value)
        {
            Order = value is int i ? i : 0;

        }
    }
}