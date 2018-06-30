using BattleTech.UI;

namespace CustomComponents
{
    /// <summary>
    /// component has specific color
    /// </summary>
    interface IColorComponent
    {
        /// <summary>
        /// color of component
        /// </summary>
        UIColor Color { get; }
    }
}
