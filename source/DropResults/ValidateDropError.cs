namespace CustomComponents
{
    public class ValidateDropError : ValidateDropHandled
    {
        public string ErrorMessage { get; }

        public ValidateDropError(string errorMessage)
        {
            ErrorMessage = errorMessage;
        }
    }
}