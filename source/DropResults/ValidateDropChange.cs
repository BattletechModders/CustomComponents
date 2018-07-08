using System.Collections.Generic;

namespace CustomComponents
{
    public class ValidateDropChange : IValidateDropResult
    {
        public ValidateDropStatus Status => ValidateDropStatus.Continue;

        public List<IChange> Changes = new List<IChange>();

        protected ValidateDropChange()
        {
        }

        public static ValidateDropChange AddOrCreate(IValidateDropResult old_result, IChange change)
        {
            if(!(old_result is ValidateDropChange new_result))
                new_result = new ValidateDropChange();
            new_result.Changes.Add(change);
            return new_result;
        }

    }
}