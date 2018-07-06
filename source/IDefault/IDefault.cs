
namespace CustomComponents
{
    public interface INotSalvagable
    { }

    public interface ICannotRemove
    { }

    public interface IHideFromInventory
    { }

    public interface IAutoRepair
    { }

    public interface IDefault : INotSalvagable, ICannotRemove, IHideFromInventory, IAutoRepair
    { }

    public interface IDefaultRepace
    {
        string DefaultID { get; }
    }
}
