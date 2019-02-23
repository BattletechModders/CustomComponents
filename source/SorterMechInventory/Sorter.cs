namespace CustomComponents
{
    [CustomComponent("Sorter")]
    public class Sorter : SimpleCustomComponent, ISorter
    {
        public int Order { get; set; }
    }
}