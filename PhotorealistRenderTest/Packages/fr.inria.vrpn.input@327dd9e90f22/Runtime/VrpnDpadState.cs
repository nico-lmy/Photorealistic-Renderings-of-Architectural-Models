using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.Scripting;

namespace Vrpn.Input
{
    public struct VrpnDpadState : IInputStateTypeInfo
    {
        #region Fields

        [Preserve, InputControl(name = "dpad", layout = "Dpad", usage = "Hatswitch", displayName = "D-Pad", format = "BIT", sizeInBits = 4, bit = 0)]
        // Order is Up, Down, Left, Right
        public uint dpad;

        #endregion

        #region Properties

        // Every state format is tagged with a FourCC code that is used for type
        // checking. The characters can be anything. Choose something that allows
        // you do easily recognize memory belonging to your own device.
        public FourCC format => new FourCC('V', 'R', 'P', 'D');

        #endregion
    }
}
