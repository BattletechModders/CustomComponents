namespace CustomComponents;

[CustomComponent("Sorter")]
public class SorterCustom : SimpleCustomComponent, ISorter, IValueComponent<int>
{
    public int Order { get; set; }
    public void LoadValue(int value)
    {
        Order = value;
    }
}