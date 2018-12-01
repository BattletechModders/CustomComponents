using BattleTech.UI;
using UnityEngine;

namespace CustomComponents
{
    public interface IColorComponent
    {
        UIColor UIColor { get; }
        Color RGBColor { get; }
    }
}