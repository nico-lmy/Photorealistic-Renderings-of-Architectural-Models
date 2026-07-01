using UnityEngine;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.Scripting;

namespace Vrpn.Input
{
    public struct VrpnStickState : IInputStateTypeInfo
    {
        #region Fields

        [Preserve, InputControl(name = "stick", displayName = "Stick", layout = "Stick", aliases = new[] { "Primary2DAxis", "Joystick" })]
        // Order is Horizontal, Vertical
        public Vector2 stick;

        #endregion

        #region Properties

        // Every state format is tagged with a FourCC code that is used for type
        // checking. The characters can be anything. Choose something that allows
        // you do easily recognize memory belonging to your own device.
        public FourCC format => new FourCC('V', 'R', 'P', 'S');

        #endregion
    }
}
