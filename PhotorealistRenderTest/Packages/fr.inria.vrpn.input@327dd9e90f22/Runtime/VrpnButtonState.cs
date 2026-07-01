using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Utilities;

namespace Vrpn.Input
{
    public struct VrpnButtonState : IInputStateTypeInfo
    {
        #region Fields

        [InputControl(name = "button", displayName = "Button", layout = "Button")]
        public bool button;

        #endregion

        #region Properties

        // Every state format is tagged with a FourCC code that is used for type
        // checking. The characters can be anything. Choose something that allows
        // you do easily recognize memory belonging to your own device.
        public FourCC format => new FourCC('V', 'R', 'P', 'B');

        #endregion
    }
}
