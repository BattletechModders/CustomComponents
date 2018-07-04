using BattleTech.UI;

namespace CustomComponents
{
    public enum ValidateDropStatus
    {
        Continue, Handled
    }

    public interface IValidateDropResult
    {
        ValidateDropStatus Status { get; } // not really necessary, but nice for semantics
    }

    public class ValidateDropReplaceItem : IValidateDropResult
    {
        public ValidateDropStatus Status => ValidateDropStatus.Continue;

        public MechLabItemSlotElement ToReplaceElement { get; }

        public ValidateDropReplaceItem(MechLabItemSlotElement toReplaceElement)
        {
            ToReplaceElement = toReplaceElement;
        }
    }

    public class ValidateDropHandled : IValidateDropResult
    {
        public ValidateDropStatus Status => ValidateDropStatus.Handled;
    }

    /// <summary>
    /// special behaviour for "upgrede kit" items
    /// </summary>
    public class ValidateDropRemoveDragItem : ValidateDropHandled
    {
        public bool ShowMessage = false;
        public string Message = "";

        public ValidateDropRemoveDragItem()
        {

        }

        public ValidateDropRemoveDragItem(string message)
        {
            ShowMessage = true;
            Message = message;
        }
    }

    public class ValidateDropError : ValidateDropHandled
    {
        public string ErrorMessage { get; }

        public ValidateDropError(string errorMessage)
        {
            ErrorMessage = errorMessage;
        }
    }
}
