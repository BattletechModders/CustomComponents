using BattleTech.UI;

namespace CustomComponents
{
    /// <summary>
    /// component has specific color
    /// </summary>
    [CustomComponent("Color")]
    public class ColorComponent : SimpleCustomComponent
    {
        /// <summary>
        /// color of component
        /// </summary>
        public UIColor Color { get; set; }
    }
}
