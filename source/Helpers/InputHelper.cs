using UnityEngine;

namespace CustomComponents;

internal static class InputHelper
{
    internal static bool IsControlPressed => Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
}