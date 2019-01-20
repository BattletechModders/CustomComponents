using System.Collections.Generic;

namespace CustomComponents
{
    public enum ComponentCount
    {
        Unique,
        Multiple
    }

    public interface IRepalceIdentifier
    {
        string ReplaceID { get; }
    }
}