using UnityEngine;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.Scripting;

namespace Vrpn.Input
{
    public struct VrpnXRHMDState : IInputStateTypeInfo
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
        [Preserve, InputControl(name = "devicePosition", displayName = "Position", layout = "Vector3", usages = new[] { "Position" })]
        public Vector3 position;

        /// <summary>
        /// name attribute is important so that TrackedDevice class can find this input
        /// </summary>
        /// <see cref="UnityEngine.InputSystem.TrackedDevice"/>
        [Preserve, InputControl(name = "deviceRotation", displayName = "Rotation", layout = "Quaternion", usages = new[] { "Orientation" })]
        public Quaternion rotation;

        #endregion

        #region Properties

        // Every state format is tagged with a FourCC code that is used for type
        // checking. The characters can be anything. Choose something that allows
        // you do easily recognize memory belonging to your own device.
        public FourCC format => new('V', 'R', 'P', 'H');

        #endregion
    }
}
