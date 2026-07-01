using UnityEngine;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.Scripting;

// A "state struct" describes the memory format used a device. Each device can
// receive and store memory in its custom format. InputControls are then connected
// the individual pieces of memory and read out values from them.
//
// In case it is important that the memory format matches 1:1 at the binary level
// to an external representation, it is generally advisable to use
// LayoutLind.Explicit.

namespace Vrpn.Input
{
    public struct VrpnTrackerState : IInputStateTypeInfo
    {
        #region Fields

        /// <summary>
        /// name attribute is important so that TrackedDevice class can find this input
        /// </summary>
        /// <see cref="UnityEngine.InputSystem.TrackedDevice"/>
        [Preserve, InputControl(name = "isTracked", displayName = "Tracked", layout = "Button")]
        public bool isTracked;


        [Preserve, InputControl(name = "trackingState", displayName = "TrackingState", layout = "Integer")]
        public int trackingState;

        /// <summary>
        /// name attribute is important so that TrackedDevice class can find this input
        /// </summary>
        /// <see cref="UnityEngine.InputSystem.TrackedDevice"/>
        [Preserve, InputControl(name = "devicePosition", displayName = "Position", layout = "Vector3")]
        public Vector3 position;

        /// <summary>
        /// name attribute is important so that TrackedDevice class can find this input
        /// </summary>
        /// /// <see cref="UnityEngine.InputSystem.TrackedDevice"/>
        [Preserve, InputControl(name = "deviceRotation", displayName = "Rotation", layout = "Quaternion")]
        public Quaternion rotation;

        #endregion

        #region Properties

        // Every state format is tagged with a FourCC code that is used for type
        // checking. The characters can be anything. Choose something that allows
        // you do easily recognize memory belonging to your own device.
        public FourCC format => new('V', 'R', 'P', 'T');

        #endregion
    }
}