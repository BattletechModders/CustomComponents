namespace CustomComponents
{
    public interface IDefaultComponent
    {
        
    }

    [CustomComponent("Default")]
    public class DefaultComponent : SimpleCustomComponent, IDefaultComponent
    {

    }
}