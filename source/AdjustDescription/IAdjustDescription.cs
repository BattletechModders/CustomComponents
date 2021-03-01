using System;

namespace CustomComponents
{
    [Obsolete]
    public interface IAdjustDescription
    {
        string AdjustDescription(string Description);
    }

    public interface IAdjustDescriptionED
    {
        void AdjustDescription();
    }
}
