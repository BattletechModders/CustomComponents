namespace CustomComponents
{
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
}