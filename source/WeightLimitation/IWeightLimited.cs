namespace CustomComponents
{
    /// <summary>
    /// component limited to mech tonnage
    /// </summary>
    public interface IWeightLimited
    {
        /// <summary>
        /// minimum allowed tonnage
        /// </summary>
        int MinTonnage { get; }
        /// <summary>
        /// maximum allowed tonnage
        /// </summary>
        int MaxTonnage { get; }
    }

    public interface IWeightAllowed
    {
        int AllowedTonnage { get; }
    }
}
