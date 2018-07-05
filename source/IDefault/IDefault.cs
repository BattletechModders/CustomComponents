
namespace CustomComponents
{
    public interface INotSalvagable
    { }

    public interface ICannotRemove
    { }

    public interface IHideFromInventory
    { }

    public interface IDefault : INotSalvagable, ICannotRemove, IHideFromInventory
    {
    }
}
